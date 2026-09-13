using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Radni nalozi. Dodjelom prijave izvršitelju nastaje nalog; ponovna dodjela
/// zatvara prethodni nalog, ali ga ne briše - tako nastaje povijest.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkAssignmentsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public WorkAssignmentsController(KvaroviDbContext context)
    {
        _context = context;
    }

    /// <summary>Povijest naloga za jednu prijavu, od najnovijeg prema najstarijem.</summary>
    [HttpGet("report/{faultReportId:int}")]
    public async Task<ActionResult<List<WorkAssignmentDto>>> GetForReport(int faultReportId)
    {
        var report = await _context.FaultReports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == faultReportId);

        if (report is null)
        {
            return NotFound();
        }

        if (!User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Technician) &&
            report.ReporterId != User.GetEmployeeId())
        {
            return Forbid();
        }

        var assignments = await _context.WorkAssignments
            .AsNoTracking()
            .Where(x => x.FaultReportId == faultReportId)
            .OrderByDescending(x => x.AssignedAt)
            .Select(DtoProjections.Assignment)
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>
    /// Radni nalozi prijavljenog izvršitelja. Identitet se čita iz tokena,
    /// pa izvršitelj ne može zatražiti tuđe naloge.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.FieldWork)]
    [HttpGet("mine")]
    public async Task<ActionResult<List<WorkAssignmentDto>>> GetMine(
        [FromQuery] bool includeFinished = false)
    {
        var employeeId = User.GetEmployeeId();

        if (employeeId is null)
        {
            return BadRequest("Vaš račun nije povezan s djelatnikom pa nemate radnih naloga.");
        }

        var query = _context.WorkAssignments
            .AsNoTracking()
            .Where(x => x.TechnicianId == employeeId.Value);

        if (!includeFinished)
        {
            query = query.Where(x => x.IsActive &&
                                     x.FaultReport!.FaultStatusId != FaultStatusIds.Zatvoreno);
        }

        var assignments = await query
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.FaultReport!.FaultPriority!.Level)
            .ThenBy(x => x.FaultReport!.Deadline)
            .Select(DtoProjections.Assignment)
            .ToListAsync();

        await FillInterventionsAsync(assignments);

        return Ok(assignments);
    }

    /// <summary>Popis svih naloga, za upravitelja.</summary>
    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpGet]
    public async Task<ActionResult<List<WorkAssignmentDto>>> GetAll(
        [FromQuery] int? technicianId = null,
        [FromQuery] bool onlyActive = true)
    {
        var query = _context.WorkAssignments.AsNoTracking();

        if (technicianId.HasValue)
        {
            query = query.Where(x => x.TechnicianId == technicianId.Value);
        }

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        var assignments = await query
            .OrderByDescending(x => x.AssignedAt)
            .Select(DtoProjections.Assignment)
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkAssignmentDto>> GetById(int id)
    {
        var assignment = await _context.WorkAssignments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(DtoProjections.Assignment)
            .FirstOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        await FillInterventionsAsync([assignment]);

        return Ok(assignment);
    }

    /// <summary>
    /// Dodjeljuje prijavu izvršitelju. Prethodni aktivni nalog se zatvara,
    /// ne briše, čime ostaje povijest tko je kad radio na kvaru.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult<WorkAssignmentDto>> Create(SaveWorkAssignmentDto dto)
    {
        var report = await _context.FaultReports.FindAsync(dto.FaultReportId);

        if (report is null)
        {
            return BadRequest("Prijava kvara ne postoji.");
        }

        if (!FaultStatusRules.AcceptsWork(report.FaultStatusId))
        {
            return BadRequest("Prijava je zatvorena pa se više ne može dodjeljivati.");
        }

        // prijava se dodjeljuje tek nakon pregleda - bez prioriteta izvršitelj
        // ne zna koliko je hitno ni do kada treba riješiti
        if (report.FaultStatusId == FaultStatusIds.Zaprimljeno)
        {
            return BadRequest("Prijavu je potrebno prvo pregledati i odrediti vrstu, prioritet i rok.");
        }

        var technician = await _context.Employees.FindAsync(dto.TechnicianId);

        if (technician is null || !technician.IsTechnician)
        {
            return BadRequest("Odabrani djelatnik nije izvršitelj.");
        }

        if (!technician.IsActive)
        {
            return BadRequest("Izvršitelj je označen kao neaktivan.");
        }

        var current = await _context.WorkAssignments
            .FirstOrDefaultAsync(x => x.FaultReportId == dto.FaultReportId && x.IsActive);

        if (current is not null && current.TechnicianId == dto.TechnicianId)
        {
            return BadRequest("Prijava je već dodijeljena tom izvršitelju.");
        }

        // transakcija: stari nalog treba zatvoriti prije nego se ubaci novi,
        // inače jedinstveni indeks odbija drugi aktivni redak za istu prijavu
        await using var transaction = await _context.Database.BeginTransactionAsync();

        if (current is not null)
        {
            current.IsActive = false;
            current.UnassignedAt = DateTime.Now;
            current.ReassignReason = dto.ReassignReason?.Trim();
            await _context.SaveChangesAsync();
        }

        var assignment = new WorkAssignment
        {
            FaultReportId = dto.FaultReportId,
            TechnicianId = dto.TechnicianId,
            AssignedById = User.GetEmployeeId(),
            AssignedAt = DateTime.Now,
            Note = dto.Note?.Trim(),
            IsActive = true
        };

        _context.WorkAssignments.Add(assignment);

        // pregledana prijava dodjelom prelazi u Dodijeljeno;
        // ako je već u radu, status ostaje
        if (report.FaultStatusId == FaultStatusIds.Pregledano)
        {
            report.FaultStatusId = FaultStatusIds.Dodijeljeno;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var created = await _context.WorkAssignments
            .AsNoTracking()
            .Where(x => x.Id == assignment.Id)
            .Select(DtoProjections.Assignment)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, created);
    }

    /// <summary>
    /// Skida nalog s izvršitelja bez dodjele novom. Zapis ostaje u povijesti,
    /// a prijava se vraća u red čekanja.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:int}/unassign")]
    public async Task<IActionResult> Unassign(int id, [FromBody] string? reason = null)
    {
        var assignment = await _context.WorkAssignments
            .Include(x => x.FaultReport)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (assignment is null)
        {
            return NotFound();
        }

        if (!assignment.IsActive)
        {
            return BadRequest("Nalog je već zatvoren.");
        }

        // nalog se uvijek samo zatvara, nikad ne briše
        assignment.IsActive = false;
        assignment.UnassignedAt = DateTime.Now;
        assignment.ReassignReason = reason?.Trim();

        // bez aktivnog izvršitelja prijava se vraća u status Pregledano
        if (assignment.FaultReport is not null &&
            assignment.FaultReport.FaultStatusId is FaultStatusIds.Dodijeljeno or FaultStatusIds.URadu)
        {
            assignment.FaultReport.FaultStatusId = FaultStatusIds.Pregledano;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Dopuni naloge njihovim intervencijama.</summary>
    private async Task FillInterventionsAsync(List<WorkAssignmentDto> assignments)
    {
        if (assignments.Count == 0)
        {
            return;
        }

        var ids = assignments.Select(x => x.Id).ToList();

        var interventions = await _context.Interventions
            .AsNoTracking()
            .Where(x => ids.Contains(x.WorkAssignmentId))
            .OrderByDescending(x => x.StartedAt)
            .Select(DtoProjections.Intervention)
            .ToListAsync();

        foreach (var assignment in assignments)
        {
            assignment.Interventions = interventions
                .Where(i => i.WorkAssignmentId == assignment.Id)
                .ToList();
        }
    }
}

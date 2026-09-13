using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Intervencije na terenu.
///
/// Izvršitelj radi samo na nalozima koji su njemu dodijeljeni i samo dok je
/// nalog aktivan. Upravitelj i administrator smiju sve.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.FieldWork)]
[ApiController]
[Route("api/[controller]")]
public class InterventionsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public InterventionsController(KvaroviDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    //  POPISI
    // =====================================================================

    /// <summary>
    /// Popis intervencija s filterima, sortiranjem i stranicenjem.
    /// Sve se radi u bazi.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<InterventionDto>>> GetAll(
        [FromQuery] InterventionFilterDto filter)
    {
        return Ok(await QueryAsync(filter));
    }

    /// <summary>Intervencije prijavljenog izvršitelja.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResultDto<InterventionDto>>> GetMine(
        [FromQuery] InterventionFilterDto filter)
    {
        var employeeId = User.GetEmployeeId();

        if (employeeId is null)
        {
            return BadRequest("Vaš račun nije povezan s djelatnikom pa nemate intervencija.");
        }

        filter.TechnicianId = employeeId.Value;

        return Ok(await QueryAsync(filter));
    }

    private async Task<PagedResultDto<InterventionDto>> QueryAsync(InterventionFilterDto filter)
    {
        var query = _context.Interventions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.Note != null && x.Note.ToLower().Contains(term)) ||
                x.WorkAssignment!.FaultReport!.Title.ToLower().Contains(term));
        }

        if (filter.FaultReportId.HasValue)
            query = query.Where(x => x.WorkAssignment!.FaultReportId == filter.FaultReportId.Value);

        if (filter.InterventionStatusId.HasValue)
            query = query.Where(x => x.InterventionStatusId == filter.InterventionStatusId.Value);

        if (filter.TechnicianId.HasValue)
            query = query.Where(x => x.WorkAssignment!.TechnicianId == filter.TechnicianId.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(x => x.WorkAssignment!.FaultReport!.LocationId == filter.LocationId.Value);

        if (filter.StartedFrom.HasValue)
            query = query.Where(x => x.StartedAt >= filter.StartedFrom.Value.Date);

        if (filter.StartedTo.HasValue)
            query = query.Where(x => x.StartedAt < filter.StartedTo.Value.Date.AddDays(1));

        var totalCount = await query.CountAsync();

        query = (filter.SortBy ?? "startedAt").ToLowerInvariant() switch
        {
            "duration" => filter.SortDescending
                ? query.OrderByDescending(x => x.DurationMinutes)
                : query.OrderBy(x => x.DurationMinutes),
            "status" => filter.SortDescending
                ? query.OrderByDescending(x => x.InterventionStatus!.SortOrder)
                : query.OrderBy(x => x.InterventionStatus!.SortOrder),
            "technician" => filter.SortDescending
                ? query.OrderByDescending(x => x.WorkAssignment!.Technician!.LastName)
                : query.OrderBy(x => x.WorkAssignment!.Technician!.LastName),
            "faultreport" => filter.SortDescending
                ? query.OrderByDescending(x => x.WorkAssignment!.FaultReport!.Title)
                : query.OrderBy(x => x.WorkAssignment!.FaultReport!.Title),
            _ => filter.SortDescending
                ? query.OrderByDescending(x => x.StartedAt)
                : query.OrderBy(x => x.StartedAt)
        };

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(DtoProjections.Intervention)
            .ToListAsync();

        await FillDetailsAsync(items);

        return new PagedResultDto<InterventionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    [HttpGet("assignment/{workAssignmentId:int}")]
    public async Task<ActionResult<List<InterventionDto>>> GetForAssignment(int workAssignmentId)
    {
        var interventions = await _context.Interventions
            .AsNoTracking()
            .Where(x => x.WorkAssignmentId == workAssignmentId)
            .OrderByDescending(x => x.StartedAt)
            .Select(DtoProjections.Intervention)
            .ToListAsync();

        await FillDetailsAsync(interventions);

        return Ok(interventions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterventionDto>> GetById(int id)
    {
        var intervention = await _context.Interventions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(DtoProjections.Intervention)
            .FirstOrDefaultAsync();

        if (intervention is null)
        {
            return NotFound();
        }

        await FillDetailsAsync([intervention]);

        return Ok(intervention);
    }

    // =====================================================================
    //  TOK RADA
    // =====================================================================

    /// <summary>
    /// Pokreće intervenciju na aktivnom nalogu. Prijava time prelazi u status U radu.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<InterventionDto>> Start(StartInterventionDto dto)
    {
        var assignment = await _context.WorkAssignments
            .Include(x => x.FaultReport)
            .FirstOrDefaultAsync(x => x.Id == dto.WorkAssignmentId);

        if (assignment is null)
        {
            return BadRequest("Radni nalog ne postoji.");
        }

        if (!CanWork(assignment.TechnicianId))
        {
            return Forbid();
        }

        // izvršitelj smije raditi samo na svojem aktivnom nalogu
        if (!assignment.IsActive)
        {
            return BadRequest("Nalog je zatvoren. Intervenciju unosi izvršitelj kojemu je prijava trenutno dodijeljena.");
        }

        if (assignment.FaultReport is not null &&
            !FaultStatusRules.AcceptsWork(assignment.FaultReport.FaultStatusId))
        {
            return BadRequest("Prijava je zatvorena pa se intervencije više ne mogu unositi.");
        }

        // na nalogu ne smiju istovremeno teći dvije intervencije
        var hasOpen = await _context.Interventions
            .AnyAsync(x => x.WorkAssignmentId == dto.WorkAssignmentId &&
                           !x.InterventionStatus!.IsFinal);

        if (hasOpen)
        {
            return BadRequest("Na ovom nalogu već postoji nezaključena intervencija. Prvo je završite.");
        }

        var intervention = new Intervention
        {
            WorkAssignmentId = dto.WorkAssignmentId,
            InterventionStatusId = InterventionStatusIds.UTijeku,
            StartedAt = dto.StartedAt ?? DateTime.Now,
            Note = dto.Note?.Trim()
        };

        _context.Interventions.Add(intervention);

        // pokretanjem rada prijava prelazi iz Dodijeljeno u U radu
        if (assignment.FaultReport is not null &&
            assignment.FaultReport.FaultStatusId == FaultStatusIds.Dodijeljeno)
        {
            assignment.FaultReport.FaultStatusId = FaultStatusIds.URadu;
        }

        await _context.SaveChangesAsync();

        var created = await _context.Interventions
            .AsNoTracking()
            .Where(x => x.Id == intervention.Id)
            .Select(DtoProjections.Intervention)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = intervention.Id }, created);
    }

    /// <summary>Ispravak podataka intervencije koja još nije zaključena.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInterventionDto dto)
    {
        var intervention = await _context.Interventions
            .Include(x => x.WorkAssignment)
            .Include(x => x.InterventionStatus)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (intervention is null)
        {
            return NotFound();
        }

        if (!CanWork(intervention.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        if (intervention.InterventionStatus!.IsFinal &&
            !User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return BadRequest("Zaključena intervencija se više ne mijenja.");
        }

        intervention.StartedAt = dto.StartedAt;
        intervention.Note = dto.Note?.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Završava intervenciju.
    ///
    /// Uspješan završetak automatski podiže prijavu u status Riješeno.
    /// Neuspješan ostaje u povijesti i dopušta otvaranje nove intervencije
    /// na istom nalogu bez gubitka prethodne.
    /// </summary>
    [HttpPut("{id:int}/finish")]
    public async Task<IActionResult> Finish(int id, FinishInterventionDto dto)
    {
        var intervention = await _context.Interventions
            .Include(x => x.InterventionStatus)
            .Include(x => x.WorkAssignment)
                .ThenInclude(a => a!.FaultReport)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (intervention is null)
        {
            return NotFound();
        }

        if (!CanWork(intervention.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        if (intervention.InterventionStatus!.IsFinal)
        {
            return BadRequest("Intervencija je već zaključena.");
        }

        // završena intervencija mora imati početak, kraj i bilješku
        if (string.IsNullOrWhiteSpace(dto.Note))
        {
            return BadRequest("Bilješka o obavljenom radu je obavezna kod završetka intervencije.");
        }

        var finishedAt = dto.FinishedAt ?? DateTime.Now;

        if (finishedAt < intervention.StartedAt)
        {
            return BadRequest("Završetak ne može biti prije početka intervencije.");
        }

        intervention.FinishedAt = finishedAt;
        intervention.Note = dto.Note.Trim();

        // trajanje se računa iz vremena, osim ako ga izvršitelj ne zada ručno
        intervention.DurationMinutes = dto.DurationMinutes
            ?? (int)Math.Round((finishedAt - intervention.StartedAt).TotalMinutes);

        intervention.InterventionStatusId = dto.Successful
            ? InterventionStatusIds.Zavrsena
            : InterventionStatusIds.Neuspjesna;

        var report = intervention.WorkAssignment.FaultReport;

        if (dto.Successful && report is not null && report.FaultStatusId != FaultStatusIds.Zatvoreno)
        {
            // uspješan završetak sam podiže prijavu u Riješeno;
            // u Zatvoreno je nakon završne provjere prebacuje upravitelj
            report.FaultStatusId = FaultStatusIds.Rijeseno;
            report.ResolvedAt = finishedAt;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var intervention = await _context.Interventions
            .Include(x => x.WorkAssignment)
            .Include(x => x.InterventionStatus)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (intervention is null)
        {
            return NotFound();
        }

        if (!CanWork(intervention.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        // zaključena intervencija je dio povijesti rješavanja
        if (intervention.InterventionStatus!.IsFinal &&
            !User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return BadRequest("Zaključena intervencija se ne briše.");
        }

        _context.Interventions.Remove(intervention);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================================================
    //  POMOĆNE METODE
    // =====================================================================

    /// <summary>Dopuni intervencije materijalom i privitcima.</summary>
    private async Task FillDetailsAsync(List<InterventionDto> interventions)
    {
        if (interventions.Count == 0)
        {
            return;
        }

        var ids = interventions.Select(x => x.Id).ToList();

        var materials = await _context.InterventionMaterials
            .AsNoTracking()
            .Where(x => ids.Contains(x.InterventionId))
            .OrderBy(x => x.Material!.Name)
            .Select(DtoProjections.InterventionMaterial)
            .ToListAsync();

        var attachments = await _context.Attachments
            .AsNoTracking()
            .Where(x => x.InterventionId != null && ids.Contains(x.InterventionId.Value))
            .OrderBy(x => x.AttachmentPurposeId)
            .Select(DtoProjections.Attachment)
            .ToListAsync();

        foreach (var intervention in interventions)
        {
            intervention.Materials = materials
                .Where(m => m.InterventionId == intervention.Id)
                .ToList();

            intervention.TotalMaterialCost = intervention.Materials.Sum(m => m.TotalPrice);

            intervention.Attachments = attachments
                .Where(a => a.InterventionId == intervention.Id)
                .ToList();
        }
    }

    /// <summary>Izvršitelj radi samo na svojim nalozima, upravitelj i admin na svima.</summary>
    private bool CanWork(int technicianId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return true;
        }

        return technicianId == User.GetEmployeeId();
    }
}

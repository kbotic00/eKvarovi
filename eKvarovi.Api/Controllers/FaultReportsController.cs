using System.Linq.Expressions;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaultReportsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public FaultReportsController(KvaroviDbContext context)
    {
        _context = context;
    }

    /// <summary>Jedno mjesto za pretvorbu u DTO, da se projekcija ne ponavlja.</summary>
    private static readonly Expression<Func<FaultReport, FaultReportDto>> ToDto = x => new FaultReportDto
    {
        Id = x.Id,
        Title = x.Title,
        Description = x.Description,
        ReportedAt = x.ReportedAt,

        LocationId = x.LocationId,
        LocationName = x.Location!.Name,
        LocationCity = x.Location.City,

        ReporterId = x.ReporterId,
        ReporterName = x.Reporter!.FirstName + " " + x.Reporter.LastName,

        FaultTypeId = x.FaultTypeId,
        FaultTypeName = x.FaultType != null ? x.FaultType.Name : null,

        FaultPriorityId = x.FaultPriorityId,
        FaultPriorityName = x.FaultPriority != null ? x.FaultPriority.Name : null,
        PriorityLevel = x.FaultPriority != null ? x.FaultPriority.Level : 0,

        Deadline = x.Deadline,
        ReviewedAt = x.ReviewedAt,

        FaultStatusId = x.FaultStatusId,
        FaultStatusName = x.FaultStatus!.Name,
        IsClosed = x.FaultStatus.IsClosed,

        ResolvedAt = x.ResolvedAt,
        ClosedAt = x.ClosedAt,
        ClosingNote = x.ClosingNote,

        CurrentAssignmentId = x.Assignments
            .Where(a => a.IsActive)
            .Select(a => (int?)a.Id)
            .FirstOrDefault(),
        CurrentTechnicianId = x.Assignments
            .Where(a => a.IsActive)
            .Select(a => (int?)a.TechnicianId)
            .FirstOrDefault(),
        CurrentTechnicianName = x.Assignments
            .Where(a => a.IsActive)
            .Select(a => a.Technician!.FirstName + " " + a.Technician.LastName)
            .FirstOrDefault(),

        AssignmentCount = x.Assignments.Count,
        InterventionCount = x.Assignments.Sum(a => a.Interventions.Count),
        AttachmentCount = x.Attachments.Count
    };

    // =====================================================================
    //  POPISI
    // =====================================================================

    /// <summary>
    /// Popis svih prijava s filterima, sortiranjem i stranicenjem.
    /// Sve se radi u bazi, ne u pregledniku.
    /// Prijavitelj ovdje nema pristup - on koristi /mine.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.FieldWork)]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<FaultReportDto>>> GetAll(
        [FromQuery] FaultReportFilterDto filter)
    {
        return Ok(await QueryAsync(filter));
    }

    /// <summary>Prijave koje je unio prijavljeni djelatnik.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResultDto<FaultReportDto>>> GetMine(
        [FromQuery] FaultReportFilterDto filter)
    {
        // id djelatnika se čita isključivo iz tokena, nikad iz zahtjeva
        var employeeId = User.GetEmployeeId();

        if (employeeId is null)
        {
            return BadRequest("Vaš račun nije povezan s djelatnikom pa nema vlastitih prijava.");
        }

        filter.ReporterId = employeeId.Value;

        return Ok(await QueryAsync(filter));
    }

    private async Task<PagedResultDto<FaultReportDto>> QueryAsync(FaultReportFilterDto filter)
    {
        var query = _context.FaultReports.AsNoTracking();

        // ---------- filtriranje ----------
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Description.ToLower().Contains(term));
        }

        if (filter.LocationId.HasValue)
            query = query.Where(x => x.LocationId == filter.LocationId.Value);

        if (filter.FaultTypeId.HasValue)
            query = query.Where(x => x.FaultTypeId == filter.FaultTypeId.Value);

        if (filter.FaultPriorityId.HasValue)
            query = query.Where(x => x.FaultPriorityId == filter.FaultPriorityId.Value);

        if (filter.FaultStatusId.HasValue)
            query = query.Where(x => x.FaultStatusId == filter.FaultStatusId.Value);

        if (filter.ReporterId.HasValue)
            query = query.Where(x => x.ReporterId == filter.ReporterId.Value);

        if (filter.TechnicianId.HasValue)
            query = query.Where(x => x.Assignments
                .Any(a => a.IsActive && a.TechnicianId == filter.TechnicianId.Value));

        if (filter.ReportedFrom.HasValue)
            query = query.Where(x => x.ReportedAt >= filter.ReportedFrom.Value.Date);

        if (filter.ReportedTo.HasValue)
            query = query.Where(x => x.ReportedAt < filter.ReportedTo.Value.Date.AddDays(1));

        if (filter.OnlyOpen)
            query = query.Where(x => x.FaultStatusId != FaultStatusIds.Zatvoreno);

        if (filter.OnlyOverdue)
        {
            var today = DateTime.Now.Date;
            query = query.Where(x => x.FaultStatusId != FaultStatusIds.Zatvoreno &&
                                     x.Deadline != null &&
                                     x.Deadline < today);
        }

        if (filter.OnlyUnassigned)
            query = query.Where(x => x.FaultStatusId != FaultStatusIds.Zatvoreno &&
                                     !x.Assignments.Any(a => a.IsActive));

        // ---------- ukupan broj prije stranicenja ----------
        var totalCount = await query.CountAsync();

        // ---------- sortiranje ----------
        query = ApplySort(query, filter.SortBy, filter.SortDescending);

        // ---------- stranicenje ----------
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync();

        return new PagedResultDto<FaultReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Sortiranje po dopuštenim stupcima. Naziv stupca iz zahtjeva se ne
    /// ubacuje u upit nego se preslikava na izraz - inače bi bio otvoren
    /// put za slanje proizvoljnog izraza.
    /// </summary>
    private static IQueryable<FaultReport> ApplySort(
        IQueryable<FaultReport> query, string? sortBy, bool descending)
    {
        Expression<Func<FaultReport, object?>> key = (sortBy ?? "reportedAt").ToLowerInvariant() switch
        {
            "title" => x => x.Title,
            "location" => x => x.Location!.Name,
            "priority" => x => x.FaultPriority != null ? x.FaultPriority.Level : 0,
            "status" => x => x.FaultStatus!.SortOrder,
            "deadline" => x => x.Deadline,
            "type" => x => x.FaultType != null ? x.FaultType.Name : "",
            _ => x => x.ReportedAt
        };

        return descending
            ? query.OrderByDescending(key)
            : query.OrderBy(key);
    }

    // =====================================================================
    //  POJEDINAČNI ZAPIS
    // =====================================================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FaultReportDto>> GetById(int id)
    {
        var report = await _context.FaultReports
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (report is null)
        {
            return NotFound();
        }

        if (!CanRead(report.ReporterId))
        {
            return Forbid();
        }

        return Ok(report);
    }

    /// <summary>
    /// Prijava s cijelom poviješću naloga, intervencijama, materijalom i privitcima.
    ///
    /// Podaci se dohvaćaju odvojenim upitima i slažu u memoriji jer SQLite
    /// ne podržava SQL APPLY, a sortirana ugniježđena kolekcija unutar
    /// jedne projekcije upravo to traži.
    /// </summary>
    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<FaultReportDetailDto>> GetDetail(int id)
    {
        var report = await _context.FaultReports
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (report is null)
        {
            return NotFound();
        }

        if (!CanRead(report.ReporterId))
        {
            return Forbid();
        }

        var assignments = await _context.WorkAssignments
            .AsNoTracking()
            .Where(x => x.FaultReportId == id)
            .OrderByDescending(x => x.AssignedAt)
            .Select(DtoProjections.Assignment)
            .ToListAsync();

        var assignmentIds = assignments.Select(x => x.Id).ToList();

        var interventions = await _context.Interventions
            .AsNoTracking()
            .Where(x => assignmentIds.Contains(x.WorkAssignmentId))
            .OrderByDescending(x => x.StartedAt)
            .Select(DtoProjections.Intervention)
            .ToListAsync();

        await FillInterventionDetailsAsync(interventions);

        foreach (var assignment in assignments)
        {
            assignment.Interventions = interventions
                .Where(i => i.WorkAssignmentId == assignment.Id)
                .ToList();
        }

        var attachments = await _context.Attachments
            .AsNoTracking()
            .Where(x => x.FaultReportId == id)
            .OrderBy(x => x.AttachmentPurposeId)
            .ThenBy(x => x.UploadedAt)
            .Select(DtoProjections.Attachment)
            .ToListAsync();

        return Ok(new FaultReportDetailDto
        {
            Report = report,
            Assignments = assignments,
            Attachments = attachments,
            TotalMinutes = interventions.Sum(i => i.DurationMinutes ?? 0),
            TotalMaterialCost = interventions.Sum(i => i.TotalMaterialCost)
        });
    }

    /// <summary>Dopuni intervencije materijalom i privitcima.</summary>
    private async Task FillInterventionDetailsAsync(List<InterventionDto> interventions)
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

    // =====================================================================
    //  UNOS I IZMJENA
    // =====================================================================

    [HttpPost]
    public async Task<ActionResult<FaultReportDto>> Create(SaveFaultReportDto dto)
    {
        var reporterId = await ResolveReporterIdAsync(dto.ReporterId);

        if (reporterId is null)
        {
            return BadRequest("Prijavitelj nije određen. Vaš račun nije povezan s djelatnikom.");
        }

        var location = await _context.Locations.FindAsync(dto.LocationId);

        if (location is null)
        {
            return BadRequest("Odabrana lokacija ne postoji.");
        }

        // prijava mora pripadati aktivnoj lokaciji
        if (!location.IsActive)
        {
            return BadRequest("Lokacija je označena kao neaktivna pa se na nju ne mogu prijavljivati kvarovi.");
        }

        var locationError = await ValidateReporterLocationAsync(reporterId.Value, dto.LocationId);

        if (locationError is not null)
        {
            return BadRequest(locationError);
        }

        var report = new FaultReport
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            ReportedAt = DateTime.Now,
            LocationId = dto.LocationId,
            ReporterId = reporterId.Value,
            FaultStatusId = FaultStatusIds.Zaprimljeno
        };

        _context.FaultReports.Add(report);
        await _context.SaveChangesAsync();

        var created = await _context.FaultReports
            .AsNoTracking()
            .Where(x => x.Id == report.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = report.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveFaultReportDto dto)
    {
        var report = await _context.FaultReports.FindAsync(id);

        if (report is null)
        {
            return NotFound();
        }

        var isManagement = User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager);

        if (!isManagement)
        {
            // prijavitelj mijenja samo svoju prijavu i samo dok je nitko nije preuzeo
            if (report.ReporterId != User.GetEmployeeId())
            {
                return Forbid();
            }

            if (report.FaultStatusId != FaultStatusIds.Zaprimljeno)
            {
                return BadRequest("Prijava je već u obradi pa se više ne može mijenjati.");
            }
        }

        var location = await _context.Locations.FindAsync(dto.LocationId);

        if (location is null)
        {
            return BadRequest("Odabrana lokacija ne postoji.");
        }

        if (!location.IsActive && location.Id != report.LocationId)
        {
            return BadRequest("Lokacija je označena kao neaktivna.");
        }

        if (!isManagement)
        {
            var locationError = await ValidateReporterLocationAsync(report.ReporterId, dto.LocationId);

            if (locationError is not null)
            {
                return BadRequest(locationError);
            }
        }

        report.Title = dto.Title.Trim();
        report.Description = dto.Description.Trim();
        report.LocationId = dto.LocationId;

        // prijavitelja smije promijeniti samo upravitelj
        if (isManagement && dto.ReporterId > 0)
        {
            report.ReporterId = dto.ReporterId;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Pregled prijave: upravitelj određuje vrstu kvara, prioritet i rok.
    /// Prijava time prelazi iz Zaprimljeno u Pregledano.
    /// Podaci se mogu i naknadno mijenjati dok prijava nije zatvorena.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:int}/review")]
    public async Task<IActionResult> Review(int id, ReviewFaultReportDto dto)
    {
        var report = await _context.FaultReports.FindAsync(id);

        if (report is null)
        {
            return NotFound();
        }

        if (report.FaultStatusId == FaultStatusIds.Zatvoreno)
        {
            return BadRequest("Zatvorena prijava se više ne pregledava.");
        }

        if (!await _context.FaultTypes.AnyAsync(x => x.Id == dto.FaultTypeId))
        {
            return BadRequest("Odabrana vrsta kvara ne postoji.");
        }

        var priority = await _context.FaultPriorities.FindAsync(dto.FaultPriorityId);

        if (priority is null)
        {
            return BadRequest("Odabrani prioritet ne postoji.");
        }

        // kritična prijava mora imati rok - pravilo dolazi iz šifarnika
        if (priority.RequiresDeadline && dto.Deadline is null)
        {
            return BadRequest($"Prijava prioriteta \"{priority.Name}\" mora imati određen rok.");
        }

        if (dto.Deadline is not null && dto.Deadline.Value.Date < report.ReportedAt.Date)
        {
            return BadRequest("Rok ne može biti prije datuma prijave.");
        }

        report.FaultTypeId = dto.FaultTypeId;
        report.FaultPriorityId = dto.FaultPriorityId;
        report.Deadline = dto.Deadline?.Date;
        report.ReviewedAt = DateTime.Now;
        report.ReviewedById = User.GetEmployeeId();

        // prvi pregled pomiče prijavu iz Zaprimljeno u Pregledano
        if (report.FaultStatusId == FaultStatusIds.Zaprimljeno)
        {
            report.FaultStatusId = FaultStatusIds.Pregledano;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Izravna promjena statusa. Radi po tablici dopuštenih prijelaza.
    /// Zatvaranje traži uspješno završenu intervenciju i obrazloženje.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeFaultStatusDto dto)
    {
        var report = await _context.FaultReports.FindAsync(id);

        if (report is null)
        {
            return NotFound();
        }

        if (report.FaultStatusId == dto.FaultStatusId)
        {
            return NoContent();
        }

        var target = await _context.FaultStatuses.FindAsync(dto.FaultStatusId);

        if (target is null)
        {
            return BadRequest("Odabrani status ne postoji.");
        }

        var current = await _context.FaultStatuses.FindAsync(report.FaultStatusId);

        if (!FaultStatusRules.IsAllowed(report.FaultStatusId, dto.FaultStatusId))
        {
            return BadRequest(
                $"Prijelaz iz statusa \"{current?.Name}\" u \"{target.Name}\" nije dopušten.");
        }

        // pregled mora biti obavljen prije dodjele
        if (dto.FaultStatusId == FaultStatusIds.Pregledano && report.FaultPriorityId is null)
        {
            return BadRequest("Prije prelaska u status Pregledano odredite vrstu kvara i prioritet.");
        }

        if (dto.FaultStatusId == FaultStatusIds.Zatvoreno)
        {
            if (string.IsNullOrWhiteSpace(dto.ClosingNote))
            {
                return BadRequest("Kod zatvaranja prijave obrazloženje završne provjere je obavezno.");
            }

            // prijava se ne smije zatvoriti bez uspješno završene intervencije
            var hasSuccessful = await _context.Interventions
                .AnyAsync(i => i.WorkAssignment!.FaultReportId == id &&
                               i.InterventionStatus!.IsSuccessful);

            if (!hasSuccessful)
            {
                return BadRequest("Prijava se ne može zatvoriti dok ne postoji uspješno završena intervencija.");
            }

            report.ClosedAt = DateTime.Now;
            report.ClosingNote = dto.ClosingNote.Trim();
        }
        else
        {
            // ponovno otvaranje briše podatke o zatvaranju
            report.ClosedAt = null;
            report.ClosingNote = null;
        }

        report.FaultStatusId = dto.FaultStatusId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.FaultReports.FindAsync(id);

        if (report is null)
        {
            return NotFound();
        }

        // prijava s intervencijama ne briše se fizički
        var hasInterventions = await _context.Interventions
            .AnyAsync(x => x.WorkAssignment!.FaultReportId == id);

        if (hasInterventions)
        {
            return BadRequest("Prijava ima evidentirane intervencije pa se ne može obrisati.");
        }

        if (await _context.WorkAssignments.AnyAsync(x => x.FaultReportId == id))
        {
            return BadRequest("Prijava ima povijest radnih naloga pa se ne može obrisati.");
        }

        _context.FaultReports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================================================
    //  POMOĆNE METODE
    // =====================================================================

    /// <summary>
    /// Prijavitelj uvijek prijavljuje u svoje ime. Upravitelj smije unijeti
    /// prijavu umjesto drugog djelatnika, npr. kad je kvar javljen telefonom.
    /// </summary>
    private async Task<int?> ResolveReporterIdAsync(int requestedReporterId)
    {
        var myEmployeeId = User.GetEmployeeId();

        if (!User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return myEmployeeId;
        }

        if (requestedReporterId > 0 &&
            await _context.Employees.AnyAsync(x => x.Id == requestedReporterId))
        {
            return requestedReporterId;
        }

        return myEmployeeId;
    }

    /// <summary>
    /// Prijavitelj prijavljuje kvar samo na lokaciji kojoj pripada.
    /// Upravitelj nije vezan na lokaciju jer unosi prijave sa svih objekata.
    ///
    /// Djelatnik bez matične lokacije (npr. serviser koji pokriva sve objekte)
    /// smije prijaviti bilo gdje - inače ne bi mogao prijaviti ništa.
    /// </summary>
    private async Task<string?> ValidateReporterLocationAsync(int reporterId, int locationId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return null;
        }

        var reporterLocationId = await _context.Employees
            .Where(x => x.Id == reporterId)
            .Select(x => x.LocationId)
            .FirstOrDefaultAsync();

        if (reporterLocationId is null || reporterLocationId == locationId)
        {
            return null;
        }

        var own = await _context.Locations
            .Where(x => x.Id == reporterLocationId.Value)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        return $"Kvar možete prijaviti samo na svojoj lokaciji ({own}).";
    }

    /// <summary>Prijavitelj smije čitati samo vlastite prijave.</summary>
    private bool CanRead(int reporterId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Technician))
        {
            return true;
        }

        return reporterId == User.GetEmployeeId();
    }
}

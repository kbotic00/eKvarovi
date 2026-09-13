using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Api.Services;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Privitci uz prijave i intervencije: fotografije prije i nakon rada te PDF dokumenti.
/// Brisanje uklanja i zapis u bazi i datoteku s diska.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly KvaroviDbContext _context;
    private readonly FileStorageService _files;

    public AttachmentsController(KvaroviDbContext context, FileStorageService files)
    {
        _context = context;
        _files = files;
    }

    [HttpGet("report/{faultReportId:int}")]
    public async Task<ActionResult<List<AttachmentDto>>> GetForReport(int faultReportId)
    {
        var report = await _context.FaultReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == faultReportId);

        if (report is null)
        {
            return NotFound();
        }

        if (!CanReadReport(report.ReporterId))
        {
            return Forbid();
        }

        var items = await _context.Attachments
            .AsNoTracking()
            .Where(x => x.FaultReportId == faultReportId)
            .OrderBy(x => x.AttachmentPurposeId)
            .ThenBy(x => x.UploadedAt)
            .Select(DtoProjections.Attachment)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("intervention/{interventionId:int}")]
    public async Task<ActionResult<List<AttachmentDto>>> GetForIntervention(int interventionId)
    {
        var items = await _context.Attachments
            .AsNoTracking()
            .Where(x => x.InterventionId == interventionId)
            .OrderBy(x => x.AttachmentPurposeId)
            .ThenBy(x => x.UploadedAt)
            .Select(DtoProjections.Attachment)
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Prilaže datoteku uz prijavu kvara. Prijavitelj smije priložiti
    /// fotografiju samo na vlastitu prijavu.
    /// </summary>
    [HttpPost("report/{faultReportId:int}")]
    public async Task<ActionResult<AttachmentDto>> UploadForReport(
        int faultReportId,
        [FromForm] IFormFile file,
        [FromForm] int purposeId = AttachmentPurposeIds.FotografijaPrije)
    {
        var report = await _context.FaultReports.FindAsync(faultReportId);

        if (report is null)
        {
            return NotFound();
        }

        var isStaff = User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Technician);

        if (!isStaff && report.ReporterId != User.GetEmployeeId())
        {
            return Forbid();
        }

        if (report.FaultStatusId == FaultStatusIds.Zatvoreno)
        {
            return BadRequest("Prijava je zatvorena pa se privitci više ne mogu dodavati.");
        }

        return await SaveAsync(file, purposeId, "kvarovi", faultReportId, null);
    }

    /// <summary>
    /// Prilaže datoteku uz intervenciju - najčešće fotografiju nakon popravka.
    /// </summary>
    [HttpPost("intervention/{interventionId:int}")]
    public async Task<ActionResult<AttachmentDto>> UploadForIntervention(
        int interventionId,
        [FromForm] IFormFile file,
        [FromForm] int purposeId = AttachmentPurposeIds.FotografijaPoslije)
    {
        var intervention = await _context.Interventions
            .Include(x => x.WorkAssignment)
            .FirstOrDefaultAsync(x => x.Id == interventionId);

        if (intervention is null)
        {
            return NotFound();
        }

        if (!CanWork(intervention.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        return await SaveAsync(file, purposeId, "intervencije", null, interventionId);
    }

    /// <summary>
    /// Briše privitak - i zapis u bazi i fizičku datoteku.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _context.Attachments
            .Include(x => x.FaultReport)
            .Include(x => x.Intervention)
                .ThenInclude(i => i!.WorkAssignment)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (attachment is null)
        {
            return NotFound();
        }

        if (!CanModify(attachment))
        {
            return Forbid();
        }

        // prvo zapis, pa datoteka - ako brisanje s diska padne, baza je već
        // čista i ostaje samo datoteka bez zapisa, što ne ruši aplikaciju
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        _files.Delete(attachment.Url);

        return NoContent();
    }

    // =====================================================================
    //  POMOĆNE METODE
    // =====================================================================

    private async Task<ActionResult<AttachmentDto>> SaveAsync(
        IFormFile file, int purposeId, string subFolder, int? faultReportId, int? interventionId)
    {
        var purpose = await _context.AttachmentPurposes.FindAsync(purposeId);

        if (purpose is null)
        {
            return BadRequest("Odabrana namjena privitka ne postoji.");
        }

        var (saved, error) = await _files.SaveAsync(file, subFolder, purpose.AllowsDocuments);

        if (saved is null)
        {
            return BadRequest(error);
        }

        var attachment = new Attachment
        {
            AttachmentPurposeId = purposeId,
            FaultReportId = faultReportId,
            InterventionId = interventionId,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = saved.StoredFileName,
            Url = saved.Url,
            ContentType = saved.ContentType,
            SizeBytes = saved.SizeBytes,
            UploadedAt = DateTime.Now,
            UploadedById = User.GetEmployeeId()
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        var created = await _context.Attachments
            .AsNoTracking()
            .Where(x => x.Id == attachment.Id)
            .Select(DtoProjections.Attachment)
            .FirstAsync();

        return Ok(created);
    }

    private bool CanReadReport(int reporterId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Technician))
        {
            return true;
        }

        return reporterId == User.GetEmployeeId();
    }

    private bool CanWork(int technicianId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return true;
        }

        return technicianId == User.GetEmployeeId();
    }

    private bool CanModify(Attachment attachment)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return true;
        }

        var employeeId = User.GetEmployeeId();

        // izvršitelj briše privitke svojih intervencija
        if (attachment.Intervention?.WorkAssignment is not null)
        {
            return attachment.Intervention.WorkAssignment.TechnicianId == employeeId;
        }

        // prijavitelj briše privitke svoje prijave
        return attachment.FaultReport?.ReporterId == employeeId;
    }
}

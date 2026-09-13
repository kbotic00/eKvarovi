using System.Linq.Expressions;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;

namespace eKvarovi.Api.Data;

/// <summary>
/// Projekcije koje dijeli više kontrolera. Drže se na jednom mjestu da se
/// isti zapis svugdje pretvara u DTO na isti način.
///
/// Ugniježđene kolekcije namjerno nisu dio projekcija - SQLite ne podržava
/// SQL APPLY, pa se one dohvaćaju zasebnim upitima i slažu u memoriji.
/// </summary>
public static class DtoProjections
{
    public static readonly Expression<Func<WorkAssignment, WorkAssignmentDto>> Assignment =
        a => new WorkAssignmentDto
        {
            Id = a.Id,
            FaultReportId = a.FaultReportId,
            FaultReportTitle = a.FaultReport!.Title,
            LocationName = a.FaultReport.Location!.Name,
            LocationAddress = a.FaultReport.Location.Address + ", " + a.FaultReport.Location.City,
            FaultStatusName = a.FaultReport.FaultStatus!.Name,
            FaultPriorityName = a.FaultReport.FaultPriority != null
                ? a.FaultReport.FaultPriority.Name
                : null,
            PriorityLevel = a.FaultReport.FaultPriority != null
                ? a.FaultReport.FaultPriority.Level
                : 0,
            Deadline = a.FaultReport.Deadline,
            IsOverdue = a.FaultReport.FaultStatusId != FaultStatusIds.Zatvoreno &&
                        a.FaultReport.Deadline != null &&
                        a.FaultReport.Deadline < DateTime.Now.Date,
            TechnicianId = a.TechnicianId,
            TechnicianName = a.Technician!.FirstName + " " + a.Technician.LastName,
            AssignedByName = a.AssignedBy != null
                ? a.AssignedBy.FirstName + " " + a.AssignedBy.LastName
                : null,
            AssignedAt = a.AssignedAt,
            UnassignedAt = a.UnassignedAt,
            Note = a.Note,
            ReassignReason = a.ReassignReason,
            IsActive = a.IsActive
        };

    public static readonly Expression<Func<Intervention, InterventionDto>> Intervention =
        i => new InterventionDto
        {
            Id = i.Id,
            WorkAssignmentId = i.WorkAssignmentId,
            InterventionStatusId = i.InterventionStatusId,
            InterventionStatusName = i.InterventionStatus!.Name,
            IsFinal = i.InterventionStatus.IsFinal,
            IsSuccessful = i.InterventionStatus.IsSuccessful,
            StartedAt = i.StartedAt,
            FinishedAt = i.FinishedAt,
            DurationMinutes = i.DurationMinutes,
            Note = i.Note,
            FaultReportId = i.WorkAssignment!.FaultReportId,
            FaultReportTitle = i.WorkAssignment.FaultReport!.Title,
            LocationName = i.WorkAssignment.FaultReport.Location!.Name,
            TechnicianId = i.WorkAssignment.TechnicianId,
            TechnicianName = i.WorkAssignment.Technician!.FirstName + " " +
                             i.WorkAssignment.Technician.LastName
        };

    public static readonly Expression<Func<InterventionMaterial, InterventionMaterialDto>> InterventionMaterial =
        m => new InterventionMaterialDto
        {
            Id = m.Id,
            InterventionId = m.InterventionId,
            MaterialId = m.MaterialId,
            MaterialName = m.Material!.Name,
            UnitAbbreviation = m.Material.MaterialUnit!.Abbreviation,
            Quantity = m.Quantity,
            UnitPrice = m.UnitPrice
        };

    public static readonly Expression<Func<Attachment, AttachmentDto>> Attachment =
        a => new AttachmentDto
        {
            Id = a.Id,
            AttachmentPurposeId = a.AttachmentPurposeId,
            AttachmentPurposeName = a.AttachmentPurpose!.Name,
            FaultReportId = a.FaultReportId,
            InterventionId = a.InterventionId,
            OriginalFileName = a.OriginalFileName,
            Url = a.Url,
            ContentType = a.ContentType,
            SizeBytes = a.SizeBytes,
            IsImage = a.ContentType.StartsWith("image/"),
            UploadedAt = a.UploadedAt,
            UploadedByName = a.UploadedBy != null
                ? a.UploadedBy.FirstName + " " + a.UploadedBy.LastName
                : null
        };
}

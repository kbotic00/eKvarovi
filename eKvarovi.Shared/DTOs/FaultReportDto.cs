namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Redak u popisu prijava. Sadrži i ime trenutnog izvršitelja i naziv
/// statusa da popis ne mora raditi dodatne pozive prema API-ju.
///
/// Vrsta kvara, prioritet i rok su neobavezni jer ih upravitelj postavlja
/// tek kod pregleda prijave.
/// </summary>
public class FaultReportDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime ReportedAt { get; set; }

    // ---------- lokacija i prijavitelj ----------

    public int LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string LocationCity { get; set; } = string.Empty;

    public int ReporterId { get; set; }

    public string ReporterName { get; set; } = string.Empty;

    // ---------- klasifikacija ----------

    public int? FaultTypeId { get; set; }

    public string? FaultTypeName { get; set; }

    public int? FaultPriorityId { get; set; }

    public string? FaultPriorityName { get; set; }

    public int PriorityLevel { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public bool IsReviewed => ReviewedAt.HasValue;

    // ---------- status ----------

    public int FaultStatusId { get; set; }

    public string FaultStatusName { get; set; } = string.Empty;

    public bool IsClosed { get; set; }

    public bool IsOpen => !IsClosed;

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? ClosingNote { get; set; }

    /// <summary>Rok je probijen samo dok je prijava još otvorena.</summary>
    public bool IsOverdue => !IsClosed && Deadline.HasValue && Deadline.Value.Date < DateTime.Now.Date;

    // ---------- trenutni nalog ----------

    public int? CurrentAssignmentId { get; set; }

    public int? CurrentTechnicianId { get; set; }

    public string? CurrentTechnicianName { get; set; }

    public bool IsAssigned => CurrentAssignmentId.HasValue;

    // ---------- brojači ----------

    public int AssignmentCount { get; set; }

    public int InterventionCount { get; set; }

    public int AttachmentCount { get; set; }
}

namespace eKvarovi.Shared.DTOs;

public class InterventionDto
{
    public int Id { get; set; }

    public int WorkAssignmentId { get; set; }

    public int InterventionStatusId { get; set; }

    public string InterventionStatusName { get; set; } = string.Empty;

    /// <summary>Intervencija je zaključena - uspješno ili neuspješno.</summary>
    public bool IsFinal { get; set; }

    /// <summary>Samo uspješan završetak podiže prijavu u status Riješeno.</summary>
    public bool IsSuccessful { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public int? DurationMinutes { get; set; }

    public string DurationText => DurationMinutes is null or <= 0
        ? "-"
        : $"{DurationMinutes.Value / 60} h {DurationMinutes.Value % 60} min";

    public string? Note { get; set; }

    // kontekst - da se intervencija može prikazati i izvan detalja prijave
    public int FaultReportId { get; set; }

    public string FaultReportTitle { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public int TechnicianId { get; set; }

    public string TechnicianName { get; set; } = string.Empty;

    public List<InterventionMaterialDto> Materials { get; set; } = new();

    public List<AttachmentDto> Attachments { get; set; } = new();

    public double TotalMaterialCost { get; set; }
}

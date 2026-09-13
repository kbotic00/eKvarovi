namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Detaljni prikaz prijave: osnovni podaci, cijela povijest radnih naloga
/// s intervencijama i materijalom, te privitci.
/// </summary>
public class FaultReportDetailDto
{
    public FaultReportDto Report { get; set; } = new();

    /// <summary>Sortirano od najnovijeg naloga prema najstarijem.</summary>
    public List<WorkAssignmentDto> Assignments { get; set; } = new();

    public List<AttachmentDto> Attachments { get; set; } = new();

    public int TotalMinutes { get; set; }

    public double TotalMaterialCost { get; set; }

    /// <summary>Sati i minute u obliku "3 h 20 min".</summary>
    public string TotalDurationText =>
        TotalMinutes <= 0 ? "-" : $"{TotalMinutes / 60} h {TotalMinutes % 60} min";
}

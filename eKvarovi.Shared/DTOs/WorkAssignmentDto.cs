namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Radni nalog. Neaktivni nalozi imaju popunjen <see cref="UnassignedAt"/>
/// i predstavljaju povijest - nikad se ne brišu.
/// </summary>
public class WorkAssignmentDto
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }

    public string FaultReportTitle { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public string FaultStatusName { get; set; } = string.Empty;

    public string? FaultPriorityName { get; set; }

    public int PriorityLevel { get; set; }

    public DateTime? Deadline { get; set; }

    public bool IsOverdue { get; set; }

    public int TechnicianId { get; set; }

    public string TechnicianName { get; set; } = string.Empty;

    public string? AssignedByName { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? UnassignedAt { get; set; }

    public string? Note { get; set; }

    public string? ReassignReason { get; set; }

    public bool IsActive { get; set; }

    public List<InterventionDto> Interventions { get; set; } = new();
}

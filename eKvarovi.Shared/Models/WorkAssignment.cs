namespace eKvarovi.Shared.Models;

/// <summary>
/// Radni nalog - dodjela prijave izvršitelju.
///
/// Jedna prijava kroz vrijeme može imati više naloga, ali najviše jedan
/// aktivan. Ponovna dodjela ne briše prethodni nalog: on dobiva
/// <see cref="UnassignedAt"/> i <see cref="IsActive"/> = false, čime nastaje povijest.
/// </summary>
public class WorkAssignment
{
    public int Id { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.Now;

    /// <summary>Popunjava se kad posao preuzme drugi izvršitelj.</summary>
    public DateTime? UnassignedAt { get; set; }

    /// <summary>Uputa upravitelja izvršitelju.</summary>
    public string? Note { get; set; }

    /// <summary>Razlog preraspodjele, upisuje se kod zatvaranja naloga.</summary>
    public string? ReassignReason { get; set; }

    /// <summary>Po prijavi najviše jedan nalog smije imati true.</summary>
    public bool IsActive { get; set; } = true;

    public int FaultReportId { get; set; }

    public FaultReport? FaultReport { get; set; }

    public int TechnicianId { get; set; }

    public Employee? Technician { get; set; }

    /// <summary>Tko je dodijelio nalog.</summary>
    public int? AssignedById { get; set; }

    public Employee? AssignedBy { get; set; }

    /// <summary>Nalog može imati više intervencija - neuspjeli pokušaj ne zatvara nalog.</summary>
    public List<Intervention> Interventions { get; set; } = new();
}

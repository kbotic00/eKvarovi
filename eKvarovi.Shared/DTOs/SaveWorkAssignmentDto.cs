using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Dodjela prijave izvršitelju. Poslužitelj pritom sam zatvara prethodni
/// aktivni nalog i prebacuje prijavu u status Dodijeljeno.
/// </summary>
public class SaveWorkAssignmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite prijavu kvara.")]
    public int FaultReportId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite izvršitelja.")]
    public int TechnicianId { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    /// <summary>Razlog preraspodjele, upisuje se na prethodni nalog.</summary>
    [MaxLength(500)]
    public string? ReassignReason { get; set; }
}

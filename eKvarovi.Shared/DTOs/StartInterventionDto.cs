using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Pokretanje intervencije na aktivnom nalogu. Prijava time prelazi
/// u status U radu.
/// </summary>
public class StartInterventionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite radni nalog.")]
    public int WorkAssignmentId { get; set; }

    /// <summary>Ako se ne pošalje, uzima se trenutno vrijeme.</summary>
    public DateTime? StartedAt { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }
}

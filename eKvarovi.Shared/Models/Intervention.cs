namespace eKvarovi.Shared.Models;

/// <summary>
/// Jedan izlazak izvršitelja na teren u sklopu radnog naloga.
///
/// Intervencija ne mora uspjeti. Neuspješna ostaje zabilježena, a na istom
/// nalogu se otvara nova - zato ih po nalogu može biti više.
/// </summary>
public class Intervention
{
    public int Id { get; set; }

    public int WorkAssignmentId { get; set; }

    public WorkAssignment? WorkAssignment { get; set; }

    public int InterventionStatusId { get; set; } = InterventionStatusIds.Planirana;

    public InterventionStatus? InterventionStatus { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.Now;

    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Trajanje u minutama. Sprema se pri završetku i računa iz početka i
    /// završetka, osim ako ga izvršitelj ručno ne ispravi.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>Bilješka o obavljenom radu. Obavezna kod završetka intervencije.</summary>
    public string? Note { get; set; }

    public List<InterventionMaterial> Materials { get; set; } = new();

    public List<Attachment> Attachments { get; set; } = new();
}

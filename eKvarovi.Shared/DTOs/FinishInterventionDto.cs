using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Završetak intervencije.
///
/// Uspješan završetak automatski podiže prijavu u status Riješeno.
/// Neuspješan ostaje u povijesti i dopušta otvaranje nove intervencije
/// na istom nalogu.
/// </summary>
public class FinishInterventionDto
{
    public bool Successful { get; set; }

    /// <summary>Ako se ne pošalje, uzima se trenutno vrijeme.</summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>Bilješka o radu je obavezna kod završetka.</summary>
    [Required(ErrorMessage = "Bilješka o obavljenom radu je obavezna.")]
    [MaxLength(2000)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Ručno zadano trajanje. Ako se izostavi, računa se iz početka i završetka.
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Trajanje ne smije biti negativno.")]
    public int? DurationMinutes { get; set; }
}

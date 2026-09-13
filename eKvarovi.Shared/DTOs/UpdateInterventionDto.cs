using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>Ispravak podataka intervencije koja još nije zaključena.</summary>
public class UpdateInterventionDto
{
    public DateTime StartedAt { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }
}

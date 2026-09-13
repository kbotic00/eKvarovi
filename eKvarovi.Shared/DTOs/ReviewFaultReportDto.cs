using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Pregled prijave od strane upravitelja: određivanje vrste kvara,
/// prioriteta i roka. Prijava pritom prelazi u status Pregledano.
/// </summary>
public class ReviewFaultReportDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite vrstu kvara.")]
    public int FaultTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite prioritet.")]
    public int FaultPriorityId { get; set; }

    /// <summary>Obavezan kod kritičnog prioriteta.</summary>
    public DateTime? Deadline { get; set; }
}

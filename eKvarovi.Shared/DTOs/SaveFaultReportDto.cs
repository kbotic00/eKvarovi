using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Unos i uređivanje prijave.
///
/// Vrsta kvara, prioritet, rok i status namjerno nisu ovdje - njih postavlja
/// upravitelj kroz zasebne akcije, da prijavitelj ne može sam sebi
/// podići prioritet ili zatvoriti kvar.
/// </summary>
public class SaveFaultReportDto
{
    [Required(ErrorMessage = "Naslov je obavezan.")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis kvara je obavezan.")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite lokaciju.")]
    public int LocationId { get; set; }

    /// <summary>
    /// Prijavitelj. Poslužitelj ga uzima iz tokena kad prijavu unosi sam
    /// prijavitelj; upravitelj ga smije zadati ručno.
    /// </summary>
    public int ReporterId { get; set; }
}

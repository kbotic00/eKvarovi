using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

public class SaveInterventionMaterialDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite intervenciju.")]
    public int InterventionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite materijal.")]
    public int MaterialId { get; set; }

    [Range(0.001, 100000, ErrorMessage = "Količina mora biti veća od nule.")]
    public double Quantity { get; set; } = 1;

    /// <summary>Ako se ne pošalje, uzima se zadana cijena iz šifarnika.</summary>
    public double? UnitPrice { get; set; }
}

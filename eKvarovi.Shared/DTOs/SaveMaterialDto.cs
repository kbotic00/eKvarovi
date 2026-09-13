using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

public class SaveMaterialDto
{
    [Required(ErrorMessage = "Naziv materijala je obavezan.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite mjernu jedinicu.")]
    public int MaterialUnitId { get; set; }

    [Range(0, 1000000, ErrorMessage = "Cijena ne smije biti negativna.")]
    public double DefaultUnitPrice { get; set; }

    public bool IsActive { get; set; } = true;
}

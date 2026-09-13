using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

public class SaveLocationDto
{
    [Required(ErrorMessage = "Naziv lokacije je obavezan.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna.")]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite vrstu lokacije.")]
    public int LocationTypeId { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;
}

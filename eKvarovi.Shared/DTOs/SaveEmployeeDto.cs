using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.DTOs;

public class SaveEmployeeDto
{
    [Required(ErrorMessage = "Ime je obavezno.")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Email nije u ispravnom obliku.")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    public bool IsTechnician { get; set; }

    public bool IsActive { get; set; } = true;

    public int? LocationId { get; set; }
}

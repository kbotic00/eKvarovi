namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Unos i izmjena korisničkog računa.
/// Kod izmjene se lozinka mijenja samo ako je <see cref="Password"/> popunjena.
/// </summary>
public class SaveUserAdminDto
{
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? EmployeeId { get; set; }

    public List<int> RoleIds { get; set; } = new();
}

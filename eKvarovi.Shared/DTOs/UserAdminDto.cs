namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Prikaz korisničkog računa na administratorskom ekranu.
/// Lozinka se nikad ne vraća prema klijentu.
/// </summary>
public class UserAdminDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public List<string> Roles { get; set; } = new();
}

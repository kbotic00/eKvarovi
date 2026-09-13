namespace eKvarovi.Shared.Models;

/// <summary>
/// Korisnički račun za prijavu u aplikaciju.
/// Račun je opcionalno vezan na djelatnika - preko te veze znamo
/// koje su prijave "moje" (kao prijavitelj) i koje dodjele su "moje" (kao serviser).
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public List<AppUserRole> UserRoles { get; set; } = new();
}

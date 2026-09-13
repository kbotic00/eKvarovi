namespace eKvarovi.Shared.Models;

/// <summary>
/// Spojna tablica korisnik - uloga (jedan korisnik može imati više uloga).
/// </summary>
public class AppUserRole
{
    public int AppUserId { get; set; }

    public AppUser? User { get; set; }

    public int AppRoleId { get; set; }

    public AppRole? Role { get; set; }
}

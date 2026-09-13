namespace eKvarovi.Shared.Models;

/// <summary>
/// Uloga u sustavu. Fiksne uloge: Admin, Manager, Technician, Reporter.
/// </summary>
public class AppRole
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<AppUserRole> UserRoles { get; set; } = new();
}

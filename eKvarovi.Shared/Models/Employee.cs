namespace eKvarovi.Shared.Models;

/// <summary>
/// Djelatnik županije. Isti model služi i prijavitelju i izvršitelju -
/// razliku radi zastavica <see cref="IsTechnician"/> i uloga dodijeljena
/// pripadajućem korisničkom računu.
/// </summary>
public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? JobTitle { get; set; }

    /// <summary>Ako je true, djelatniku se mogu dodjeljivati radni nalozi.</summary>
    public bool IsTechnician { get; set; }

    public bool IsActive { get; set; } = true;

    public int? LocationId { get; set; }

    public Location? Location { get; set; }

    public List<FaultReport> ReportedFaults { get; set; } = new();

    public List<WorkAssignment> Assignments { get; set; } = new();
}

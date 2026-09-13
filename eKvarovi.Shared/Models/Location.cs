namespace eKvarovi.Shared.Models;

/// <summary>
/// Lokacija u vlasništvu županije. Neaktivna lokacija ostaje u bazi
/// zbog povijesti, ali se na nju ne smiju unositi nove prijave.
/// </summary>
public class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;

    public int LocationTypeId { get; set; }

    public LocationType? LocationType { get; set; }

    public List<Employee> Employees { get; set; } = new();

    public List<FaultReport> FaultReports { get; set; } = new();
}

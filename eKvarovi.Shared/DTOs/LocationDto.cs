namespace eKvarovi.Shared.DTOs;

public class LocationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public int LocationTypeId { get; set; }

    public string LocationTypeName { get; set; } = string.Empty;

    /// <summary>Koliko je kvarova na ovoj lokaciji trenutno otvoreno.</summary>
    public int OpenFaultCount { get; set; }

    public int EmployeeCount { get; set; }
}

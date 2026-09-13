namespace eKvarovi.Shared.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? JobTitle { get; set; }

    public bool IsTechnician { get; set; }

    public bool IsActive { get; set; }

    public int? LocationId { get; set; }

    public string? LocationName { get; set; }

    /// <summary>Broj dodjela koje su serviseru trenutno aktivne.</summary>
    public int ActiveAssignmentCount { get; set; }
}

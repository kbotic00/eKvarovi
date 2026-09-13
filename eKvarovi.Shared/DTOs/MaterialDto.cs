namespace eKvarovi.Shared.DTOs;

/// <summary>Stavka šifarnika materijala.</summary>
public class MaterialDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public int MaterialUnitId { get; set; }

    public string UnitName { get; set; } = string.Empty;

    public string UnitAbbreviation { get; set; } = string.Empty;

    public double DefaultUnitPrice { get; set; }

    public bool IsActive { get; set; }

    /// <summary>Na koliko je intervencija materijal utrošen.</summary>
    public int UsageCount { get; set; }
}

namespace eKvarovi.Shared.DTOs;

/// <summary>Materijal utrošen na jednoj intervenciji.</summary>
public class InterventionMaterialDto
{
    public int Id { get; set; }

    public int InterventionId { get; set; }

    public int MaterialId { get; set; }

    public string MaterialName { get; set; } = string.Empty;

    public string UnitAbbreviation { get; set; } = string.Empty;

    public double Quantity { get; set; }

    public double UnitPrice { get; set; }

    public double TotalPrice => Quantity * UnitPrice;
}

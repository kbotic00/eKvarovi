namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Materijal za odabir na intervenciji. Donosi mjernu jedinicu i zadanu
/// cijenu da se obrazac popuni sam nakon odabira.
/// </summary>
public class MaterialLookupDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string UnitAbbreviation { get; set; } = string.Empty;

    public double DefaultUnitPrice { get; set; }
}

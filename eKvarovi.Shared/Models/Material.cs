namespace eKvarovi.Shared.Models;

/// <summary>
/// Šifarnik materijala. Isti materijal koristi se u više intervencija,
/// pa je zaseban entitet, a ne slobodan tekst upisan po intervenciji.
/// </summary>
public class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Interna šifra artikla, ako je ustanova vodi.</summary>
    public string? Code { get; set; }

    public int MaterialUnitId { get; set; }

    public MaterialUnit? MaterialUnit { get; set; }

    /// <summary>Zadana cijena. Na intervenciji se može ispraviti stvarnom cijenom.</summary>
    public double DefaultUnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public List<InterventionMaterial> Interventions { get; set; } = new();
}

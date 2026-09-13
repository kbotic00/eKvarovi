namespace eKvarovi.Shared.Models;

/// <summary>
/// Spojna tablica intervencija - materijal, s količinom.
///
/// Veza je M:N jer jedna intervencija troši više materijala, a isti materijal
/// se troši na više intervencija. Količina i cijena pripadaju vezi, a ne
/// ni materijalu ni intervenciji zasebno.
/// </summary>
public class InterventionMaterial
{
    public int Id { get; set; }

    public int InterventionId { get; set; }

    public Intervention? Intervention { get; set; }

    public int MaterialId { get; set; }

    public Material? Material { get; set; }

    /// <summary>Mora biti veća od nule.</summary>
    public double Quantity { get; set; }

    /// <summary>
    /// Cijena u trenutku utroška. Prepisuje se iz šifarnika, ali se pamti
    /// zasebno da kasnija promjena cjenika ne mijenja stare naloge.
    /// </summary>
    public double UnitPrice { get; set; }
}

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Prioritet za padajući izbornik. Nosi i podatak traži li rok, pa
/// sučelje može odmah označiti polje obaveznim bez dodatnog poziva.
/// </summary>
public class PriorityLookupDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }

    public bool RequiresDeadline { get; set; }
}

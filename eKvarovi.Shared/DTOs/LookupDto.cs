namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Univerzalni par id-naziv za punjenje padajućih izbornika.
/// </summary>
public class LookupDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Par naziv-broj za grupirane statistike na nadzornoj ploči.
/// </summary>
public class CountByNameDto
{
    public string Name { get; set; } = string.Empty;

    public int Count { get; set; }
}

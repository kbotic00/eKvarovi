namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Izravna promjena statusa prijave. Smije je raditi samo upravitelj,
/// i to po tablici dopuštenih prijelaza.
/// </summary>
public class ChangeFaultStatusDto
{
    public int FaultStatusId { get; set; }

    /// <summary>Obavezno kod zatvaranja prijave.</summary>
    public string? ClosingNote { get; set; }
}

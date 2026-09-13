namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Svi šifarnici u jednom odgovoru. Obrasci tako jednim pozivom napune
/// sve padajuće izbornike umjesto da rade sedam zasebnih zahtjeva.
/// </summary>
public class AllLookupsDto
{
    public List<LookupDto> LocationTypes { get; set; } = new();

    public List<LookupDto> FaultTypes { get; set; } = new();

    public List<PriorityLookupDto> FaultPriorities { get; set; } = new();

    public List<LookupDto> FaultStatuses { get; set; } = new();

    public List<LookupDto> InterventionStatuses { get; set; } = new();

    public List<LookupDto> MaterialUnits { get; set; } = new();

    public List<AttachmentPurposeLookupDto> AttachmentPurposes { get; set; } = new();

    public List<LookupDto> Locations { get; set; } = new();

    public List<LookupDto> Technicians { get; set; } = new();

    public List<MaterialLookupDto> Materials { get; set; } = new();
}

namespace eKvarovi.Shared.DTOs;

public class AttachmentPurposeLookupDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Namjena "Dokument" prima PDF, fotografije samo slike.</summary>
    public bool AllowsDocuments { get; set; }
}

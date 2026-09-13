namespace eKvarovi.Shared.DTOs;

public class AttachmentDto
{
    public int Id { get; set; }

    public int AttachmentPurposeId { get; set; }

    public string AttachmentPurposeName { get; set; } = string.Empty;

    public int? FaultReportId { get; set; }

    public int? InterventionId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Relativna putanja, npr. /uploads/kvarovi/abc.jpg</summary>
    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public bool IsImage { get; set; }

    public DateTime UploadedAt { get; set; }

    public string? UploadedByName { get; set; }

    /// <summary>Veličina u čitljivom obliku, npr. "1,4 MB".</summary>
    public string SizeText => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} B",
        < 1024 * 1024 => $"{SizeBytes / 1024.0:0.#} KB",
        _ => $"{SizeBytes / (1024.0 * 1024.0):0.#} MB"
    };
}

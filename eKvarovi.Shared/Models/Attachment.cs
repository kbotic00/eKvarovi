namespace eKvarovi.Shared.Models;

/// <summary>
/// Privitak uz prijavu ili uz intervenciju.
///
/// Fizičko ime datoteke (<see cref="StoredFileName"/>) generira poslužitelj,
/// a izvorno ime korisnika čuva se samo za prikaz i preuzimanje. Time se
/// sprječava da poslano ime utječe na putanju pisanja na disku.
/// </summary>
public class Attachment
{
    public int Id { get; set; }

    public int AttachmentPurposeId { get; set; }

    public AttachmentPurpose? AttachmentPurpose { get; set; }

    // Privitak pripada ili prijavi ili intervenciji - jedan od dva je popunjen.
    public int? FaultReportId { get; set; }

    public FaultReport? FaultReport { get; set; }

    public int? InterventionId { get; set; }

    public Intervention? Intervention { get; set; }

    /// <summary>Ime koje je korisnik vidio na svom računalu.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Nasumično ime pod kojim je datoteka spremljena na disk.</summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>Relativna putanja za prikaz, npr. /uploads/kvarovi/abc.jpg</summary>
    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    public int? UploadedById { get; set; }

    public Employee? UploadedBy { get; set; }

    /// <summary>Je li privitak slika - određuje prikazuje li se u galeriji ili kao poveznica.</summary>
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}

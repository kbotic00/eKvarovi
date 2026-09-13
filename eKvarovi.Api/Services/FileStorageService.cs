namespace eKvarovi.Api.Services;

/// <summary>
/// Sprema prenesene datoteke u wwwroot i briše ih s diska.
/// Metapodatke o datoteci vodi <c>AttachmentsController</c> u bazi.
/// </summary>
public class FileStorageService
{
    /// <summary>Dopuštene slike - fotografije prije i nakon rada.</summary>
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>Dopušteni dokumenti.</summary>
    private static readonly string[] DocumentExtensions = [".pdf"];

    private const long MaxImageBytes = 5 * 1024 * 1024;   // 5 MB
    private const long MaxDocumentBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public record SavedFile(string Url, string StoredFileName, string ContentType, long SizeBytes);

    /// <summary>
    /// Provjeri i spremi datoteku. Vraća podatke o spremljenoj datoteci
    /// ili poruku greške ako nije prihvatljiva.
    /// </summary>
    public async Task<(SavedFile? File, string? Error)> SaveAsync(
        IFormFile? file, string subFolder, bool allowDocuments)
    {
        if (file is null || file.Length == 0)
        {
            return (null, "Datoteka nije odabrana.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowed = allowDocuments
            ? ImageExtensions.Concat(DocumentExtensions).ToArray()
            : ImageExtensions;

        if (!allowed.Contains(extension))
        {
            return (null, allowDocuments
                ? "Dozvoljene su slike (jpg, jpeg, png, webp) i PDF dokumenti."
                : "Za ovu namjenu dozvoljene su samo slike: jpg, jpeg, png, webp.");
        }

        var isDocument = DocumentExtensions.Contains(extension);
        var maxBytes = isDocument ? MaxDocumentBytes : MaxImageBytes;

        if (file.Length > maxBytes)
        {
            return (null, $"Datoteka je veća od {maxBytes / (1024 * 1024)} MB.");
        }

        var targetFolder = Path.Combine(GetWebRoot(), "uploads", subFolder);
        Directory.CreateDirectory(targetFolder);

        // ime iz zahtjeva se nikad ne koristi za putanju na disku -
        // inače bi netko poslao "../../appsettings.json" i pisao izvan mape
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetFolder, storedName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var contentType = ResolveContentType(extension);

        return (new SavedFile(
            Url: $"/uploads/{subFolder}/{storedName}",
            StoredFileName: storedName,
            ContentType: contentType,
            SizeBytes: file.Length), null);
    }

    /// <summary>Briše fizičku datoteku. Poziva se uz brisanje zapisa u bazi.</summary>
    public void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/"))
        {
            return;
        }

        var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(GetWebRoot(), relative);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string GetWebRoot()
    {
        var webRoot = _environment.WebRootPath;

        return string.IsNullOrWhiteSpace(webRoot)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : webRoot;
    }

    /// <summary>Tip se određuje iz nastavka, ne iz zaglavlja koje šalje klijent.</summary>
    private static string ResolveContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}

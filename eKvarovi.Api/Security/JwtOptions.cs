namespace eKvarovi.Api.Security;

/// <summary>
/// Postavke za potpisivanje i provjeru JWT tokena. Čitaju se iz appsettings.json, odjeljak "Jwt".
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Tajni ključ. Mora imati barem 32 znaka za HMAC-SHA256.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 480;
}

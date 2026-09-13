namespace eKvarovi.Api.Security;

/// <summary>
/// Nazivi pravila pristupa. Koriste se u atributu [Authorize(Policy = ...)].
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Samo administrator - korisnički računi.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Administrator i voditelj - šifarnici, dodjele, promjena statusa.</summary>
    public const string Management = "Management";

    /// <summary>Administrator, voditelj i serviser - intervencije i materijal.</summary>
    public const string FieldWork = "FieldWork";
}

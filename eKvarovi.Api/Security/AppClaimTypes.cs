using System.Security.Claims;

namespace eKvarovi.Api.Security;

/// <summary>
/// Vlastiti tipovi podataka u tokenu.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Id djelatnika vezanog na prijavljeni račun.</summary>
    public const string EmployeeId = "employee_id";
}

/// <summary>
/// Čitanje podataka iz tokena. Kontroleri nikad ne primaju id korisnika iz tijela
/// zahtjeva - uvijek ga uzimaju odavde, inače bi se korisnik mogao lažno predstaviti.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    /// <summary>Vraća null ako račun nije vezan na djelatnika (npr. čisti administrator).</summary>
    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AppClaimTypes.EmployeeId);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles) =>
        roles.Any(user.IsInRole);
}

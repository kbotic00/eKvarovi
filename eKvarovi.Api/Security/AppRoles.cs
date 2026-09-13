namespace eKvarovi.Api.Security;

/// <summary>
/// Nazivi uloga na jednom mjestu, da se ne pišu kao slobodan tekst po kontrolerima.
/// Vrijednosti moraju odgovarati onima u tablici AppRoles.
/// </summary>
public static class AppRoles
{
    /// <summary>Upravlja korisničkim računima i šifarnicima.</summary>
    public const string Admin = "Admin";

    /// <summary>Voditelj održavanja - dodjeljuje kvarove i mijenja statuse.</summary>
    public const string Manager = "Manager";

    /// <summary>Serviser - unosi intervencije na kvarovima koji su njemu dodijeljeni.</summary>
    public const string Technician = "Technician";

    /// <summary>Djelatnik koji prijavljuje kvarove na svojoj lokaciji.</summary>
    public const string Reporter = "Reporter";
}

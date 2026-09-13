namespace eKvarovi.Shared.Models;

/// <summary>
/// Fiksni id-evi šifarnika. Vrijednosti su zadane u migraciji preko HasData,
/// pa se u kodu na njih možemo sigurno pozivati umjesto na "magične brojeve"
/// ili na naziv koji netko može promijeniti.
/// </summary>
public static class LocationTypeIds
{
    public const int UpravnaZgrada = 1;
    public const int Skola = 2;
    public const int ZdravstvenaUstanova = 3;
    public const int Skladiste = 4;
}

public static class FaultTypeIds
{
    public const int Elektrika = 1;
    public const int Voda = 2;
    public const int Grijanje = 3;
    public const int Mreza = 4;
    public const int GradevinskiRadovi = 5;
    public const int Ostalo = 6;
}

public static class FaultPriorityIds
{
    public const int Nizak = 1;
    public const int Srednji = 2;
    public const int Visok = 3;
    public const int Kritican = 4;
}

public static class FaultStatusIds
{
    public const int Zaprimljeno = 1;
    public const int Pregledano = 2;
    public const int Dodijeljeno = 3;
    public const int URadu = 4;
    public const int Rijeseno = 5;
    public const int Zatvoreno = 6;
}

public static class InterventionStatusIds
{
    public const int Planirana = 1;
    public const int UTijeku = 2;

    /// <summary>Uspješno obavljen posao.</summary>
    public const int Zavrsena = 3;

    public const int Neuspjesna = 4;
}

public static class MaterialUnitIds
{
    public const int Komad = 1;
    public const int Metar = 2;
    public const int Litra = 3;
    public const int Kilogram = 4;
    public const int Paket = 5;
}

public static class AttachmentPurposeIds
{
    public const int FotografijaPrije = 1;
    public const int FotografijaPoslije = 2;
    public const int Dokument = 3;
}

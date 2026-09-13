namespace eKvarovi.Shared.Models;

/// <summary>
/// Zajednička osnova svih šifarnika. Naziv se prikazuje korisniku,
/// <see cref="SortOrder"/> određuje redoslijed u padajućim izbornicima,
/// a <see cref="IsActive"/> omogućuje povlačenje stavke iz upotrebe
/// bez brisanja postojećih zapisa koji je koriste.
/// </summary>
public abstract class LookupBase
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Vrsta županijske lokacije: upravna zgrada, škola, zdravstvena ustanova, skladište.</summary>
public class LocationType : LookupBase
{
    public List<Location> Locations { get; set; } = new();
}

/// <summary>Vrsta kvara: elektrika, voda, grijanje, mreža, građevinski radovi, ostalo.</summary>
public class FaultType : LookupBase
{
    public List<FaultReport> FaultReports { get; set; } = new();
}

/// <summary>
/// Prioritet prijave. Kod najvišeg prioriteta rok je obavezan,
/// što je zapisano u samom šifarniku umjesto da bude zakucano u kodu.
/// </summary>
public class FaultPriority : LookupBase
{
    /// <summary>Veći broj znači hitniji kvar. Koristi se za sortiranje.</summary>
    public int Level { get; set; }

    public bool RequiresDeadline { get; set; }

    public List<FaultReport> FaultReports { get; set; } = new();
}

/// <summary>
/// Status prijave kroz životni ciklus.
/// Zaprimljeno -> Pregledano -> Dodijeljeno -> U radu -> Riješeno -> Zatvoreno
/// </summary>
public class FaultStatus : LookupBase
{
    /// <summary>Zatvorena prijava ne prima nove dodjele ni intervencije.</summary>
    public bool IsClosed { get; set; }

    public List<FaultReport> FaultReports { get; set; } = new();
}

/// <summary>
/// Status intervencije: Planirana, U tijeku, Završena, Neuspješna.
/// "Završena" znači uspješno obavljen posao i podiže prijavu u Riješeno.
/// "Neuspješna" ostaje u povijesti i dopušta otvaranje nove intervencije.
/// </summary>
public class InterventionStatus : LookupBase
{
    /// <summary>Intervencija je zaključena - uspješno ili neuspješno.</summary>
    public bool IsFinal { get; set; }

    /// <summary>Samo uspješan završetak podiže prijavu u status Riješeno.</summary>
    public bool IsSuccessful { get; set; }

    public List<Intervention> Interventions { get; set; } = new();
}

/// <summary>Mjerna jedinica materijala: komad, metar, litra, kilogram, paket.</summary>
public class MaterialUnit : LookupBase
{
    /// <summary>Kratica za prikaz u tablicama, npr. "kom".</summary>
    public string Abbreviation { get; set; } = string.Empty;

    public List<Material> Materials { get; set; } = new();
}

/// <summary>Namjena privitka: fotografija prije rada, fotografija nakon rada, dokument.</summary>
public class AttachmentPurpose : LookupBase
{
    /// <summary>Dokumenti dopuštaju PDF, fotografije samo slike.</summary>
    public bool AllowsDocuments { get; set; }

    public List<Attachment> Attachments { get; set; } = new();
}

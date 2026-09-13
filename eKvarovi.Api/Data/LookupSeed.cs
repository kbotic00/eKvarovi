using eKvarovi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

/// <summary>
/// Početne vrijednosti svih šifarnika. Idu kroz HasData, dakle postaju dio
/// migracije - baza ih dobiva pri stvaranju, bez obzira pokreće li se seeder.
/// Id-evi su fiksni i odgovaraju konstantama u <c>LookupIds</c>.
/// </summary>
public static class LookupSeed
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocationType>().HasData(
            new LocationType { Id = LocationTypeIds.UpravnaZgrada, Name = "Upravna zgrada", SortOrder = 1 },
            new LocationType { Id = LocationTypeIds.Skola, Name = "Škola", SortOrder = 2 },
            new LocationType { Id = LocationTypeIds.ZdravstvenaUstanova, Name = "Zdravstvena ustanova", SortOrder = 3 },
            new LocationType { Id = LocationTypeIds.Skladiste, Name = "Skladište", SortOrder = 4 }
        );

        modelBuilder.Entity<FaultType>().HasData(
            new FaultType { Id = FaultTypeIds.Elektrika, Name = "Elektrika", SortOrder = 1 },
            new FaultType { Id = FaultTypeIds.Voda, Name = "Voda", SortOrder = 2 },
            new FaultType { Id = FaultTypeIds.Grijanje, Name = "Grijanje", SortOrder = 3 },
            new FaultType { Id = FaultTypeIds.Mreza, Name = "Mreža", SortOrder = 4 },
            new FaultType { Id = FaultTypeIds.GradevinskiRadovi, Name = "Građevinski radovi", SortOrder = 5 },
            new FaultType { Id = FaultTypeIds.Ostalo, Name = "Ostalo", SortOrder = 6 }
        );

        // samo kritičan prioritet traži rok - pravilo je podatak, ne kod
        modelBuilder.Entity<FaultPriority>().HasData(
            new FaultPriority { Id = FaultPriorityIds.Nizak, Name = "Nizak", SortOrder = 1, Level = 1, RequiresDeadline = false },
            new FaultPriority { Id = FaultPriorityIds.Srednji, Name = "Srednji", SortOrder = 2, Level = 2, RequiresDeadline = false },
            new FaultPriority { Id = FaultPriorityIds.Visok, Name = "Visok", SortOrder = 3, Level = 3, RequiresDeadline = false },
            new FaultPriority { Id = FaultPriorityIds.Kritican, Name = "Kritičan", SortOrder = 4, Level = 4, RequiresDeadline = true }
        );

        modelBuilder.Entity<FaultStatus>().HasData(
            new FaultStatus { Id = FaultStatusIds.Zaprimljeno, Name = "Zaprimljeno", SortOrder = 1, IsClosed = false },
            new FaultStatus { Id = FaultStatusIds.Pregledano, Name = "Pregledano", SortOrder = 2, IsClosed = false },
            new FaultStatus { Id = FaultStatusIds.Dodijeljeno, Name = "Dodijeljeno", SortOrder = 3, IsClosed = false },
            new FaultStatus { Id = FaultStatusIds.URadu, Name = "U radu", SortOrder = 4, IsClosed = false },
            new FaultStatus { Id = FaultStatusIds.Rijeseno, Name = "Riješeno", SortOrder = 5, IsClosed = false },
            new FaultStatus { Id = FaultStatusIds.Zatvoreno, Name = "Zatvoreno", SortOrder = 6, IsClosed = true }
        );

        // "Završena" je jedini status koji znači uspjeh i podiže prijavu u Riješeno
        modelBuilder.Entity<InterventionStatus>().HasData(
            new InterventionStatus { Id = InterventionStatusIds.Planirana, Name = "Planirana", SortOrder = 1, IsFinal = false, IsSuccessful = false },
            new InterventionStatus { Id = InterventionStatusIds.UTijeku, Name = "U tijeku", SortOrder = 2, IsFinal = false, IsSuccessful = false },
            new InterventionStatus { Id = InterventionStatusIds.Zavrsena, Name = "Završena", SortOrder = 3, IsFinal = true, IsSuccessful = true },
            new InterventionStatus { Id = InterventionStatusIds.Neuspjesna, Name = "Neuspješna", SortOrder = 4, IsFinal = true, IsSuccessful = false }
        );

        modelBuilder.Entity<MaterialUnit>().HasData(
            new MaterialUnit { Id = MaterialUnitIds.Komad, Name = "Komad", Abbreviation = "kom", SortOrder = 1 },
            new MaterialUnit { Id = MaterialUnitIds.Metar, Name = "Metar", Abbreviation = "m", SortOrder = 2 },
            new MaterialUnit { Id = MaterialUnitIds.Litra, Name = "Litra", Abbreviation = "l", SortOrder = 3 },
            new MaterialUnit { Id = MaterialUnitIds.Kilogram, Name = "Kilogram", Abbreviation = "kg", SortOrder = 4 },
            new MaterialUnit { Id = MaterialUnitIds.Paket, Name = "Paket", Abbreviation = "pak", SortOrder = 5 }
        );

        modelBuilder.Entity<AttachmentPurpose>().HasData(
            new AttachmentPurpose { Id = AttachmentPurposeIds.FotografijaPrije, Name = "Fotografija prije rada", SortOrder = 1, AllowsDocuments = false },
            new AttachmentPurpose { Id = AttachmentPurposeIds.FotografijaPoslije, Name = "Fotografija nakon rada", SortOrder = 2, AllowsDocuments = false },
            new AttachmentPurpose { Id = AttachmentPurposeIds.Dokument, Name = "Dokument", SortOrder = 3, AllowsDocuments = true }
        );

        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = 1, Name = "Admin" },
            new AppRole { Id = 2, Name = "Manager" },
            new AppRole { Id = 3, Name = "Technician" },
            new AppRole { Id = 4, Name = "Reporter" }
        );
    }
}

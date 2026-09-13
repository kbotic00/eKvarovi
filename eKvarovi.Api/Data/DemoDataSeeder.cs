using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

/// <summary>
/// Puni bazu demo podacima pri prvom pokretanju. Šifarnici dolaze iz migracije
/// (vidi <see cref="LookupSeed"/>), ovdje se unose poslovni podaci.
/// Svaki korak provjerava postoji li već nešto, pa ponovno pokretanje ne stvara duplikate.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(KvaroviDbContext db)
    {
        await SeedLocationsAsync(db);
        await SeedEmployeesAsync(db);
        await SeedMaterialsAsync(db);
        await SeedUsersAsync(db);
        await SeedFaultReportsAsync(db);
    }

    private static async Task SeedLocationsAsync(KvaroviDbContext db)
    {
        if (await db.Locations.AnyAsync())
            return;

        db.Locations.AddRange(
            new Location
            {
                Name = "Osnovna škola Petra Preradovića",
                Address = "Trg bana Jelačića 12",
                City = "Bjelovar",
                LocationTypeId = LocationTypeIds.Skola
            },
            new Location
            {
                Name = "Dom zdravlja Bjelovar",
                Address = "Ulica Matice hrvatske 3",
                City = "Bjelovar",
                LocationTypeId = LocationTypeIds.ZdravstvenaUstanova
            },
            new Location
            {
                Name = "Srednja škola Daruvar",
                Address = "Gundulićeva 22",
                City = "Daruvar",
                LocationTypeId = LocationTypeIds.Skola
            },
            new Location
            {
                Name = "Zgrada županijske uprave",
                Address = "Dr. Ante Starčevića 8",
                City = "Bjelovar",
                Note = "Sjedište županije, prioritetna lokacija.",
                LocationTypeId = LocationTypeIds.UpravnaZgrada
            },
            new Location
            {
                Name = "Dječji vrtić Sunce",
                Address = "Vatroslava Lisinskog 5",
                City = "Garešnica",
                LocationTypeId = LocationTypeIds.Skola
            },
            new Location
            {
                Name = "Središnje skladište županije",
                Address = "Industrijska 4",
                City = "Bjelovar",
                LocationTypeId = LocationTypeIds.Skladiste
            },
            new Location
            {
                Name = "Sportska dvorana Čazma",
                Address = "Kralja Tomislava 14",
                City = "Čazma",
                IsActive = false,
                Note = "U rekonstrukciji, privremeno izvan upotrebe.",
                LocationTypeId = LocationTypeIds.UpravnaZgrada
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(KvaroviDbContext db)
    {
        if (await db.Employees.AnyAsync())
            return;

        var uprava = await db.Locations.FirstAsync(x => x.Name.Contains("županijske uprave"));
        var skolaBj = await db.Locations.FirstAsync(x => x.Name.Contains("Preradovića"));
        var domZdravlja = await db.Locations.FirstAsync(x => x.Name.Contains("Dom zdravlja"));
        var skolaDa = await db.Locations.FirstAsync(x => x.Name.Contains("Daruvar"));
        var vrtic = await db.Locations.FirstAsync(x => x.Name.Contains("Sunce"));

        db.Employees.AddRange(
            // izvršitelji
            new Employee
            {
                FirstName = "Ivan", LastName = "Horvat",
                Email = "ivan.horvat@ekvarovi.local", Phone = "099 111 2233",
                JobTitle = "Električar", IsTechnician = true, LocationId = uprava.Id
            },
            new Employee
            {
                FirstName = "Marko", LastName = "Babić",
                Email = "marko.babic@ekvarovi.local", Phone = "098 222 3344",
                JobTitle = "Vodoinstalater", IsTechnician = true, LocationId = uprava.Id
            },
            new Employee
            {
                FirstName = "Petra", LastName = "Novak",
                Email = "petra.novak@ekvarovi.local", Phone = "091 333 4455",
                JobTitle = "Serviser informatičke opreme", IsTechnician = true, LocationId = uprava.Id
            },
            // upravitelj
            new Employee
            {
                FirstName = "Tomislav", LastName = "Jurić",
                Email = "tomislav.juric@ekvarovi.local", Phone = "095 444 5566",
                JobTitle = "Voditelj održavanja", LocationId = uprava.Id
            },
            // prijavitelji
            new Employee
            {
                FirstName = "Ana", LastName = "Kovačević",
                Email = "ana.kovacevic@ekvarovi.local", Phone = "092 555 6677",
                JobTitle = "Tajnica škole", LocationId = skolaBj.Id
            },
            new Employee
            {
                FirstName = "Josip", LastName = "Marić",
                Email = "josip.maric@ekvarovi.local", Phone = "097 666 7788",
                JobTitle = "Domar", LocationId = domZdravlja.Id
            },
            new Employee
            {
                FirstName = "Lucija", LastName = "Perić",
                Email = "lucija.peric@ekvarovi.local", Phone = "091 777 8899",
                JobTitle = "Voditeljica računovodstva", LocationId = skolaDa.Id
            },
            new Employee
            {
                FirstName = "Marija", LastName = "Vuković",
                Email = "marija.vukovic@ekvarovi.local", Phone = "098 888 9900",
                JobTitle = "Odgojiteljica", LocationId = vrtic.Id
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedMaterialsAsync(KvaroviDbContext db)
    {
        if (await db.Materials.AnyAsync())
            return;

        db.Materials.AddRange(
            new Material { Name = "Osigurač 6A", Code = "EL-006", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 1.80 },
            new Material { Name = "Žarulja LED 10W", Code = "EL-010", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 4.20 },
            new Material { Name = "Kabel NYM 3x1,5", Code = "EL-015", MaterialUnitId = MaterialUnitIds.Metar, DefaultUnitPrice = 1.35 },
            new Material { Name = "Brtva 1/2\"", Code = "VO-002", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 0.90 },
            new Material { Name = "Slavina jednoručna", Code = "VO-020", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 38.00 },
            new Material { Name = "Termostatska glava", Code = "GR-005", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 22.50 },
            new Material { Name = "Sredstvo protiv kamenca", Code = "GR-009", MaterialUnitId = MaterialUnitIds.Litra, DefaultUnitPrice = 6.40 },
            new Material { Name = "HDMI kabel 5 m", Code = "IT-005", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 12.50 },
            new Material { Name = "Mrežni kabel Cat6", Code = "IT-006", MaterialUnitId = MaterialUnitIds.Metar, DefaultUnitPrice = 0.95 },
            new Material { Name = "Sigurnosna brava", Code = "GR-100", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 68.00 },
            new Material { Name = "Vijci za bravu", Code = "GR-101", MaterialUnitId = MaterialUnitIds.Paket, DefaultUnitPrice = 2.40 },
            new Material { Name = "Silikon sanitarni", Code = "GR-110", MaterialUnitId = MaterialUnitIds.Komad, DefaultUnitPrice = 5.10 }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(KvaroviDbContext db)
    {
        if (await db.AppUsers.AnyAsync())
            return;

        var hasher = new PasswordHasher<AppUser>();

        var manager = await db.Employees.FirstOrDefaultAsync(x => x.Email == "tomislav.juric@ekvarovi.local");
        var technician = await db.Employees.FirstOrDefaultAsync(x => x.Email == "ivan.horvat@ekvarovi.local");
        var technician2 = await db.Employees.FirstOrDefaultAsync(x => x.Email == "marko.babic@ekvarovi.local");
        var reporter = await db.Employees.FirstOrDefaultAsync(x => x.Email == "ana.kovacevic@ekvarovi.local");

        var admin = new AppUser { Email = "admin@ekvarovi.local", DisplayName = "Administrator sustava" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var managerUser = new AppUser
        {
            Email = "voditelj@ekvarovi.local",
            DisplayName = "Tomislav Jurić",
            EmployeeId = manager?.Id
        };
        managerUser.PasswordHash = hasher.HashPassword(managerUser, "Voditelj123!");

        var technicianUser = new AppUser
        {
            Email = "serviser@ekvarovi.local",
            DisplayName = "Ivan Horvat",
            EmployeeId = technician?.Id
        };
        technicianUser.PasswordHash = hasher.HashPassword(technicianUser, "Serviser123!");

        var technicianUser2 = new AppUser
        {
            Email = "serviser2@ekvarovi.local",
            DisplayName = "Marko Babić",
            EmployeeId = technician2?.Id
        };
        technicianUser2.PasswordHash = hasher.HashPassword(technicianUser2, "Serviser123!");

        var reporterUser = new AppUser
        {
            Email = "prijavitelj@ekvarovi.local",
            DisplayName = "Ana Kovačević",
            EmployeeId = reporter?.Id
        };
        reporterUser.PasswordHash = hasher.HashPassword(reporterUser, "Prijava123!");

        db.AppUsers.AddRange(admin, managerUser, technicianUser, technicianUser2, reporterUser);
        await db.SaveChangesAsync();

        // 1 = Admin, 2 = Manager, 3 = Technician, 4 = Reporter
        db.AppUserRoles.AddRange(
            new AppUserRole { AppUserId = admin.Id, AppRoleId = 1 },
            new AppUserRole { AppUserId = admin.Id, AppRoleId = 2 },
            new AppUserRole { AppUserId = managerUser.Id, AppRoleId = 2 },
            new AppUserRole { AppUserId = technicianUser.Id, AppRoleId = 3 },
            new AppUserRole { AppUserId = technicianUser2.Id, AppRoleId = 3 },
            new AppUserRole { AppUserId = reporterUser.Id, AppRoleId = 4 }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedFaultReportsAsync(KvaroviDbContext db)
    {
        if (await db.FaultReports.AnyAsync())
            return;

        var loc = await db.Locations.ToDictionaryAsync(x => x.Name);
        var emp = await db.Employees.ToDictionaryAsync(x => x.Email);
        var mat = await db.Materials.ToDictionaryAsync(x => x.Name);

        var elektricar = emp["ivan.horvat@ekvarovi.local"];
        var vodoinstalater = emp["marko.babic@ekvarovi.local"];
        var informaticar = emp["petra.novak@ekvarovi.local"];
        var upravitelj = emp["tomislav.juric@ekvarovi.local"];
        var tajnica = emp["ana.kovacevic@ekvarovi.local"];
        var domar = emp["josip.maric@ekvarovi.local"];
        var racunovotkinja = emp["lucija.peric@ekvarovi.local"];
        var odgojiteljica = emp["marija.vukovic@ekvarovi.local"];

        var skolaBj = loc["Osnovna škola Petra Preradovića"];
        var domZdravlja = loc["Dom zdravlja Bjelovar"];
        var skolaDa = loc["Srednja škola Daruvar"];
        var uprava = loc["Zgrada županijske uprave"];
        var vrtic = loc["Dječji vrtić Sunce"];

        var danas = DateTime.Now.Date;

        // 1) Zaprimljeno - upravitelj još nije pregledao, pa nema vrste ni prioriteta
        var zaprimljena = new FaultReport
        {
            Title = "Ne radi rasvjeta u učionici 12",
            Description = "Dvije neonske cijevi trepere, treća ne svijetli uopće. Nastava se održava uz upaljena svjetla u hodniku.",
            ReportedAt = danas.AddDays(-1).AddHours(9),
            LocationId = skolaBj.Id,
            ReporterId = tajnica.Id,
            FaultStatusId = FaultStatusIds.Zaprimljeno
        };

        // 2) Pregledano - određeni vrsta, prioritet i rok, čeka dodjelu
        var pregledana = new FaultReport
        {
            Title = "Puca vodovodna cijev u podrumu",
            Description = "Iz spoja ispod stubišta curi voda, podrum se polako plavi.",
            ReportedAt = danas.AddDays(-2).AddHours(7),
            LocationId = domZdravlja.Id,
            ReporterId = domar.Id,
            FaultStatusId = FaultStatusIds.Pregledano,
            FaultTypeId = FaultTypeIds.Voda,
            FaultPriorityId = FaultPriorityIds.Kritican,
            Deadline = danas.AddDays(1),
            ReviewedAt = danas.AddDays(-2).AddHours(8),
            ReviewedById = upravitelj.Id
        };

        // 3) Dodijeljeno - izvršitelj ima nalog, još nije izašao na teren
        var dodijeljena = new FaultReport
        {
            Title = "Curi slavina u sanitarnom čvoru",
            Description = "Slavina u ženskom WC-u na prvom katu curi neprekidno.",
            ReportedAt = danas.AddDays(-3).AddHours(11),
            LocationId = domZdravlja.Id,
            ReporterId = domar.Id,
            FaultStatusId = FaultStatusIds.Dodijeljeno,
            FaultTypeId = FaultTypeIds.Voda,
            FaultPriorityId = FaultPriorityIds.Visok,
            Deadline = danas.AddDays(3),
            ReviewedAt = danas.AddDays(-3).AddHours(13),
            ReviewedById = upravitelj.Id,
            Assignments =
            {
                new WorkAssignment
                {
                    TechnicianId = vodoinstalater.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-3).AddHours(14),
                    Note = "Ponijeti rezervnu kartušu.",
                    IsActive = true
                }
            }
        };

        // 4) U radu, PREKORAČEN ROK, s poviješću dvije dodjele -
        //    prva intervencija neuspješna, posao predan drugom izvršitelju
        var uRadu = new FaultReport
        {
            Title = "Kotlovnica se gasi tijekom noći",
            Description = "Grijanje ujutro ne radi, kotao je u stanju greške. Ručnim resetom proradi pa se navečer opet ugasi.",
            ReportedAt = danas.AddDays(-12).AddHours(7),
            LocationId = skolaDa.Id,
            ReporterId = racunovotkinja.Id,
            FaultStatusId = FaultStatusIds.URadu,
            FaultTypeId = FaultTypeIds.Grijanje,
            FaultPriorityId = FaultPriorityIds.Kritican,
            Deadline = danas.AddDays(-4),
            ReviewedAt = danas.AddDays(-12).AddHours(8),
            ReviewedById = upravitelj.Id,
            Assignments =
            {
                new WorkAssignment
                {
                    TechnicianId = elektricar.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-12).AddHours(9),
                    UnassignedAt = danas.AddDays(-8).AddHours(16),
                    Note = "Prva provjera - sumnja na elektroniku.",
                    ReassignReason = "Elektronika ispravna, problem je vjerojatno u tlaku sustava.",
                    IsActive = false,
                    Interventions =
                    {
                        new Intervention
                        {
                            InterventionStatusId = InterventionStatusIds.Neuspjesna,
                            StartedAt = danas.AddDays(-10).AddHours(8),
                            FinishedAt = danas.AddDays(-10).AddHours(11),
                            DurationMinutes = 180,
                            Note = "Provjerena elektronika kotla i očitane greške. Zamijenjen osigurač, kotao proradio, ali se navečer opet ugasio."
                        }
                    }
                },
                new WorkAssignment
                {
                    TechnicianId = vodoinstalater.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-8).AddHours(16),
                    Note = "Preuzima kolega, provjeriti tlak u sustavu.",
                    IsActive = true,
                    Interventions =
                    {
                        new Intervention
                        {
                            InterventionStatusId = InterventionStatusIds.UTijeku,
                            StartedAt = danas.AddDays(-6).AddHours(9),
                            Note = "Izmjeren tlak u sustavu, dopunjena voda. Prati se ponašanje kroz nekoliko dana."
                        }
                    }
                }
            }
        };

        // 5) Riješeno - uspješna intervencija, čeka završnu provjeru upravitelja
        var rijesena = new FaultReport
        {
            Title = "Projektor u dvorani ne prikazuje sliku",
            Description = "Projektor se uključuje, ali nema signala s računala. Provjeren kabel, problem ostaje.",
            ReportedAt = danas.AddDays(-15).AddHours(10),
            LocationId = vrtic.Id,
            ReporterId = odgojiteljica.Id,
            FaultStatusId = FaultStatusIds.Rijeseno,
            FaultTypeId = FaultTypeIds.Mreza,
            FaultPriorityId = FaultPriorityIds.Srednji,
            Deadline = danas.AddDays(-5),
            ReviewedAt = danas.AddDays(-15).AddHours(12),
            ReviewedById = upravitelj.Id,
            ResolvedAt = danas.AddDays(-13).AddHours(12),
            Assignments =
            {
                new WorkAssignment
                {
                    TechnicianId = informaticar.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-15).AddHours(13),
                    IsActive = true,
                    Interventions =
                    {
                        new Intervention
                        {
                            InterventionStatusId = InterventionStatusIds.Zavrsena,
                            StartedAt = danas.AddDays(-13).AddHours(10),
                            FinishedAt = danas.AddDays(-13).AddHours(12),
                            DurationMinutes = 120,
                            Note = "Zamijenjen neispravan HDMI kabel i ažuriran upravljački program grafičke kartice.",
                            Materials =
                            {
                                new InterventionMaterial { MaterialId = mat["HDMI kabel 5 m"].Id, Quantity = 1, UnitPrice = 12.50 }
                            }
                        }
                    }
                }
            }
        };

        // 6) Zatvoreno - prošlo završnu provjeru
        var zatvorena = new FaultReport
        {
            Title = "Ulazna vrata se ne zaključavaju",
            Description = "Brava na glavnom ulazu ne uhvaća, vrata ostaju otključana preko noći.",
            ReportedAt = danas.AddDays(-30).AddHours(8),
            LocationId = uprava.Id,
            ReporterId = tajnica.Id,
            FaultStatusId = FaultStatusIds.Zatvoreno,
            FaultTypeId = FaultTypeIds.GradevinskiRadovi,
            FaultPriorityId = FaultPriorityIds.Visok,
            Deadline = danas.AddDays(-24),
            ReviewedAt = danas.AddDays(-30).AddHours(9),
            ReviewedById = upravitelj.Id,
            ResolvedAt = danas.AddDays(-26).AddHours(13),
            ClosedAt = danas.AddDays(-25).AddHours(15),
            ClosingNote = "Brava zamijenjena, provjereno zaključavanje. Prijavitelj potvrdio.",
            Assignments =
            {
                new WorkAssignment
                {
                    TechnicianId = elektricar.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-30).AddHours(10),
                    IsActive = true,
                    Interventions =
                    {
                        new Intervention
                        {
                            InterventionStatusId = InterventionStatusIds.Zavrsena,
                            StartedAt = danas.AddDays(-26).AddHours(9),
                            FinishedAt = danas.AddDays(-26).AddHours(13),
                            DurationMinutes = 240,
                            Note = "Demontirana stara brava, ugrađena nova sigurnosna brava s tri ključa.",
                            Materials =
                            {
                                new InterventionMaterial { MaterialId = mat["Sigurnosna brava"].Id, Quantity = 1, UnitPrice = 68.00 },
                                new InterventionMaterial { MaterialId = mat["Vijci za bravu"].Id, Quantity = 1, UnitPrice = 2.40 }
                            }
                        }
                    }
                }
            }
        };

        // 7) Dodijeljeno elektricaru, bez ijedne intervencije -
        //    sluzi da izvrsitelj Horvat ima na cemu pokrenuti rad
        var zaElektricara = new FaultReport
        {
            Title = "Iskri utičnica u zbornici",
            Description = "Prilikom uključivanja grijalice utičnica iskri i osjeti se miris paljevine. Utičnica je privremeno izvan upotrebe.",
            ReportedAt = danas.AddDays(-1).AddHours(13),
            LocationId = skolaBj.Id,
            ReporterId = tajnica.Id,
            FaultStatusId = FaultStatusIds.Dodijeljeno,
            FaultTypeId = FaultTypeIds.Elektrika,
            FaultPriorityId = FaultPriorityIds.Kritican,
            Deadline = danas.AddDays(2),
            ReviewedAt = danas.AddDays(-1).AddHours(14),
            ReviewedById = upravitelj.Id,
            Assignments =
            {
                new WorkAssignment
                {
                    TechnicianId = elektricar.Id,
                    AssignedById = upravitelj.Id,
                    AssignedAt = danas.AddDays(-1).AddHours(15),
                    Note = "Hitno, prije nastave. Ponijeti mjerni instrument.",
                    IsActive = true
                }
            }
        };

        db.FaultReports.AddRange(
            zaprimljena, pregledana, dodijeljena, uRadu,
            rijesena, zatvorena, zaElektricara);

        await db.SaveChangesAsync();
    }
}

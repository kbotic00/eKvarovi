using eKvarovi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Data;

public class KvaroviDbContext : DbContext
{
    public KvaroviDbContext(DbContextOptions<KvaroviDbContext> options)
        : base(options)
    {
    }

    // ---------- šifarnici ----------
    public DbSet<LocationType> LocationTypes => Set<LocationType>();
    public DbSet<FaultType> FaultTypes => Set<FaultType>();
    public DbSet<FaultPriority> FaultPriorities => Set<FaultPriority>();
    public DbSet<FaultStatus> FaultStatuses => Set<FaultStatus>();
    public DbSet<InterventionStatus> InterventionStatuses => Set<InterventionStatus>();
    public DbSet<MaterialUnit> MaterialUnits => Set<MaterialUnit>();
    public DbSet<AttachmentPurpose> AttachmentPurposes => Set<AttachmentPurpose>();

    // ---------- osnovni podaci ----------
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Material> Materials => Set<Material>();

    // ---------- glavni tok ----------
    public DbSet<FaultReport> FaultReports => Set<FaultReport>();
    public DbSet<WorkAssignment> WorkAssignments => Set<WorkAssignment>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<InterventionMaterial> InterventionMaterials => Set<InterventionMaterial>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    // ---------- autentikacija ----------
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureLookups(modelBuilder);
        ConfigureCoreData(modelBuilder);
        ConfigureFaultFlow(modelBuilder);
        ConfigureAttachments(modelBuilder);
        ConfigureSecurity(modelBuilder);

        LookupSeed.Apply(modelBuilder);
    }

    /// <summary>Zajednička pravila za sve šifarnike - naziv obavezan i jedinstven.</summary>
    private static void ConfigureLookups(ModelBuilder modelBuilder)
    {
        ConfigureLookup<LocationType>(modelBuilder);
        ConfigureLookup<FaultType>(modelBuilder);
        ConfigureLookup<FaultPriority>(modelBuilder);
        ConfigureLookup<FaultStatus>(modelBuilder);
        ConfigureLookup<InterventionStatus>(modelBuilder);
        ConfigureLookup<MaterialUnit>(modelBuilder);
        ConfigureLookup<AttachmentPurpose>(modelBuilder);

        modelBuilder.Entity<MaterialUnit>()
            .Property(x => x.Abbreviation)
            .IsRequired()
            .HasMaxLength(10);
    }

    private static void ConfigureLookup<TLookup>(ModelBuilder modelBuilder)
        where TLookup : LookupBase
    {
        modelBuilder.Entity<TLookup>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(x => x.Name).IsUnique();
        });
    }

    private static void ConfigureCoreData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Address).IsRequired().HasMaxLength(200);
            entity.Property(x => x.City).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Note).HasMaxLength(500);

            entity.HasOne(x => x.LocationType)
                  .WithMany(t => t.Locations)
                  .HasForeignKey(x => x.LocationTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.JobTitle).HasMaxLength(100);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Location)
                  .WithMany(l => l.Employees)
                  .HasForeignKey(x => x.LocationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Code).HasMaxLength(50);

            entity.HasIndex(x => x.Name).IsUnique();

            entity.HasOne(x => x.MaterialUnit)
                  .WithMany(u => u.Materials)
                  .HasForeignKey(x => x.MaterialUnitId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFaultFlow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FaultReport>(entity =>
        {
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.ClosingNote).HasMaxLength(1000);

            entity.HasIndex(x => x.FaultStatusId);
            entity.HasIndex(x => x.ReportedAt);
            entity.HasIndex(x => x.Deadline);

            // lokacija i prijavitelj se ne smiju obrisati dok postoje prijave
            entity.HasOne(x => x.Location)
                  .WithMany(l => l.FaultReports)
                  .HasForeignKey(x => x.LocationId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Reporter)
                  .WithMany(e => e.ReportedFaults)
                  .HasForeignKey(x => x.ReporterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(x => x.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.FaultStatus)
                  .WithMany(s => s.FaultReports)
                  .HasForeignKey(x => x.FaultStatusId)
                  .OnDelete(DeleteBehavior.Restrict);

            // vrsta i prioritet su neobavezni dok upravitelj ne pregleda prijavu
            entity.HasOne(x => x.FaultType)
                  .WithMany(t => t.FaultReports)
                  .HasForeignKey(x => x.FaultTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.FaultPriority)
                  .WithMany(p => p.FaultReports)
                  .HasForeignKey(x => x.FaultPriorityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkAssignment>(entity =>
        {
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.ReassignReason).HasMaxLength(500);

            // brisanjem prijave nestaje i njezina povijest naloga
            entity.HasOne(x => x.FaultReport)
                  .WithMany(f => f.Assignments)
                  .HasForeignKey(x => x.FaultReportId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Technician)
                  .WithMany(e => e.Assignments)
                  .HasForeignKey(x => x.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedBy)
                  .WithMany()
                  .HasForeignKey(x => x.AssignedById)
                  .OnDelete(DeleteBehavior.SetNull);

            // baza sama jamči najviše JEDAN aktivan nalog po prijavi
            entity.HasIndex(x => x.FaultReportId)
                  .IsUnique()
                  .HasFilter("IsActive = 1")
                  .HasDatabaseName("IX_WorkAssignments_OneActivePerReport");
        });

        modelBuilder.Entity<Intervention>(entity =>
        {
            entity.Property(x => x.Note).HasMaxLength(2000);

            entity.HasIndex(x => x.StartedAt);

            entity.HasOne(x => x.WorkAssignment)
                  .WithMany(a => a.Interventions)
                  .HasForeignKey(x => x.WorkAssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.InterventionStatus)
                  .WithMany(s => s.Interventions)
                  .HasForeignKey(x => x.InterventionStatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InterventionMaterial>(entity =>
        {
            entity.HasOne(x => x.Intervention)
                  .WithMany(i => i.Materials)
                  .HasForeignKey(x => x.InterventionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // materijal iz šifarnika se ne briše dok je utrošen na nekom nalogu
            entity.HasOne(x => x.Material)
                  .WithMany(m => m.Interventions)
                  .HasForeignKey(x => x.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            // isti materijal se na jednoj intervenciji vodi u jednom retku
            entity.HasIndex(x => new { x.InterventionId, x.MaterialId }).IsUnique();
        });
    }

    private static void ConfigureAttachments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
            entity.Property(x => x.StoredFileName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Url).IsRequired().HasMaxLength(300);
            entity.Property(x => x.ContentType).IsRequired().HasMaxLength(100);

            entity.Ignore(x => x.IsImage);

            entity.HasOne(x => x.AttachmentPurpose)
                  .WithMany(p => p.Attachments)
                  .HasForeignKey(x => x.AttachmentPurposeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FaultReport)
                  .WithMany(f => f.Attachments)
                  .HasForeignKey(x => x.FaultReportId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Intervention)
                  .WithMany(i => i.Attachments)
                  .HasForeignKey(x => x.InterventionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UploadedBy)
                  .WithMany()
                  .HasForeignKey(x => x.UploadedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(150);
            entity.Property(x => x.PasswordHash).IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Employee)
                  .WithMany()
                  .HasForeignKey(x => x.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<AppUserRole>(entity =>
        {
            entity.HasKey(x => new { x.AppUserId, x.AppRoleId });

            entity.HasOne(x => x.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(x => x.AppUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(x => x.AppRoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

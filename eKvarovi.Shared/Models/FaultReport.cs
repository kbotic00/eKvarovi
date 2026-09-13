namespace eKvarovi.Shared.Models;

/// <summary>
/// Prijava kvara - središnji entitet aplikacije.
///
/// Vrsta kvara, prioritet i rok namjerno su neobavezni: prijavitelj ih ne
/// zna ni ne smije određivati, popunjava ih upravitelj kod pregleda prijave.
/// Zato prijava u statusu Zaprimljeno smije postojati bez njih.
/// </summary>
public class FaultReport
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime ReportedAt { get; set; } = DateTime.Now;

    // ---------- lokacija i prijavitelj ----------

    public int LocationId { get; set; }

    public Location? Location { get; set; }

    public int ReporterId { get; set; }

    public Employee? Reporter { get; set; }

    // ---------- podaci koje postavlja upravitelj kod pregleda ----------

    public int? FaultTypeId { get; set; }

    public FaultType? FaultType { get; set; }

    public int? FaultPriorityId { get; set; }

    public FaultPriority? FaultPriority { get; set; }

    /// <summary>Rok za rješavanje. Kod kritičnog prioriteta je obavezan.</summary>
    public DateTime? Deadline { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedById { get; set; }

    public Employee? ReviewedBy { get; set; }

    // ---------- status ----------

    public int FaultStatusId { get; set; } = FaultStatusIds.Zaprimljeno;

    public FaultStatus? FaultStatus { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    /// <summary>Bilješka upravitelja kod završne provjere i zatvaranja.</summary>
    public string? ClosingNote { get; set; }

    // ---------- povezani zapisi ----------

    /// <summary>Povijest dodjela. Najviše jedna smije biti aktivna.</summary>
    public List<WorkAssignment> Assignments { get; set; } = new();

    public List<Attachment> Attachments { get; set; } = new();
}

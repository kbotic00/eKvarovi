namespace eKvarovi.Shared.DTOs;

/// <summary>Brojke za nadzornu ploču.</summary>
public class DashboardSummaryDto
{
    public int TotalReports { get; set; }

    public int OpenReports { get; set; }

    /// <summary>Otvorene prijave bez aktivnog izvršitelja.</summary>
    public int UnassignedReports { get; set; }

    /// <summary>Otvorene prijave kritičnog prioriteta.</summary>
    public int CriticalOpenReports { get; set; }

    /// <summary>Otvorene prijave kojima je rok probijen.</summary>
    public int OverdueReports { get; set; }

    /// <summary>Intervencije koje su pokrenute, a još nisu zaključene.</summary>
    public int ActiveInterventions { get; set; }

    public int ResolvedAwaitingClosure { get; set; }

    public int ClosedThisMonth { get; set; }

    /// <summary>Prosječno vrijeme od prijave do zatvaranja, u danima.</summary>
    public double AverageResolutionDays { get; set; }

    public int TotalMinutesThisMonth { get; set; }

    // ---------- osobni podaci prijavljenog korisnika ----------

    /// <summary>Broj vlastitih prijava, ako je račun vezan na djelatnika.</summary>
    public int MyReportCount { get; set; }

    public int MyOpenReportCount { get; set; }

    /// <summary>Broj aktivnih radnih naloga, ako je korisnik izvršitelj.</summary>
    public int MyActiveAssignmentCount { get; set; }

    public int MyInterventionCount { get; set; }

    // ---------- grupirane statistike ----------

    public List<CountByNameDto> ByStatus { get; set; } = new();

    public List<CountByNameDto> ByPriority { get; set; } = new();

    public List<CountByNameDto> ByFaultType { get; set; } = new();

    public List<CountByNameDto> TopLocations { get; set; } = new();

    public List<FaultReportDto> RecentReports { get; set; } = new();
}

namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Filteri popisa prijava. Šalju se API-ju kao query parametri, a filtriranje,
/// sortiranje i stranicenje rade se u bazi - ne u pregledniku.
/// </summary>
public class FaultReportFilterDto
{
    /// <summary>Pretraga po naslovu i opisu.</summary>
    public string? Search { get; set; }

    public int? LocationId { get; set; }

    public int? FaultTypeId { get; set; }

    public int? FaultPriorityId { get; set; }

    public int? FaultStatusId { get; set; }

    public int? TechnicianId { get; set; }

    public int? ReporterId { get; set; }

    public DateTime? ReportedFrom { get; set; }

    public DateTime? ReportedTo { get; set; }

    public bool OnlyOpen { get; set; }

    /// <summary>Samo prijave kojima je rok probijen.</summary>
    public bool OnlyOverdue { get; set; }

    /// <summary>Samo prijave bez aktivnog izvršitelja.</summary>
    public bool OnlyUnassigned { get; set; }

    /// <summary>Dopušteno: reportedAt, title, location, priority, status, deadline.</summary>
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    /// <summary>Pretvara filtere u query string za poziv prema API-ju.</summary>
    public string ToQueryString()
    {
        var parts = new List<string>();

        void Add(string key, object? value)
        {
            if (value is null) return;
            if (value is string s && string.IsNullOrWhiteSpace(s)) return;
            if (value is bool b && !b) return;

            var text = value is DateTime d
                ? d.ToString("yyyy-MM-dd")
                : value.ToString();

            parts.Add($"{key}={Uri.EscapeDataString(text ?? string.Empty)}");
        }

        Add("search", Search);
        Add("locationId", LocationId);
        Add("faultTypeId", FaultTypeId);
        Add("faultPriorityId", FaultPriorityId);
        Add("faultStatusId", FaultStatusId);
        Add("technicianId", TechnicianId);
        Add("reporterId", ReporterId);
        Add("reportedFrom", ReportedFrom);
        Add("reportedTo", ReportedTo);
        Add("onlyOpen", OnlyOpen);
        Add("onlyOverdue", OnlyOverdue);
        Add("onlyUnassigned", OnlyUnassigned);
        Add("sortBy", SortBy);
        Add("sortDescending", SortDescending);
        Add("page", Page);
        Add("pageSize", PageSize);

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    /// <summary>Vraća filtere na početne vrijednosti.</summary>
    public void Reset()
    {
        Search = null;
        LocationId = null;
        FaultTypeId = null;
        FaultPriorityId = null;
        FaultStatusId = null;
        TechnicianId = null;
        ReporterId = null;
        ReportedFrom = null;
        ReportedTo = null;
        OnlyOpen = false;
        OnlyOverdue = false;
        OnlyUnassigned = false;
        SortBy = null;
        SortDescending = true;
        Page = 1;
    }
}

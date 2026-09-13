namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Filteri popisa intervencija. Isti pristup kao kod prijava -
/// filtriranje i sortiranje rade se na poslužitelju.
/// </summary>
public class InterventionFilterDto
{
    /// <summary>Pretraga po bilješci i naslovu prijave.</summary>
    public string? Search { get; set; }

    /// <summary>Traženje po broju prijave.</summary>
    public int? FaultReportId { get; set; }

    public int? InterventionStatusId { get; set; }

    public int? TechnicianId { get; set; }

    public int? LocationId { get; set; }

    public DateTime? StartedFrom { get; set; }

    public DateTime? StartedTo { get; set; }

    /// <summary>Dopušteno: startedAt, duration, status, technician, faultReport.</summary>
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

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
        Add("faultReportId", FaultReportId);
        Add("interventionStatusId", InterventionStatusId);
        Add("technicianId", TechnicianId);
        Add("locationId", LocationId);
        Add("startedFrom", StartedFrom);
        Add("startedTo", StartedTo);
        Add("sortBy", SortBy);
        Add("sortDescending", SortDescending);
        Add("page", Page);
        Add("pageSize", PageSize);

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    public void Reset()
    {
        Search = null;
        FaultReportId = null;
        InterventionStatusId = null;
        TechnicianId = null;
        LocationId = null;
        StartedFrom = null;
        StartedTo = null;
        SortBy = null;
        SortDescending = true;
        Page = 1;
    }
}

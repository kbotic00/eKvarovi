namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Omot oko jedne stranice rezultata. Uz same retke vraća i ukupan broj,
/// da sučelje zna nacrtati stranicenje bez dodatnog poziva.
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

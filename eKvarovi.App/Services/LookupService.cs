using System.Net.Http.Json;
using eKvarovi.Shared.DTOs;

namespace eKvarovi.App.Services;

/// <summary>
/// Dohvaća i pamti šifarnike za vrijeme trajanja korisničke veze.
///
/// Šifarnici se gotovo ne mijenjaju, pa nema smisla da ih svaka stranica
/// povlači iznova. Prvi poziv ide na poslužitelj, ostali čitaju iz memorije.
/// </summary>
public class LookupService
{
    private readonly HttpClient _http;

    private AllLookupsDto? _cache;

    public LookupService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AllLookupsDto> GetAsync(bool forceReload = false)
    {
        if (_cache is not null && !forceReload)
        {
            return _cache;
        }

        try
        {
            _cache = await _http.GetFromJsonAsync<AllLookupsDto>("api/lookups/all") ?? new();
        }
        catch
        {
            // ako dohvat padne, vraćamo prazne popise da se stranica
            // ipak iscrta i prikaže vlastitu poruku o grešci
            _cache = new AllLookupsDto();
        }

        return _cache;
    }

    /// <summary>Poziva se nakon izmjene šifarnika da se osvježi zapamćeno.</summary>
    public void Invalidate() => _cache = null;

    /// <summary>Naziv stavke iz popisa, npr. za prikaz u tablici.</summary>
    public static string NameOf(IEnumerable<LookupDto> items, int? id) =>
        id is null ? "—" : items.FirstOrDefault(x => x.Id == id.Value)?.Name ?? "—";
}

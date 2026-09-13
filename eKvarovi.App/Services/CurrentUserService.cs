using System.Net.Http.Headers;
using System.Net.Http.Json;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace eKvarovi.App.Services;

/// <summary>
/// Drži podatke o prijavljenom korisniku za vrijeme trajanja veze i
/// stavlja token na svaki odlazni zahtjev prema API-ju.
/// Podaci se čuvaju u zaštićenoj pohrani preglednika da prijava preživi osvježavanje stranice.
/// </summary>
public class CurrentUserService
{
    private const string StorageKey = "ekvarovi.session";

    private readonly ProtectedLocalStorage _storage;
    private readonly HttpClient _http;

    public CurrentUserService(ProtectedLocalStorage storage, HttpClient http)
    {
        _storage = storage;
        _http = http;
    }

    public LoginResponseDto? User { get; private set; }

    /// <summary>Postaje true tek nakon prvog čitanja pohrane - dok je false, sučelje čeka.</summary>
    public bool IsInitialized { get; private set; }

    public bool IsLoggedIn => User is not null && User.ExpiresAt > DateTime.UtcNow;

    public string DisplayName => User?.DisplayName ?? string.Empty;

    public int? EmployeeId => User?.EmployeeId;

    public bool IsAdmin => IsInRole("Admin");

    public bool IsManager => IsInRole("Manager");

    public bool IsTechnician => IsInRole("Technician");

    public bool IsReporter => IsInRole("Reporter");

    /// <summary>Administrator i voditelj - dodjele, statusi, šifarnici.</summary>
    public bool IsManagement => IsAdmin || IsManager;

    /// <summary>Svi koji rade s kvarovima na terenu.</summary>
    public bool IsFieldWorker => IsManagement || IsTechnician;

    public event Action? OnChange;

    public bool IsInRole(string role) =>
        User?.Roles.Contains(role, StringComparer.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Čita spremljenu prijavu. Smije se pozvati tek iz OnAfterRenderAsync
    /// jer zaštićena pohrana radi preko JavaScripta, a on postoji tek nakon prvog iscrtavanja.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        try
        {
            var stored = await _storage.GetAsync<LoginResponseDto>(StorageKey);

            if (stored.Success && stored.Value is not null && stored.Value.ExpiresAt > DateTime.UtcNow)
            {
                User = stored.Value;
                ApplyToken(stored.Value.Token);
            }
            else if (stored.Success && stored.Value is not null)
            {
                // token je istekao pa se odmah briše
                await _storage.DeleteAsync(StorageKey);
            }
        }
        catch
        {
            // oštećen ili nečitljiv zapis - korisnik se jednostavno mora ponovno prijaviti
        }

        IsInitialized = true;
        OnChange?.Invoke();
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequestDto
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                return string.IsNullOrWhiteSpace(message)
                    ? "Prijava nije uspjela."
                    : message;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            if (result is null)
            {
                return "Poslužitelj je vratio prazan odgovor.";
            }

            User = result;
            ApplyToken(result.Token);
            await _storage.SetAsync(StorageKey, result);

            OnChange?.Invoke();

            return null;
        }
        catch (Exception ex)
        {
            return $"Poslužitelj nije dostupan: {ex.Message}";
        }
    }

    public async Task LogoutAsync()
    {
        User = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _storage.DeleteAsync(StorageKey);
        }
        catch
        {
            // ako pohrana nije dostupna, odjava u memoriji je dovoljna
        }

        OnChange?.Invoke();
    }

    private void ApplyToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

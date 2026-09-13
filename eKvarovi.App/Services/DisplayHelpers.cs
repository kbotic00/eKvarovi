using eKvarovi.Shared.Models;
using MudBlazor;

namespace eKvarovi.App.Services;

/// <summary>
/// Boje i oblikovanje za prikaz. Na jednom mjestu da se ista pravila
/// ne prepisuju po svakoj stranici.
///
/// Boje se određuju po id-u iz šifarnika, a nazivi dolaze s poslužitelja -
/// tako prijevod naziva postoji samo u bazi.
/// </summary>
public static class DisplayHelpers
{
    public static Color StatusColor(int faultStatusId) => faultStatusId switch
    {
        FaultStatusIds.Zaprimljeno => Color.Warning,
        FaultStatusIds.Pregledano => Color.Secondary,
        FaultStatusIds.Dodijeljeno => Color.Info,
        FaultStatusIds.URadu => Color.Primary,
        FaultStatusIds.Rijeseno => Color.Success,
        FaultStatusIds.Zatvoreno => Color.Dark,
        _ => Color.Default
    };

    public static Color PriorityColor(int? faultPriorityId) => faultPriorityId switch
    {
        FaultPriorityIds.Nizak => Color.Default,
        FaultPriorityIds.Srednji => Color.Info,
        FaultPriorityIds.Visok => Color.Warning,
        FaultPriorityIds.Kritican => Color.Error,
        _ => Color.Default
    };

    public static string PriorityCssClass(int? faultPriorityId) => faultPriorityId switch
    {
        FaultPriorityIds.Kritican => "p-kriticno",
        FaultPriorityIds.Visok => "p-visok",
        FaultPriorityIds.Srednji => "p-srednji",
        _ => "p-nizak"
    };

    public static Color InterventionColor(int interventionStatusId) => interventionStatusId switch
    {
        InterventionStatusIds.Planirana => Color.Default,
        InterventionStatusIds.UTijeku => Color.Info,
        InterventionStatusIds.Zavrsena => Color.Success,
        InterventionStatusIds.Neuspjesna => Color.Error,
        _ => Color.Default
    };

    public static string RoleLabel(string role) => role switch
    {
        "Admin" => "Administrator",
        "Manager" => "Upravitelj",
        "Technician" => "Izvršitelj",
        "Reporter" => "Prijavitelj",
        _ => role
    };

    public static string Date(DateTime? value) =>
        value?.ToString("dd.MM.yyyy.") ?? "—";

    public static string DateTimeShort(DateTime? value) =>
        value?.ToString("dd.MM.yyyy. HH:mm") ?? "—";

    public static string Money(double value) =>
        value.ToString("0.00") + " €";

    public static string Duration(int? minutes) =>
        minutes is null or <= 0 ? "—" : $"{minutes.Value / 60} h {minutes.Value % 60} min";

    /// <summary>Koliko je dana ostalo do roka, ili koliko je probijen.</summary>
    public static string DeadlineText(DateTime? deadline, bool isClosed)
    {
        if (deadline is null)
        {
            return "—";
        }

        var text = deadline.Value.ToString("dd.MM.yyyy.");

        if (isClosed)
        {
            return text;
        }

        var days = (deadline.Value.Date - DateTime.Now.Date).Days;

        return days switch
        {
            < 0 => $"{text} (kasni {-days} d)",
            0 => $"{text} (danas)",
            _ => $"{text} (za {days} d)"
        };
    }
}

using MudBlazor;

namespace eKvarovi.App.Services;

/// <summary>
/// Izgled aplikacije na jednom mjestu. Paleta je namjerno prigušena,
/// s jednom naglašenom bojom - tako statusne oznake ostanu uočljive.
/// </summary>
public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4F46E5",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#0EA5E9",
            Tertiary = "#8B5CF6",

            Success = "#10B981",
            Info = "#3B82F6",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Dark = "#334155",

            Background = "#F8FAFC",
            BackgroundGray = "#F1F5F9",
            Surface = "#FFFFFF",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#1E293B",

            DrawerBackground = "#FFFFFF",
            DrawerText = "#475569",
            DrawerIcon = "#64748B",

            TextPrimary = "#0F172A",
            TextSecondary = "#64748B",
            TextDisabled = "#94A3B8",

            ActionDefault = "#64748B",
            Divider = "#E2E8F0",
            DividerLight = "#F1F5F9",
            TableLines = "#E2E8F0",
            LinesDefault = "#E2E8F0",
            LinesInputs = "#CBD5E1",

            GrayLight = "#F1F5F9",
            GrayLighter = "#F8FAFC"
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "264px",
            AppbarHeight = "68px"
        }
    };
}

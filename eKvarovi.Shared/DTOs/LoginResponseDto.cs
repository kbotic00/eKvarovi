namespace eKvarovi.Shared.DTOs;

/// <summary>
/// Odgovor na uspješnu prijavu. Token ide u Authorization zaglavlje,
/// ostatak koristi sučelje za prikaz i skrivanje izbornika po ulozi.
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Djelatnik vezan na račun - null za čisti administratorski račun.</summary>
    public int? EmployeeId { get; set; }

    public List<string> Roles { get; set; } = new();
}

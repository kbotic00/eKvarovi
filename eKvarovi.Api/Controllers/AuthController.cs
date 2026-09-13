using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly KvaroviDbContext _context;
    private readonly JwtTokenService _tokenService;

    public AuthController(KvaroviDbContext context, JwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Jedini endpoint dostupan bez tokena.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Upišite e-mail i lozinku.");
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.AppUsers
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        // ista poruka za nepostojećeg korisnika i za krivu lozinku,
        // da se izvana ne može zaključiti koji e-mailovi postoje
        if (user is null || !user.IsActive)
        {
            return Unauthorized("Neispravan e-mail ili lozinka.");
        }

        var hasher = new PasswordHasher<AppUser>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Neispravan e-mail ili lozinka.");
        }

        // lozinka je ispravna, ali je zapisana starijim algoritmom - tiho je osvježimo
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, dto.Password);
            await _context.SaveChangesAsync();
        }

        var roles = user.UserRoles
            .Where(x => x.Role is not null)
            .Select(x => x.Role!.Name)
            .ToList();

        var (token, expiresAt) = _tokenService.CreateToken(user, roles);

        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            EmployeeId = user.EmployeeId,
            Roles = roles
        });
    }

    /// <summary>
    /// Vraća podatke o prijavljenom korisniku iz tokena.
    /// Služi za provjeru je li token još valjan.
    /// </summary>
    [HttpGet("me")]
    public ActionResult<LoginResponseDto> Me()
    {
        return Ok(new LoginResponseDto
        {
            UserId = User.GetUserId(),
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
            DisplayName = User.Identity?.Name ?? string.Empty,
            EmployeeId = User.GetEmployeeId(),
            Roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList()
        });
    }
}

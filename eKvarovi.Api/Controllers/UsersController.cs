using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Upravljanje korisničkim računima. Cijeli kontroler je zaključan na administratora.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public UsersController(KvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserAdminDto>>> GetAll()
    {
        var users = await _context.AppUsers
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new UserAdminDto
            {
                Id = x.Id,
                Email = x.Email,
                DisplayName = x.DisplayName,
                IsActive = x.IsActive,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null
                    ? x.Employee.FirstName + " " + x.Employee.LastName
                    : null,
                Roles = x.UserRoles.Select(r => r.Role!.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserAdminDto>> GetById(int id)
    {
        var user = await _context.AppUsers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserAdminDto
            {
                Id = x.Id,
                Email = x.Email,
                DisplayName = x.DisplayName,
                IsActive = x.IsActive,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null
                    ? x.Employee.FirstName + " " + x.Employee.LastName
                    : null,
                Roles = x.UserRoles.Select(r => r.Role!.Name).ToList()
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>Popis svih uloga za odabir na obrascu.</summary>
    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDto>>> GetRoles()
    {
        var roles = await _context.AppRoles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new RoleDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<UserAdminDto>> Create(SaveUserAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Lozinka je obavezna kod novog računa.");
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(x => x.Email.ToLower() == email))
        {
            return BadRequest("Račun s tim e-mailom već postoji.");
        }

        if (dto.RoleIds.Count == 0)
        {
            return BadRequest("Račun mora imati barem jednu ulogu.");
        }

        var user = new AppUser
        {
            Email = email,
            DisplayName = dto.DisplayName.Trim(),
            IsActive = dto.IsActive,
            EmployeeId = dto.EmployeeId
        };

        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, dto.Password);

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        await ReplaceRolesAsync(user.Id, dto.RoleIds);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserAdminDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            EmployeeId = user.EmployeeId
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveUserAdminDto dto)
    {
        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(x => x.Id != id && x.Email.ToLower() == email))
        {
            return BadRequest("Drugi račun već koristi taj e-mail.");
        }

        if (dto.RoleIds.Count == 0)
        {
            return BadRequest("Račun mora imati barem jednu ulogu.");
        }

        // administrator ne smije sam sebi oduzeti administratorsku ulogu ili se deaktivirati
        if (id == User.GetUserId())
        {
            const int adminRoleId = 1;

            if (!dto.RoleIds.Contains(adminRoleId))
            {
                return BadRequest("Ne možete sami sebi oduzeti administratorsku ulogu.");
            }

            if (!dto.IsActive)
            {
                return BadRequest("Ne možete deaktivirati vlastiti račun.");
            }
        }

        user.Email = email;
        user.DisplayName = dto.DisplayName.Trim();
        user.IsActive = dto.IsActive;
        user.EmployeeId = dto.EmployeeId;

        // lozinka se mijenja samo ako je upisana nova
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, dto.Password);
        }

        await _context.SaveChangesAsync();
        await ReplaceRolesAsync(user.Id, dto.RoleIds);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == User.GetUserId())
        {
            return BadRequest("Ne možete obrisati vlastiti račun.");
        }

        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Obriše postojeće uloge računa i postavi nove.</summary>
    private async Task ReplaceRolesAsync(int userId, List<int> roleIds)
    {
        var existing = await _context.AppUserRoles
            .Where(x => x.AppUserId == userId)
            .ToListAsync();

        _context.AppUserRoles.RemoveRange(existing);

        var validRoleIds = await _context.AppRoles
            .Where(x => roleIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        _context.AppUserRoles.AddRange(validRoleIds.Select(roleId => new AppUserRole
        {
            AppUserId = userId,
            AppRoleId = roleId
        }));

        await _context.SaveChangesAsync();
    }
}

using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Materijal utrošen na intervenciji. Pravila pristupa su ista kao za
/// intervencije - izvršitelj dira samo svoje.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.FieldWork)]
[ApiController]
[Route("api/[controller]")]
public class InterventionMaterialsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public InterventionMaterialsController(KvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet("intervention/{interventionId:int}")]
    public async Task<ActionResult<List<InterventionMaterialDto>>> GetForIntervention(int interventionId)
    {
        var items = await _context.InterventionMaterials
            .AsNoTracking()
            .Where(x => x.InterventionId == interventionId)
            .OrderBy(x => x.Material!.Name)
            .Select(DtoProjections.InterventionMaterial)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterventionMaterialDto>> GetById(int id)
    {
        var item = await _context.InterventionMaterials
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(DtoProjections.InterventionMaterial)
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<InterventionMaterialDto>> Create(SaveInterventionMaterialDto dto)
    {
        // količina mora biti veća od nule
        if (dto.Quantity <= 0)
        {
            return BadRequest("Količina mora biti veća od nule.");
        }

        var intervention = await _context.Interventions
            .Include(x => x.WorkAssignment)
            .Include(x => x.InterventionStatus)
            .FirstOrDefaultAsync(x => x.Id == dto.InterventionId);

        if (intervention is null)
        {
            return BadRequest("Intervencija ne postoji.");
        }

        if (!CanWork(intervention.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        if (intervention.InterventionStatus!.IsFinal &&
            !User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return BadRequest("Intervencija je zaključena pa se materijal više ne može dodavati.");
        }

        var material = await _context.Materials.FindAsync(dto.MaterialId);

        if (material is null)
        {
            return BadRequest("Odabrani materijal ne postoji.");
        }

        // isti materijal se na jednoj intervenciji vodi u jednom retku -
        // ponovni unos samo zbraja količinu
        var existing = await _context.InterventionMaterials
            .FirstOrDefaultAsync(x => x.InterventionId == dto.InterventionId &&
                                      x.MaterialId == dto.MaterialId);

        if (existing is not null)
        {
            existing.Quantity += dto.Quantity;
            existing.UnitPrice = dto.UnitPrice ?? existing.UnitPrice;
            await _context.SaveChangesAsync();

            var updated = await _context.InterventionMaterials
                .AsNoTracking()
                .Where(x => x.Id == existing.Id)
                .Select(DtoProjections.InterventionMaterial)
                .FirstAsync();

            return Ok(updated);
        }

        var item = new InterventionMaterial
        {
            InterventionId = dto.InterventionId,
            MaterialId = dto.MaterialId,
            Quantity = dto.Quantity,
            // cijena se pamti u trenutku utroška da kasnija promjena
            // cjenika ne mijenja stare naloge
            UnitPrice = dto.UnitPrice ?? material.DefaultUnitPrice
        };

        _context.InterventionMaterials.Add(item);
        await _context.SaveChangesAsync();

        var created = await _context.InterventionMaterials
            .AsNoTracking()
            .Where(x => x.Id == item.Id)
            .Select(DtoProjections.InterventionMaterial)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveInterventionMaterialDto dto)
    {
        if (dto.Quantity <= 0)
        {
            return BadRequest("Količina mora biti veća od nule.");
        }

        var item = await _context.InterventionMaterials
            .Include(x => x.Intervention)
                .ThenInclude(i => i!.WorkAssignment)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (!CanWork(item.Intervention!.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        item.Quantity = dto.Quantity;

        if (dto.UnitPrice.HasValue)
        {
            item.UnitPrice = dto.UnitPrice.Value;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.InterventionMaterials
            .Include(x => x.Intervention)
                .ThenInclude(i => i!.WorkAssignment)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (!CanWork(item.Intervention!.WorkAssignment!.TechnicianId))
        {
            return Forbid();
        }

        _context.InterventionMaterials.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CanWork(int technicianId)
    {
        if (User.IsInAnyRole(AppRoles.Admin, AppRoles.Manager))
        {
            return true;
        }

        return technicianId == User.GetEmployeeId();
    }
}

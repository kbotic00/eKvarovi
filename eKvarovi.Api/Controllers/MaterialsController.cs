using System.Linq.Expressions;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Šifarnik materijala. Čitanje treba svakom izvršitelju koji evidentira
/// utrošak, a održavanje šifarnika radi upravitelj.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public MaterialsController(KvaroviDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<Material, MaterialDto>> ToDto = x => new MaterialDto
    {
        Id = x.Id,
        Name = x.Name,
        Code = x.Code,
        MaterialUnitId = x.MaterialUnitId,
        UnitName = x.MaterialUnit!.Name,
        UnitAbbreviation = x.MaterialUnit.Abbreviation,
        DefaultUnitPrice = x.DefaultUnitPrice,
        IsActive = x.IsActive,
        UsageCount = x.Interventions.Count
    };

    [HttpGet]
    public async Task<ActionResult<List<MaterialDto>>> GetAll(
        [FromQuery] bool onlyActive = false,
        [FromQuery] string? search = null)
    {
        var query = _context.Materials.AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term) ||
                (x.Code != null && x.Code.ToLower().Contains(term)));
        }

        var materials = await query
            .OrderBy(x => x.Name)
            .Select(ToDto)
            .ToListAsync();

        return Ok(materials);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaterialDto>> GetById(int id)
    {
        var material = await _context.Materials
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (material is null)
        {
            return NotFound();
        }

        return Ok(material);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult<MaterialDto>> Create(SaveMaterialDto dto)
    {
        var name = dto.Name.Trim();

        if (await _context.Materials.AnyAsync(x => x.Name.ToLower() == name.ToLower()))
        {
            return BadRequest("Materijal s tim nazivom već postoji.");
        }

        if (!await _context.MaterialUnits.AnyAsync(x => x.Id == dto.MaterialUnitId))
        {
            return BadRequest("Odabrana mjerna jedinica ne postoji.");
        }

        var material = new Material
        {
            Name = name,
            Code = dto.Code?.Trim(),
            MaterialUnitId = dto.MaterialUnitId,
            DefaultUnitPrice = dto.DefaultUnitPrice,
            IsActive = dto.IsActive
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        var created = await _context.Materials
            .AsNoTracking()
            .Where(x => x.Id == material.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = material.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveMaterialDto dto)
    {
        var material = await _context.Materials.FindAsync(id);

        if (material is null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();

        if (await _context.Materials.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower()))
        {
            return BadRequest("Drugi materijal već koristi taj naziv.");
        }

        if (!await _context.MaterialUnits.AnyAsync(x => x.Id == dto.MaterialUnitId))
        {
            return BadRequest("Odabrana mjerna jedinica ne postoji.");
        }

        material.Name = name;
        material.Code = dto.Code?.Trim();
        material.MaterialUnitId = dto.MaterialUnitId;
        material.DefaultUnitPrice = dto.DefaultUnitPrice;
        material.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await _context.Materials.FindAsync(id);

        if (material is null)
        {
            return NotFound();
        }

        // utrošeni materijal ostaje u evidenciji naloga
        if (await _context.InterventionMaterials.AnyAsync(x => x.MaterialId == id))
        {
            return BadRequest("Materijal je utrošen na intervencijama pa se ne može obrisati. Označite ga kao neaktivan.");
        }

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

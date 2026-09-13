using System.Linq.Expressions;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public LocationsController(KvaroviDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<Location, LocationDto>> ToDto = x => new LocationDto
    {
        Id = x.Id,
        Name = x.Name,
        Address = x.Address,
        City = x.City,
        Note = x.Note,
        IsActive = x.IsActive,
        LocationTypeId = x.LocationTypeId,
        LocationTypeName = x.LocationType!.Name,
        OpenFaultCount = x.FaultReports.Count(f => f.FaultStatusId != FaultStatusIds.Zatvoreno),
        EmployeeCount = x.Employees.Count
    };

    /// <summary>
    /// Popis lokacija. Čitanje je dostupno svim prijavljenima jer i prijavitelj
    /// mora vidjeti lokaciju kad prijavljuje kvar.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll(
        [FromQuery] bool onlyActive = false,
        [FromQuery] int? locationTypeId = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Locations.AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (locationTypeId.HasValue)
        {
            query = query.Where(x => x.LocationTypeId == locationTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term) ||
                x.City.ToLower().Contains(term) ||
                x.Address.ToLower().Contains(term));
        }

        var locations = await query
            .OrderBy(x => x.City)
            .ThenBy(x => x.Name)
            .Select(ToDto)
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var location = await _context.Locations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (location is null)
        {
            return NotFound();
        }

        return Ok(location);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(SaveLocationDto dto)
    {
        if (!await _context.LocationTypes.AnyAsync(x => x.Id == dto.LocationTypeId))
        {
            return BadRequest("Odabrana vrsta lokacije ne postoji.");
        }

        var location = new Location
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Note = dto.Note?.Trim(),
            IsActive = dto.IsActive,
            LocationTypeId = dto.LocationTypeId
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var created = await _context.Locations
            .AsNoTracking()
            .Where(x => x.Id == location.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = location.Id }, created);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveLocationDto dto)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        if (!await _context.LocationTypes.AnyAsync(x => x.Id == dto.LocationTypeId))
        {
            return BadRequest("Odabrana vrsta lokacije ne postoji.");
        }

        // lokacija s otvorenim kvarovima ne smije se deaktivirati -
        // izvršitelji bi ostali s nalozima na lokaciji izvan upotrebe
        if (location.IsActive && !dto.IsActive)
        {
            var hasOpenFaults = await _context.FaultReports
                .AnyAsync(x => x.LocationId == id && x.FaultStatusId != FaultStatusIds.Zatvoreno);

            if (hasOpenFaults)
            {
                return BadRequest("Lokacija ima otvorene kvarove pa se ne može označiti neaktivnom.");
            }
        }

        location.Name = dto.Name.Trim();
        location.Address = dto.Address.Trim();
        location.City = dto.City.Trim();
        location.Note = dto.Note?.Trim();
        location.IsActive = dto.IsActive;
        location.LocationTypeId = dto.LocationTypeId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        // lokacija s prijavama se ne briše - povijest kvarova mora ostati
        if (await _context.FaultReports.AnyAsync(x => x.LocationId == id))
        {
            return BadRequest("Lokacija ima prijavljene kvarove pa se ne može obrisati. Označite je kao neaktivnu.");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

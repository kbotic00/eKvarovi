using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

/// <summary>
/// Šifarnici za padajuće izbornike. Dostupni su svim prijavljenim
/// korisnicima jer bez njih nijedan obrazac ne može raditi.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public LookupsController(KvaroviDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Svi šifarnici odjednom. Obrasci tako jednim pozivom napune
    /// sve izbornike umjesto da rade deset zasebnih zahtjeva.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<AllLookupsDto>> GetAll()
    {
        var result = new AllLookupsDto
        {
            LocationTypes = await _context.LocationTypes
                .AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync(),

            FaultTypes = await _context.FaultTypes
                .AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync(),

            FaultPriorities = await _context.FaultPriorities
                .AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
                .Select(x => new PriorityLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Level = x.Level,
                    RequiresDeadline = x.RequiresDeadline
                }).ToListAsync(),

            FaultStatuses = await _context.FaultStatuses
                .AsNoTracking().OrderBy(x => x.SortOrder)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync(),

            InterventionStatuses = await _context.InterventionStatuses
                .AsNoTracking().OrderBy(x => x.SortOrder)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync(),

            MaterialUnits = await _context.MaterialUnits
                .AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync(),

            AttachmentPurposes = await _context.AttachmentPurposes
                .AsNoTracking().OrderBy(x => x.SortOrder)
                .Select(x => new AttachmentPurposeLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    AllowsDocuments = x.AllowsDocuments
                }).ToListAsync(),

            // samo aktivne lokacije - na neaktivne se ne smiju unositi prijave
            Locations = await _context.Locations
                .AsNoTracking().Where(x => x.IsActive)
                .OrderBy(x => x.City).ThenBy(x => x.Name)
                .Select(x => new LookupDto { Id = x.Id, Name = x.Name + " (" + x.City + ")" })
                .ToListAsync(),

            Technicians = await _context.Employees
                .AsNoTracking().Where(x => x.IsTechnician && x.IsActive)
                .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
                .Select(x => new LookupDto
                {
                    Id = x.Id,
                    Name = x.FirstName + " " + x.LastName +
                           (x.JobTitle != null ? " - " + x.JobTitle : "")
                })
                .ToListAsync(),

            Materials = await _context.Materials
                .AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
                .Select(x => new MaterialLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UnitAbbreviation = x.MaterialUnit!.Abbreviation,
                    DefaultUnitPrice = x.DefaultUnitPrice
                })
                .ToListAsync()
        };

        return Ok(result);
    }

    [HttpGet("location-types")]
    public async Task<ActionResult<List<LookupDto>>> GetLocationTypes() =>
        Ok(await _context.LocationTypes.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync());

    [HttpGet("fault-types")]
    public async Task<ActionResult<List<LookupDto>>> GetFaultTypes() =>
        Ok(await _context.FaultTypes.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync());

    [HttpGet("fault-priorities")]
    public async Task<ActionResult<List<PriorityLookupDto>>> GetFaultPriorities() =>
        Ok(await _context.FaultPriorities.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PriorityLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level,
                RequiresDeadline = x.RequiresDeadline
            }).ToListAsync());

    [HttpGet("fault-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetFaultStatuses() =>
        Ok(await _context.FaultStatuses.AsNoTracking().OrderBy(x => x.SortOrder)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync());

    [HttpGet("intervention-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetInterventionStatuses() =>
        Ok(await _context.InterventionStatuses.AsNoTracking().OrderBy(x => x.SortOrder)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync());

    [HttpGet("material-units")]
    public async Task<ActionResult<List<LookupDto>>> GetMaterialUnits() =>
        Ok(await _context.MaterialUnits.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync());

    [HttpGet("attachment-purposes")]
    public async Task<ActionResult<List<AttachmentPurposeLookupDto>>> GetAttachmentPurposes() =>
        Ok(await _context.AttachmentPurposes.AsNoTracking().OrderBy(x => x.SortOrder)
            .Select(x => new AttachmentPurposeLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                AllowsDocuments = x.AllowsDocuments
            }).ToListAsync());

    [HttpGet("locations")]
    public async Task<ActionResult<List<LookupDto>>> GetLocations() =>
        Ok(await _context.Locations.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.City).ThenBy(x => x.Name)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name + " (" + x.City + ")" })
            .ToListAsync());

    [HttpGet("employees")]
    public async Task<ActionResult<List<LookupDto>>> GetEmployees([FromQuery] bool onlyTechnicians = false)
    {
        var query = _context.Employees.AsNoTracking().Where(x => x.IsActive);

        if (onlyTechnicians)
        {
            query = query.Where(x => x.IsTechnician);
        }

        return Ok(await query
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new LookupDto
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName +
                       (x.JobTitle != null ? " - " + x.JobTitle : "")
            })
            .ToListAsync());
    }

    [HttpGet("materials")]
    public async Task<ActionResult<List<MaterialLookupDto>>> GetMaterials() =>
        Ok(await _context.Materials.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new MaterialLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                UnitAbbreviation = x.MaterialUnit!.Abbreviation,
                DefaultUnitPrice = x.DefaultUnitPrice
            })
            .ToListAsync());
}

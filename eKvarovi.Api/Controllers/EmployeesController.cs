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
public class EmployeesController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public EmployeesController(KvaroviDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Popis djelatnika. Čitanje je dostupno svima prijavljenima jer voditelj
    /// bira servisera, a sučelje prikazuje imena prijavitelja.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll(
        [FromQuery] bool? onlyTechnicians = null,
        [FromQuery] int? locationId = null)
    {
        var query = _context.Employees.AsNoTracking();

        if (onlyTechnicians == true)
        {
            query = query.Where(x => x.IsTechnician);
        }

        if (locationId.HasValue)
        {
            query = query.Where(x => x.LocationId == locationId.Value);
        }

        var employees = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Phone = x.Phone,
                JobTitle = x.JobTitle,
                IsTechnician = x.IsTechnician,
                IsActive = x.IsActive,
                LocationId = x.LocationId,
                LocationName = x.Location != null ? x.Location.Name : null,
                ActiveAssignmentCount = x.Assignments.Count(a =>
                    a.IsActive &&
                    a.FaultReport!.FaultStatusId != FaultStatusIds.Zatvoreno)
            })
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>Skraćeni popis za padajuće izbornike.</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetLookup([FromQuery] bool onlyTechnicians = false)
    {
        var query = _context.Employees.AsNoTracking().Where(x => x.IsActive);

        if (onlyTechnicians)
        {
            query = query.Where(x => x.IsTechnician);
        }

        var items = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new LookupDto
            {
                Id = x.Id,
                Name = x.FirstName + " " + x.LastName +
                       (x.JobTitle != null ? " - " + x.JobTitle : "")
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EmployeeDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Phone = x.Phone,
                JobTitle = x.JobTitle,
                IsTechnician = x.IsTechnician,
                IsActive = x.IsActive,
                LocationId = x.LocationId,
                LocationName = x.Location != null ? x.Location.Name : null,
                ActiveAssignmentCount = x.Assignments.Count(a =>
                    a.IsActive &&
                    a.FaultReport!.FaultStatusId != FaultStatusIds.Zatvoreno)
            })
            .FirstOrDefaultAsync();

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(SaveEmployeeDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Employees.AnyAsync(x => x.Email.ToLower() == email))
        {
            return BadRequest("Djelatnik s tim e-mailom već postoji.");
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            JobTitle = dto.JobTitle?.Trim(),
            IsTechnician = dto.IsTechnician,
            IsActive = dto.IsActive,
            LocationId = dto.LocationId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            JobTitle = employee.JobTitle,
            IsTechnician = employee.IsTechnician,
            IsActive = employee.IsActive,
            LocationId = employee.LocationId
        });
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Employees.AnyAsync(x => x.Id != id && x.Email.ToLower() == email))
        {
            return BadRequest("Drugi djelatnik već koristi taj e-mail.");
        }

        // serviser s aktivnim dodjelama ne smije izgubiti status servisera
        if (employee.IsTechnician && !dto.IsTechnician)
        {
            var hasActiveWork = await _context.WorkAssignments
                .AnyAsync(x => x.TechnicianId == id && x.IsActive);

            if (hasActiveWork)
            {
                return BadRequest("Djelatnik ima aktivne dodjele pa mu se ne može ukloniti oznaka servisera.");
            }
        }

        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.Email = email;
        employee.Phone = dto.Phone?.Trim();
        employee.JobTitle = dto.JobTitle?.Trim();
        employee.IsTechnician = dto.IsTechnician;
        employee.IsActive = dto.IsActive;
        employee.LocationId = dto.LocationId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        if (await _context.FaultReports.AnyAsync(x => x.ReporterId == id))
        {
            return BadRequest("Djelatnik ima prijavljene kvarove pa se ne može obrisati. Označite ga kao neaktivnog.");
        }

        if (await _context.WorkAssignments.AnyAsync(x => x.TechnicianId == id))
        {
            return BadRequest("Djelatnik ima dodjele u povijesti pa se ne može obrisati. Označite ga kao neaktivnog.");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

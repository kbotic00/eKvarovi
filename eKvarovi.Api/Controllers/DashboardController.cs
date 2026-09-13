using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly KvaroviDbContext _context;

    public DashboardController(KvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var today = DateTime.Now.Date;
        var firstOfMonth = new DateTime(today.Year, today.Month, 1);

        var reports = _context.FaultReports.AsNoTracking();

        // grupiranja se rade u bazi, a zbrajanje nad malim skupom u memoriji
        var byStatus = await reports
            .GroupBy(x => new { x.FaultStatusId, x.FaultStatus!.Name, x.FaultStatus.SortOrder })
            .Select(g => new { g.Key.FaultStatusId, g.Key.Name, g.Key.SortOrder, Count = g.Count() })
            .ToListAsync();

        var byPriority = await reports
            .Where(x => x.FaultPriorityId != null)
            .GroupBy(x => new { x.FaultPriority!.Name, x.FaultPriority.Level })
            .Select(g => new { g.Key.Name, g.Key.Level, Count = g.Count() })
            .ToListAsync();

        var byFaultType = await reports
            .Where(x => x.FaultTypeId != null)
            .GroupBy(x => x.FaultType!.Name)
            .Select(g => new CountByNameDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var topLocations = await reports
            .Where(x => x.FaultStatusId != FaultStatusIds.Zatvoreno)
            .GroupBy(x => x.Location!.Name)
            .Select(g => new CountByNameDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // prosječno vrijeme rješavanja - od prijave do zatvaranja.
        // Razlika se računa u memoriji jer SQLite nema funkciju za razliku datuma.
        var closedDates = await reports
            .Where(x => x.ClosedAt != null)
            .Select(x => new { x.ReportedAt, ClosedAt = x.ClosedAt!.Value })
            .ToListAsync();

        var closedDurations = closedDates
            .Select(x => (x.ClosedAt - x.ReportedAt).TotalDays)
            .ToList();

        var recent = await reports
            .OrderByDescending(x => x.ReportedAt)
            .Take(5)
            .Select(x => new FaultReportDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ReportedAt = x.ReportedAt,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                LocationCity = x.Location.City,
                ReporterId = x.ReporterId,
                ReporterName = x.Reporter!.FirstName + " " + x.Reporter.LastName,
                FaultTypeName = x.FaultType != null ? x.FaultType.Name : null,
                FaultPriorityId = x.FaultPriorityId,
                FaultPriorityName = x.FaultPriority != null ? x.FaultPriority.Name : null,
                PriorityLevel = x.FaultPriority != null ? x.FaultPriority.Level : 0,
                Deadline = x.Deadline,
                FaultStatusId = x.FaultStatusId,
                FaultStatusName = x.FaultStatus!.Name,
                IsClosed = x.FaultStatus.IsClosed,
                CurrentTechnicianName = x.Assignments
                    .Where(a => a.IsActive)
                    .Select(a => a.Technician!.FirstName + " " + a.Technician.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var minutesThisMonth = await _context.Interventions
            .AsNoTracking()
            .Where(x => x.StartedAt >= firstOfMonth)
            .SumAsync(x => (int?)x.DurationMinutes) ?? 0;

        var summary = new DashboardSummaryDto
        {
            TotalReports = byStatus.Sum(x => x.Count),

            OpenReports = byStatus
                .Where(x => x.FaultStatusId != FaultStatusIds.Zatvoreno)
                .Sum(x => x.Count),

            ResolvedAwaitingClosure = byStatus
                .Where(x => x.FaultStatusId == FaultStatusIds.Rijeseno)
                .Sum(x => x.Count),

            UnassignedReports = await reports
                .CountAsync(x => x.FaultStatusId != FaultStatusIds.Zatvoreno &&
                                 !x.Assignments.Any(a => a.IsActive)),

            CriticalOpenReports = await reports
                .CountAsync(x => x.FaultPriorityId == FaultPriorityIds.Kritican &&
                                 x.FaultStatusId != FaultStatusIds.Zatvoreno),

            OverdueReports = await reports
                .CountAsync(x => x.FaultStatusId != FaultStatusIds.Zatvoreno &&
                                 x.Deadline != null &&
                                 x.Deadline < today),

            ActiveInterventions = await _context.Interventions
                .CountAsync(x => !x.InterventionStatus!.IsFinal),

            ClosedThisMonth = await reports
                .CountAsync(x => x.ClosedAt != null && x.ClosedAt >= firstOfMonth),

            AverageResolutionDays = closedDurations.Count == 0
                ? 0
                : Math.Round(closedDurations.Average(), 1),

            TotalMinutesThisMonth = minutesThisMonth,

            ByStatus = byStatus
                .OrderBy(x => x.SortOrder)
                .Select(x => new CountByNameDto { Name = x.Name, Count = x.Count })
                .ToList(),

            ByPriority = byPriority
                .OrderByDescending(x => x.Level)
                .Select(x => new CountByNameDto { Name = x.Name, Count = x.Count })
                .ToList(),

            ByFaultType = byFaultType,
            TopLocations = topLocations,
            RecentReports = recent
        };

        await AddPersonalCountsAsync(summary);

        return Ok(summary);
    }

    /// <summary>
    /// Osobne brojke prijavljenog korisnika. Djelatnik se čita iz tokena,
    /// pa nitko ne može zatražiti tuđu statistiku.
    /// </summary>
    private async Task AddPersonalCountsAsync(DashboardSummaryDto summary)
    {
        var employeeId = User.GetEmployeeId();

        if (employeeId is null)
        {
            return;
        }

        summary.MyReportCount = await _context.FaultReports
            .CountAsync(x => x.ReporterId == employeeId.Value);

        summary.MyOpenReportCount = await _context.FaultReports
            .CountAsync(x => x.ReporterId == employeeId.Value &&
                             x.FaultStatusId != FaultStatusIds.Zatvoreno);

        summary.MyActiveAssignmentCount = await _context.WorkAssignments
            .CountAsync(x => x.TechnicianId == employeeId.Value &&
                             x.IsActive &&
                             x.FaultReport!.FaultStatusId != FaultStatusIds.Zatvoreno);

        summary.MyInterventionCount = await _context.Interventions
            .CountAsync(x => x.WorkAssignment!.TechnicianId == employeeId.Value);
    }
}

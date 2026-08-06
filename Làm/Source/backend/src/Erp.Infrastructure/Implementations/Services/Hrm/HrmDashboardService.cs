using System.Globalization;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmDashboardService : IHrmDashboardService
{
    private readonly AppDbContext _db;
    private readonly IHrmHeadcountService _headcount;
    private readonly IHrmPayrollService _payroll;

    public HrmDashboardService(AppDbContext db, IHrmHeadcountService headcount, IHrmPayrollService payroll)
    {
        _db = db;
        _headcount = headcount;
        _payroll = payroll;
    }

    public async Task<HrmDashboardHeadcountDto> HeadcountAsync(Guid tenantId, CancellationToken ct = default)
    {
        // Include soft-deleted for left stats; active roster excludes IsDeleted.
        var active = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Status, x.OrgUnitId, x.HireDate, x.TerminateDate })
            .ToListAsync(ct);
        var left = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsDeleted)
            .Select(x => new { x.Status, x.TerminateDate, x.HireDate })
            .ToListAsync(ct);

        var byStatus = active.GroupBy(x => x.Status)
            .Select(g => new HrmHeadcountByStatusDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var orgIds = active.Select(x => x.OrgUnitId).Distinct().ToList();
        var orgNames = await _db.OrgUnits.AsNoTracking()
            .Where(x => orgIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var byOrg = active.GroupBy(x => x.OrgUnitId)
            .Select(g => new HrmHeadcountByOrgDto(
                g.Key, orgNames.GetValueOrDefault(g.Key, ""), g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var movements = new List<HrmHeadcountMovementDto>();
        for (var i = 5; i >= 0; i--)
        {
            var monthStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var key = monthStart.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var hired = active.Count(x => x.HireDate is DateOnly h && h >= monthStart && h <= monthEnd)
                        + left.Count(x => x.HireDate is DateOnly h && h >= monthStart && h <= monthEnd);
            var resigned = left.Count(x => x.TerminateDate is DateOnly t && t >= monthStart && t <= monthEnd)
                           + active.Count(x => x.TerminateDate is DateOnly t && t >= monthStart && t <= monthEnd
                                               && (x.Status is "Resigned" or "Terminated" or "Inactive"));
            movements.Add(new HrmHeadcountMovementDto(key, hired, resigned, hired - resigned));
        }

        var totalActive = active.Count(x => x.Status is "Active");
        var totalProb = active.Count(x => x.Status is "Probation");
        var totalLeft = left.Count + active.Count(x => x.Status is "Resigned" or "Terminated" or "Inactive");

        return new HrmDashboardHeadcountDto(totalActive, totalProb, totalLeft, byStatus, byOrg, movements);
    }

    public async Task<IReadOnlyList<HrmAttendanceReportRowDto>> AttendanceReportAsync(
        Guid tenantId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var f = from ?? new DateOnly(today.Year, today.Month, 1);
        var t = to ?? today;

        var rows = await (
            from r in _db.AttendanceRecords.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on r.EmployeeId equals e.Id
            where r.TenantId == tenantId && !r.IsDeleted && r.WorkDate >= f && r.WorkDate <= t
            select new { e.OrgUnitId, r.WorkUnit, r.OtMinutes, r.LateMinutes }
        ).ToListAsync(ct);

        var orgIds = rows.Select(x => x.OrgUnitId).Distinct().ToList();
        var orgNames = await _db.OrgUnits.AsNoTracking()
            .Where(x => orgIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.GroupBy(x => x.OrgUnitId)
            .Select(g => new HrmAttendanceReportRowDto(
                g.Key, orgNames.GetValueOrDefault(g.Key, ""), g.Count(),
                g.Sum(x => x.WorkUnit), g.Sum(x => x.OtMinutes), g.Sum(x => x.LateMinutes),
                g.Count(x => x.LateMinutes > 0)))
            .OrderByDescending(x => x.WorkUnits)
            .ToList();
    }

    public async Task<IReadOnlyList<HrmRecruitFunnelRowDto>> RecruitFunnelAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var statuses = await _db.Candidates.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => x.PipelineStatus)
            .ToListAsync(ct);
        return statuses.GroupBy(x => x)
            .Select(g => new HrmRecruitFunnelRowDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public async Task<IReadOnlyList<HrmLeaveSummaryRowDto>> LeaveSummaryAsync(
        Guid tenantId, int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var rows = await (
            from b in _db.LeaveBalances.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on b.EmployeeId equals e.Id
            where b.TenantId == tenantId && !b.IsDeleted && b.Year == y && !e.IsDeleted
            select new { e.OrgUnitId, e.Id, b.Entitled, b.Used, b.Remaining }
        ).ToListAsync(ct);

        var orgIds = rows.Select(x => x.OrgUnitId).Distinct().ToList();
        var orgNames = await _db.OrgUnits.AsNoTracking()
            .Where(x => orgIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.GroupBy(x => x.OrgUnitId)
            .Select(g =>
            {
                var byEmp = g.GroupBy(x => x.Id).Select(eg => new
                {
                    Entitled = eg.Sum(x => x.Entitled),
                    Used = eg.Sum(x => x.Used),
                    Remaining = eg.Sum(x => x.Remaining)
                }).ToList();
                return new HrmLeaveSummaryRowDto(
                    g.Key, orgNames.GetValueOrDefault(g.Key, ""),
                    byEmp.Sum(x => x.Entitled), byEmp.Sum(x => x.Used), byEmp.Sum(x => x.Remaining), byEmp.Count);
            })
            .OrderByDescending(x => x.Used)
            .ToList();
    }

    public async Task<HrmCostSummaryDto> CostSummaryAsync(
        Guid tenantId, Guid? periodId, CancellationToken ct = default)
    {
        Guid? pid = periodId;
        string? key = null;
        string? status = null;
        if (pid is null)
        {
            var latest = await _db.PayrollPeriods.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .OrderByDescending(x => x.PeriodKey)
                .Select(x => new { x.Id, x.PeriodKey, x.Status })
                .FirstOrDefaultAsync(ct);
            if (latest is null)
                return new HrmCostSummaryDto(null, null, null, 0, 0, 0, 0, Array.Empty<PayrollCostByOrgDto>());
            pid = latest.Id;
            key = latest.PeriodKey;
            status = latest.Status;
        }
        else
        {
            var p = await _db.PayrollPeriods.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == pid && x.TenantId == tenantId && !x.IsDeleted, ct);
            key = p?.PeriodKey;
            status = p?.Status;
        }

        var byOrg = await _payroll.CostByOrgAsync(tenantId, pid.Value, ct);
        return new HrmCostSummaryDto(
            pid, key, status,
            byOrg.Sum(x => x.Gross), byOrg.Sum(x => x.Net), byOrg.Sum(x => x.Insurance),
            byOrg.Sum(x => x.Headcount), byOrg);
    }

    public Task<IReadOnlyList<HeadcountCompareRowDto>> HeadcountVsPlanAsync(
        Guid tenantId, CancellationToken ct = default)
        => _headcount.CompareAsync(tenantId, ct);

    public async Task<HrmDashboardBundleDto> BundleAsync(
        Guid tenantId, DateOnly? attFrom, DateOnly? attTo, int? leaveYear, Guid? periodId, CancellationToken ct = default)
    {
        var hc = await HeadcountAsync(tenantId, ct);
        var att = await AttendanceReportAsync(tenantId, attFrom, attTo, ct);
        var funnel = await RecruitFunnelAsync(tenantId, ct);
        var leave = await LeaveSummaryAsync(tenantId, leaveYear, ct);
        var cost = await CostSummaryAsync(tenantId, periodId, ct);
        var plan = await HeadcountVsPlanAsync(tenantId, ct);
        return new HrmDashboardBundleDto(hc, att, funnel, leave, cost, plan);
    }
}

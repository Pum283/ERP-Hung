using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pjm;
using Erp.Application.Interfaces.Services.Pjm;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pjm;

public sealed class PjmReportService : IPjmReportService
{
    private readonly AppDbContext _db;
    public PjmReportService(AppDbContext db) => _db = db;

    public async Task<PjmDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var projects = await _db.PjmProjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var wbs = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var active = projects.Where(x => x.StatusCode is "Active" or "OnHold").ToList();
        var healthRows = BuildPortfolio(projects, wbs, now);
        var overdueProj = healthRows.Count(x => x.Health == "Late");
        var overdueWbs = wbs.Count(IsOverdue(now));
        var overdueMs = wbs.Count(x => x.IsMilestone && IsOverdue(now)(x));
        var activeHealth = healthRows.Where(x => active.Any(a => a.Id == x.ProjectId)).ToList();
        var avg = activeHealth.Count == 0 ? 0
            : decimal.Round(activeHealth.Average(x => x.ProgressPercent), 1);

        return new PjmDashboardDto(
            active.Count,
            projects.Count(x => x.StatusCode == "Draft"),
            projects.Count(x => x.StatusCode is "Closed" or "Cancelled" or "Completed"),
            overdueProj, overdueWbs, overdueMs, avg);
    }

    public async Task<IReadOnlyList<PjmPortfolioRowDto>> PortfolioAsync(Guid tenantId, CancellationToken ct = default)
    {
        var projects = await _db.PjmProjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.StatusCode == "Active" || x.StatusCode == "OnHold" || x.StatusCode == "Draft"))
            .OrderBy(x => x.Code).ToListAsync(ct);
        var ids = projects.Select(x => x.Id).ToList();
        var wbs = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.ProjectId))
            .ToListAsync(ct);
        return BuildPortfolio(projects, wbs, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<PjmProgressHealthRowDto>> ProgressHealthAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var projects = await _db.PjmProjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        var wbs = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return projects.Select(p =>
        {
            var items = wbs.Where(x => x.ProjectId == p.Id).ToList();
            var progress = CalcProgress(items);
            var overdue = items.Where(IsOverdue(now)).ToList();
            var health = CalcHealth(p, progress, overdue.Count, now);
            return new PjmProgressHealthRowDto(
                p.Id, p.Code, p.Name, p.StatusCode, progress, health,
                items.Count(x => x.Status is "Open" or "InProgress"),
                items.Count(x => x.Status == "Done" || x.PercentComplete >= 100),
                overdue.Count, overdue.Count(x => x.IsMilestone),
                p.EndDate,
                p.EndDate is DateTimeOffset e && e < now && p.StatusCode is "Active" or "OnHold" && progress < 100);
        }).ToList();
    }

    public async Task<IReadOnlyList<PjmOverdueRowDto>> OverdueAsync(Guid tenantId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var wbs = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.DueDate != null)
            .ToListAsync(ct);
        var overdue = wbs.Where(IsOverdue(now)).OrderBy(x => x.DueDate).Take(300).ToList();
        if (overdue.Count == 0) return Array.Empty<PjmOverdueRowDto>();
        var pids = overdue.Select(x => x.ProjectId).Distinct().ToList();
        var projects = await _db.PjmProjects.AsNoTracking().Where(x => pids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return overdue.Select(x =>
        {
            projects.TryGetValue(x.ProjectId, out var p);
            return new PjmOverdueRowDto(
                x.ProjectId, p?.Code ?? "", p?.Name ?? "",
                x.Id, x.Code, x.Name, x.IsMilestone, x.DueDate!.Value,
                x.PercentComplete, x.AssigneeName);
        }).ToList();
    }

    public async Task<string> ExportCsvAsync(Guid tenantId, string report, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "dashboard")
        {
            var d = await DashboardAsync(tenantId, ct);
            sb.AppendLine("Active,Draft,Closed,OverdueProjects,OverdueWbs,OverdueMilestones,AvgActiveProgress");
            sb.AppendLine($"{d.ActiveCount},{d.DraftCount},{d.ClosedCount},{d.OverdueProjectCount},{d.OverdueWbsCount},{d.OverdueMilestoneCount},{N(d.AvgActiveProgressPercent)}");
            return sb.ToString();
        }
        if (kind is "portfolio" or "038")
        {
            var rows = await PortfolioAsync(tenantId, ct);
            sb.AppendLine("Code,Name,Status,PM,Budget,Progress,Health,Wbs,Overdue,Milestones,EndDate");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.Name)},{Csv(r.StatusCode)},{Csv(r.PmName)},{N(r.Budget)},{N(r.ProgressPercent)},{Csv(r.Health)},{r.WbsCount},{r.OverdueCount},{r.MilestoneCount},{r.EndDate:yyyy-MM-dd}");
            return sb.ToString();
        }
        if (kind is "progress" or "health" or "039")
        {
            var rows = await ProgressHealthAsync(tenantId, ct);
            sb.AppendLine("Code,Name,Status,Progress,Health,OpenWbs,DoneWbs,OverdueWbs,OverdueMilestones,EndDate,ProjectEndOverdue");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.Name)},{Csv(r.StatusCode)},{N(r.ProgressPercent)},{Csv(r.Health)},{r.OpenWbs},{r.DoneWbs},{r.OverdueWbs},{r.OverdueMilestones},{r.EndDate:yyyy-MM-dd},{r.ProjectEndOverdue}");
            return sb.ToString();
        }
        if (kind is "overdue" or "017")
        {
            var rows = await OverdueAsync(tenantId, ct);
            sb.AppendLine("Project,Wbs,Milestone,DueDate,Percent,Assignee");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.ProjectCode)},{Csv(r.WbsCode)},{r.IsMilestone},{r.DueDate:yyyy-MM-dd},{N(r.PercentComplete)},{Csv(r.AssigneeName)}");
            return sb.ToString();
        }
        if (kind is "profit" or "040" or "budget" or "023")
        {
            var rows = await ProfitAsync(tenantId, ct);
            sb.AppendLine("Code,Name,Status,Budget,ActualCost,Revenue,Margin,MarginPct,BudgetVariance,OverBudget");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.Name)},{Csv(r.StatusCode)},{N(r.Budget)},{N(r.ActualCost)},{N(r.RecognizedRevenue)},{N(r.Margin)},{N(r.MarginPct)},{N(r.BudgetVariance)},{r.OverBudget}");
            return sb.ToString();
        }
        throw new AppException("report: dashboard | portfolio | progress | overdue | profit.");
    }

    public async Task<IReadOnlyList<PjmProfitRowDto>> ProfitAsync(Guid tenantId, CancellationToken ct = default)
    {
        var projects = await _db.PjmProjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        if (projects.Count == 0) return Array.Empty<PjmProfitRowDto>();
        var ids = projects.Select(x => x.Id).ToList();
        var expense = await _db.PjmExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted && x.Status == "Posted")
            .GroupBy(x => x.ProjectId)
            .Select(g => new { g.Key, S = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.S, ct);
        var issues = await _db.PjmMaterialIssues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted && x.Status == "Posted")
            .Select(x => new { x.Id, x.ProjectId }).ToListAsync(ct);
        var issueIds = issues.Select(x => x.Id).ToList();
        var lineCost = issueIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _db.PjmMaterialIssueLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && issueIds.Contains(x.MaterialIssueId) && !x.IsDeleted)
                .GroupBy(x => x.MaterialIssueId)
                .Select(g => new { g.Key, S = g.Sum(x => x.Qty * x.UnitCost) })
                .ToDictionaryAsync(x => x.Key, x => x.S, ct);
        var mat = issues.GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => g.Sum(i => lineCost.GetValueOrDefault(i.Id)));

        return projects.Select(p =>
        {
            var actual = decimal.Round(expense.GetValueOrDefault(p.Id) + mat.GetValueOrDefault(p.Id), 2);
            var margin = decimal.Round(p.RecognizedRevenue - actual, 2);
            var marginPct = p.RecognizedRevenue == 0 ? 0
                : decimal.Round(100m * margin / p.RecognizedRevenue, 1);
            var variance = decimal.Round(p.Budget - actual, 2);
            return new PjmProfitRowDto(
                p.Id, p.Code, p.Name, p.StatusCode, p.Budget, actual,
                p.RecognizedRevenue, margin, marginPct, variance, actual > p.Budget && p.Budget > 0);
        }).ToList();
    }

    private static List<PjmPortfolioRowDto> BuildPortfolio(
        List<PjmProject> projects, List<PjmWbsItem> wbs, DateTimeOffset now) =>
        projects.Select(p =>
        {
            var items = wbs.Where(x => x.ProjectId == p.Id).ToList();
            var progress = CalcProgress(items);
            var overdue = items.Count(IsOverdue(now));
            return new PjmPortfolioRowDto(
                p.Id, p.Code, p.Name, p.StatusCode, p.PmName, p.Budget,
                p.StartDate, p.EndDate, progress, CalcHealth(p, progress, overdue, now),
                items.Count, overdue, items.Count(x => x.IsMilestone));
        }).ToList();

    private static decimal CalcProgress(List<PjmWbsItem> items)
    {
        if (items.Count == 0) return 0;
        var leaves = items.Where(x => items.All(c => c.ParentItemId != x.Id)).ToList();
        var set = leaves.Count > 0 ? leaves : items;
        return decimal.Round(set.Average(x => x.PercentComplete), 1);
    }

    private static string CalcHealth(PjmProject p, decimal progress, int overdueCount, DateTimeOffset now)
    {
        if (p.StatusCode is "Closed" or "Completed" or "Cancelled") return "Done";
        if (overdueCount > 0 || (p.EndDate is DateTimeOffset e && e < now && progress < 100))
            return "Late";
        if (p.EndDate is DateTimeOffset end)
        {
            var totalDays = p.StartDate is DateTimeOffset s
                ? Math.Max(1, (end - s).TotalDays)
                : 30;
            var elapsed = Math.Max(0, (now - (p.StartDate ?? end.AddDays(-totalDays))).TotalDays);
            var expected = 100m * (decimal)(elapsed / totalDays);
            if (progress + 15 < expected) return "AtRisk";
        }
        return "OnTrack";
    }

    private static Func<PjmWbsItem, bool> IsOverdue(DateTimeOffset now) =>
        x => x.DueDate is DateTimeOffset d && d < now
             && x.Status is not ("Done" or "Cancelled")
             && x.PercentComplete < 100;

    private static string Csv(string? s)
    {
        var v = s ?? "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static string N(decimal n) => n.ToString(CultureInfo.InvariantCulture);
}

using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fsm;
using Erp.Application.Interfaces.Services.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fsm;

public sealed class FsmReportService : IFsmReportService
{
    private readonly AppDbContext _db;
    public FsmReportService(AppDbContext db) => _db = db;

    public async Task<FsmDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmTickets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var today = now.UtcDateTime.Date;
        int C(string s) => list.Count(x => x.Status == s);
        var openish = list.Where(x => x.Status is "Open" or "Assigned" or "InProgress" or "Escalated").ToList();
        var overdue = openish.Count(x => x.DueResolveAt is DateTimeOffset d && d < now);
        var closedToday = list.Count(x => x.ClosedAt is DateTimeOffset c && c.UtcDateTime.Date == today);
        var closed = list.Where(x => x.Status == "Closed" && x.SlaResolveMet.HasValue).ToList();
        var hit = closed.Count == 0 ? 0
            : decimal.Round(100m * closed.Count(x => x.SlaResolveMet == true) / closed.Count, 1);
        var apptToday = list.Count(x => x.AppointmentAt is DateTimeOffset a && a.UtcDateTime.Date == today
                                         && x.Status is not ("Closed" or "Cancelled"));

        return new FsmDashboardDto(
            C("Open"), C("Assigned"), C("InProgress"), C("Escalated"),
            C("Resolved"), C("Closed"), overdue, closedToday, hit, apptToday);
    }

    public async Task<IReadOnlyList<FsmSlaComplianceRowDto>> SlaComplianceAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmTickets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var priorities = new[] { "Critical", "High", "Normal", "Low" };
        return priorities.Select(p =>
        {
            var rows = list.Where(x => x.Priority == p).ToList();
            var open = rows.Where(x => x.Status is "Open" or "Assigned" or "InProgress" or "Escalated").ToList();
            var closed = rows.Where(x => x.Status == "Closed").ToList();
            var met = closed.Count(x => x.SlaResolveMet == true);
            var miss = closed.Count(x => x.SlaResolveMet == false);
            var hit = closed.Count == 0 ? 0 : decimal.Round(100m * met / closed.Count, 1);
            return new FsmSlaComplianceRowDto(
                p, open.Count,
                open.Count(x => x.DueResolveAt is DateTimeOffset d && d < now),
                closed.Count, met, miss, hit);
        }).ToList();
    }

    public async Task<IReadOnlyList<FsmTechProductivityRowDto>> TechProductivityAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmTickets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.AssignedTechUserId != null)
            .ToListAsync(ct);
        return list.GroupBy(x => new { x.AssignedTechUserId, Name = x.AssignedTechName ?? "—" })
            .Select(g =>
            {
                var resolved = g.Where(x => x.ResolvedAt.HasValue || x.Status is "Resolved" or "Closed").ToList();
                var closed = g.Where(x => x.Status == "Closed").ToList();
                var onSla = closed.Count(x => x.SlaResolveMet == true);
                var avgH = 0m;
                var timed = resolved.Where(x => x.ResolvedAt.HasValue).ToList();
                if (timed.Count > 0)
                    avgH = decimal.Round((decimal)timed.Average(x => (x.ResolvedAt!.Value - x.CreatedAt).TotalHours), 1);
                var hit = closed.Count == 0 ? 0 : decimal.Round(100m * onSla / closed.Count, 1);
                return new FsmTechProductivityRowDto(
                    g.Key.AssignedTechUserId, g.Key.Name,
                    g.Count(), resolved.Count, closed.Count, onSla, hit, avgH);
            })
            .OrderByDescending(x => x.ClosedCount).ThenBy(x => x.TechName)
            .ToList();
    }

    public async Task<string> ExportCsvAsync(Guid tenantId, string report, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "dashboard" or "045-dash")
        {
            var d = await DashboardAsync(tenantId, ct);
            sb.AppendLine("Open,Assigned,InProgress,Escalated,Resolved,Closed,OverdueOpen,ClosedToday,SlaHitRate,AppointmentToday");
            sb.AppendLine($"{d.OpenCount},{d.AssignedCount},{d.InProgressCount},{d.EscalatedCount},{d.ResolvedCount},{d.ClosedCount},{d.OverdueOpenCount},{d.ClosedTodayCount},{N(d.SlaHitRatePercent)},{d.AppointmentTodayCount}");
            return sb.ToString();
        }
        if (kind is "sla" or "sla-compliance" or "045")
        {
            var rows = await SlaComplianceAsync(tenantId, ct);
            sb.AppendLine("Priority,Open,OverdueOpen,Closed,SlaMet,SlaMiss,HitRate");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Priority)},{r.OpenCount},{r.OverdueOpenCount},{r.ClosedCount},{r.SlaMetCount},{r.SlaMissCount},{N(r.SlaHitRatePercent)}");
            return sb.ToString();
        }
        if (kind is "productivity" or "046")
        {
            var rows = await TechProductivityAsync(tenantId, ct);
            sb.AppendLine("Tech,Assigned,Resolved,Closed,OnSla,OnSlaPercent,AvgResolveHours");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.TechName)},{r.AssignedCount},{r.ResolvedCount},{r.ClosedCount},{r.OnSlaCount},{N(r.OnSlaPercent)},{N(r.AvgResolveHours)}");
            return sb.ToString();
        }
        if (kind is "parts" or "part-cost" or "047")
        {
            var s = await PartCostAsync(tenantId, ct);
            sb.AppendLine("PartCode,PartName,Qty,Amount,TicketCount");
            foreach (var r in s.ByPart)
                sb.AppendLine($"{Csv(r.PartCode)},{Csv(r.PartName)},{N(r.Qty)},{N(r.Amount)},{r.TicketCount}");
            sb.AppendLine($"TOTAL,,{N(s.TotalQty)},{N(s.TotalAmount)},{s.TicketCount}");
            return sb.ToString();
        }
        throw new AppException("report: dashboard | sla | productivity | parts.");
    }

    public async Task<FsmPartCostSummaryDto> PartCostAsync(Guid tenantId, CancellationToken ct = default)
    {
        var lines = await _db.FsmTicketPartLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0)
            return new FsmPartCostSummaryDto(0, 0, 0, 0, Array.Empty<FsmPartCostRowDto>());
        var partIds = lines.Select(x => x.PartId).Distinct().ToList();
        var parts = await _db.FsmParts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && partIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var byPart = lines.GroupBy(x => x.PartId).Select(g =>
        {
            parts.TryGetValue(g.Key, out var p);
            return new FsmPartCostRowDto(
                g.Key, p?.Code ?? "", p?.Name ?? "",
                g.Sum(x => x.Qty),
                decimal.Round(g.Sum(x => x.Qty * x.UnitCost), 2),
                g.Select(x => x.TicketId).Distinct().Count());
        }).OrderByDescending(x => x.Amount).ToList();
        return new FsmPartCostSummaryDto(
            byPart.Sum(x => x.Qty),
            decimal.Round(byPart.Sum(x => x.Amount), 2),
            lines.Count,
            lines.Select(x => x.TicketId).Distinct().Count(),
            byPart);
    }

    private static string Csv(string? s)
    {
        var v = s ?? "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static string N(decimal n) => n.ToString(CultureInfo.InvariantCulture);
}

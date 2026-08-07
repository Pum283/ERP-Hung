using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mfg;
using Erp.Application.Interfaces.Services.Mfg;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Mfg;

public sealed class MfgReportService : IMfgReportService
{
    private static readonly HashSet<string> OpenStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Approved", "Released", "MaterialsIssued", "Paused"
    };

    private readonly AppDbContext _db;
    public MfgReportService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<MfgWoProgressRowDto>> WoProgressAsync(
        Guid tenantId, string? status = null, Guid? workshopId = null, CancellationToken ct = default)
    {
        var q = _db.MfgWorkOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status.Trim());
        if (workshopId is Guid wid) q = q.Where(x => x.WorkshopId == wid);

        var list = await q.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync(ct);
        if (list.Count == 0) return Array.Empty<MfgWoProgressRowDto>();

        var itemIds = list.Select(x => x.ItemId).Distinct().ToList();
        var wsIds = list.Where(x => x.WorkshopId.HasValue).Select(x => x.WorkshopId!.Value).Distinct().ToList();
        var items = await _db.MfgItems.AsNoTracking().Where(x => itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var workshops = await _db.MfgWorkshops.AsNoTracking().Where(x => wsIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return list.Select(w =>
        {
            items.TryGetValue(w.ItemId, out var item);
            MfgWorkshop? ws = null;
            if (w.WorkshopId is Guid id) workshops.TryGetValue(id, out ws);
            var pct = w.Qty <= 0 ? 0 : decimal.Round(100m * w.QtyFgReceived / w.Qty, 1);
            return new MfgWoProgressRowDto(
                w.Id, w.Code, item?.Code ?? "", item?.Name ?? "",
                ws?.Code, ws?.Name, w.Status,
                w.Qty, w.QtyFgReceived, w.QtyScrap, pct, w.ReleasedAt, w.ClosedAt);
        }).ToList();
    }

    public async Task<IReadOnlyList<MfgOutputRowDto>> OutputByPeriodAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? workshopId = null, CancellationToken ct = default)
    {
        if (to < from) throw new AppException("to ≥ from.");
        var receipts = await _db.MfgFgReceipts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.ReceivedAt >= from && x.ReceivedAt <= to)
            .ToListAsync(ct);
        if (receipts.Count == 0) return Array.Empty<MfgOutputRowDto>();

        var woIds = receipts.Select(x => x.WorkOrderId).Distinct().ToList();
        var wos = await _db.MfgWorkOrders.AsNoTracking()
            .Where(x => woIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        if (workshopId is Guid wid)
            receipts = receipts.Where(r => wos.TryGetValue(r.WorkOrderId, out var w) && w.WorkshopId == wid).ToList();

        var wsIds = wos.Values.Where(x => x.WorkshopId.HasValue).Select(x => x.WorkshopId!.Value).Distinct().ToList();
        var workshops = await _db.MfgWorkshops.AsNoTracking().Where(x => wsIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return receipts
            .GroupBy(r =>
            {
                wos.TryGetValue(r.WorkOrderId, out var wo);
                var localTime = r.ReceivedAt.ToLocalTime();
                var shift = GetShiftLabel(localTime.Hour);
                return (Day: localTime.ToString("yyyy-MM-dd"), ShiftLabel: shift, WorkshopId: wo?.WorkshopId);
            })
            .Select(g =>
            {
                workshops.TryGetValue(g.Key.WorkshopId ?? Guid.Empty, out var ws);
                return new MfgOutputRowDto(
                    g.Key.Day, g.Key.ShiftLabel, g.Key.WorkshopId, ws?.Code, ws?.Name,
                    g.Sum(x => x.Qty), g.Count(),
                    g.Select(x => x.WorkOrderId).Distinct().Count());
            })
            .OrderByDescending(x => x.Day).ThenBy(x => x.ShiftLabel).ThenBy(x => x.WorkshopCode)
            .ToList();
    }

    private static string GetShiftLabel(int hour) => hour switch
    {
        >= 6 and < 14 => "Ca 1 (06:00-14:00)",
        >= 14 and < 22 => "Ca 2 (14:00-22:00)",
        _ => "Ca 3 (22:00-06:00)"
    };

    public async Task<IReadOnlyList<MfgMaterialVarianceRowDto>> MaterialVarianceAsync(
        Guid tenantId, Guid? workOrderId = null, CancellationToken ct = default)
    {
        var woQ = _db.MfgWorkOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.BomId != null
                        && x.Status != "Draft" && x.Status != "Cancelled");
        if (workOrderId is Guid woid) woQ = woQ.Where(x => x.Id == woid);
        var wos = await woQ.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        if (wos.Count == 0) return Array.Empty<MfgMaterialVarianceRowDto>();

        var bomIds = wos.Where(x => x.BomId.HasValue).Select(x => x.BomId!.Value).Distinct().ToList();
        var bomLines = await _db.MfgBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && bomIds.Contains(x.BomId))
            .ToListAsync(ct);
        var woIds = wos.Select(x => x.Id).ToList();
        var issues = await _db.MfgMaterialIssues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && woIds.Contains(x.WorkOrderId))
            .ToListAsync(ct);
        var itemIds = bomLines.Select(x => x.ComponentItemId)
            .Concat(issues.Select(x => x.ItemId)).Distinct().ToList();
        var items = await _db.MfgItems.AsNoTracking().Where(x => itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var rows = new List<MfgMaterialVarianceRowDto>();
        foreach (var wo in wos)
        {
            if (wo.BomId is not Guid bid) continue;
            var lines = bomLines.Where(l => l.BomId == bid).ToList();
            var issuedByItem = issues.Where(i => i.WorkOrderId == wo.Id)
                .GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Sum(x => x.Qty));
            var plannedItems = new HashSet<Guid>();
            foreach (var line in lines)
            {
                plannedItems.Add(line.ComponentItemId);
                var planned = line.Qty * wo.Qty;
                var actual = issuedByItem.GetValueOrDefault(line.ComponentItemId);
                var variance = actual - planned;
                var pct = planned == 0 ? (actual == 0 ? 0 : 100m)
                    : decimal.Round(100m * variance / planned, 1);
                items.TryGetValue(line.ComponentItemId, out var item);
                rows.Add(new MfgMaterialVarianceRowDto(
                    wo.Id, wo.Code, wo.Status, line.ComponentItemId,
                    item?.Code ?? "", item?.Name ?? "",
                    planned, actual, variance, pct));
            }
            foreach (var kv in issuedByItem.Where(x => !plannedItems.Contains(x.Key)))
            {
                items.TryGetValue(kv.Key, out var item);
                rows.Add(new MfgMaterialVarianceRowDto(
                    wo.Id, wo.Code, wo.Status, kv.Key,
                    item?.Code ?? "", item?.Name ?? "",
                    0, kv.Value, kv.Value, 100));
            }
        }
        return rows.OrderBy(x => x.WorkOrderCode).ThenBy(x => x.ItemCode).ToList();
    }

    public async Task<MfgDashboardDto> DashboardAsync(
        Guid tenantId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var f = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var t = to ?? DateTimeOffset.UtcNow;
        var wos = await _db.MfgWorkOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);

        int Count(string s) => wos.Count(x => x.Status == s);
        var inProgress = wos.Count(x => x.Status is "Released" or "MaterialsIssued");
        var open = wos.Where(x => OpenStatuses.Contains(x.Status)).ToList();

        var fgQty = await _db.MfgFgReceipts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.ReceivedAt >= f && x.ReceivedAt <= t)
            .SumAsync(x => (decimal?)x.Qty, ct) ?? 0;
        var scrapQty = await _db.MfgScraps.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.RecordedAt >= f && x.RecordedAt <= t)
            .SumAsync(x => (decimal?)x.Qty, ct) ?? 0;

        var variance = await MaterialVarianceAsync(tenantId, null, ct);
        var over = variance.Count(x => x.QtyVariance > 0);

        return new MfgDashboardDto(
            Count("Draft"), Count("Released") + Count("Approved"), inProgress, Count("Paused"),
            Count("Completed"), Count("Closed"),
            open.Sum(x => x.Qty), fgQty, scrapQty, open.Count, over);
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, string? status = null, Guid? workshopId = null,
        Guid? workOrderId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "wo-progress" or "041")
        {
            var rows = await WoProgressAsync(tenantId, status, workshopId, ct);
            sb.AppendLine("Code,Item,Workshop,Status,QtyPlanned,QtyFg,QtyScrap,ProgressPercent,ReleasedAt,ClosedAt");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.ItemCode)},{Csv(r.WorkshopCode)},{Csv(r.Status)},{N(r.QtyPlanned)},{N(r.QtyFgReceived)},{N(r.QtyScrap)},{N(r.ProgressPercent)},{r.ReleasedAt:yyyy-MM-dd},{r.ClosedAt:yyyy-MM-dd}");
            return sb.ToString();
        }
        if (kind is "output" or "042")
        {
            var f = from ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var t = to ?? DateTimeOffset.UtcNow;
            var rows = await OutputByPeriodAsync(tenantId, f, t, workshopId, ct);
            sb.AppendLine("Day,Shift,Workshop,QtyFg,ReceiptCount,WorkOrderCount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Day)},{Csv(r.ShiftLabel)},{Csv(r.WorkshopCode)},{N(r.QtyFg)},{r.ReceiptCount},{r.WorkOrderCount}");
            return sb.ToString();
        }
        if (kind is "material-variance" or "043")
        {
            var rows = await MaterialVarianceAsync(tenantId, workOrderId, ct);
            sb.AppendLine("WorkOrder,Status,Item,QtyPlanned,QtyActual,QtyVariance,VariancePercent");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.WorkOrderCode)},{Csv(r.Status)},{Csv(r.ItemCode)},{N(r.QtyPlanned)},{N(r.QtyActual)},{N(r.QtyVariance)},{N(r.VariancePercent)}");
            return sb.ToString();
        }
        if (kind is "dashboard" or "045")
        {
            var d = await DashboardAsync(tenantId, from, to, ct);
            sb.AppendLine("Draft,Released,InProgress,Paused,Completed,Closed,QtyPlannedOpen,QtyFgPeriod,QtyScrapPeriod,OpenWo,VarianceOver");
            sb.AppendLine($"{d.DraftCount},{d.ReleasedCount},{d.InProgressCount},{d.PausedCount},{d.CompletedCount},{d.ClosedCount},{N(d.QtyPlannedOpen)},{N(d.QtyFgPeriod)},{N(d.QtyScrapPeriod)},{d.OpenWoCount},{d.VarianceOverCount}");
            return sb.ToString();
        }
        throw new AppException("report: wo-progress | output | material-variance | dashboard.");
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

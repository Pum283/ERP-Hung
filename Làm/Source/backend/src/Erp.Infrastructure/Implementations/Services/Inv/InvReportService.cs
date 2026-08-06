using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Inv;

public sealed class InvReportService : IInvReportService
{
    private readonly AppDbContext _db;
    public InvReportService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvStockValueRowDto>> StockValueAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var q = _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.QtyOnHand != 0);
        if (warehouseId is Guid wid) q = q.Where(x => x.WarehouseId == wid);
        var bals = await q.Take(2000).ToListAsync(ct);
        if (bals.Count == 0) return Array.Empty<InvStockValueRowDto>();
        var sids = bals.Select(x => x.SkuId).Distinct().ToList();
        var wids = bals.Select(x => x.WarehouseId).Distinct().ToList();
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return bals.Select(b =>
        {
            skus.TryGetValue(b.SkuId, out var sku);
            var cost = sku?.StandardCost ?? 0;
            return new InvStockValueRowDto(
                b.SkuId, sku?.Code ?? "", sku?.Name ?? "", b.WarehouseId,
                whs.GetValueOrDefault(b.WarehouseId), b.QtyOnHand, cost,
                decimal.Round(b.QtyOnHand * cost, 2));
        }).OrderByDescending(x => x.StockValue).ToList();
    }

    public async Task<IReadOnlyList<InvMovementPeriodRowDto>> MovementByPeriodAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? warehouseId = null, CancellationToken ct = default)
    {
        if (to < from) throw new AppException("to ≥ from.");
        var docsQ = _db.InvStockDocs.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted"
                        && x.PostedAt != null && x.PostedAt >= from && x.PostedAt <= to);
        if (warehouseId is Guid wid) docsQ = docsQ.Where(x => x.WarehouseId == wid);
        var docs = await docsQ.ToListAsync(ct);
        if (docs.Count == 0) return Array.Empty<InvMovementPeriodRowDto>();
        var dids = docs.Select(x => x.Id).ToList();
        var lines = await _db.InvStockDocLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && dids.Contains(x.DocId)).ToListAsync(ct);
        var docMap = docs.ToDictionary(x => x.Id);
        var sids = lines.Select(x => x.SkuId).Distinct().ToList();
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return lines.GroupBy(x => x.SkuId).Select(g =>
        {
            skus.TryGetValue(g.Key, out var sku);
            decimal qtyIn = 0, qtyOut = 0, valIn = 0, valOut = 0;
            foreach (var line in g)
            {
                if (!docMap.TryGetValue(line.DocId, out var doc)) continue;
                var amt = line.Qty * line.UnitCost;
                if (doc.DocType == "Receipt") { qtyIn += line.Qty; valIn += amt; }
                else { qtyOut += line.Qty; valOut += amt; }
            }
            return new InvMovementPeriodRowDto(
                g.Key, sku?.Code ?? "", sku?.Name ?? "",
                qtyIn, qtyOut, qtyIn - qtyOut, valIn, valOut);
        }).OrderByDescending(x => Math.Abs(x.QtyNet)).Take(500).ToList();
    }

    public async Task<IReadOnlyList<InvSkuCardLineDto>> SkuCardAsync(
        Guid tenantId, Guid skuId, Guid? warehouseId = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var lines = await _db.InvStockDocLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.SkuId == skuId).ToListAsync(ct);
        if (lines.Count == 0) return Array.Empty<InvSkuCardLineDto>();
        var dids = lines.Select(x => x.DocId).Distinct().ToList();
        var docsQ = _db.InvStockDocs.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted" && dids.Contains(x.Id));
        if (warehouseId is Guid wid) docsQ = docsQ.Where(x => x.WarehouseId == wid);
        if (from is DateTimeOffset f) docsQ = docsQ.Where(x => x.PostedAt >= f);
        if (to is DateTimeOffset t) docsQ = docsQ.Where(x => x.PostedAt <= t);
        var docs = await docsQ.ToListAsync(ct);
        var wids = docs.Select(x => x.WarehouseId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var docMap = docs.ToDictionary(x => x.Id);

        return lines.Where(l => docMap.ContainsKey(l.DocId)).Select(l =>
        {
            var d = docMap[l.DocId];
            var sign = d.DocType == "Receipt" ? 1m : -1m;
            return new InvSkuCardLineDto(
                d.PostedAt ?? d.CreatedAt, d.Code, d.DocType, d.SourceType,
                whs.GetValueOrDefault(d.WarehouseId) ?? "",
                sign * l.Qty, l.UnitCost, sign * l.Qty * l.UnitCost, d.RefCode);
        }).OrderByDescending(x => x.At).Take(500).ToList();
    }

    public async Task<IReadOnlyList<InvMinMaxAlertRowDto>> MinMaxAlertsAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var bals = await StockValueAsync(tenantId, warehouseId, ct);
        var sids = bals.Select(x => x.SkuId).Distinct().ToList();
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var rows = new List<InvMinMaxAlertRowDto>();
        foreach (var b in bals)
        {
            if (!skus.TryGetValue(b.SkuId, out var sku)) continue;
            if (sku.MinQty is decimal min && b.QtyOnHand < min)
                rows.Add(new InvMinMaxAlertRowDto(b.SkuId, b.SkuCode, b.SkuName, b.WarehouseId, b.WarehouseName,
                    b.QtyOnHand, sku.MinQty, sku.MaxQty, "BelowMin"));
            if (sku.MaxQty is decimal max && b.QtyOnHand > max)
                rows.Add(new InvMinMaxAlertRowDto(b.SkuId, b.SkuCode, b.SkuName, b.WarehouseId, b.WarehouseName,
                    b.QtyOnHand, sku.MinQty, sku.MaxQty, "AboveMax"));
        }
        return rows.OrderBy(x => x.AlertType).ThenBy(x => x.SkuCode).ToList();
    }

    public async Task<IReadOnlyList<InvStocktakeReportRowDto>> StocktakeResultAsync(
        Guid tenantId, Guid? stocktakeId = null, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var q = _db.InvStocktakes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Reviewed" || x.Status == "Posted" || x.Status == "Counting"));
        if (stocktakeId is Guid sid) q = q.Where(x => x.Id == sid);
        if (warehouseId is Guid wid) q = q.Where(x => x.WarehouseId == wid);
        var sts = await q.OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync(ct);
        if (sts.Count == 0) return Array.Empty<InvStocktakeReportRowDto>();
        var ids = sts.Select(x => x.Id).ToList();
        var lines = await _db.InvStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.StocktakeId)).ToListAsync(ct);
        var wids = sts.Select(x => x.WarehouseId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var stMap = sts.ToDictionary(x => x.Id);
        return lines.Select(l =>
        {
            var st = stMap[l.StocktakeId];
            return new InvStocktakeReportRowDto(
                st.Id, st.Code, whs.GetValueOrDefault(st.WarehouseId), st.Status,
                l.SkuCode, l.SkuName, l.SystemQty, l.CountedQty, l.VarianceQty);
        }).OrderBy(x => x.StocktakeCode).ThenBy(x => x.SkuCode).ToList();
    }

    public async Task<InvDashboardDto> DashboardAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var values = await StockValueAsync(tenantId, warehouseId, ct);
        var alerts = await MinMaxAlertsAsync(tenantId, warehouseId, ct);
        var skuCount = await _db.InvSkus.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);
        var whCount = await _db.InvWarehouses.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);
        var openSt = await _db.InvStocktakes.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && !x.IsDeleted
                             && (x.Status == "Counting" || x.Status == "Reviewed"), ct);
        var near = await NearExpiryAsync(tenantId, 30, warehouseId, ct);
        var balsQ = _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.QtyOnHand - x.QtyReserved <= 0 && x.QtyReserved > 0);
        if (warehouseId is Guid wAtp) balsQ = balsQ.Where(x => x.WarehouseId == wAtp);
        var atpCount = await balsQ.CountAsync(ct);
        return new InvDashboardDto(
            skuCount, whCount,
            values.Sum(x => x.QtyOnHand), values.Sum(x => x.StockValue),
            alerts.Count(x => x.AlertType == "BelowMin"),
            alerts.Count(x => x.AlertType == "AboveMax"),
            openSt, alerts.Take(20).ToList(),
            near.Count(x => x.AlertType == "NearExpiry"),
            near.Count(x => x.AlertType == "Expired"),
            atpCount);
    }

    public async Task<IReadOnlyList<InvNearExpiryRowDto>> NearExpiryAsync(
        Guid tenantId, int withinDays = 30, Guid? warehouseId = null, CancellationToken ct = default)
    {
        if (withinDays < 0) withinDays = 30;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(withinDays);
        var q = _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.ExpiryDate != null
                        && x.QtyOnHand > 0 && x.ExpiryDate <= until);
        if (warehouseId is Guid wid) q = q.Where(x => x.WarehouseId == wid);
        var bals = await q.OrderBy(x => x.ExpiryDate).Take(500).ToListAsync(ct);
        if (bals.Count == 0) return Array.Empty<InvNearExpiryRowDto>();
        var wids = bals.Select(x => x.WarehouseId).Distinct().ToList();
        var sids = bals.Select(x => x.SkuId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return bals.Select(b =>
        {
            skus.TryGetValue(b.SkuId, out var sku);
            var exp = b.ExpiryDate!.Value;
            var days = exp.DayNumber - today.DayNumber;
            return new InvNearExpiryRowDto(
                b.WarehouseId, whs.GetValueOrDefault(b.WarehouseId),
                b.SkuId, sku?.Code ?? "", sku?.Name ?? "",
                string.IsNullOrEmpty(b.LotCode) ? null : b.LotCode, exp,
                b.QtyOnHand, b.QtyReserved, b.QtyOnHand - b.QtyReserved,
                days, days < 0 ? "Expired" : "NearExpiry");
        }).ToList();
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, Guid? warehouseId = null, Guid? skuId = null,
        Guid? stocktakeId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        int? withinDays = null, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "stock-value" or "060" or "063")
        {
            var rows = await StockValueAsync(tenantId, warehouseId, ct);
            sb.AppendLine("SkuCode,SkuName,Warehouse,QtyOnHand,StandardCost,StockValue");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.SkuCode)},{Csv(r.SkuName)},{Csv(r.WarehouseName)},{N(r.QtyOnHand)},{N(r.StandardCost)},{N(r.StockValue)}");
            return sb.ToString();
        }
        if (kind is "movement" or "064")
        {
            var f = from ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var t = to ?? DateTimeOffset.UtcNow;
            var rows = await MovementByPeriodAsync(tenantId, f, t, warehouseId, ct);
            sb.AppendLine("SkuCode,SkuName,QtyIn,QtyOut,QtyNet,ValueIn,ValueOut");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.SkuCode)},{Csv(r.SkuName)},{N(r.QtyIn)},{N(r.QtyOut)},{N(r.QtyNet)},{N(r.ValueIn)},{N(r.ValueOut)}");
            return sb.ToString();
        }
        if (kind is "sku-card" or "065")
        {
            if (skuId is not Guid sid) throw new AppException("sku-card cần skuId.");
            var rows = await SkuCardAsync(tenantId, sid, warehouseId, from, to, ct);
            sb.AppendLine("At,DocCode,DocType,SourceType,Warehouse,Qty,UnitCost,Amount,RefCode");
            foreach (var r in rows)
                sb.AppendLine($"{r.At:yyyy-MM-dd HH:mm},{Csv(r.DocCode)},{Csv(r.DocType)},{Csv(r.SourceType)},{Csv(r.WarehouseName)},{N(r.QtySigned)},{N(r.UnitCost)},{N(r.Amount)},{Csv(r.RefCode)}");
            return sb.ToString();
        }
        if (kind is "min-max" or "067")
        {
            var rows = await MinMaxAlertsAsync(tenantId, warehouseId, ct);
            sb.AppendLine("AlertType,SkuCode,SkuName,Warehouse,QtyOnHand,MinQty,MaxQty");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.AlertType)},{Csv(r.SkuCode)},{Csv(r.SkuName)},{Csv(r.WarehouseName)},{N(r.QtyOnHand)},{N(r.MinQty ?? 0)},{N(r.MaxQty ?? 0)}");
            return sb.ToString();
        }
        if (kind is "stocktake" or "055")
        {
            var rows = await StocktakeResultAsync(tenantId, stocktakeId, warehouseId, ct);
            sb.AppendLine("Stocktake,Warehouse,Status,SkuCode,SkuName,SystemQty,CountedQty,Variance");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.StocktakeCode)},{Csv(r.WarehouseName)},{Csv(r.Status)},{Csv(r.SkuCode)},{Csv(r.SkuName)},{N(r.SystemQty)},{N(r.CountedQty ?? 0)},{N(r.VarianceQty)}");
            return sb.ToString();
        }
        if (kind is "dashboard" or "069")
        {
            var d = await DashboardAsync(tenantId, warehouseId, ct);
            sb.AppendLine("SkuCount,WarehouseCount,TotalQty,TotalValue,BelowMin,AboveMax,OpenStocktakes,NearExpiry,Expired,InsufficientAtp");
            sb.AppendLine($"{d.SkuCount},{d.WarehouseCount},{N(d.TotalQtyOnHand)},{N(d.TotalStockValue)},{d.BelowMinCount},{d.AboveMaxCount},{d.OpenStocktakeCount},{d.NearExpiryCount},{d.ExpiredCount},{d.InsufficientAtpCount}");
            return sb.ToString();
        }
        if (kind is "near-expiry" or "048" or "expiry" or "044")
        {
            var rows = await NearExpiryAsync(tenantId, withinDays ?? 30, warehouseId, ct);
            sb.AppendLine("AlertType,SkuCode,SkuName,Warehouse,Lot,Expiry,Days,QtyOnHand,QtyReserved,QtyAvailable");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.AlertType)},{Csv(r.SkuCode)},{Csv(r.SkuName)},{Csv(r.WarehouseName)},{Csv(r.LotCode)},{r.ExpiryDate:yyyy-MM-dd},{r.DaysToExpiry},{N(r.QtyOnHand)},{N(r.QtyReserved)},{N(r.QtyAvailable)}");
            return sb.ToString();
        }
        throw new AppException("report: stock-value | movement | sku-card | min-max | stocktake | dashboard | near-expiry.");
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

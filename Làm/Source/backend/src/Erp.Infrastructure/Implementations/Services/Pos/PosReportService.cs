using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pos;

public sealed class PosReportService : IPosReportService
{
    private readonly AppDbContext _db;
    public PosReportService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PosRevenueByTimeRowDto>> RevenueByTimeAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, string grain,
        Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var g = (grain ?? "day").Trim().ToLowerInvariant();
        if (g is not ("hour" or "day" or "shift")) throw new AppException("grain: hour | day | shift.");

        var sales = await PaidSalesQuery(tenantId, from, to, storeId).ToListAsync(ct);
        if (g == "shift")
        {
            var shiftIds = sales.Select(x => x.ShiftId).Distinct().ToList();
            var shifts = await _db.PosShifts.AsNoTracking().Where(x => shiftIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
            return sales.GroupBy(x => x.ShiftId).Select(grp =>
            {
                shifts.TryGetValue(grp.Key, out var sh);
                return new PosRevenueByTimeRowDto(
                    sh?.Code ?? grp.Key.ToString("N")[..8],
                    sh?.OpenedAt, grp.Key, sh?.Code,
                    grp.Count(),
                    grp.Sum(x => x.TotalAmount - x.ReturnedAmount),
                    grp.Sum(x => x.DiscountAmount));
            }).OrderByDescending(x => x.BucketStart).ToList();
        }

        return sales.GroupBy(x =>
        {
            var t = x.PaidAt ?? x.CreatedAt;
            return g == "hour"
                ? new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, 0, 0, t.Offset)
                : new DateTimeOffset(t.Year, t.Month, t.Day, 0, 0, 0, t.Offset);
        }).Select(grp => new PosRevenueByTimeRowDto(
            g == "hour" ? grp.Key.ToString("yyyy-MM-dd HH:00") : grp.Key.ToString("yyyy-MM-dd"),
            grp.Key, null, null,
            grp.Count(),
            grp.Sum(x => x.TotalAmount - x.ReturnedAmount),
            grp.Sum(x => x.DiscountAmount)
        )).OrderBy(x => x.BucketStart).ToList();
    }

    public async Task<IReadOnlyList<PosRevenueByProductRowDto>> RevenueByProductAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var saleIds = await PaidSalesQuery(tenantId, from, to, storeId).Select(x => x.Id).ToListAsync(ct);
        if (saleIds.Count == 0) return Array.Empty<PosRevenueByProductRowDto>();

        var lines = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && saleIds.Contains(x.SaleId)
                        && x.Status == "Active")
            .ToListAsync(ct);

        return lines.GroupBy(x => new { x.ProductCode, x.ProductName })
            .Select(g => new PosRevenueByProductRowDto(
                g.Key.ProductCode, g.Key.ProductName,
                g.Sum(x => x.Quantity), g.Sum(x => x.LineAmount), g.Count()))
            .OrderByDescending(x => x.Revenue).Take(500).ToList();
    }

    public async Task<IReadOnlyList<PosRevenueByCashierRowDto>> RevenueByCashierAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var sales = await PaidSalesQuery(tenantId, from, to, storeId).ToListAsync(ct);
        if (sales.Count == 0) return Array.Empty<PosRevenueByCashierRowDto>();

        var shiftIds = sales.Select(x => x.ShiftId).Distinct().ToList();
        var shifts = await _db.PosShifts.AsNoTracking().Where(x => shiftIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var userIds = shifts.Values.Select(x => x.CashierUserId).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username ?? x.Id.ToString("N")[..8], ct);

        return sales.GroupBy(s =>
        {
            shifts.TryGetValue(s.ShiftId, out var sh);
            return sh?.CashierUserId ?? Guid.Empty;
        }).Where(g => g.Key != Guid.Empty).Select(g => new PosRevenueByCashierRowDto(
            g.Key, users.GetValueOrDefault(g.Key) ?? g.Key.ToString("N")[..8],
            g.Count(),
            g.Sum(x => x.TotalAmount - x.ReturnedAmount),
            g.Sum(x => x.DiscountAmount)
        )).OrderByDescending(x => x.Revenue).ToList();
    }

    public async Task<PosCancelDiscountReportDto> CancelDiscountRatesAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var q = _db.PosSales.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.CreatedAt >= from && x.CreatedAt <= to);
        if (storeId is Guid sid) q = q.Where(x => x.StoreId == sid);
        var list = await q.ToListAsync(ct);
        var total = list.Count;
        var paid = list.Count(x => x.Status is "Paid" or "Returned");
        var cancelled = list.Count(x => x.Status == "Cancelled");
        var discounted = list.Count(x => x.Status is "Paid" or "Returned"
            && (x.DiscountAmount > 0 || (x.DiscountSource != null && x.DiscountSource != "None")));
        var revenue = list.Where(x => x.Status is "Paid" or "Returned").Sum(x => x.TotalAmount - x.ReturnedAmount);
        var discount = list.Where(x => x.Status is "Paid" or "Returned").Sum(x => x.DiscountAmount);
        var cancelPct = total == 0 ? 0 : Math.Round(cancelled * 100m / total, 2);
        var discPct = paid == 0 ? 0 : Math.Round(discounted * 100m / paid, 2);
        return new PosCancelDiscountReportDto(
            total, paid, cancelled, discounted, cancelPct, discPct, revenue, discount);
    }

    public async Task<IReadOnlyList<PosTopProductRowDto>> TopProductsAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        int top = 10, string by = "qty", Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var mode = (by ?? "qty").Trim().ToLowerInvariant();
        if (mode is not ("qty" or "revenue")) throw new AppException("by: qty | revenue.");
        if (top < 1 || top > 100) throw new AppException("top: 1–100.");

        var products = await RevenueByProductAsync(tenantId, from, to, storeId, ct);
        var ordered = mode == "qty"
            ? products.OrderByDescending(x => x.Qty).ThenByDescending(x => x.Revenue)
            : products.OrderByDescending(x => x.Revenue).ThenByDescending(x => x.Qty);
        return ordered.Take(top)
            .Select((r, i) => new PosTopProductRowDto(
                i + 1, r.ProductCode, r.ProductName, r.Qty, r.Revenue, r.LineCount))
            .ToList();
    }

    public async Task<IReadOnlyList<PosStoreCompareRowDto>> CompareStoresAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var sales = await PaidSalesQuery(tenantId, from, to, null).ToListAsync(ct);
        if (sales.Count == 0) return Array.Empty<PosStoreCompareRowDto>();

        var storeIds = sales.Select(x => x.StoreId).Distinct().ToList();
        var stores = await _db.PosStores.AsNoTracking()
            .Where(x => x.TenantId == tenantId && storeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var totalRevenue = sales.Sum(x => x.TotalAmount - x.ReturnedAmount);

        return sales.GroupBy(x => x.StoreId).Select(g =>
        {
            stores.TryGetValue(g.Key, out var st);
            var revenue = g.Sum(x => x.TotalAmount - x.ReturnedAmount);
            var count = g.Count();
            return new PosStoreCompareRowDto(
                g.Key,
                st?.Code ?? g.Key.ToString("N")[..8],
                st?.Name ?? "?",
                count, revenue,
                g.Sum(x => x.DiscountAmount),
                count == 0 ? 0 : Math.Round(revenue / count, 2),
                totalRevenue == 0 ? 0 : Math.Round(revenue * 100m / totalRevenue, 2));
        }).OrderByDescending(x => x.Revenue).ToList();
    }

    public async Task<PosChainLiveReportDto> ChainLiveAsync(
        Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var now = asOf ?? DateTimeOffset.UtcNow;
        var dayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var elapsedPct = Math.Min(100m,
            Math.Round((decimal)((now - monthStart).TotalDays * 100 / daysInMonth), 2));

        var stores = await _db.PosStores.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active")
            .OrderBy(x => x.Code).ToListAsync(ct);
        if (stores.Count == 0)
            return new PosChainLiveReportDto(now, 0, 0, 0, 0, 0, 0, Array.Empty<PosChainLiveRowDto>());

        var monthSales = await PaidSalesQuery(tenantId, monthStart, now, null).ToListAsync(ct);
        var openShifts = await _db.PosShifts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Open")
            .GroupBy(x => x.StoreId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var rows = stores.Select(st =>
        {
            var mine = monthSales.Where(x => x.StoreId == st.Id).ToList();
            var today = mine.Where(x => (x.PaidAt ?? x.CreatedAt) >= dayStart).ToList();
            var monthRevenue = mine.Sum(x => x.TotalAmount - x.ReturnedAmount);
            var attainment = st.MonthlyRevenueTarget == 0
                ? 0
                : Math.Round(monthRevenue * 100m / st.MonthlyRevenueTarget, 2);
            return new PosChainLiveRowDto(
                st.Id, st.Code, st.Name, st.Status,
                openShifts.GetValueOrDefault(st.Id),
                today.Count,
                today.Sum(x => x.TotalAmount - x.ReturnedAmount),
                monthRevenue, st.MonthlyRevenueTarget, attainment, elapsedPct);
        }).OrderByDescending(x => x.TodayRevenue).ToList();

        var totalTarget = rows.Sum(x => x.MonthlyTarget);
        var totalMonth = rows.Sum(x => x.MonthRevenue);
        return new PosChainLiveReportDto(
            now, rows.Count, rows.Sum(x => x.OpenShiftCount),
            rows.Sum(x => x.TodayRevenue), totalMonth, totalTarget,
            totalTarget == 0 ? 0 : Math.Round(totalMonth * 100m / totalTarget, 2),
            rows);
    }

    public async Task<PosCostVarianceReportDto> CostVarianceAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var saleIds = await PaidSalesQuery(tenantId, from, to, storeId).Select(x => x.Id).ToListAsync(ct);

        // Lý thuyết: BOM explode từ dòng bán Active
        var theo = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (saleIds.Count > 0)
        {
            var lines = await _db.PosSaleLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active"
                            && x.ProductId != null && saleIds.Contains(x.SaleId))
                .Select(x => new { ProductId = x.ProductId!.Value, x.Quantity })
                .ToListAsync(ct);
            var productIds = lines.Select(x => x.ProductId).Distinct().ToList();
            var bom = await _db.PosBomLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && productIds.Contains(x.ProductId))
                .ToListAsync(ct);
            var bomByProduct = bom.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var line in lines)
            {
                if (!bomByProduct.TryGetValue(line.ProductId, out var mats)) continue;
                foreach (var m in mats)
                {
                    var code = m.MaterialCode.Trim().ToUpperInvariant();
                    if (code.Length == 0 || m.Qty <= 0) continue;
                    theo[code] = theo.GetValueOrDefault(code) + Math.Round(m.Qty * line.Quantity, 3);
                }
            }
        }

        // Thực tế: INV Issue Posted RefModule=POS gắn với các đơn trong kỳ
        var actualQty = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var actualCost = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (saleIds.Count > 0)
        {
            var docIds = await _db.InvStockDocs.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && x.DocType == "Issue" && x.Status == "Posted"
                            && x.RefModule == "POS" && x.RefId != null && saleIds.Contains(x.RefId.Value))
                .Select(x => x.Id).ToListAsync(ct);
            if (docIds.Count > 0)
            {
                var docLines = await _db.InvStockDocLines.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && docIds.Contains(x.DocId))
                    .ToListAsync(ct);
                foreach (var dl in docLines)
                {
                    var code = dl.SkuCode.Trim().ToUpperInvariant();
                    if (code.Length == 0) continue;
                    actualQty[code] = actualQty.GetValueOrDefault(code) + dl.Qty;
                    actualCost[code] = actualCost.GetValueOrDefault(code) + dl.Qty * dl.UnitCost;
                }
            }
        }

        var codes = theo.Keys.Union(actualQty.Keys, StringComparer.OrdinalIgnoreCase).ToList();
        if (codes.Count == 0)
            return new PosCostVarianceReportDto(0, 0, 0, 0, Array.Empty<PosCostVarianceRowDto>());

        var skus = await _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && codes.Contains(x.Code))
            .ToListAsync(ct);
        var skuByCode = skus.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);

        var rows = new List<PosCostVarianceRowDto>();
        foreach (var code in codes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            skuByCode.TryGetValue(code, out var sku);
            var std = sku?.StandardCost ?? 0;
            var tQty = Math.Round(theo.GetValueOrDefault(code), 3);
            var aQty = Math.Round(actualQty.GetValueOrDefault(code), 3);
            var tCost = Math.Round(tQty * std, 2);
            // UnitCost 0 trên phiếu → fallback StandardCost để so sánh có nghĩa
            var aCostRaw = actualCost.GetValueOrDefault(code);
            var aCost = Math.Round(aCostRaw > 0 ? aCostRaw : aQty * std, 2);
            var variance = Math.Round(aCost - tCost, 2);
            var variancePct = tCost == 0 ? 0 : Math.Round(variance * 100m / tCost, 2);
            rows.Add(new PosCostVarianceRowDto(
                code, sku?.Name ?? "?", tQty, aQty, std, tCost, aCost, variance, variancePct));
        }

        var totalTheo = Math.Round(rows.Sum(x => x.TheoreticalCost), 2);
        var totalActual = Math.Round(rows.Sum(x => x.ActualCost), 2);
        var totalVar = Math.Round(totalActual - totalTheo, 2);
        var totalVarPct = totalTheo == 0 ? 0 : Math.Round(totalVar * 100m / totalTheo, 2);
        return new PosCostVarianceReportDto(totalTheo, totalActual, totalVar, totalVarPct,
            rows.OrderByDescending(x => Math.Abs(x.VarianceCost)).ToList());
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, DateTimeOffset from, DateTimeOffset to,
        string? grain = null, Guid? storeId = null, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "time" or "061")
        {
            var rows = await RevenueByTimeAsync(tenantId, from, to, grain ?? "day", storeId, ct);
            sb.AppendLine("Bucket,SaleCount,Revenue,Discount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Bucket)},{r.SaleCount},{N(r.Revenue)},{N(r.Discount)}");
            return sb.ToString();
        }
        if (kind is "product" or "062")
        {
            var rows = await RevenueByProductAsync(tenantId, from, to, storeId, ct);
            sb.AppendLine("ProductCode,ProductName,Qty,Revenue,LineCount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.ProductCode)},{Csv(r.ProductName)},{N(r.Qty)},{N(r.Revenue)},{r.LineCount}");
            return sb.ToString();
        }
        if (kind is "cashier" or "063")
        {
            var rows = await RevenueByCashierAsync(tenantId, from, to, storeId, ct);
            sb.AppendLine("Cashier,SaleCount,Revenue,Discount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.CashierName)},{r.SaleCount},{N(r.Revenue)},{N(r.Discount)}");
            return sb.ToString();
        }
        if (kind is "cancel-discount" or "064")
        {
            var r = await CancelDiscountRatesAsync(tenantId, from, to, storeId, ct);
            sb.AppendLine("TotalSales,PaidSales,CancelledSales,DiscountedSales,CancelRate%,DiscountRate%,Revenue,Discount");
            sb.AppendLine($"{r.TotalSales},{r.PaidSales},{r.CancelledSales},{r.DiscountedSales},{N(r.CancelRatePercent)},{N(r.DiscountRatePercent)},{N(r.TotalRevenue)},{N(r.TotalDiscount)}");
            return sb.ToString();
        }
        if (kind is "top-products" or "066")
        {
            var rows = await TopProductsAsync(tenantId, from, to, 20, "qty", storeId, ct);
            sb.AppendLine("Rank,ProductCode,ProductName,Qty,Revenue,LineCount");
            foreach (var r in rows)
                sb.AppendLine($"{r.Rank},{Csv(r.ProductCode)},{Csv(r.ProductName)},{N(r.Qty)},{N(r.Revenue)},{r.LineCount}");
            return sb.ToString();
        }
        if (kind is "stores" or "067")
        {
            var rows = await CompareStoresAsync(tenantId, from, to, ct);
            sb.AppendLine("StoreCode,StoreName,SaleCount,Revenue,Discount,AvgTicket,Share%");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.StoreCode)},{Csv(r.StoreName)},{r.SaleCount},{N(r.Revenue)},{N(r.Discount)},{N(r.AvgTicket)},{N(r.RevenueSharePercent)}");
            return sb.ToString();
        }
        if (kind is "cost-variance" or "065")
        {
            var r = await CostVarianceAsync(tenantId, from, to, storeId, ct);
            sb.AppendLine("MaterialCode,MaterialName,TheoQty,ActualQty,StdCost,TheoCost,ActualCost,Variance,Variance%");
            foreach (var row in r.Rows)
                sb.AppendLine($"{Csv(row.MaterialCode)},{Csv(row.MaterialName)},{N(row.TheoreticalQty)},{N(row.ActualQty)},{N(row.StandardCost)},{N(row.TheoreticalCost)},{N(row.ActualCost)},{N(row.VarianceCost)},{N(row.VariancePercent)}");
            sb.AppendLine($"TOTAL,,,,,{N(r.TotalTheoreticalCost)},{N(r.TotalActualCost)},{N(r.TotalVarianceCost)},{N(r.TotalVariancePercent)}");
            return sb.ToString();
        }
        throw new AppException("report: time | product | cashier | cancel-discount | top-products | stores | cost-variance.");
    }

    private IQueryable<Domain.Entities.Pos.PosSale> PaidSalesQuery(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? storeId)
    {
        var q = _db.PosSales.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Paid" || x.Status == "Returned")
                        && ((x.PaidAt ?? x.CreatedAt) >= from)
                        && ((x.PaidAt ?? x.CreatedAt) <= to));
        if (storeId is Guid sid) q = q.Where(x => x.StoreId == sid);
        return q;
    }

    private static void EnsureRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from) throw new AppException("to ≥ from.");
        if ((to - from).TotalDays > 366) throw new AppException("Khoảng báo cáo tối đa 366 ngày.");
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

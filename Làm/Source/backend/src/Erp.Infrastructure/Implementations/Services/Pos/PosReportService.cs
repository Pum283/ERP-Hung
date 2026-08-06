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
        throw new AppException("report: time | product | cashier | cancel-discount.");
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

using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Application.Interfaces.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pur;

public sealed class PurReportService : IPurReportService
{
    private readonly AppDbContext _db;
    public PurReportService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PurPurchaseByVendorRowDto>> PurchaseByVendorAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? vendorId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var grns = await PostedGrns(tenantId, from, to, vendorId).ToListAsync(ct);
        if (grns.Count == 0) return Array.Empty<PurPurchaseByVendorRowDto>();
        var gids = grns.Select(x => x.Id).ToList();
        var lines = await _db.PurGrnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && gids.Contains(x.GrnId)).ToListAsync(ct);
        var vids = grns.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return grns.GroupBy(x => x.VendorId).Select(g =>
        {
            vendors.TryGetValue(g.Key, out var v);
            var glines = lines.Where(l => g.Select(x => x.Id).Contains(l.GrnId)).ToList();
            return new PurPurchaseByVendorRowDto(
                g.Key, v?.Code ?? "", v?.Name ?? "",
                g.Count(),
                glines.Sum(x => x.AcceptedQty),
                glines.Sum(x => x.AcceptedQty * x.UnitPrice));
        }).OrderByDescending(x => x.Amount).ToList();
    }

    public async Task<IReadOnlyList<PurPurchaseByProductRowDto>> PurchaseByProductAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? vendorId = null, CancellationToken ct = default)
    {
        EnsureRange(from, to);
        var grns = await PostedGrns(tenantId, from, to, vendorId).Select(x => x.Id).ToListAsync(ct);
        if (grns.Count == 0) return Array.Empty<PurPurchaseByProductRowDto>();
        var lines = await _db.PurGrnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && grns.Contains(x.GrnId) && x.AcceptedQty > 0)
            .ToListAsync(ct);
        return lines.GroupBy(x => new { x.ProductCode, x.ProductName })
            .Select(g => new PurPurchaseByProductRowDto(
                g.Key.ProductCode, g.Key.ProductName,
                g.Sum(x => x.AcceptedQty),
                g.Sum(x => x.AcceptedQty * x.UnitPrice),
                g.Count()))
            .OrderByDescending(x => x.Amount).Take(500).ToList();
    }

    public async Task<IReadOnlyList<PurOpenPrAgingRowDto>> OpenPrAgingAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurPurchaseRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Submitted" || x.Status == "Approved"))
            .OrderBy(x => x.CreatedAt).Take(300).ToListAsync(ct);
        if (list.Count == 0) return Array.Empty<PurOpenPrAgingRowDto>();
        var ids = list.Select(x => x.Id).ToList();
        var lineStats = await _db.PurPrLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.PrId))
            .GroupBy(x => x.PrId)
            .Select(g => new { g.Key, C = g.Count(), Qty = g.Sum(x => x.Qty) })
            .ToDictionaryAsync(x => x.Key, ct);
        var today = DateTimeOffset.UtcNow;
        return list.Select(p =>
        {
            lineStats.TryGetValue(p.Id, out var s);
            return new PurOpenPrAgingRowDto(
                p.Id, p.Code, p.Status, p.CreatedAt,
                Math.Max(0, (int)(today - p.CreatedAt).TotalDays),
                null, s?.C ?? 0, s?.Qty ?? 0);
        }).ToList();
    }

    public async Task<IReadOnlyList<PurOpenPoAgingRowDto>> OpenPoAgingAsync(
        Guid tenantId, Guid? vendorId = null, CancellationToken ct = default)
    {
        var q = _db.PurPurchaseOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Approved" || x.Status == "Sent"));
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        var list = await q.OrderBy(x => x.CreatedAt).Take(300).ToListAsync(ct);
        if (list.Count == 0) return Array.Empty<PurOpenPoAgingRowDto>();
        var ids = list.Select(x => x.Id).ToList();
        var lines = await _db.PurPoLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.PoId)).ToListAsync(ct);
        var vids = list.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var today = DateTimeOffset.UtcNow;
        return list.Select(p =>
        {
            vendors.TryGetValue(p.VendorId, out var v);
            var pl = lines.Where(l => l.PoId == p.Id).ToList();
            var openQty = pl.Sum(l => Math.Max(0, l.Qty - l.ReceivedQty));
            var openAmt = pl.Sum(l => Math.Max(0, l.Qty - l.ReceivedQty) * l.UnitPrice);
            return new PurOpenPoAgingRowDto(
                p.Id, p.Code, p.VendorId, v?.Code, v?.Name, p.Status, p.CreatedAt,
                Math.Max(0, (int)(today - p.CreatedAt).TotalDays), openQty, openAmt);
        }).Where(x => x.OpenQty > 0 || x.Status is "Approved" or "Sent").ToList();
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, DateTimeOffset? from = null, DateTimeOffset? to = null,
        Guid? vendorId = null, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        var f = from ?? DateTimeOffset.UtcNow.AddMonths(-1);
        var t = to ?? DateTimeOffset.UtcNow;

        if (kind is "by-vendor" or "048-vendor")
        {
            var rows = await PurchaseByVendorAsync(tenantId, f, t, vendorId, ct);
            sb.AppendLine("VendorCode,VendorName,GrnCount,AcceptedQty,Amount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.VendorCode)},{Csv(r.VendorName)},{r.GrnCount},{N(r.AcceptedQty)},{N(r.Amount)}");
            return sb.ToString();
        }
        if (kind is "by-product" or "048-product")
        {
            var rows = await PurchaseByProductAsync(tenantId, f, t, vendorId, ct);
            sb.AppendLine("ProductCode,ProductName,AcceptedQty,Amount,LineCount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.ProductCode)},{Csv(r.ProductName)},{N(r.AcceptedQty)},{N(r.Amount)},{r.LineCount}");
            return sb.ToString();
        }
        if (kind is "open-pr" or "051-pr")
        {
            var rows = await OpenPrAgingAsync(tenantId, ct);
            sb.AppendLine("Code,Status,CreatedAt,AgeDays,LineCount,TotalQty");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.Status)},{r.CreatedAt:yyyy-MM-dd},{r.AgeDays},{r.LineCount},{N(r.TotalQty)}");
            return sb.ToString();
        }
        if (kind is "open-po" or "051-po")
        {
            var rows = await OpenPoAgingAsync(tenantId, vendorId, ct);
            sb.AppendLine("Code,Vendor,Status,CreatedAt,AgeDays,OpenQty,OpenAmount");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Code)},{Csv(r.VendorCode)},{Csv(r.Status)},{r.CreatedAt:yyyy-MM-dd},{r.AgeDays},{N(r.OpenQty)},{N(r.OpenAmount)}");
            return sb.ToString();
        }
        throw new AppException("report: by-vendor | by-product | open-pr | open-po.");
    }

    private IQueryable<Domain.Entities.Pur.PurGoodsReceipt> PostedGrns(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? vendorId)
    {
        var q = _db.PurGoodsReceipts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted"
                        && x.ReceivedAt >= from && x.ReceivedAt <= to);
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        return q;
    }

    private static void EnsureRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from) throw new AppException("to ≥ from.");
        if ((to - from).TotalDays > 730) throw new AppException("Khoảng tối đa 730 ngày.");
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

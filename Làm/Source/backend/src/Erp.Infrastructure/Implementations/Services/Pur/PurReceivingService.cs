using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Application.Interfaces.Services.Pur;
using Erp.Domain.Base;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pur;

public sealed class PurReceivingService : IPurReceivingService
{
    private readonly AppDbContext _db;
    private readonly IInvStockService _inv;
    public PurReceivingService(AppDbContext db, IInvStockService inv)
    {
        _db = db;
        _inv = inv;
    }

    public async Task<IReadOnlyList<PurGrnDto>> ListGrnsAsync(
        Guid tenantId, Guid? poId = null, CancellationToken ct = default)
    {
        var q = _db.PurGoodsReceipts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (poId is Guid pid) q = q.Where(x => x.PoId == pid);
        var list = await q.OrderByDescending(x => x.ReceivedAt).Take(200).ToListAsync(ct);
        return await MapGrnsAsync(tenantId, list, ct);
    }

    public async Task<PurGrnDetailDto> GetGrnDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var grn = await RequireAsync(_db.PurGoodsReceipts, tenantId, id, "phiếu nhận", ct);
        var lines = await _db.PurGrnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.GrnId == id && !x.IsDeleted)
            .OrderBy(x => x.ProductCode).ToListAsync(ct);
        return new PurGrnDetailDto(
            (await MapGrnsAsync(tenantId, [grn], ct))[0],
            lines.Select(MapGrnLine).ToList());
    }

    public async Task<PurGrnDto> CreateGrnFromPoAsync(
        Guid tenantId, Guid userId, PurGrnCreateRequest req, CancellationToken ct = default)
    {
        var po = await RequireAsync(_db.PurPurchaseOrders, tenantId, req.PoId, "PO", ct);
        if (po.Status != "Sent")
            throw new AppException("Chỉ nhận hàng PO Sent (chưa đóng/hủy).");

        var poLines = await _db.PurPoLines
            .Where(x => x.TenantId == tenantId && x.PoId == po.Id && !x.IsDeleted && x.ReceivedQty < x.Qty)
            .OrderBy(x => x.ProductCode).ToListAsync(ct);
        if (poLines.Count == 0) throw new AppException("PO đã nhận đủ — không còn dòng mở.");

        var grn = new PurGoodsReceipt
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "GRN", _db.PurGoodsReceipts, ct),
            PoId = po.Id,
            VendorId = po.VendorId,
            Status = "Draft",
            ReceivedAt = DateTimeOffset.UtcNow,
            QualityNote = Opt(req.QualityNote, 1000),
            InventoryPushStatus = "None",
            Note = Opt(req.Note, 1000),
            CreatedBy = userId
        };
        _db.PurGoodsReceipts.Add(grn);
        await _db.SaveChangesAsync(ct);

        foreach (var pl in poLines)
        {
            var remain = pl.Qty - pl.ReceivedQty;
            _db.PurGrnLines.Add(new PurGrnLine
            {
                TenantId = tenantId, GrnId = grn.Id, PoLineId = pl.Id,
                ProductCode = pl.ProductCode, ProductName = pl.ProductName,
                OrderedQty = remain, ReceivedQty = remain, AcceptedQty = remain, RejectedQty = 0,
                Unit = pl.Unit, UnitPrice = pl.UnitPrice, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapGrnsAsync(tenantId, [grn], ct))[0];
    }

    public async Task<PurGrnLineDto> UpdateGrnLineAsync(
        Guid tenantId, Guid userId, Guid grnId, PurGrnLineUpdateRequest req, CancellationToken ct = default)
    {
        var grn = await RequireAsync(_db.PurGoodsReceipts, tenantId, grnId, "phiếu nhận", ct);
        if (grn.Status != "Draft") throw new AppException("Chỉ sửa GRN Draft.");
        var line = await RequireAsync(_db.PurGrnLines, tenantId, req.LineId, "dòng GRN", ct);
        if (line.GrnId != grnId) throw new AppException("Dòng không thuộc GRN.");
        if (req.ReceivedQty < 0 || req.AcceptedQty < 0 || req.RejectedQty < 0)
            throw new AppException("SL ≥ 0.");
        if (req.AcceptedQty + req.RejectedQty > req.ReceivedQty + 0.0001m)
            throw new AppException("Accepted + Rejected ≤ Received.");

        line.ReceivedQty = req.ReceivedQty;
        line.AcceptedQty = req.AcceptedQty;
        line.RejectedQty = req.RejectedQty;
        line.UpdatedBy = userId;
        if (req.RejectedQty > 0 && string.IsNullOrWhiteSpace(grn.QualityNote))
            grn.QualityNote = "Có SL từ chối chất lượng / lệch.";
        grn.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapGrnLine(line);
    }

    public async Task<PurGrnDto> PostGrnAsync(
        Guid tenantId, Guid userId, Guid grnId, CancellationToken ct = default)
    {
        var grn = await RequireAsync(_db.PurGoodsReceipts, tenantId, grnId, "phiếu nhận", ct);
        if (grn.Status != "Draft") throw new AppException("GRN đã post.");
        var lines = await _db.PurGrnLines
            .Where(x => x.TenantId == tenantId && x.GrnId == grnId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("GRN trống.");
        if (lines.All(x => x.AcceptedQty <= 0))
            throw new AppException("Cần ít nhất 1 dòng Accepted > 0.");

        foreach (var line in lines.Where(x => x.PoLineId.HasValue && x.AcceptedQty > 0))
        {
            var pl = await RequireAsync(_db.PurPoLines, tenantId, line.PoLineId!.Value, "dòng PO", ct);
            var next = pl.ReceivedQty + line.AcceptedQty;
            if (next > pl.Qty + 0.0001m)
                throw new AppException($"Vượt PO {pl.ProductCode}: nhận {next}/{pl.Qty}.");
            pl.ReceivedQty = next;
            pl.UpdatedBy = userId;
        }

        grn.Status = "Posted";
        grn.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await TryPushInvAsync(tenantId, userId, grn, ct);
        return (await MapGrnsAsync(tenantId, [grn], ct))[0];
    }

    public async Task<PurGrnDto> PushGrnToInventoryAsync(
        Guid tenantId, Guid userId, Guid grnId, CancellationToken ct = default)
    {
        var grn = await RequireAsync(_db.PurGoodsReceipts, tenantId, grnId, "phiếu nhận", ct);
        if (grn.Status != "Posted") throw new AppException("Chỉ đẩy INV khi GRN Posted.");
        await TryPushInvAsync(tenantId, userId, grn, ct);
        return (await MapGrnsAsync(tenantId, [grn], ct))[0];
    }

    private async Task TryPushInvAsync(Guid tenantId, Guid userId, PurGoodsReceipt grn, CancellationToken ct)
    {
        try
        {
            await _inv.PostPurchaseReceiptFromGrnAsync(tenantId, userId, grn.Id, null, ct);
            grn.InventoryPushStatus = "Pushed";
        }
        catch
        {
            grn.InventoryPushStatus = "Failed";
        }
        grn.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PurInvoiceDto>> ListInvoicesAsync(
        Guid tenantId, Guid? vendorId = null, CancellationToken ct = default)
    {
        var q = _db.PurVendorInvoices.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        var list = await q.OrderByDescending(x => x.InvoiceDate).Take(200).ToListAsync(ct);
        return await MapInvoicesAsync(tenantId, list, ct);
    }

    public async Task<PurInvoiceDetailDto> GetInvoiceDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var inv = await RequireAsync(_db.PurVendorInvoices, tenantId, id, "hóa đơn", ct);
        var lines = await _db.PurInvoiceLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InvoiceId == id && !x.IsDeleted)
            .OrderBy(x => x.ProductCode).ToListAsync(ct);
        return new PurInvoiceDetailDto(
            (await MapInvoicesAsync(tenantId, [inv], ct))[0],
            lines.Select(MapInvLine).ToList());
    }

    public async Task<PurInvoiceDto> CreateInvoiceAsync(
        Guid tenantId, Guid userId, PurInvoiceCreateRequest req, CancellationToken ct = default)
    {
        await RequireAsync(_db.PurVendors, tenantId, req.VendorId, "NCC", ct);
        PurPurchaseOrder? po = null;
        if (req.PoId is Guid pid)
        {
            po = await RequireAsync(_db.PurPurchaseOrders, tenantId, pid, "PO", ct);
            if (po.VendorId != req.VendorId) throw new AppException("PO không thuộc NCC.");
        }

        var inv = new PurVendorInvoice
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "VIN", _db.PurVendorInvoices, ct),
            VendorId = req.VendorId,
            PoId = req.PoId,
            InvoiceNumber = Req(req.InvoiceNumber, 80, "Số HĐ"),
            InvoiceDate = req.InvoiceDate ?? DateTimeOffset.UtcNow,
            Status = "Draft",
            TaxAmount = req.TaxAmount ?? 0,
            MatchStatus = "Pending",
            ApPushStatus = "None",
            Note = Opt(req.Note, 1000),
            CreatedBy = userId
        };
        _db.PurVendorInvoices.Add(inv);
        await _db.SaveChangesAsync(ct);

        // Prefill from accepted GRN lines of PO if linked
        if (po is not null)
        {
            var grnIds = await _db.PurGoodsReceipts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.PoId == po.Id && x.Status == "Posted" && !x.IsDeleted)
                .Select(x => x.Id).ToListAsync(ct);
            var grnLines = await _db.PurGrnLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && grnIds.Contains(x.GrnId) && !x.IsDeleted && x.AcceptedQty > 0)
                .ToListAsync(ct);
            foreach (var gl in grnLines)
            {
                var open = gl.AcceptedQty;
                if (gl.PoLineId is Guid pid2)
                {
                    var pl = await _db.PurPoLines.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == pid2 && !x.IsDeleted, ct);
                    if (pl is not null) open = Math.Max(0, pl.ReceivedQty - pl.InvoicedQty);
                }
                if (open <= 0) continue;
                _db.PurInvoiceLines.Add(new PurInvoiceLine
                {
                    TenantId = tenantId, InvoiceId = inv.Id,
                    PoLineId = gl.PoLineId, GrnLineId = gl.Id,
                    ProductCode = gl.ProductCode, ProductName = gl.ProductName,
                    Qty = open, UnitPrice = gl.UnitPrice,
                    LineAmount = Math.Round(open * gl.UnitPrice, 2),
                    CreatedBy = userId
                });
            }
            await _db.SaveChangesAsync(ct);
            await RecalcInvoiceAsync(tenantId, inv, ct);
        }

        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<PurInvoiceLineDto> UpsertInvoiceLineAsync(
        Guid tenantId, Guid userId, Guid invoiceId, PurInvoiceLineUpsertRequest req, CancellationToken ct = default)
    {
        var inv = await RequireAsync(_db.PurVendorInvoices, tenantId, invoiceId, "hóa đơn", ct);
        if (inv.Status is not ("Draft" or "Disputed"))
            throw new AppException("Chỉ sửa hóa đơn Draft/Disputed.");
        if (req.Qty <= 0 || req.UnitPrice < 0) throw new AppException("Qty/giá không hợp lệ.");

        PurInvoiceLine line;
        if (req.Id is Guid id)
        {
            line = await RequireAsync(_db.PurInvoiceLines, tenantId, id, "dòng HĐ", ct);
            if (line.InvoiceId != invoiceId) throw new AppException("Dòng không thuộc HĐ.");
            line.UpdatedBy = userId;
        }
        else
        {
            line = new PurInvoiceLine { TenantId = tenantId, InvoiceId = invoiceId, CreatedBy = userId };
            _db.PurInvoiceLines.Add(line);
        }
        line.PoLineId = req.PoLineId;
        line.GrnLineId = req.GrnLineId;
        line.ProductCode = NormCode(req.ProductCode);
        line.ProductName = Req(req.ProductName, 200, "Tên SP");
        line.Qty = req.Qty;
        line.UnitPrice = req.UnitPrice;
        line.LineAmount = Math.Round(req.Qty * req.UnitPrice, 2);
        await _db.SaveChangesAsync(ct);
        await RecalcInvoiceAsync(tenantId, inv, ct);
        return MapInvLine(line);
    }

    public async Task<PurInvoiceDto> MatchThreeWayAsync(
        Guid tenantId, Guid userId, Guid invoiceId, CancellationToken ct = default)
    {
        var inv = await RequireAsync(_db.PurVendorInvoices, tenantId, invoiceId, "hóa đơn", ct);
        if (inv.MatchStatus == "Matched" && inv.Status is "Matched" or "Posted")
            return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
        if (inv.PoId is null) throw new AppException("HĐ cần gắn PO để đối soát 3 chiều.");
        var lines = await _db.PurInvoiceLines
            .Where(x => x.TenantId == tenantId && x.InvoiceId == invoiceId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("HĐ chưa có dòng.");

        var poLines = await _db.PurPoLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PoId == inv.PoId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);

        var issues = new List<string>();
        foreach (var line in lines)
        {
            if (line.PoLineId is not Guid plid || !poLines.TryGetValue(plid, out var pl))
            {
                issues.Add($"{line.ProductCode}: không khớp dòng PO");
                continue;
            }
            var invQty = line.Qty;
            if (invQty > pl.ReceivedQty + 0.0001m)
                issues.Add($"{line.ProductCode}: HĐ {invQty} > đã nhận {pl.ReceivedQty}");
            if (Math.Abs(line.UnitPrice - pl.UnitPrice) > 0.01m)
                issues.Add($"{line.ProductCode}: giá HĐ {line.UnitPrice} ≠ PO {pl.UnitPrice}");
        }

        var poTotal = poLines.Values.Sum(x => x.Qty * x.UnitPrice);
        var grnAcceptedValue = await (
            from g in _db.PurGoodsReceipts.AsNoTracking()
            join l in _db.PurGrnLines.AsNoTracking() on g.Id equals l.GrnId
            where g.TenantId == tenantId && g.PoId == inv.PoId && g.Status == "Posted" && !g.IsDeleted && !l.IsDeleted
            select l.AcceptedQty * l.UnitPrice).SumAsync(ct);

        if (Math.Abs(inv.SubTotal - grnAcceptedValue) > 1m && grnAcceptedValue > 0)
            issues.Add($"SubTotal HĐ {inv.SubTotal:N0} ≠ GRN {grnAcceptedValue:N0}");

        if (issues.Count == 0)
        {
            inv.MatchStatus = "Matched";
            inv.Status = "Matched";
            inv.MatchNote = $"3-way OK · PO total {poTotal:N0}";
            foreach (var line in lines.Where(x => x.PoLineId.HasValue))
            {
                var pl = await RequireAsync(_db.PurPoLines, tenantId, line.PoLineId!.Value, "dòng PO", ct);
                pl.InvoicedQty = Math.Min(pl.Qty, pl.InvoicedQty + line.Qty);
                pl.UpdatedBy = userId;
            }
        }
        else
        {
            inv.MatchStatus = "Variance";
            inv.Status = "Disputed";
            inv.MatchNote = string.Join("; ", issues.Take(5));
        }
        inv.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<PurInvoiceDto> PushInvoiceToApAsync(
        Guid tenantId, Guid userId, Guid invoiceId, CancellationToken ct = default)
    {
        var inv = await RequireAsync(_db.PurVendorInvoices, tenantId, invoiceId, "hóa đơn", ct);
        if (inv.MatchStatus != "Matched")
            throw new AppException("Chỉ đẩy AP khi đã khớp 3 chiều.");
        inv.ApPushStatus = "Pushed";
        inv.Status = "Posted";
        inv.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    private async Task RecalcInvoiceAsync(Guid tenantId, PurVendorInvoice inv, CancellationToken ct)
    {
        var sub = await _db.PurInvoiceLines
            .Where(x => x.TenantId == tenantId && x.InvoiceId == inv.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.LineAmount, ct) ?? 0;
        inv.SubTotal = Math.Round(sub, 2);
        inv.TotalAmount = Math.Round(inv.SubTotal + inv.TaxAmount, 2);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<PurGrnDto>> MapGrnsAsync(
        Guid tenantId, List<PurGoodsReceipt> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var poIds = list.Select(x => x.PoId).Distinct().ToList();
        var vIds = list.Select(x => x.VendorId).Distinct().ToList();
        var pos = await _db.PurPurchaseOrders.AsNoTracking().Where(x => poIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var agg = await _db.PurGrnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.GrnId) && !x.IsDeleted)
            .GroupBy(x => x.GrnId)
            .Select(g => new
            {
                g.Key, C = g.Count(),
                Rec = g.Sum(x => x.ReceivedQty), Acc = g.Sum(x => x.AcceptedQty), Rej = g.Sum(x => x.RejectedQty)
            }).ToDictionaryAsync(x => x.Key, ct);

        return list.Select(g =>
        {
            agg.TryGetValue(g.Id, out var a);
            return new PurGrnDto(
                g.Id, g.Code, g.PoId, pos.GetValueOrDefault(g.PoId),
                g.VendorId, vendors.GetValueOrDefault(g.VendorId),
                g.Status, g.ReceivedAt, g.QualityNote, g.InventoryPushStatus, g.Note,
                a?.C ?? 0, a?.Rec ?? 0, a?.Acc ?? 0, a?.Rej ?? 0);
        }).ToList();
    }

    private async Task<IReadOnlyList<PurInvoiceDto>> MapInvoicesAsync(
        Guid tenantId, List<PurVendorInvoice> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var vIds = list.Select(x => x.VendorId).Distinct().ToList();
        var poIds = list.Where(x => x.PoId.HasValue).Select(x => x.PoId!.Value).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var pos = poIds.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.PurPurchaseOrders.AsNoTracking().Where(x => poIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var counts = await _db.PurInvoiceLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.InvoiceId) && !x.IsDeleted)
            .GroupBy(x => x.InvoiceId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(i => new PurInvoiceDto(
            i.Id, i.Code, i.VendorId, vendors.GetValueOrDefault(i.VendorId),
            i.PoId, i.PoId is Guid p ? pos.GetValueOrDefault(p) : null,
            i.InvoiceNumber, i.InvoiceDate, i.Status,
            i.SubTotal, i.TaxAmount, i.TotalAmount,
            i.MatchStatus, i.MatchNote, i.ApPushStatus, i.Note,
            counts.GetValueOrDefault(i.Id))).ToList();
    }

    private static PurGrnLineDto MapGrnLine(PurGrnLine l) =>
        new(l.Id, l.GrnId, l.PoLineId, l.ProductCode, l.ProductName,
            l.OrderedQty, l.ReceivedQty, l.AcceptedQty, l.RejectedQty, l.Unit, l.UnitPrice);
    private static PurInvoiceLineDto MapInvLine(PurInvoiceLine l) =>
        new(l.Id, l.InvoiceId, l.PoLineId, l.GrnLineId, l.ProductCode, l.ProductName,
            l.Qty, l.UnitPrice, l.LineAmount);

    private static async Task<T> RequireAsync<T>(DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static async Task<string> NextCodeAsync<T>(
        Guid tenantId, string prefix, DbSet<T> set, CancellationToken ct) where T : TenantEntity
    {
        var p = $"{prefix}-{DateTime.UtcNow:yyyyMM}-";
        var last = await set.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && EF.Property<string>(x, "Code").StartsWith(p))
            .OrderByDescending(x => EF.Property<string>(x, "Code"))
            .Select(x => EF.Property<string>(x, "Code")).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is 0) throw new AppException($"{label} bắt buộc.");
        if (v.Length > max) throw new AppException($"{label} tối đa {max} ký tự.");
        return v;
    }

    private static string? Opt(string? value, int max)
    {
        var v = (value ?? "").Trim();
        if (v.Length == 0) return null;
        if (v.Length > max) throw new AppException($"Tối đa {max} ký tự.");
        return v;
    }
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Application.Interfaces.Services.Pos;
using Erp.Domain.Base;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pos;

public sealed class PosSalesService : IPosSalesService
{
    private static readonly HashSet<string> PayMethods =
        new(StringComparer.OrdinalIgnoreCase) { "Cash", "Transfer", "Card", "Wallet" };

    private readonly AppDbContext _db;
    private readonly IFinRevenueService _rev;
    private readonly IInvStockService _stock;
    public PosSalesService(AppDbContext db, IFinRevenueService rev, IInvStockService stock)
    {
        _db = db;
        _rev = rev;
        _stock = stock;
    }

    public async Task<IReadOnlyList<PosShiftDto>> ListShiftsAsync(
        Guid tenantId, Guid? storeId = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.PosShifts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (storeId is Guid sid) q = q.Where(x => x.StoreId == sid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        var list = await q.OrderByDescending(x => x.OpenedAt).Take(100).ToListAsync(ct);
        return await MapShiftsAsync(tenantId, list, ct);
    }

    public async Task<PosShiftDetailDto> GetShiftDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var shift = await RequireAsync(_db.PosShifts, tenantId, id, "ca bán", ct);
        var sales = await _db.PosSales.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ShiftId == id && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        return new PosShiftDetailDto(
            (await MapShiftsAsync(tenantId, [shift], ct))[0],
            await MapSalesAsync(tenantId, sales, ct));
    }

    public async Task<PosShiftDto> OpenShiftAsync(
        Guid tenantId, Guid userId, PosShiftOpenRequest req, CancellationToken ct = default)
    {
        await RequireAsync(_db.PosStores, tenantId, req.StoreId, "điểm bán", ct);
        if (req.TerminalId is Guid tid)
            await RequireAsync(_db.PosTerminals, tenantId, tid, "quầy/máy", ct);
        if (req.OpeningCash < 0) throw new AppException("Tiền đầu ca ≥ 0.");

        var openExists = await _db.PosShifts.AnyAsync(
            x => x.TenantId == tenantId && x.StoreId == req.StoreId && x.Status == "Open" && !x.IsDeleted
                 && (req.TerminalId == null || x.TerminalId == req.TerminalId), ct);
        if (openExists) throw new AppException("Đã có ca Open trên điểm bán/quầy này.");

        var shift = new PosShift
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "SH", _db.PosShifts, ct),
            StoreId = req.StoreId,
            TerminalId = req.TerminalId,
            CashierUserId = userId,
            OpenedAt = DateTimeOffset.UtcNow,
            OpeningCash = req.OpeningCash,
            Status = "Open",
            Note = Opt(req.Note, 1000),
            CreatedBy = userId
        };
        _db.PosShifts.Add(shift);
        await _db.SaveChangesAsync(ct);
        return (await MapShiftsAsync(tenantId, [shift], ct))[0];
    }

    public async Task<PosShiftDto> CloseShiftAsync(
        Guid tenantId, Guid userId, Guid shiftId, PosShiftCloseRequest req, CancellationToken ct = default)
    {
        var shift = await RequireAsync(_db.PosShifts, tenantId, shiftId, "ca bán", ct);
        if (shift.Status != "Open") throw new AppException("Ca đã đóng.");
        if (req.ClosingCashCounted < 0) throw new AppException("Tiền đếm ≥ 0.");

        var openSales = await _db.PosSales.CountAsync(
            x => x.TenantId == tenantId && x.ShiftId == shiftId && !x.IsDeleted
                 && (x.Status == "Open" || x.Status == "Held"), ct);
        if (openSales > 0) throw new AppException($"Còn {openSales} đơn Open/Held — thanh toán hoặc hủy trước.");

        var paidSaleIds = await _db.PosSales.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.ShiftId == shiftId && !s.IsDeleted
                        && (s.Status == "Paid" || s.Status == "Returned"))
            .Select(s => s.Id).ToListAsync(ct);
        var cashSales = paidSaleIds.Count == 0 ? 0
            : await _db.PosSalePayments.AsNoTracking()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Method == "Cash"
                            && paidSaleIds.Contains(p.SaleId))
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;
        var cashRefunds = await _db.PosReturns.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.ShiftId == shiftId && !r.IsDeleted
                        && r.Status == "Completed" && r.RefundMethod == "Cash")
            .SumAsync(r => (decimal?)r.RefundAmount, ct) ?? 0;

        shift.ClosingCashCounted = req.ClosingCashCounted;
        shift.ExpectedCash = Math.Round(shift.OpeningCash + cashSales - cashRefunds, 2);
        shift.Variance = Math.Round(req.ClosingCashCounted - shift.ExpectedCash.Value, 2);
        shift.ClosedAt = DateTimeOffset.UtcNow;
        shift.Status = "Closed";
        if (!string.IsNullOrWhiteSpace(req.Note)) shift.Note = Opt(req.Note, 1000);
        shift.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        // UC_POS_059 — bắt kịp DT FIN (idempotent; PaySale thường đã ghi nhận).
        var sync = await SyncShiftRevenueToFinAsync(tenantId, userId, shiftId, ct);
        var tag = FormatFinSyncTag(sync);
        var combined = string.IsNullOrWhiteSpace(shift.Note) ? tag : $"{shift.Note.Trim()} | {tag}";
        if (combined.Length > 1000) combined = combined[..1000];
        shift.Note = combined;
        shift.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        return (await MapShiftsAsync(tenantId, [shift], ct))[0];
    }

    public async Task<PosShiftFinSyncResult> SyncShiftRevenueToFinAsync(
        Guid tenantId, Guid userId, Guid shiftId, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.PosShifts, tenantId, shiftId, "ca bán", ct);
        var paidIds = await _db.PosSales.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.ShiftId == shiftId && !s.IsDeleted && s.Status == "Paid")
            .Select(s => s.Id).ToListAsync(ct);

        var existingIds = await _db.FinRevenueDocuments.AsNoTracking()
            .Where(d => d.TenantId == tenantId && !d.IsDeleted && d.SourceModule == "POS"
                        && d.Status != "Void" && d.SourceId != null && paidIds.Contains(d.SourceId.Value))
            .Select(d => d.SourceId!.Value)
            .ToListAsync(ct);
        var had = existingIds.ToHashSet();

        var synced = 0;
        var failed = 0;
        var already = 0;
        foreach (var saleId in paidIds)
        {
            if (had.Contains(saleId))
            {
                already++;
                continue;
            }
            try
            {
                await _rev.RecognizeFromPosAsync(tenantId, userId, saleId, null, ct);
                synced++;
            }
            catch
            {
                failed++;
            }
        }

        return new PosShiftFinSyncResult(
            shiftId, paidIds.Count, synced, already, failed,
            $"FIN ca: {synced} mới, {already} đã có, {failed} lỗi / {paidIds.Count} đơn Paid.");
    }

    public static string FormatFinSyncTag(PosShiftFinSyncResult sync)
        => $"FIN:{sync.SyncedCount}+{sync.AlreadyHadCount}/{sync.PaidSaleCount} fail={sync.FailedCount}";

    public async Task<PosShiftDto> PrintShiftReportAsync(
        Guid tenantId, Guid userId, Guid shiftId, CancellationToken ct = default)
    {
        var shift = await RequireAsync(_db.PosShifts, tenantId, shiftId, "ca bán", ct);
        shift.ReportPrintedAt = DateTimeOffset.UtcNow;
        shift.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapShiftsAsync(tenantId, [shift], ct))[0];
    }

    public async Task<(string FileName, string Content)> BuildShiftReportTextAsync(
        Guid tenantId, Guid userId, Guid shiftId, CancellationToken ct = default)
    {
        var shift = await RequireAsync(_db.PosShifts, tenantId, shiftId, "ca bán", ct);
        var store = await _db.PosStores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shift.StoreId && x.TenantId == tenantId, ct);
        var cashier = await _db.Users.AsNoTracking()
            .Where(x => x.Id == shift.CashierUserId)
            .Select(x => x.DisplayName ?? x.Username).FirstOrDefaultAsync(ct);

        var sales = await _db.PosSales.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ShiftId == shiftId && !x.IsDeleted)
            .ToListAsync(ct);
        var saleIds = sales.Select(x => x.Id).ToList();
        var payByMethod = await _db.PosSalePayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId) && !x.IsDeleted)
            .GroupBy(x => x.Method)
            .Select(g => new { Method = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
            .ToListAsync(ct);

        var paid = sales.Where(x => x.Status is "Paid" or "Returned").ToList();
        var revenue = paid.Sum(x => x.TotalAmount);
        var returned = sales.Sum(x => x.ReturnedAmount);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"BÁO CÁO CA {shift.Code}");
        sb.AppendLine($"Điểm bán : {store?.Name ?? "—"}");
        sb.AppendLine($"Thu ngân : {cashier ?? "—"}");
        sb.AppendLine($"Mở ca    : {shift.OpenedAt.ToLocalTime():dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Đóng ca  : {(shift.ClosedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "(đang mở)")}");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"Đơn đã thanh toán : {paid.Count}/{sales.Count}");
        sb.AppendLine($"Doanh thu         : {revenue:N0}");
        if (returned > 0) sb.AppendLine($"Hoàn trả          : {returned:N0}");
        foreach (var p in payByMethod.OrderByDescending(x => x.Total))
            sb.AppendLine($"  {p.Method,-10}: {p.Total,14:N0} ({p.Count} lượt)");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"Tiền đầu ca       : {shift.OpeningCash:N0}");
        sb.AppendLine($"Tiền mặt dự kiến  : {(shift.ExpectedCash?.ToString("N0") ?? "—")}");
        sb.AppendLine($"Tiền mặt kiểm đếm : {(shift.ClosingCashCounted?.ToString("N0") ?? "—")}");
        sb.AppendLine($"Chênh lệch        : {(shift.Variance?.ToString("N0") ?? "—")}");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"In lúc {DateTimeOffset.UtcNow.ToLocalTime():dd/MM/yyyy HH:mm}");

        shift.ReportPrintedAt = DateTimeOffset.UtcNow;
        shift.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return ($"{shift.Code}-baocao-ca.txt", sb.ToString());
    }

    public async Task<IReadOnlyList<PosSaleDto>> ListSalesAsync(
        Guid tenantId, Guid? shiftId = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.PosSales.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (shiftId is Guid sid) q = q.Where(x => x.ShiftId == sid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapSalesAsync(tenantId, list, ct);
    }

    public async Task<PosSaleDetailDto> GetSaleDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, id, "đơn bán", ct);
        var lines = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == id && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var pays = await _db.PosSalePayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == id && !x.IsDeleted)
            .OrderBy(x => x.PaidAt).ToListAsync(ct);
        return new PosSaleDetailDto(
            (await MapSalesAsync(tenantId, [sale], ct))[0],
            lines.Select(MapLine).ToList(),
            pays.Select(MapPay).ToList());
    }

    public async Task<PosSaleDto> OpenSaleAsync(
        Guid tenantId, Guid userId, PosSaleOpenRequest req, CancellationToken ct = default)
    {
        var shift = await RequireAsync(_db.PosShifts, tenantId, req.ShiftId, "ca bán", ct);
        if (shift.Status != "Open") throw new AppException("Ca đã đóng — không mở đơn.");
        var sale = new PosSale
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "PS", _db.PosSales, ct),
            ShiftId = shift.Id,
            StoreId = shift.StoreId,
            TerminalId = shift.TerminalId,
            Status = "Open",
            AreaName = Opt(req.AreaName, 100),
            Note = Opt(req.Note, 1000),
            CreatedBy = userId
        };
        _db.PosSales.Add(sale);
        await _db.SaveChangesAsync(ct);
        return (await MapSalesAsync(tenantId, [sale], ct))[0];
    }

    public async Task<PosSaleLineDto> UpsertSaleLineAsync(
        Guid tenantId, Guid userId, Guid saleId, PosSaleLineUpsertRequest req, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        EnsureSaleEditable(sale);
        if (req.Quantity <= 0) throw new AppException("Số lượng > 0.");

        PosProduct? product = null;
        if (req.ProductId is Guid pid)
            product = await RequireAsync(_db.PosProducts, tenantId, pid, "sản phẩm", ct);
        else if (!string.IsNullOrWhiteSpace(req.ProductCode))
            product = await _db.PosProducts.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted
                     && x.Code == req.ProductCode.Trim().ToUpperInvariant(), ct);

        if (product is not null && product.Status == "Suspended")
            throw new AppException("Sản phẩm đang ngưng bán.");

        var code = product?.Code ?? NormCode(req.ProductCode);
        var name = product?.Name ?? Req(req.ProductName, 200, "Tên SP");
        var (unitPrice, taxPct) = await ResolvePriceAsync(tenantId, sale.StoreId, product?.Id, req, ct);

        PosSaleLine line;
        if (req.Id is Guid lid)
        {
            line = await RequireAsync(_db.PosSaleLines, tenantId, lid, "dòng đơn", ct);
            if (line.SaleId != saleId) throw new AppException("Dòng không thuộc đơn.");
            if (line.Status == "Cancelled") throw new AppException("Dòng đã hủy.");
            line.UpdatedBy = userId;
        }
        else
        {
            var maxNo = await _db.PosSaleLines
                .Where(x => x.TenantId == tenantId && x.SaleId == saleId && !x.IsDeleted)
                .Select(x => (int?)x.LineNo).MaxAsync(ct) ?? 0;
            line = new PosSaleLine
            {
                TenantId = tenantId, SaleId = saleId, LineNo = maxNo + 1,
                Status = "Active", CreatedBy = userId
            };
            _db.PosSaleLines.Add(line);
        }

        line.ProductId = product?.Id;
        line.ProductCode = code;
        line.ProductName = name;
        line.Quantity = req.Quantity;
        line.UnitPrice = unitPrice;
        line.TaxRatePct = taxPct;
        line.LineAmount = Math.Round(line.Quantity * line.UnitPrice * (1 + line.TaxRatePct / 100m), 2);
        await _db.SaveChangesAsync(ct);
        await RecalcSaleAsync(tenantId, sale, userId, ct);
        return MapLine(line);
    }

    public async Task<PosSaleDto> HoldSaleAsync(
        Guid tenantId, Guid userId, Guid saleId, PosSaleHoldRequest req, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status is not ("Open" or "Held")) throw new AppException("Chỉ giữ đơn Open.");
        sale.Status = "Held";
        if (!string.IsNullOrWhiteSpace(req.Note)) sale.Note = Opt(req.Note, 1000);
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapSalesAsync(tenantId, [sale], ct))[0];
    }

    public async Task<PosSaleDto> ResumeSaleAsync(
        Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status != "Held") throw new AppException("Chỉ mở lại đơn Held.");
        var shift = await RequireAsync(_db.PosShifts, tenantId, sale.ShiftId, "ca bán", ct);
        if (shift.Status != "Open") throw new AppException("Ca đã đóng.");
        sale.Status = "Open";
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapSalesAsync(tenantId, [sale], ct))[0];
    }

    public async Task<PosSaleLineDto> CancelSaleLineAsync(
        Guid tenantId, Guid userId, Guid saleId, Guid lineId, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        EnsureSaleEditable(sale);
        var line = await RequireAsync(_db.PosSaleLines, tenantId, lineId, "dòng đơn", ct);
        if (line.SaleId != saleId) throw new AppException("Dòng không thuộc đơn.");
        line.Status = "Cancelled";
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await RecalcSaleAsync(tenantId, sale, userId, ct);
        return MapLine(line);
    }

    public async Task<PosSaleDto> CancelSaleAsync(
        Guid tenantId, Guid userId, Guid saleId, string? note = null, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status is "Paid" or "Returned") throw new AppException("Đơn đã thanh toán — dùng trả hàng.");
        if (sale.Status == "Cancelled") throw new AppException("Đơn đã hủy.");
        sale.Status = "Cancelled";
        if (!string.IsNullOrWhiteSpace(note)) sale.Note = Opt(note, 1000);
        sale.UpdatedBy = userId;
        var lines = await _db.PosSaleLines
            .Where(x => x.TenantId == tenantId && x.SaleId == saleId && !x.IsDeleted && x.Status == "Active")
            .ToListAsync(ct);
        foreach (var l in lines) { l.Status = "Cancelled"; l.UpdatedBy = userId; }
        await _db.SaveChangesAsync(ct);
        return (await MapSalesAsync(tenantId, [sale], ct))[0];
    }

    public async Task<PosSalePaymentDto> PaySaleAsync(
        Guid tenantId, Guid userId, Guid saleId, PosSalePayRequest req, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status is "Cancelled" or "Returned") throw new AppException("Đơn không thể thanh toán.");
        if (sale.Status == "Held") throw new AppException("Mở lại đơn Held trước khi thanh toán.");
        if (sale.DiscountApprovalStatus == "Pending")
            throw new AppException("Giảm tay đang chờ duyệt — không thanh toán.");
        var method = PayMethods.FirstOrDefault(x => x.Equals(req.Method, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("HT: Cash | Transfer | Card | Wallet.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        if (sale.TotalAmount <= 0)
            throw new AppException("Đơn chưa có giá trị — thêm sản phẩm trước.");

        var remain = sale.TotalAmount - sale.PaidAmount;
        if (req.Amount > remain + 0.01m) throw new AppException($"Vượt còn lại ({remain:N0}).");

        var willBePaid = sale.PaidAmount + req.Amount + 0.01m >= sale.TotalAmount;
        if (willBePaid)
            await DeductBomStockForSaleAsync(tenantId, userId, sale, ct);

        var pay = new PosSalePayment
        {
            TenantId = tenantId, SaleId = saleId,
            Code = await NextCodeAsync(tenantId, "PP", _db.PosSalePayments, ct),
            PaidAt = DateTimeOffset.UtcNow, Amount = req.Amount, Method = method,
            Note = Opt(req.Note, 1000), CreatedBy = userId
        };
        _db.PosSalePayments.Add(pay);
        sale.PaidAmount = Math.Round(sale.PaidAmount + req.Amount, 2);
        if (willBePaid)
        {
            sale.Status = "Paid";
            sale.PaidAt = DateTimeOffset.UtcNow;
            if (sale.VoucherId is Guid vid)
            {
                var voucher = await _db.PosVouchers.FirstOrDefaultAsync(
                    x => x.Id == vid && x.TenantId == tenantId && !x.IsDeleted, ct);
                if (voucher is not null)
                {
                    voucher.UsedCount += 1;
                    if (voucher.UsedCount >= voucher.MaxUses) voucher.Status = "Exhausted";
                    voucher.UpdatedBy = userId;
                }
            }
        }
        else sale.Status = "Open";
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        if (sale.Status == "Paid")
        {
            try { await _rev.RecognizeFromPosAsync(tenantId, userId, sale.Id, null, ct); }
            catch (AppException) { /* FIN chưa sẵn sàng — bỏ qua */ }
        }
        return MapPay(pay);
    }

    public async Task<IReadOnlyList<PosStockAlertDto>> ListStockAlertsAsync(
        Guid tenantId, Guid? storeId = null, CancellationToken ct = default)
    {
        Guid? whFilter = null;
        if (storeId is Guid sid)
        {
            var store = await RequireAsync(_db.PosStores, tenantId, sid, "điểm bán", ct);
            whFilter = store.WarehouseId;
        }

        var warehouses = await _db.InvWarehouses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active"
                        && (whFilter == null || x.Id == whFilter))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        if (warehouses.Count == 0) return Array.Empty<PosStockAlertDto>();

        var whIds = warehouses.Keys.ToList();
        var balances = await _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && whIds.Contains(x.WarehouseId))
            .GroupBy(x => new { x.WarehouseId, x.SkuId })
            .Select(g => new { g.Key.WarehouseId, g.Key.SkuId, Qty = g.Sum(x => x.QtyOnHand) })
            .ToListAsync(ct);
        var skuIds = balances.Select(b => b.SkuId).Distinct().ToList();
        var skus = await _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && skuIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var rows = new List<PosStockAlertDto>();
        foreach (var b in balances)
        {
            if (!skus.TryGetValue(b.SkuId, out var sku)) continue;
            string? alert = null;
            if (sku.MinQty is decimal min && b.Qty <= min) alert = "BelowMin";
            else if (sku.ReorderQty is decimal reo && b.Qty <= reo) alert = "NearReorder";
            else if (b.Qty <= 0) alert = "OutOfStock";
            if (alert is null) continue;
            rows.Add(new PosStockAlertDto(
                b.WarehouseId, warehouses.GetValueOrDefault(b.WarehouseId),
                sku.Id, sku.Code, sku.Name, b.Qty, sku.MinQty, sku.ReorderQty, alert));
        }
        return rows.OrderBy(x => x.AlertType).ThenBy(x => x.SkuCode).Take(100).ToList();
    }

    /// <summary>UC_POS_054 — trừ tồn INV theo BOM khi đơn Paid (idempotent theo Ref POS).</summary>
    private async Task DeductBomStockForSaleAsync(
        Guid tenantId, Guid userId, PosSale sale, CancellationToken ct)
    {
        var already = await _db.InvStockDocs.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted
                 && x.RefModule == "POS" && x.RefId == sale.Id
                 && x.DocType == "Issue" && x.Status == "Posted", ct);
        if (already) return;

        var shift = await RequireAsync(_db.PosShifts, tenantId, sale.ShiftId, "ca bán", ct);
        var store = await RequireAsync(_db.PosStores, tenantId, shift.StoreId, "điểm bán", ct);
        var whId = store.WarehouseId ?? await _db.InvWarehouses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "Active" && !x.IsDeleted)
            .OrderBy(x => x.Code).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        if (whId is null || whId == Guid.Empty)
            throw new AppException("Chưa gắn kho INV cho điểm bán / chưa có kho Active — không trừ tồn BOM.");

        var saleLines = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == sale.Id && !x.IsDeleted && x.Status == "Active")
            .ToListAsync(ct);
        var productIds = saleLines.Where(x => x.ProductId is not null).Select(x => x.ProductId!.Value).Distinct().ToList();
        if (productIds.Count == 0) return;

        var bom = await _db.PosBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && productIds.Contains(x.ProductId))
            .ToListAsync(ct);
        if (bom.Count == 0) return;

        var need = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in saleLines)
        {
            if (line.ProductId is not Guid pid) continue;
            foreach (var b in bom.Where(x => x.ProductId == pid))
            {
                var code = b.MaterialCode.Trim().ToUpperInvariant();
                if (code.Length == 0 || b.Qty <= 0) continue;
                need[code] = need.GetValueOrDefault(code) + Math.Round(b.Qty * line.Quantity, 3);
            }
        }
        if (need.Count == 0) return;

        var codes = need.Keys.ToList();
        var skus = await _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && codes.Contains(x.Code))
            .ToListAsync(ct);
        var byCode = skus.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);
        var missing = need.Keys.Where(k => !byCode.ContainsKey(k)).ToList();
        if (missing.Count > 0)
            throw new AppException($"BOM thiếu SKU INV: {string.Join(", ", missing)}.");

        var docDto = await _stock.CreateDocAsync(
            tenantId, userId,
            new InvStockDocCreateRequest("Issue", "Sales", whId.Value, $"POS sale {sale.Code}"),
            ct);
        var doc = await _db.InvStockDocs.FirstAsync(x => x.Id == docDto.Id && x.TenantId == tenantId, ct);
        doc.RefModule = "POS";
        doc.RefId = sale.Id;
        doc.RefCode = sale.Code;
        doc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        foreach (var (code, qty) in need)
        {
            var sku = byCode[code];
            await _stock.UpsertDocLineAsync(
                tenantId, userId, doc.Id,
                new InvStockDocLineRequest(null, sku.Id, qty, null, null, null),
                ct);
        }
        await _stock.PostDocAsync(tenantId, userId, doc.Id, ct);
    }

    public async Task<PosSaleDto> PrintReceiptAsync(
        Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status is not ("Paid" or "Returned"))
            throw new AppException("Chỉ in hóa đơn đơn đã thanh toán.");
        sale.ReceiptPrintedAt = DateTimeOffset.UtcNow;
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapSalesAsync(tenantId, [sale], ct))[0];
    }

    public async Task<(string FileName, string Content)> BuildReceiptTextAsync(
        Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, saleId, "đơn bán", ct);
        if (sale.Status is not ("Paid" or "Returned"))
            throw new AppException("Chỉ in hóa đơn đơn đã thanh toán.");

        var store = await _db.PosStores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sale.StoreId && x.TenantId == tenantId, ct);
        var lines = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == saleId && !x.IsDeleted && x.Status == "Active")
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var pays = await _db.PosSalePayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == saleId && !x.IsDeleted)
            .OrderBy(x => x.PaidAt).ToListAsync(ct);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(store?.Name ?? "ĐIỂM BÁN");
        if (!string.IsNullOrWhiteSpace(store?.Address)) sb.AppendLine(store!.Address);
        sb.AppendLine(new string('=', 42));
        sb.AppendLine("HÓA ĐƠN BÁN LẺ");
        sb.AppendLine($"Số: {sale.Code}");
        sb.AppendLine($"Ngày: {(sale.PaidAt ?? sale.CreatedAt).ToLocalTime():dd/MM/yyyy HH:mm}");
        if (!string.IsNullOrWhiteSpace(sale.AreaName)) sb.AppendLine($"Khu vực: {sale.AreaName}");
        sb.AppendLine(new string('-', 42));
        foreach (var l in lines)
        {
            sb.AppendLine(l.ProductName);
            sb.AppendLine($"  {l.Quantity:0.##} x {l.UnitPrice:N0} = {l.LineAmount,14:N0}");
        }
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"Tạm tính  : {sale.SubTotal,14:N0}");
        if (sale.DiscountAmount > 0)
        {
            var src = sale.DiscountSource switch
            {
                "Voucher" => $" (voucher {sale.AppliedVoucherCode})",
                "Promotion" => " (CTKM)",
                "Manual" => " (giảm tay)",
                _ => "",
            };
            sb.AppendLine($"Giảm giá  : {sale.DiscountAmount,14:N0}{src}");
        }
        if (sale.TaxAmount > 0) sb.AppendLine($"Thuế      : {sale.TaxAmount,14:N0}");
        sb.AppendLine($"TỔNG CỘNG : {sale.TotalAmount,14:N0}");
        foreach (var p in pays)
            sb.AppendLine($"  {p.Method,-8}: {p.Amount,14:N0}");
        if (sale.ReturnedAmount > 0)
            sb.AppendLine($"Đã hoàn   : {sale.ReturnedAmount,14:N0}");
        sb.AppendLine(new string('=', 42));
        sb.AppendLine("Cảm ơn quý khách!");

        sale.ReceiptPrintedAt = DateTimeOffset.UtcNow;
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return ($"{sale.Code}-hoadon.txt", sb.ToString());
    }

    public async Task<IReadOnlyList<PosReturnDto>> ListReturnsAsync(
        Guid tenantId, Guid? saleId = null, CancellationToken ct = default)
    {
        var q = _db.PosReturns.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (saleId is Guid sid) q = q.Where(x => x.SaleId == sid);
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        return await MapReturnsAsync(tenantId, list, ct);
    }

    public async Task<PosReturnDetailDto> GetReturnDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var ret = await RequireAsync(_db.PosReturns, tenantId, id, "phiếu trả", ct);
        var lines = await _db.PosReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReturnId == id && !x.IsDeleted).ToListAsync(ct);
        return new PosReturnDetailDto(
            (await MapReturnsAsync(tenantId, [ret], ct))[0],
            lines.Select(MapRetLine).ToList());
    }

    public async Task<PosReturnDto> CreateReturnAsync(
        Guid tenantId, Guid userId, PosReturnCreateRequest req, CancellationToken ct = default)
    {
        var sale = await RequireAsync(_db.PosSales, tenantId, req.SaleId, "đơn bán", ct);
        if (sale.Status is not ("Paid" or "Returned"))
            throw new AppException("Chỉ trả hàng đơn đã thanh toán.");
        var ret = new PosReturn
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "PR", _db.PosReturns, ct),
            SaleId = sale.Id,
            ShiftId = sale.ShiftId,
            Status = "Draft",
            Reason = Opt(req.Reason, 500),
            CreatedBy = userId
        };
        _db.PosReturns.Add(ret);
        await _db.SaveChangesAsync(ct);
        return (await MapReturnsAsync(tenantId, [ret], ct))[0];
    }

    public async Task<PosReturnLineDto> AddReturnLineAsync(
        Guid tenantId, Guid userId, Guid returnId, PosReturnLineRequest req, CancellationToken ct = default)
    {
        var ret = await RequireAsync(_db.PosReturns, tenantId, returnId, "phiếu trả", ct);
        if (ret.Status != "Draft") throw new AppException("Phiếu trả đã hoàn tất.");
        if (req.Quantity <= 0) throw new AppException("Số lượng > 0.");
        var saleLine = await RequireAsync(_db.PosSaleLines, tenantId, req.SaleLineId, "dòng đơn", ct);
        if (saleLine.SaleId != ret.SaleId) throw new AppException("Dòng không thuộc đơn gốc.");
        if (saleLine.Status != "Active") throw new AppException("Dòng đã hủy.");

        var already = await _db.PosReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.SaleLineId == saleLine.Id
                && _db.PosReturns.Any(r => r.Id == x.ReturnId && r.Status == "Completed" && !r.IsDeleted))
            .SumAsync(x => (decimal?)x.Quantity, ct) ?? 0;
        var draftSame = await _db.PosReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReturnId == returnId && !x.IsDeleted && x.SaleLineId == saleLine.Id)
            .SumAsync(x => (decimal?)x.Quantity, ct) ?? 0;
        if (already + draftSame + req.Quantity > saleLine.Quantity)
            throw new AppException("Số lượng trả vượt dòng gốc.");

        var unit = saleLine.Quantity == 0 ? 0 : saleLine.LineAmount / saleLine.Quantity;
        var line = new PosReturnLine
        {
            TenantId = tenantId, ReturnId = returnId, SaleLineId = saleLine.Id,
            ProductCode = saleLine.ProductCode, ProductName = saleLine.ProductName,
            Quantity = req.Quantity,
            LineAmount = Math.Round(unit * req.Quantity, 2),
            CreatedBy = userId
        };
        _db.PosReturnLines.Add(line);
        await _db.SaveChangesAsync(ct);
        ret.RefundAmount = await _db.PosReturnLines
            .Where(x => x.TenantId == tenantId && x.ReturnId == returnId && !x.IsDeleted)
            .SumAsync(x => x.LineAmount, ct);
        ret.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapRetLine(line);
    }

    public async Task<PosReturnDto> CompleteReturnAsync(
        Guid tenantId, Guid userId, Guid returnId, PosReturnCompleteRequest req, CancellationToken ct = default)
    {
        var ret = await RequireAsync(_db.PosReturns, tenantId, returnId, "phiếu trả", ct);
        if (ret.Status != "Draft") throw new AppException("Phiếu trả đã hoàn tất.");
        var method = PayMethods.FirstOrDefault(x => x.Equals(req.RefundMethod, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("HT hoàn: Cash | Transfer | Card | Wallet.");
        var lineCount = await _db.PosReturnLines.CountAsync(
            x => x.TenantId == tenantId && x.ReturnId == returnId && !x.IsDeleted, ct);
        if (lineCount == 0) throw new AppException("Chưa có dòng trả.");

        ret.RefundMethod = method;
        ret.Status = "Completed";
        ret.CompletedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Reason)) ret.Reason = Opt(req.Reason, 500);
        ret.UpdatedBy = userId;

        var sale = await RequireAsync(_db.PosSales, tenantId, ret.SaleId, "đơn bán", ct);
        sale.ReturnedAmount = Math.Round(sale.ReturnedAmount + ret.RefundAmount, 2);
        sale.Status = "Returned";
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapReturnsAsync(tenantId, [ret], ct))[0];
    }

    private async Task<(decimal UnitPrice, decimal TaxPct)> ResolvePriceAsync(
        Guid tenantId, Guid storeId, Guid? productId, PosSaleLineUpsertRequest req, CancellationToken ct)
    {
        decimal? price = req.UnitPrice;
        decimal tax = req.TaxRatePct ?? 0;
        if (productId is Guid pid)
        {
            var pl = await _db.PosPriceLists.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.StoreId == storeId && x.Status == "Active" && !x.IsDeleted)
                .OrderBy(x => x.Code).Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (pl != Guid.Empty)
            {
                var item = await _db.PosPriceListItems.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PriceListId == pl
                                              && x.ProductId == pid && !x.IsDeleted, ct);
                if (item is not null)
                {
                    price ??= item.Price;
                    if (req.TaxRatePct is null && item.TaxRateId is Guid trid)
                    {
                        var tr = await _db.PosTaxRates.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == trid && !x.IsDeleted, ct);
                        if (tr is not null) tax = tr.RatePct;
                    }
                }
            }
            if (req.TaxRatePct is null && tax == 0)
            {
                var def = await _db.PosTaxRates.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsDefault && x.IsActive && !x.IsDeleted, ct);
                if (def is not null) tax = def.RatePct;
            }
        }
        if (price is null) throw new AppException("Thiếu đơn giá (bảng giá hoặc nhập tay).");
        if (price < 0) throw new AppException("Đơn giá ≥ 0.");
        if (tax is < 0 or > 100) throw new AppException("Thuế 0–100%.");
        return (price.Value, tax);
    }

    private async Task RecalcSaleAsync(Guid tenantId, PosSale sale, Guid userId, CancellationToken ct)
    {
        var lines = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleId == sale.Id && !x.IsDeleted && x.Status == "Active")
            .ToListAsync(ct);
        var sub = Math.Round(lines.Sum(x => Math.Round(x.Quantity * x.UnitPrice, 2)), 2);
        var tax = Math.Round(lines.Sum(x => Math.Round(x.Quantity * x.UnitPrice * x.TaxRatePct / 100m, 2)), 2);
        sale.SubTotal = sub;
        sale.TaxAmount = tax;
        var baseAmt = sub + tax;
        decimal discount = 0;

        if (sale.DiscountSource is ("Promotion" or "Voucher") && sale.PromotionId is Guid pid)
        {
            var promo = await _db.PosPromotions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == pid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (promo is not null && baseAmt >= promo.MinOrderAmount)
            {
                discount = promo.DiscountType == "Percent"
                    ? Math.Round(baseAmt * promo.DiscountValue / 100m, 2)
                    : Math.Min(promo.DiscountValue, baseAmt);
            }
            else if (promo is not null && baseAmt < promo.MinOrderAmount)
                discount = 0;
        }
        else if (sale.DiscountSource == "Manual" && sale.DiscountApprovalStatus == "Approved"
                 && sale.ManualDiscountType is string mt)
        {
            discount = mt == "Percent"
                ? Math.Round(baseAmt * sale.ManualDiscountValue / 100m, 2)
                : Math.Min(sale.ManualDiscountValue, baseAmt);
        }

        sale.DiscountAmount = discount;
        sale.TotalAmount = Math.Max(0, baseAmt - discount);
        sale.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    private static void EnsureSaleEditable(PosSale sale)
    {
        if (sale.Status is "Paid" or "Cancelled" or "Returned")
            throw new AppException("Đơn đã khóa — không chỉnh dòng.");
        if (sale.Status == "Held")
            throw new AppException("Đơn đang giữ — mở lại trước khi sửa.");
    }

    private async Task<IReadOnlyList<PosShiftDto>> MapShiftsAsync(
        Guid tenantId, List<PosShift> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var sids = list.Select(x => x.StoreId).Distinct().ToList();
        var tids = list.Where(x => x.TerminalId.HasValue).Select(x => x.TerminalId!.Value).Distinct().ToList();
        var uids = list.Select(x => x.CashierUserId).Distinct().ToList();
        var stores = await _db.PosStores.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var terms = tids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.PosTerminals.AsNoTracking().Where(x => tids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var saleAgg = await _db.PosSales.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ShiftId) && !x.IsDeleted)
            .GroupBy(x => x.ShiftId)
            .Select(g => new
            {
                g.Key,
                SalesTotal = g.Where(x => x.Status == "Paid" || x.Status == "Returned").Sum(x => x.TotalAmount - x.ReturnedAmount),
                SaleCount = g.Count(x => x.Status == "Paid" || x.Status == "Returned"),
                OpenCount = g.Count(x => x.Status == "Open" || x.Status == "Held")
            }).ToDictionaryAsync(x => x.Key, ct);

        var paidSaleIds = await _db.PosSales.AsNoTracking()
            .Where(s => s.TenantId == tenantId && ids.Contains(s.ShiftId) && !s.IsDeleted
                        && (s.Status == "Paid" || s.Status == "Returned"))
            .Select(s => new { s.Id, s.ShiftId }).ToListAsync(ct);
        var paidIdSet = paidSaleIds.Select(x => x.Id).ToHashSet();
        var saleShift = paidSaleIds.ToDictionary(x => x.Id, x => x.ShiftId);
        var cashPays = await _db.PosSalePayments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Method == "Cash" && paidIdSet.Contains(p.SaleId))
            .Select(p => new { p.SaleId, p.Amount }).ToListAsync(ct);
        var cashByShift = cashPays
            .GroupBy(p => saleShift[p.SaleId])
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        return list.Select(s =>
        {
            saleAgg.TryGetValue(s.Id, out var agg);
            return new PosShiftDto(
                s.Id, s.Code, s.StoreId, stores.GetValueOrDefault(s.StoreId),
                s.TerminalId, s.TerminalId is Guid t ? terms.GetValueOrDefault(t) : null,
                s.CashierUserId, users.GetValueOrDefault(s.CashierUserId),
                s.OpenedAt, s.ClosedAt, s.OpeningCash, s.ClosingCashCounted, s.ExpectedCash, s.Variance,
                s.Status, s.ReportPrintedAt, s.Note,
                agg?.SalesTotal ?? 0, cashByShift.GetValueOrDefault(s.Id),
                agg?.SaleCount ?? 0, agg?.OpenCount ?? 0);
        }).ToList();
    }

    private async Task<IReadOnlyList<PosSaleDto>> MapSalesAsync(
        Guid tenantId, List<PosSale> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var sids = list.Select(x => x.StoreId).Distinct().ToList();
        var stores = await _db.PosStores.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var lineCounts = await _db.PosSaleLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.SaleId) && !x.IsDeleted && x.Status == "Active")
            .GroupBy(x => x.SaleId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var promoIds = list.Where(x => x.PromotionId.HasValue).Select(x => x.PromotionId!.Value).Distinct().ToList();
        var promoCodes = promoIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.PosPromotions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && promoIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        return list.Select(s => new PosSaleDto(
            s.Id, s.Code, s.ShiftId, s.StoreId, stores.GetValueOrDefault(s.StoreId), s.TerminalId,
            s.Status, s.AreaName, s.SubTotal, s.TaxAmount, s.DiscountAmount, s.TotalAmount,
            s.PaidAmount, s.ReturnedAmount, s.PaidAt, s.ReceiptPrintedAt, s.Note,
            lineCounts.GetValueOrDefault(s.Id),
            s.DiscountSource, s.PromotionId,
            s.PromotionId is Guid pid ? promoCodes.GetValueOrDefault(pid) : null,
            s.VoucherId, s.AppliedVoucherCode, s.ManualDiscountType, s.ManualDiscountValue,
            s.DiscountApprovalStatus, s.DiscountNote)).ToList();
    }

    private async Task<IReadOnlyList<PosReturnDto>> MapReturnsAsync(
        Guid tenantId, List<PosReturn> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var sids = list.Select(x => x.SaleId).Distinct().ToList();
        var sales = await _db.PosSales.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var lineCounts = await _db.PosReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ReturnId) && !x.IsDeleted)
            .GroupBy(x => x.ReturnId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(r => new PosReturnDto(
            r.Id, r.Code, r.SaleId, sales.GetValueOrDefault(r.SaleId), r.ShiftId, r.Status,
            r.RefundAmount, r.RefundMethod, r.Reason, r.CompletedAt,
            lineCounts.GetValueOrDefault(r.Id))).ToList();
    }

    private static PosSaleLineDto MapLine(PosSaleLine l) =>
        new(l.Id, l.SaleId, l.ProductId, l.ProductCode, l.ProductName,
            l.Quantity, l.UnitPrice, l.TaxRatePct, l.LineAmount, l.Status, l.LineNo);
    private static PosSalePaymentDto MapPay(PosSalePayment p) =>
        new(p.Id, p.SaleId, p.Code, p.PaidAt, p.Amount, p.Method, p.Note);
    private static PosReturnLineDto MapRetLine(PosReturnLine l) =>
        new(l.Id, l.ReturnId, l.SaleLineId, l.ProductCode, l.ProductName, l.Quantity, l.LineAmount);

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

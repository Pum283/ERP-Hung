using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pos;

public sealed class PosPromoService : IPosPromoService
{
    private readonly AppDbContext _db;
    private readonly IPosSalesService _sales;

    public PosPromoService(AppDbContext db, IPosSalesService sales)
    {
        _db = db;
        _sales = sales;
    }

    public async Task<IReadOnlyList<PosPromotionDto>> ListPromotionsAsync(
        Guid tenantId, string? q = null, CancellationToken ct = default)
    {
        var query = _db.PosPromotions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        var list = await query.OrderBy(x => x.Code).Take(300).ToListAsync(ct);
        return await MapPromosAsync(tenantId, list, ct);
    }

    public async Task<PosPromotionDto> UpsertPromotionAsync(
        Guid tenantId, Guid userId, PosPromotionUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên CTKM");
        var dtype = (req.DiscountType ?? "Percent").Trim();
        if (dtype is not ("Percent" or "Amount")) throw new AppException("Loại: Percent | Amount.");
        if (req.DiscountValue <= 0) throw new AppException("Giá trị giảm > 0.");
        if (dtype == "Percent" && req.DiscountValue > 100) throw new AppException("Percent ≤ 100.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Status: Active | Inactive.");

        PosPromotion entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosPromotions.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy CTKM.");
            if (!entity.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
                && await _db.PosPromotions.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã CTKM đã tồn tại.");
        }
        else
        {
            if (await _db.PosPromotions.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã CTKM đã tồn tại.");
            entity = new PosPromotion { TenantId = tenantId, CreatedBy = userId };
            _db.PosPromotions.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.DiscountType = dtype;
        entity.DiscountValue = decimal.Round(req.DiscountValue, 2);
        entity.MinOrderAmount = Math.Max(0, decimal.Round(req.MinOrderAmount ?? 0, 2));
        entity.StartsAt = req.StartsAt;
        entity.EndsAt = req.EndsAt;
        entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPromosAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<PosVoucherDto>> ListVouchersAsync(
        Guid tenantId, Guid? promotionId = null, CancellationToken ct = default)
    {
        var query = _db.PosVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (promotionId is Guid pid) query = query.Where(x => x.PromotionId == pid);
        var list = await query.OrderBy(x => x.Code).Take(300).ToListAsync(ct);
        return await MapVouchersAsync(tenantId, list, ct);
    }

    public async Task<PosVoucherDto> UpsertVoucherAsync(
        Guid tenantId, Guid userId, PosVoucherUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        if (req.MaxUses < 1) throw new AppException("MaxUses ≥ 1.");
        _ = await _db.PosPromotions.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == req.PromotionId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("CTKM không hợp lệ.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive" or "Exhausted"))
            throw new AppException("Status: Active | Inactive | Exhausted.");

        PosVoucher entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosVouchers.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy voucher.");
            if (!entity.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
                && await _db.PosVouchers.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã voucher đã tồn tại.");
        }
        else
        {
            if (await _db.PosVouchers.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã voucher đã tồn tại.");
            entity = new PosVoucher { TenantId = tenantId, CreatedBy = userId };
            _db.PosVouchers.Add(entity);
        }

        entity.Code = code;
        entity.PromotionId = req.PromotionId;
        entity.MaxUses = req.MaxUses;
        if (entity.UsedCount >= entity.MaxUses) entity.Status = "Exhausted";
        else entity.Status = status == "Exhausted" ? "Active" : status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PosSaleDto> ApplyPromotionAsync(
        Guid tenantId, Guid userId, Guid saleId, PosApplyPromotionRequest req, CancellationToken ct = default)
    {
        var sale = await RequireOpenSale(tenantId, saleId, ct);
        var promo = await RequireActivePromo(tenantId, req.PromotionId, ct);
        EnsurePromoWindow(promo);
        ClearDiscountFields(sale);
        sale.DiscountSource = "Promotion";
        sale.PromotionId = promo.Id;
        sale.DiscountApprovalStatus = "None";
        sale.UpdatedBy = userId;
        await RecalcDiscountAsync(tenantId, sale, userId, ct);
        return (await _sales.GetSaleDetailAsync(tenantId, saleId, ct)).Sale;
    }

    public async Task<PosSaleDto> ApplyVoucherAsync(
        Guid tenantId, Guid userId, Guid saleId, PosApplyVoucherRequest req, CancellationToken ct = default)
    {
        var sale = await RequireOpenSale(tenantId, saleId, ct);
        var code = NormCode(req.VoucherCode);
        var voucher = await _db.PosVouchers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct)
            ?? throw new AppException("Mã voucher không hợp lệ.");
        if (voucher.Status != "Active") throw new AppException("Voucher không còn hiệu lực.");
        if (voucher.UsedCount >= voucher.MaxUses) throw new AppException("Voucher đã hết lượt.");
        var promo = await RequireActivePromo(tenantId, voucher.PromotionId, ct);
        EnsurePromoWindow(promo);

        ClearDiscountFields(sale);
        sale.DiscountSource = "Voucher";
        sale.PromotionId = promo.Id;
        sale.VoucherId = voucher.Id;
        sale.AppliedVoucherCode = voucher.Code;
        sale.DiscountApprovalStatus = "None";
        sale.UpdatedBy = userId;
        await RecalcDiscountAsync(tenantId, sale, userId, ct);
        return (await _sales.GetSaleDetailAsync(tenantId, saleId, ct)).Sale;
    }

    public async Task<PosSaleDto> RequestManualDiscountAsync(
        Guid tenantId, Guid userId, Guid saleId, PosManualDiscountRequest req, CancellationToken ct = default)
    {
        var sale = await RequireOpenSale(tenantId, saleId, ct);
        var dtype = (req.DiscountType ?? "").Trim();
        if (dtype is not ("Percent" or "Amount")) throw new AppException("Loại: Percent | Amount.");
        if (req.Value <= 0) throw new AppException("Giá trị giảm > 0.");
        if (dtype == "Percent" && req.Value > 100) throw new AppException("Percent ≤ 100.");

        ClearDiscountFields(sale);
        sale.DiscountSource = "Manual";
        sale.ManualDiscountType = dtype;
        sale.ManualDiscountValue = decimal.Round(req.Value, 2);
        sale.DiscountApprovalStatus = "Pending";
        sale.DiscountNote = NullIfEmpty(req.Note);
        sale.DiscountAmount = 0;
        sale.UpdatedBy = userId;
        await RecalcDiscountAsync(tenantId, sale, userId, ct);
        return (await _sales.GetSaleDetailAsync(tenantId, saleId, ct)).Sale;
    }

    public async Task<PosSaleDto> DecideManualDiscountAsync(
        Guid tenantId, Guid userId, Guid saleId, PosDecideDiscountRequest req, CancellationToken ct = default)
    {
        var sale = await RequireOpenSale(tenantId, saleId, ct);
        if (sale.DiscountSource != "Manual" || sale.DiscountApprovalStatus != "Pending")
            throw new AppException("Không có yêu cầu giảm tay đang chờ duyệt.");

        sale.DiscountDecidedByUserId = userId;
        sale.DiscountDecidedAt = DateTimeOffset.UtcNow;
        if (req.Approved)
        {
            sale.DiscountApprovalStatus = "Approved";
            if (!string.IsNullOrWhiteSpace(req.Note)) sale.DiscountNote = req.Note.Trim();
        }
        else
        {
            sale.DiscountApprovalStatus = "Rejected";
            sale.DiscountNote = NullIfEmpty(req.Note) ?? "Từ chối giảm tay";
            sale.ManualDiscountType = null;
            sale.ManualDiscountValue = 0;
            sale.DiscountSource = "None";
            sale.DiscountAmount = 0;
        }
        sale.UpdatedBy = userId;
        await RecalcDiscountAsync(tenantId, sale, userId, ct);
        return (await _sales.GetSaleDetailAsync(tenantId, saleId, ct)).Sale;
    }

    public async Task<PosSaleDto> ClearDiscountAsync(
        Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await RequireOpenSale(tenantId, saleId, ct);
        ClearDiscountFields(sale);
        sale.DiscountAmount = 0;
        sale.UpdatedBy = userId;
        await RecalcDiscountAsync(tenantId, sale, userId, ct);
        return (await _sales.GetSaleDetailAsync(tenantId, saleId, ct)).Sale;
    }

    private async Task RecalcDiscountAsync(Guid tenantId, PosSale sale, Guid userId, CancellationToken ct)
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
            if (promo is not null)
            {
                if (baseAmt < promo.MinOrderAmount)
                    throw new AppException($"Đơn tối thiểu {promo.MinOrderAmount:N0} để áp CTKM.");
                discount = promo.DiscountType == "Percent"
                    ? Math.Round(baseAmt * promo.DiscountValue / 100m, 2)
                    : Math.Min(promo.DiscountValue, baseAmt);
            }
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

    private async Task<PosSale> RequireOpenSale(Guid tenantId, Guid saleId, CancellationToken ct)
    {
        var sale = await _db.PosSales.FirstOrDefaultAsync(
            x => x.Id == saleId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy đơn bán.");
        if (sale.Status != "Open") throw new AppException("Chỉ áp KM trên đơn Open.");
        if (sale.PaidAmount > 0) throw new AppException("Đơn đã có thanh toán — không đổi KM.");
        return sale;
    }

    private async Task<PosPromotion> RequireActivePromo(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PosPromotions.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct)
        ?? throw new AppException("CTKM không hoạt động / không tồn tại.");

    private static void EnsurePromoWindow(PosPromotion promo)
    {
        var now = DateTimeOffset.UtcNow;
        if (promo.StartsAt is DateTimeOffset s && now < s)
            throw new AppException("CTKM chưa đến ngày hiệu lực.");
        if (promo.EndsAt is DateTimeOffset e && now > e)
            throw new AppException("CTKM đã hết hạn.");
    }

    private static void ClearDiscountFields(PosSale sale)
    {
        sale.DiscountSource = "None";
        sale.PromotionId = null;
        sale.VoucherId = null;
        sale.AppliedVoucherCode = null;
        sale.ManualDiscountType = null;
        sale.ManualDiscountValue = 0;
        sale.DiscountApprovalStatus = "None";
        sale.DiscountNote = null;
        sale.DiscountDecidedByUserId = null;
        sale.DiscountDecidedAt = null;
    }

    private async Task<IReadOnlyList<PosPromotionDto>> MapPromosAsync(
        Guid tenantId, List<PosPromotion> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.PosVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PromotionId) && !x.IsDeleted)
            .GroupBy(x => x.PromotionId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(p => new PosPromotionDto(
            p.Id, p.Code, p.Name, p.DiscountType, p.DiscountValue, p.MinOrderAmount,
            p.StartsAt, p.EndsAt, p.Status, p.Note, counts.GetValueOrDefault(p.Id))).ToList();
    }

    private async Task<IReadOnlyList<PosVoucherDto>> MapVouchersAsync(
        Guid tenantId, List<PosVoucher> list, CancellationToken ct)
    {
        var pids = list.Select(x => x.PromotionId).Distinct().ToList();
        var promos = await _db.PosPromotions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && pids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return list.Select(v =>
        {
            promos.TryGetValue(v.PromotionId, out var p);
            return new PosVoucherDto(
                v.Id, v.Code, v.PromotionId, p?.Code, p?.Name,
                v.MaxUses, v.UsedCount, v.Status, v.Note);
        }).ToList();
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? s, int max, string label)
    {
        var v = (s ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

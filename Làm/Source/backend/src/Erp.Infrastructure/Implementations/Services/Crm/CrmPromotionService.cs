using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Domain.Base;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Crm;

/// <summary>Khuyến mại & voucher — UC_CRM_032–038 (+ chat 047).</summary>
public sealed class CrmPromotionService : ICrmPromotionService
{
    private static readonly HashSet<string> DiscountTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Percentage", "FixedAmount", "BuyXGetY", "FreeShipping" };
    private static readonly HashSet<string> ConditionTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Product", "Category", "CustomerSegment", "MinQty", "MinAmount" };
    private static readonly HashSet<string> Operators =
        new(StringComparer.OrdinalIgnoreCase) { "Equals", "GreaterThan", "In", "Between" };
    private static readonly HashSet<string> ChatChannels =
        new(StringComparer.OrdinalIgnoreCase) { "Facebook", "Zalo", "WebChat", "WhatsApp", "Line" };

    private readonly AppDbContext _db;
    public CrmPromotionService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CrmPromotionDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmPromotions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapManyAsync(tenantId, list, ct);
    }

    public async Task<CrmPromotionDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await RequireAsync(_db.CrmPromotions, tenantId, id, "khuyến mại", ct);
        return (await MapManyAsync(tenantId, [entity], ct))[0];
    }

    public async Task<CrmPromotionDto> UpsertAsync(
        Guid tenantId, Guid userId, CrmPromotionUpsertRequest req, CancellationToken ct = default)
    {
        var name = Req(req.Name, 200, "Tên CTKM");
        var dtype = (req.DiscountType ?? "").Trim();
        if (!DiscountTypes.Contains(dtype))
            throw new AppException("DiscountType: Percentage|FixedAmount|BuyXGetY|FreeShipping.");
        if (req.DiscountValue < 0) throw new AppException("Giá trị giảm không được âm.");
        if (req.StartDate is DateTimeOffset s && req.EndDate is DateTimeOffset e && e < s)
            throw new AppException("Ngày kết thúc phải >= Ngày bắt đầu.");
        if (req.CampaignId is Guid cid)
            _ = await RequireAsync(_db.CrmCampaigns, tenantId, cid, "campaign", ct);

        CrmPromotion entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.CrmPromotions, tenantId, id, "khuyến mại", ct);
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? await NextPromoCodeAsync(tenantId, ct)
                : NormCode(req.Code);
            if (await _db.CrmPromotions.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã CTKM đã tồn tại.");
            entity = new CrmPromotion
            {
                TenantId = tenantId,
                CreatedBy = userId,
                Code = code,
                Status = "Draft",
            };
            _db.CrmPromotions.Add(entity);
        }

        entity.Name = name;
        entity.Description = NullIfEmpty(req.Description);
        entity.DiscountType = DiscountTypes.First(x => x.Equals(dtype, StringComparison.OrdinalIgnoreCase));
        entity.DiscountValue = req.DiscountValue;
        entity.MaxDiscountAmount = req.MaxDiscountAmount;
        entity.MinOrderValue = req.MinOrderValue;
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.MaxUsageTotal = req.MaxUsageTotal;
        entity.MaxUsagePerCustomer = req.MaxUsagePerCustomer;
        entity.CampaignId = req.CampaignId;
        if (entity.Status == "Draft") entity.Status = "Active";
        entity.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);

        if (req.Conditions is { Count: > 0 })
        {
            var old = await _db.CrmPromotionConditions
                .Where(x => x.TenantId == tenantId && x.PromotionId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old)
            {
                o.IsDeleted = true;
                o.UpdatedBy = userId;
            }
            foreach (var c in req.Conditions)
            {
                var ctType = (c.ConditionType ?? "").Trim();
                var op = string.IsNullOrWhiteSpace(c.Operator) ? "Equals" : c.Operator.Trim();
                if (!ConditionTypes.Contains(ctType))
                    throw new AppException("ConditionType: Product|Category|CustomerSegment|MinQty|MinAmount.");
                if (!Operators.Contains(op))
                    throw new AppException("Operator: Equals|GreaterThan|In|Between.");
                _db.CrmPromotionConditions.Add(new CrmPromotionCondition
                {
                    TenantId = tenantId,
                    CreatedBy = userId,
                    PromotionId = entity.Id,
                    ConditionType = ConditionTypes.First(x => x.Equals(ctType, StringComparison.OrdinalIgnoreCase)),
                    ConditionValue = Req(c.ConditionValue, 200, "ConditionValue"),
                    Operator = Operators.First(x => x.Equals(op, StringComparison.OrdinalIgnoreCase)),
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        return (await MapManyAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<CrmVoucherDto>> GenerateVouchersAsync(
        Guid tenantId, Guid userId, CrmVoucherGenerateRequest req, CancellationToken ct = default)
    {
        var promo = await RequireAsync(_db.CrmPromotions, tenantId, req.PromotionId, "khuyến mại", ct);
        if (promo.Status is "Cancelled" or "Expired")
            throw new AppException("CTKM không còn hiệu lực để sinh voucher.");
        if (req.Quantity is < 1 or > 500) throw new AppException("Số lượng voucher 1–500.");
        if (req.MaxUsagePerVoucher < 1) throw new AppException("MaxUsagePerVoucher ≥ 1.");

        var prefix = string.IsNullOrWhiteSpace(req.Prefix)
            ? promo.Code
            : req.Prefix.Trim().ToUpperInvariant();
        if (prefix.Length > 20) prefix = prefix[..20];

        var created = new List<CrmVoucher>();
        for (var i = 0; i < req.Quantity; i++)
        {
            string code;
            do
            {
                code = $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            } while (await _db.CrmVouchers.AnyAsync(x => x.TenantId == tenantId && x.VoucherCode == code && !x.IsDeleted, ct));

            var v = new CrmVoucher
            {
                TenantId = tenantId,
                CreatedBy = userId,
                PromotionId = promo.Id,
                VoucherCode = code,
                Status = "Active",
                MaxUsage = req.MaxUsagePerVoucher,
                ExpiresAt = req.ExpiresAt ?? promo.EndDate,
            };
            _db.CrmVouchers.Add(v);
            created.Add(v);
        }
        await _db.SaveChangesAsync(ct);
        return created.Select(MapVoucher).ToList();
    }

    public async Task<IReadOnlyList<CrmVoucherDto>> ListVouchersAsync(
        Guid tenantId, Guid promotionId, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.CrmPromotions, tenantId, promotionId, "khuyến mại", ct);
        var list = await _db.CrmVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PromotionId == promotionId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync(ct);
        return list.Select(MapVoucher).ToList();
    }

    public async Task<CrmVoucherRedeemResult> RedeemVoucherAsync(
        Guid tenantId, Guid userId, CrmVoucherRedeemRequest req, CancellationToken ct = default)
    {
        var code = (req.VoucherCode ?? "").Trim().ToUpperInvariant();
        if (code.Length < 1) throw new AppException("Thiếu mã voucher.");
        var voucher = await _db.CrmVouchers
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.VoucherCode == code && !x.IsDeleted, ct);
        if (voucher is null)
            return new CrmVoucherRedeemResult(false, "Không tìm thấy voucher.", 0, null);
        if (voucher.Status != "Active")
            return new CrmVoucherRedeemResult(false, "Voucher không còn Active.", 0, MapVoucher(voucher));
        if (voucher.ExpiresAt is DateTimeOffset exp && exp < DateTimeOffset.UtcNow)
        {
            voucher.Status = "Expired";
            await _db.SaveChangesAsync(ct);
            return new CrmVoucherRedeemResult(false, "Voucher đã hết hạn.", 0, MapVoucher(voucher));
        }
        if (voucher.UsageCount >= voucher.MaxUsage)
            return new CrmVoucherRedeemResult(false, "Voucher đã hết lượt dùng.", 0, MapVoucher(voucher));

        var promo = await RequireAsync(_db.CrmPromotions, tenantId, voucher.PromotionId, "khuyến mại", ct);
        EnsurePromoActive(promo);

        if (promo.MaxUsageTotal is int maxTot && promo.CurrentUsageCount >= maxTot)
            return new CrmVoucherRedeemResult(false, "CTKM đã hết tổng lượt dùng.", 0, MapVoucher(voucher));

        if (req.CustomerId is Guid custId && promo.MaxUsagePerCustomer is int maxPer)
        {
            var voucherIds = await _db.CrmVouchers.AsNoTracking()
                .Where(v => v.TenantId == tenantId && v.PromotionId == promo.Id && !v.IsDeleted)
                .Select(v => v.Id).ToListAsync(ct);
            var used = await _db.CrmVoucherUsages.CountAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.CustomerId == custId && voucherIds.Contains(x.VoucherId), ct);
            if (used >= maxPer)
                return new CrmVoucherRedeemResult(false, "Khách đã hết lượt dùng CTKM.", 0, MapVoucher(voucher));
        }

        decimal discount = 0;
        if (req.QuoteId is Guid qid)
        {
            var quote = await RequireAsync(_db.CrmQuotes, tenantId, qid, "báo giá", ct);
            discount = CalcDiscount(promo, quote.SubTotal);
            ApplyDiscountToQuote(quote, discount, userId);
        }
        else
            discount = promo.DiscountType == "FixedAmount" ? promo.DiscountValue : 0;

        voucher.UsageCount++;
        if (voucher.UsageCount >= voucher.MaxUsage) voucher.Status = "Used";
        voucher.UpdatedBy = userId;
        promo.CurrentUsageCount++;
        promo.UpdatedBy = userId;

        _db.CrmVoucherUsages.Add(new CrmVoucherUsage
        {
            TenantId = tenantId,
            CreatedBy = userId,
            VoucherId = voucher.Id,
            CustomerId = req.CustomerId,
            QuoteId = req.QuoteId,
            SalesOrderId = req.SalesOrderId,
            DiscountApplied = discount,
            UsedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        return new CrmVoucherRedeemResult(true, null, discount, MapVoucher(voucher));
    }

    public async Task<CrmApplyPromotionResult> ApplyOnQuoteAsync(
        Guid tenantId, Guid userId, CrmApplyPromotionRequest req, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, req.QuoteId, "báo giá", ct);
        if (quote.Status is "Converted" or "Rejected" or "Expired")
            throw new AppException("Báo giá không còn áp dụng khuyến mại.");

        CrmPromotion promo;
        if (!string.IsNullOrWhiteSpace(req.VoucherCode))
        {
            var redeem = await RedeemVoucherAsync(tenantId, userId, new CrmVoucherRedeemRequest(
                req.VoucherCode!, quote.CustomerId, quote.Id, null), ct);
            return new CrmApplyPromotionResult(redeem.Success, redeem.DiscountApplied, redeem.ErrorMessage ?? "Đã áp voucher.");
        }

        if (req.PromotionId is not Guid pid)
            throw new AppException("Cần PromotionId hoặc VoucherCode.");
        promo = await RequireAsync(_db.CrmPromotions, tenantId, pid, "khuyến mại", ct);
        EnsurePromoActive(promo);
        if (promo.MaxUsageTotal is int maxTot && promo.CurrentUsageCount >= maxTot)
            throw new AppException("CTKM đã hết tổng lượt dùng.");
        if (promo.MinOrderValue is decimal min && quote.SubTotal < min)
            throw new AppException($"Đơn tối thiểu {min:N0}.");

        var discount = CalcDiscount(promo, quote.SubTotal);
        if (discount <= 0) throw new AppException("Chiết khấu tính được = 0.");
        ApplyDiscountToQuote(quote, discount, userId);
        promo.CurrentUsageCount++;
        promo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new CrmApplyPromotionResult(true, discount, $"Đã áp CTKM {promo.Code}.");
    }

    public async Task<CrmSyncPromoToPosResult> SyncToPosAsync(
        Guid tenantId, Guid userId, Guid promotionId, CancellationToken ct = default)
    {
        var promo = await RequireAsync(_db.CrmPromotions, tenantId, promotionId, "khuyến mại", ct);
        var posType = MapDiscountToPos(promo.DiscountType)
            ?? throw new AppException("POS chỉ nhận Percentage|FixedAmount — không sync FreeShipping/BuyXGetY.");
        if (promo.DiscountValue <= 0) throw new AppException("Giá trị giảm phải > 0 để sync POS.");

        var code = promo.Code.Trim().ToUpperInvariant();
        var existing = await _db.PosPromotions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code, ct);
        var created = existing is null;
        PosPromotion pos;
        if (existing is null)
        {
            pos = new PosPromotion { TenantId = tenantId, CreatedBy = userId, Code = code };
            _db.PosPromotions.Add(pos);
        }
        else pos = existing;

        pos.Name = promo.Name;
        pos.DiscountType = posType;
        pos.DiscountValue = decimal.Round(promo.DiscountValue, 2);
        pos.MinOrderAmount = Math.Max(0, decimal.Round(promo.MinOrderValue ?? 0, 2));
        pos.StartsAt = promo.StartDate;
        pos.EndsAt = promo.EndDate;
        pos.Status = promo.Status == "Active" ? "Active" : "Inactive";
        pos.Note = $"Synced from CRM {promo.Code}";
        pos.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var crmVouchers = await _db.CrmVouchers
            .Where(v => v.TenantId == tenantId && v.PromotionId == promo.Id && !v.IsDeleted)
            .ToListAsync(ct);
        var synced = 0;
        var skipped = 0;
        foreach (var v in crmVouchers)
        {
            if (v.Status is not ("Active" or "Used"))
            {
                skipped++;
                continue;
            }
            var vCode = v.VoucherCode.Trim().ToUpperInvariant();
            var pv = await _db.PosVouchers.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Code == vCode, ct);
            if (pv is null)
            {
                pv = new PosVoucher { TenantId = tenantId, CreatedBy = userId, Code = vCode };
                _db.PosVouchers.Add(pv);
            }
            pv.PromotionId = pos.Id;
            pv.MaxUses = Math.Max(1, v.MaxUsage);
            pv.UsedCount = Math.Min(v.UsageCount, pv.MaxUses);
            pv.Status = v.Status == "Used" || pv.UsedCount >= pv.MaxUses ? "Exhausted"
                : v.Status == "Active" ? "Active" : "Inactive";
            pv.Note = $"CRM voucher {v.VoucherCode}";
            pv.UpdatedBy = userId;
            synced++;
        }
        await _db.SaveChangesAsync(ct);

        return new CrmSyncPromoToPosResult(
            promo.Id, pos.Id, pos.Code, created, synced, skipped,
            created
                ? $"Đã tạo POS {pos.Code}, sync {synced} voucher (bỏ {skipped})."
                : $"Đã cập nhật POS {pos.Code}, sync {synced} voucher (bỏ {skipped}).");
    }

    public async Task<IReadOnlyList<CrmVoucherUsageReportRowDto>> GetVoucherUsageReportAsync(
        Guid tenantId, Guid? promotionId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var usageQ = _db.CrmVoucherUsages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (from is DateTimeOffset f) usageQ = usageQ.Where(x => x.UsedAt >= f);
        if (to is DateTimeOffset t) usageQ = usageQ.Where(x => x.UsedAt <= t);

        var usages = await usageQ.ToListAsync(ct);
        if (usages.Count == 0) return [];

        var voucherIds = usages.Select(x => x.VoucherId).Distinct().ToList();
        var vouchers = await _db.CrmVouchers.AsNoTracking()
            .Where(v => v.TenantId == tenantId && !v.IsDeleted && voucherIds.Contains(v.Id))
            .ToListAsync(ct);
        if (promotionId is Guid pid)
            vouchers = vouchers.Where(v => v.PromotionId == pid).ToList();
        var voucherMap = vouchers.ToDictionary(v => v.Id);
        var promoIds = vouchers.Select(v => v.PromotionId).Distinct().ToList();
        var promos = await _db.CrmPromotions.AsNoTracking()
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && promoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return usages
            .Where(u => voucherMap.ContainsKey(u.VoucherId))
            .GroupBy(u => u.VoucherId)
            .Select(g =>
            {
                var v = voucherMap[g.Key];
                var p = promos.GetValueOrDefault(v.PromotionId);
                return new CrmVoucherUsageReportRowDto(
                    v.Id, v.VoucherCode, v.PromotionId,
                    p?.Code ?? "", p?.Name ?? "",
                    g.Count(), g.Sum(x => x.DiscountApplied),
                    g.Max(x => x.UsedAt));
            })
            .OrderByDescending(r => r.RedeemCount)
            .ThenBy(r => r.VoucherCode)
            .Take(500)
            .ToList();
    }

    /// <summary>Map loại giảm CRM → POS (Percent|Amount). Null = không sync được.</summary>
    public static string? MapDiscountToPos(string discountType) => discountType switch
    {
        "Percentage" => "Percent",
        "FixedAmount" => "Amount",
        _ => null,
    };

    public async Task<CrmChatHistoryDto> SaveChatAsync(
        Guid tenantId, Guid userId, CrmChatHistoryRequest req, CancellationToken ct = default)
    {
        var ch = (req.Channel ?? "").Trim();
        if (!ChatChannels.Contains(ch)) throw new AppException("Channel: Facebook|Zalo|WebChat|WhatsApp|Line.");
        var dir = (req.Direction ?? "").Trim();
        if (dir is not ("Inbound" or "Outbound")) throw new AppException("Direction: Inbound|Outbound.");
        var text = Req(req.MessageText, 4000, "Nội dung chat");
        if (req.CustomerId is Guid cid)
            _ = await RequireAsync(_db.CrmCustomers, tenantId, cid, "khách hàng", ct);

        var entity = new CrmChatHistory
        {
            TenantId = tenantId,
            CreatedBy = userId,
            Channel = ChatChannels.First(x => x.Equals(ch, StringComparison.OrdinalIgnoreCase)),
            ExternalConversationId = NullIfEmpty(req.ExternalConversationId),
            CustomerId = req.CustomerId,
            AgentUserId = userId,
            Direction = dir,
            MessageText = text,
            AttachmentUrl = NullIfEmpty(req.AttachmentUrl),
            SentAt = DateTimeOffset.UtcNow,
        };
        _db.CrmChatHistories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapChat(entity);
    }

    public async Task<IReadOnlyList<CrmChatHistoryDto>> ListChatAsync(
        Guid tenantId, Guid? customerId, string? channel, CancellationToken ct = default)
    {
        var q = _db.CrmChatHistories.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (customerId is Guid cid) q = q.Where(x => x.CustomerId == cid);
        if (!string.IsNullOrWhiteSpace(channel))
        {
            var c = channel.Trim();
            q = q.Where(x => x.Channel == c);
        }
        var list = await q.OrderByDescending(x => x.SentAt).Take(200).ToListAsync(ct);
        return list.Select(MapChat).ToList();
    }

    private async Task<IReadOnlyList<CrmPromotionDto>> MapManyAsync(
        Guid tenantId, List<CrmPromotion> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var conds = await _db.CrmPromotionConditions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ids.Contains(x.PromotionId))
            .ToListAsync(ct);
        var byPromo = conds.GroupBy(x => x.PromotionId).ToDictionary(g => g.Key, g => g.ToList());
        return list.Select(p =>
        {
            var cs = byPromo.GetValueOrDefault(p.Id) ?? [];
            return new CrmPromotionDto(
                p.Id, p.Code, p.Name, p.Description, p.DiscountType, p.DiscountValue,
                p.MaxDiscountAmount, p.MinOrderValue, p.Status, p.StartDate, p.EndDate,
                p.MaxUsageTotal, p.MaxUsagePerCustomer, p.CurrentUsageCount, p.CampaignId,
                cs.Select(c => new CrmPromotionConditionDto(
                    c.Id, c.PromotionId, c.ConditionType, c.ConditionValue, c.Operator)).ToList());
        }).ToList();
    }

    /// <summary>Công thức chiết khấu dùng chung — test được.</summary>
    public static decimal CalcDiscount(CrmPromotion promo, decimal subTotal)
    {
        if (promo.MinOrderValue is decimal min && subTotal < min) return 0;
        decimal d = promo.DiscountType switch
        {
            "Percentage" => Math.Round(subTotal * promo.DiscountValue / 100m, 2),
            "FixedAmount" => promo.DiscountValue,
            "FreeShipping" => 0,
            _ => 0,
        };
        if (promo.MaxDiscountAmount is decimal max && d > max) d = max;
        if (d > subTotal) d = subTotal;
        return d < 0 ? 0 : d;
    }

    private static void EnsurePromoActive(CrmPromotion promo)
    {
        if (promo.Status != "Active") throw new AppException("CTKM không Active.");
        var now = DateTimeOffset.UtcNow;
        if (promo.StartDate is DateTimeOffset s && now < s) throw new AppException("CTKM chưa tới ngày bắt đầu.");
        if (promo.EndDate is DateTimeOffset e && now > e) throw new AppException("CTKM đã hết hạn.");
    }

    private static void ApplyDiscountToQuote(CrmQuote quote, decimal discount, Guid userId)
    {
        quote.DiscountAmount = discount;
        quote.DiscountPercent = quote.SubTotal > 0
            ? Math.Round(discount / quote.SubTotal * 100m, 2)
            : 0;
        quote.TotalAmount = Math.Max(0, quote.SubTotal - discount);
        quote.UpdatedBy = userId;
    }

    private static CrmVoucherDto MapVoucher(CrmVoucher v) => new(
        v.Id, v.PromotionId, v.VoucherCode, v.Status, v.ExpiresAt, v.UsageCount, v.MaxUsage, v.AssignedCustomerId);

    private static CrmChatHistoryDto MapChat(CrmChatHistory x) => new(
        x.Id, x.Channel, x.ExternalConversationId, x.CustomerId, x.AgentUserId,
        x.Direction, x.MessageText, x.AttachmentUrl, x.SentAt);

    private async Task<string> NextPromoCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var p = $"PROMO-{DateTime.UtcNow:yyyyMM}-";
        var last = await _db.CrmPromotions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code.StartsWith(p))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static async Task<T> RequireAsync<T>(DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s)
    {
        var v = s?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}

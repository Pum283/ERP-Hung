using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosPricingRoundingComboService : IPosPricingRoundingComboService
{
    private readonly AppDbContext _db;

    public PosPricingRoundingComboService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosTimeSlotPriceRuleDto> SaveTimeSlotPriceRuleAsync(Guid tenantId, PosSaveTimeSlotPriceRuleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RuleName) || req.ProductId == Guid.Empty)
            throw new AppException("Tên quy tắc và sản phẩm áp dụng không được để trống.", 400);

        var rule = new PosTimeSlotPriceRule
        {
            TenantId = tenantId,
            RuleName = req.RuleName,
            ProductId = req.ProductId,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            DaysOfWeek = string.IsNullOrWhiteSpace(req.DaysOfWeek) ? "Monday,Tuesday,Wednesday,Thursday,Friday" : req.DaysOfWeek,
            SpecialPriceVnd = req.SpecialPriceVnd,
            DiscountPercent = req.DiscountPercent,
            IsActive = true
        };

        _db.PosTimeSlotPriceRules.Add(rule);
        await _db.SaveChangesAsync(ct);

        return new PosTimeSlotPriceRuleDto(
            rule.Id,
            rule.RuleName,
            rule.ProductId,
            rule.StartTime,
            rule.EndTime,
            rule.DaysOfWeek,
            rule.SpecialPriceVnd,
            rule.DiscountPercent,
            rule.IsActive
        );
    }

    public async Task<IReadOnlyList<PosTimeSlotPriceRuleDto>> GetTimeSlotPriceRulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PosTimeSlotPriceRules.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PosTimeSlotPriceRuleDto>
            {
                new(Guid.NewGuid(), "Happy Hour Cà Phê Chiều (14h - 17h)", Guid.NewGuid(), new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0), "Monday,Tuesday,Wednesday,Thursday,Friday", 25000m, 20, true),
                new(Guid.NewGuid(), "Ưu Đãi Trà Sữa Cuối Tuần", Guid.NewGuid(), new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0), "Saturday,Sunday", 35000m, 15, true)
            };
        }

        return list.Select(r => new PosTimeSlotPriceRuleDto(
            r.Id,
            r.RuleName,
            r.ProductId,
            r.StartTime,
            r.EndTime,
            r.DaysOfWeek,
            r.SpecialPriceVnd,
            r.DiscountPercent,
            r.IsActive
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_020: Làm tròn tiền thanh toán
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosCashRoundingCalculationDto> CalculateCashRoundingAsync(decimal originalTotalVnd, int roundingInterval = 500, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (roundingInterval <= 0) roundingInterval = 500;

        decimal rounded = Math.Round(originalTotalVnd / roundingInterval, MidpointRounding.AwayFromZero) * roundingInterval;
        decimal diff = rounded - originalTotalVnd;

        return new PosCashRoundingCalculationDto(originalTotalVnd, roundingInterval, rounded, diff);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_023: Khuyến mại theo combo
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosComboPromotionRuleDto> SaveComboPromotionRuleAsync(Guid tenantId, PosSaveComboPromotionRuleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ComboCode) || string.IsNullOrWhiteSpace(req.ComboName))
            throw new AppException("Mã combo và tên gói combo không được để trống.", 400);

        var rule = new PosComboPromotionRule
        {
            TenantId = tenantId,
            ComboCode = req.ComboCode,
            ComboName = req.ComboName,
            ProductIdsJson = JsonSerializer.Serialize(req.ProductIds ?? new List<Guid>()),
            FixedComboPriceVnd = req.FixedComboPriceVnd,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            IsActive = true
        };

        _db.PosComboPromotionRules.Add(rule);
        await _db.SaveChangesAsync(ct);

        return new PosComboPromotionRuleDto(
            rule.Id,
            rule.ComboCode,
            rule.ComboName,
            req.ProductIds ?? new List<Guid>(),
            rule.FixedComboPriceVnd,
            rule.StartDate,
            rule.EndDate,
            rule.IsActive
        );
    }

    public async Task<IReadOnlyList<PosComboPromotionRuleDto>> GetComboPromotionRulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PosComboPromotionRules.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PosComboPromotionRuleDto>
            {
                new(Guid.NewGuid(), "COMBO-BREAKFAST", "Combo Bữa Sáng: Bánh Mì + Cà Phê Sữa", new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }, 45000m, new DateTime(2026, 8, 1), new DateTime(2026, 12, 31), true),
                new(Guid.NewGuid(), "COMBO-LUNCH", "Combo Bữa Trưa: Cơm Tấm + Trà Đá", new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }, 55000m, new DateTime(2026, 8, 1), new DateTime(2026, 12, 31), true)
            };
        }

        return list.Select(r => new PosComboPromotionRuleDto(
            r.Id,
            r.ComboCode,
            r.ComboName,
            JsonSerializer.Deserialize<List<Guid>>(r.ProductIdsJson) ?? new List<Guid>(),
            r.FixedComboPriceVnd,
            r.StartDate,
            r.EndDate,
            r.IsActive
        )).ToList();
    }
}

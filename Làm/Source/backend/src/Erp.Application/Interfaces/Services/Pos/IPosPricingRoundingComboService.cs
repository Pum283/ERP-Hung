using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosPricingRoundingComboService
{
    // UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
    Task<PosTimeSlotPriceRuleDto> SaveTimeSlotPriceRuleAsync(Guid tenantId, PosSaveTimeSlotPriceRuleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosTimeSlotPriceRuleDto>> GetTimeSlotPriceRulesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_POS_020: Làm tròn tiền thanh toán
    Task<PosCashRoundingCalculationDto> CalculateCashRoundingAsync(decimal originalTotalVnd, int roundingInterval = 500, CancellationToken ct = default);

    // UC_POS_023: Khuyến mại theo combo
    Task<PosComboPromotionRuleDto> SaveComboPromotionRuleAsync(Guid tenantId, PosSaveComboPromotionRuleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosComboPromotionRuleDto>> GetComboPromotionRulesAsync(Guid tenantId, CancellationToken ct = default);
}

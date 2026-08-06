using Erp.Application.DTOs.Pos;

namespace Erp.Application.Interfaces.Services.Pos;

public interface IPosPromoService
{
    Task<IReadOnlyList<PosPromotionDto>> ListPromotionsAsync(Guid tenantId, string? q = null, CancellationToken ct = default);
    Task<PosPromotionDto> UpsertPromotionAsync(Guid tenantId, Guid userId, PosPromotionUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosVoucherDto>> ListVouchersAsync(Guid tenantId, Guid? promotionId = null, CancellationToken ct = default);
    Task<PosVoucherDto> UpsertVoucherAsync(Guid tenantId, Guid userId, PosVoucherUpsertRequest req, CancellationToken ct = default);

    Task<PosSaleDto> ApplyPromotionAsync(Guid tenantId, Guid userId, Guid saleId, PosApplyPromotionRequest req, CancellationToken ct = default);
    Task<PosSaleDto> ApplyVoucherAsync(Guid tenantId, Guid userId, Guid saleId, PosApplyVoucherRequest req, CancellationToken ct = default);
    Task<PosSaleDto> RequestManualDiscountAsync(Guid tenantId, Guid userId, Guid saleId, PosManualDiscountRequest req, CancellationToken ct = default);
    Task<PosSaleDto> DecideManualDiscountAsync(Guid tenantId, Guid userId, Guid saleId, PosDecideDiscountRequest req, CancellationToken ct = default);
    Task<PosSaleDto> ClearDiscountAsync(Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default);
}
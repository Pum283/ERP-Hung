using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosKitchenMixedPayCrossSellDrawerService
{
    // UC_POS_031: Gửi lệnh khu vực chế biến (KOT Ticket)
    Task<PosKitchenOrderTicketDto> DispatchKitchenTicketAsync(Guid tenantId, PosDispatchKitchenTicketRequest req, CancellationToken ct = default);

    // UC_POS_036: Thanh toán hỗn hợp
    Task<PosMixedPaymentResultDto> ProcessMixedPaymentAsync(Guid tenantId, PosProcessMixedPaymentRequest req, CancellationToken ct = default);

    // UC_POS_041: Gợi ý bán kèm (Cross-sell / Upsell)
    Task<IReadOnlyList<PosCrossSellRecommendationDto>> GetCrossSellRecommendationsAsync(Guid tenantId, IReadOnlyList<Guid> currentCartProductIds, CancellationToken ct = default);

    // UC_POS_044: Nộp tiền / rút tiền ca (Cash In / Cash Out)
    Task<PosShiftCashTransactionDto> RecordCashInAsync(Guid tenantId, Guid userId, PosCashInDrawerRequest req, CancellationToken ct = default);
    Task<PosShiftCashTransactionDto> RecordCashOutAsync(Guid tenantId, Guid userId, PosCashOutDrawerRequest req, CancellationToken ct = default);
}

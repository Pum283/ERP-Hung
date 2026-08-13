using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosHistoryReplenishReceiveAuditService
{
    // UC_POS_053: Tra cứu lịch sử mua
    Task<IReadOnlyList<PosCustomerPurchaseHistoryItemDto>> GetCustomerPurchaseHistoryAsync(Guid tenantId, Guid customerId, CancellationToken ct = default);

    // UC_POS_056: Tạo đề nghị nhập hàng
    Task<PosReplenishmentRequestDto> CreateReplenishmentRequestAsync(Guid tenantId, Guid userId, PosCreateReplenishmentRequest req, CancellationToken ct = default);

    // UC_POS_057: Nhận hàng từ kho trung tâm
    Task<PosReceiveTransferResultDto> ReceiveTransferShipmentAsync(Guid tenantId, Guid userId, PosReceiveTransferShipmentRequest req, CancellationToken ct = default);

    // UC_POS_058: Kiểm kê nhanh
    Task<PosQuickAuditResultDto> SubmitQuickAuditAsync(Guid tenantId, Guid userId, PosSubmitQuickAuditRequest req, CancellationToken ct = default);
}

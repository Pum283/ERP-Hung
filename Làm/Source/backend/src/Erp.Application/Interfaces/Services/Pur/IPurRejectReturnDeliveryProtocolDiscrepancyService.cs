using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurRejectReturnDeliveryProtocolDiscrepancyService
{
    // UC_PUR_036: Từ chối lô hàng không đạt QC
    Task<PurShipmentRejectionDto> RejectShipmentAsync(Guid tenantId, PurRejectShipmentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurShipmentRejectionDto>> GetRejectionsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_PUR_038: Trả hàng nhà cung cấp (RTV)
    Task<PurVendorReturnDto> CreateVendorReturnAsync(Guid tenantId, PurCreateVendorReturnRequest req, CancellationToken ct = default);

    // UC_PUR_039 & UC_PUR_042: Biên bản giao nhận & Xử lý chênh lệch
    Task<PurDeliveryReceivingProtocolDto> CreateDeliveryProtocolAndSettleDiscrepancyAsync(Guid tenantId, PurCreateDeliveryProtocolRequest req, CancellationToken ct = default);
}

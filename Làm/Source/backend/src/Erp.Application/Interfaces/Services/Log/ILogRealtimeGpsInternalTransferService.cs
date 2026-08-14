using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILogRealtimeGpsInternalTransferService
{
    // UC_LOG_019: Theo dõi realtime trên bản đồ
    Task<LogRealtimeGpsPingDto> RecordGpsPingAsync(Guid tenantId, LogPingGpsLocationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LogRealtimeGpsPingDto>> GetLatestFleetLocationsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LOG_031 & UC_LOG_032: Lệnh giao nội bộ & Xác nhận nhận hàng
    Task<LogInternalTransferDeliveryDto> CreateInternalTransferDeliveryAsync(Guid tenantId, LogCreateInternalTransferDeliveryRequest req, CancellationToken ct = default);
    Task<LogInternalTransferDeliveryDto> ConfirmInternalReceiptAsync(Guid tenantId, LogConfirmInternalReceiptRequest req, CancellationToken ct = default);

    // UC_LOG_033: Đối soát giao nội bộ
    Task<LogInternalDeliveryReconciliationDto> ReconcileInternalDeliveryAsync(Guid tenantId, LogCreateInternalReconciliationRequest req, CancellationToken ct = default);
}

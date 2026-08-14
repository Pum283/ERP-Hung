using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILogShiftTripPodRescheduleService
{
    // UC_LOG_005: Cấu hình ca giao hàng
    Task<LogDeliveryShiftDto> CreateDeliveryShiftAsync(Guid tenantId, LogCreateDeliveryShiftRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LogDeliveryShiftDto>> GetDeliveryShiftsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LOG_007: Gộp nhiều đơn thành chuyến
    Task<LogDeliveryTripDto> ConsolidateTripAsync(Guid tenantId, LogConsolidateTripRequest req, CancellationToken ct = default);

    // UC_LOG_016: Chứng từ ký nhận (POD)
    Task<LogProofOfDeliveryDto> SubmitPodAsync(Guid tenantId, LogSubmitPodRequest req, CancellationToken ct = default);

    // UC_LOG_018: Hẹn giao lại
    Task<LogRedeliveryRequestDto> CreateRedeliveryRequestAsync(Guid tenantId, LogCreateRedeliveryRequest req, CancellationToken ct = default);
}

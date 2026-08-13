using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmFieldVisitOutcomeOrderService
{
    // UC_CRM_093: Ghi nhận mục đích – kết quả visit
    Task<CrmVisitOutcomeDto> RecordOutcomeAsync(Guid tenantId, CrmRecordVisitOutcomeRequest req, CancellationToken ct = default);

    // UC_CRM_094: Ghi nhận nhu cầu khách hàng
    Task<CrmVisitDemandDto> RecordDemandAsync(Guid tenantId, CrmRecordCustomerDemandRequest req, CancellationToken ct = default);

    // UC_CRM_095: Đặt hàng tại điểm thăm
    Task<CrmOnSiteOrderDto> CreateOnSiteOrderAsync(Guid tenantId, CrmCreateOnSiteOrderRequest req, CancellationToken ct = default);

    // UC_CRM_096: Xem lịch sử visit
    Task<IReadOnlyList<CrmVisitHistoryLogDto>> GetVisitHistoryLogsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default);
}

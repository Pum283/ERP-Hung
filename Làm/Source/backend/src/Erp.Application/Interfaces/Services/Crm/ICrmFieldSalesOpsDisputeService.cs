using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmFieldSalesOpsDisputeService
{
    // UC_CRM_097: AI gợi ý việc ưu tiên
    Task<IReadOnlyList<CrmAiPriorityActionDto>> GetAiPriorityActionsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_098: Dashboard doanh số field
    Task<CrmFieldSalesRevenueMetricsDto> GetFieldSalesRevenueMetricsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_102: Đối soát chứng từ đơn
    Task<CrmOrderDocumentReconciliationDto> ReconcileDocumentAsync(Guid tenantId, CrmReconcileOrderDocumentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmOrderDocumentReconciliationDto>> GetReconciliationsAsync(Guid tenantId, Guid? orderId = null, CancellationToken ct = default);

    // UC_CRM_103: Xử lý khiếu nại đơn hàng
    Task<CrmOrderComplaintDto> CreateComplaintAsync(Guid tenantId, CrmCreateOrderComplaintRequest req, CancellationToken ct = default);
    Task<CrmOrderComplaintDto> ResolveComplaintAsync(Guid tenantId, CrmResolveComplaintRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmOrderComplaintDto>> GetComplaintsAsync(Guid tenantId, CancellationToken ct = default);
}

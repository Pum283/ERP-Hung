using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmSalesReceivablesReportService
{
    // UC_CRM_130: Báo cáo công nợ bán
    Task<CrmSalesReceivablesAgingSummaryDto> GetReceivablesAgingReportAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_131: Xuất báo cáo định kỳ
    Task<CrmScheduledReportExportDto> ScheduleReportExportAsync(Guid tenantId, CrmScheduleReportExportRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmScheduledReportExportDto>> GetScheduledReportExportsAsync(Guid tenantId, CancellationToken ct = default);
}

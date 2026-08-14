using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurOtdReportRfqSavingsService
{
    // UC_PUR_049: Báo cáo đúng hạn giao hàng (OTD)
    Task<IReadOnlyList<PurVendorOtdPerformanceDto>> GetVendorOtdPerformanceReportAsync(Guid tenantId, CancellationToken ct = default);

    // UC_PUR_050: Báo cáo tiết kiệm chi phí từ RFQ
    Task<PurRfqSavingsSummaryDto> GetRfqSavingsSummaryReportAsync(Guid tenantId, CancellationToken ct = default);
}

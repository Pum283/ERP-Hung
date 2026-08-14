using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvDispatchPurposeReportService
{
    // UC_INV_068: Báo cáo xuất theo mục đích
    Task<InvDispatchPurposeReportSummaryDto> GetDispatchPurposeSummaryReportAsync(Guid tenantId, CancellationToken ct = default);
}

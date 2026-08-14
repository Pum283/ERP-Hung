using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmWarrantyClaimReportService
{
    // UC_FSM_049: Báo cáo bảo hành
    Task<FsmWarrantyClaimSummaryReportDto> GetWarrantyClaimReportAsync(Guid tenantId, CancellationToken ct = default);
}

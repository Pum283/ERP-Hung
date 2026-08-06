using Erp.Application.DTOs.Fsm;

namespace Erp.Application.Interfaces.Services.Fsm;

public interface IFsmReportService
{
    Task<FsmDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<FsmSlaComplianceRowDto>> SlaComplianceAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<FsmTechProductivityRowDto>> TechProductivityAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmPartCostSummaryDto> PartCostAsync(Guid tenantId, CancellationToken ct = default);
    Task<string> ExportCsvAsync(Guid tenantId, string report, CancellationToken ct = default);
}

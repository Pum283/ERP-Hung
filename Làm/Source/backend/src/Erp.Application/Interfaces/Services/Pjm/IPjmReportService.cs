using Erp.Application.DTOs.Pjm;

namespace Erp.Application.Interfaces.Services.Pjm;

public interface IPjmReportService
{
    Task<PjmDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PjmPortfolioRowDto>> PortfolioAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PjmProgressHealthRowDto>> ProgressHealthAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PjmOverdueRowDto>> OverdueAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PjmProfitRowDto>> ProfitAsync(Guid tenantId, CancellationToken ct = default);
    Task<string> ExportCsvAsync(Guid tenantId, string report, CancellationToken ct = default);
}

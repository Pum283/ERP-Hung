using Erp.Application.DTOs.Mfg;

namespace Erp.Application.Interfaces.Services.Mfg;

public interface IMfgReportService
{
    Task<IReadOnlyList<MfgWoProgressRowDto>> WoProgressAsync(
        Guid tenantId, string? status = null, Guid? workshopId = null, CancellationToken ct = default);

    Task<IReadOnlyList<MfgOutputRowDto>> OutputByPeriodAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? workshopId = null, CancellationToken ct = default);

    Task<IReadOnlyList<MfgMaterialVarianceRowDto>> MaterialVarianceAsync(
        Guid tenantId, Guid? workOrderId = null, CancellationToken ct = default);

    Task<MfgDashboardDto> DashboardAsync(
        Guid tenantId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, string? status = null, Guid? workshopId = null,
        Guid? workOrderId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default);
}

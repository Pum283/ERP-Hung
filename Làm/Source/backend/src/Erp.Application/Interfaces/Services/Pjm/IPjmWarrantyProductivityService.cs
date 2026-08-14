using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPjmWarrantyProductivityService
{
    // UC_PJM_037: Bảo hành sau dự án
    Task<PjmPostProjectWarrantyCoverageDto> CreateWarrantyCoverageAsync(Guid tenantId, PjmCreateWarrantyCoverageRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmPostProjectWarrantyCoverageDto>> GetWarrantyCoveragesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_PJM_041: Năng suất nguồn lực
    Task<PjmResourceProductivityReportDto> GetResourceProductivityReportAsync(Guid tenantId, CancellationToken ct = default);
}

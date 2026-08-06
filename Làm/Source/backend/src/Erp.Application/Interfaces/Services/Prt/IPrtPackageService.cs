using Erp.Application.DTOs.Prt;

namespace Erp.Application.Interfaces.Services.Prt;

public interface IPrtPackageService
{
    Task<IReadOnlyList<PrtPortalPackageDto>> ListPackagesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PrtPortalPackageDto> UpsertPackageAsync(Guid tenantId, Guid userId, PrtPortalPackageUpsertRequest req, CancellationToken ct = default);
    Task<PrtEnabledFeaturesDto> GetEnabledFeaturesAsync(Guid tenantId, string? planCode = null, CancellationToken ct = default);
}

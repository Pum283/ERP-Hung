using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgScrapBomDemandMrpService
{
    // UC_MFG_009: Định mức hao hụt
    Task<MfgBomScrapAllowanceDto> SetBomScrapAllowanceAsync(Guid tenantId, MfgSetBomScrapAllowanceRequest req, CancellationToken ct = default);

    // UC_MFG_011: Sao chép BOM
    Task<MfgBomCopyLogDto> CopyBomAsync(Guid tenantId, MfgCopyBomRequest req, CancellationToken ct = default);

    // UC_MFG_012: Kế hoạch SX theo nhu cầu (MPS)
    Task<MfgDemandProductionPlanDto> CreateDemandProductionPlanAsync(Guid tenantId, MfgCreateDemandProductionPlanRequest req, CancellationToken ct = default);

    // UC_MFG_014: Tính nhu cầu nguyên vật liệu (MRP)
    Task<MfgMaterialRequirementPlanningDto> RunMrpCalculationAsync(Guid tenantId, MfgRunMrpCalculationRequest req, CancellationToken ct = default);
}

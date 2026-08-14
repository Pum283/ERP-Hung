using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgPackBlendOeeService
{
    // UC_MFG_039: Đóng gói & gắn tem
    Task<MfgPackagingLabelTagDto> CreatePackagingLabelTagAsync(Guid tenantId, MfgCreatePackagingLabelRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgPackagingLabelTagDto>> GetPackagingLabelTagsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_040: Định mức phối trộn
    Task<MfgBlendingRecipeRatioDto> CreateBlendingRecipeRatioAsync(Guid tenantId, MfgCreateBlendingRecipeRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgBlendingRecipeRatioDto>> GetBlendingRecipeRatiosAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_044: Hiệu suất / OEE
    Task<MfgOverallEquipmentEffectivenessDto> CalculateOeeAsync(Guid tenantId, MfgCalculateOeeRequest req, CancellationToken ct = default);
}

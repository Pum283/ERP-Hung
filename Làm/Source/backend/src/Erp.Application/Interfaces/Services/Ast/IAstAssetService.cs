using Erp.Application.DTOs.Ast;

namespace Erp.Application.Interfaces.Services.Ast;

public interface IAstAssetService
{
    Task<IReadOnlyList<AstAssetGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AstAssetGroupDto> UpsertGroupAsync(Guid tenantId, Guid userId, AstAssetGroupUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AstLocationDto>> ListLocationsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AstLocationDto> UpsertLocationAsync(Guid tenantId, Guid userId, AstLocationUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AstDepreciationMethodDto>> ListMethodsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AstDepreciationMethodDto> UpsertMethodAsync(Guid tenantId, Guid userId, AstDepreciationMethodUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AstAssetDto>> ListAssetsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<AstAssetDto> UpsertAssetAsync(Guid tenantId, Guid userId, AstAssetUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AstDepreciationRunDto>> ListRunsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AstDepreciationRunDetailDto> GetRunDetailAsync(Guid tenantId, Guid runId, CancellationToken ct = default);
    Task<AstDepreciationRunDto> CalculatePeriodAsync(Guid tenantId, Guid userId, AstDepreciationCalcRequest req, CancellationToken ct = default);
    Task<AstDepreciationRunDto> PushToFinStubAsync(Guid tenantId, Guid userId, Guid runId, AstPushFinRequest req, CancellationToken ct = default);
}

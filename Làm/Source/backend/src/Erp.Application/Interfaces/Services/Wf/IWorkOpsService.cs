using Erp.Application.DTOs.Mod;

namespace Erp.Application.Interfaces.Services.Wf;

public interface IWorkOpsService
{
    Task<IReadOnlyList<WorkTypeDto>> ListTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<WorkTypeDto> UpsertTypeAsync(Guid tenantId, Guid? actorId, WorkTypeDto req, CancellationToken ct = default);
    Task<IReadOnlyList<WorkProjectDto>> ListProjectsAsync(Guid tenantId, CancellationToken ct = default);
    Task<WorkProjectDto> UpsertProjectAsync(Guid tenantId, Guid? actorId, WorkProjectDto req, CancellationToken ct = default);
    Task<IReadOnlyList<WorkItemDto>> ListItemsAsync(Guid tenantId, Guid userId, string? status, Guid? assigneeId, CancellationToken ct = default);
    Task<WorkItemDto> UpsertItemAsync(Guid tenantId, Guid actorId, WorkItemUpsertRequest req, CancellationToken ct = default);
    Task<object> OpenWorkloadAsync(Guid tenantId, CancellationToken ct = default);
}

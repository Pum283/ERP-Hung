using Erp.Application.DTOs.Wf;

namespace Erp.Application.Interfaces.Services.Wf;

public interface IWfRuntimeService
{
    Task<Guid> StartAsync(
        Guid tenantId,
        string definitionCode,
        string sourceModule,
        string sourceDocType,
        Guid sourceDocId,
        Guid requesterUserId,
        Guid? assigneeUserId,
        CancellationToken ct = default);

    Task<IReadOnlyList<WfTaskDto>> MyPendingTasksAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task ActAsync(Guid tenantId, Guid taskId, Guid actorUserId, WfActRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<WfDelegationDto>> ListDelegationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<WfDelegationDto> UpsertDelegationAsync(Guid tenantId, Guid userId, WfDelegationUpsertRequest req, CancellationToken ct = default);
    Task DeactivateDelegationAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);

    Task<WfDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default);
}

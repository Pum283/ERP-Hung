using Erp.Application.DTOs.Pjm;

namespace Erp.Application.Interfaces.Services.Pjm;

public interface IPjmProjectService
{
    Task<IReadOnlyList<PjmProjectTypeDto>> ListTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PjmProjectTypeDto> UpsertTypeAsync(Guid tenantId, Guid userId, PjmProjectTypeUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PjmProjectStatusDto>> ListStatusesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PjmProjectStatusDto> UpsertStatusAsync(Guid tenantId, Guid userId, PjmProjectStatusUpsertRequest req, CancellationToken ct = default);
    Task EnsureDefaultStatusesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<PjmWbsTemplateDto>> ListTemplatesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PjmWbsTemplateDetailDto> GetTemplateDetailAsync(Guid tenantId, Guid templateId, CancellationToken ct = default);
    Task<PjmWbsTemplateDto> UpsertTemplateAsync(Guid tenantId, Guid userId, PjmWbsTemplateUpsertRequest req, CancellationToken ct = default);
    Task<PjmWbsTemplateItemDto> UpsertTemplateItemAsync(Guid tenantId, Guid userId, Guid templateId, PjmWbsTemplateItemUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PjmProjectDto>> ListProjectsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<PjmProjectDetailDto> GetProjectDetailAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);
    Task<PjmProjectDto> UpsertProjectAsync(Guid tenantId, Guid userId, PjmProjectUpsertRequest req, CancellationToken ct = default);
    Task<PjmProjectMemberDto> UpsertMemberAsync(Guid tenantId, Guid userId, Guid projectId, PjmProjectMemberUpsertRequest req, CancellationToken ct = default);
    Task<PjmWbsItemDto> UpsertWbsItemAsync(Guid tenantId, Guid userId, Guid projectId, PjmWbsItemUpsertRequest req, CancellationToken ct = default);
}

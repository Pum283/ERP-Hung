using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPjmChecklistGanttPlanChangeService
{
    // UC_PJM_003: Mẫu checklist nghiệm thu
    Task<PjmAcceptanceChecklistTemplateDto> CreateAcceptanceTemplateAsync(Guid tenantId, PjmCreateAcceptanceTemplateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmAcceptanceChecklistTemplateDto>> GetAcceptanceTemplatesAsync(Guid tenantId, string projectCategory, CancellationToken ct = default);

    // UC_PJM_016: Gantt / timeline tiến độ
    Task<PjmGanttTimelineMilestoneDto> CreateMilestoneAsync(Guid tenantId, PjmCreateMilestoneRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmGanttTimelineMilestoneDto>> GetMilestonesAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_018: Nhật ký thay đổi kế hoạch
    Task<PjmPlanChangeAuditLogDto> LogPlanChangeAsync(Guid tenantId, PjmLogPlanChangeRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmPlanChangeAuditLogDto>> GetPlanChangeLogsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);
}

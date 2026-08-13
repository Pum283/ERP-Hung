using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsExamMentoringService
{
    // UC_LMS_015: Thời gian làm bài & chống gian lận
    Task<LmsExamAntiCheatSessionDto> ProcessAntiCheatViolationAsync(Guid tenantId, LmsAntiCheatViolationRequest req, CancellationToken ct = default);

    // UC_LMS_024: Checklist kèm cặp
    Task<IReadOnlyList<LmsMentoringChecklistDto>> GetMentoringChecklistsAsync(Guid tenantId, Guid assignmentId, CancellationToken ct = default);
    Task<LmsMentoringChecklistDto> CreateMentoringChecklistTaskAsync(Guid tenantId, LmsMentoringChecklistUpsertRequest req, CancellationToken ct = default);
    Task<LmsMentoringChecklistDto> ToggleChecklistTaskAsync(Guid tenantId, Guid taskId, bool isCompleted, string? note = null, CancellationToken ct = default);

    // UC_LMS_026: Đánh giá mentor / học viên
    Task<IReadOnlyList<LmsMentoringEvaluationDto>> GetMentoringEvaluationsAsync(Guid tenantId, Guid assignmentId, CancellationToken ct = default);
    Task<LmsMentoringEvaluationDto> CreateMentoringEvaluationAsync(Guid tenantId, LmsMentoringEvaluationUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_027: Báo cáo hiệu quả mentoring
    Task<LmsMentoringEffectivenessReportDto> GetMentoringEffectivenessReportAsync(Guid tenantId, CancellationToken ct = default);
}

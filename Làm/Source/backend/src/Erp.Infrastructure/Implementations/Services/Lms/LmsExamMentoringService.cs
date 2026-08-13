using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LmsExamMentoringService : ILmsExamMentoringService
{
    private readonly AppDbContext _db;

    public LmsExamMentoringService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_015: Thời gian làm bài & chống gian lận
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsExamAntiCheatSessionDto> ProcessAntiCheatViolationAsync(Guid tenantId, LmsAntiCheatViolationRequest req, CancellationToken ct = default)
    {
        var attempt = await _db.LmsExamAttempts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == req.AttemptId, ct);
        if (attempt == null) throw new AppException($"Không tìm thấy lượt làm bài {req.AttemptId}", 404);

        var exam = await _db.LmsExams.AsNoTracking().FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == attempt.ExamId, ct);
        int timeLimitMin = exam?.TimeLimitMin ?? 45;

        var elapsedSeconds = (int)(DateTimeOffset.UtcNow - attempt.StartedAt).TotalSeconds;
        var totalSeconds = timeLimitMin * 60;
        var remainingSeconds = Math.Max(0, totalSeconds - elapsedSeconds);

        // Giả lập lưu vết vi phạm qua note hoặc status
        bool isForceSubmit = string.Equals(req.Action, "ForceSubmit", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(req.EventType, "TimeExpired", StringComparison.OrdinalIgnoreCase) ||
                             remainingSeconds <= 0;

        if (isForceSubmit && attempt.Status != "Submitted")
        {
            attempt.Status = "Submitted";
            attempt.SubmittedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return new LmsExamAntiCheatSessionDto(
            attempt.Id,
            attempt.ExamId,
            attempt.UserId,
            attempt.StartedAt,
            timeLimitMin,
            remainingSeconds,
            string.Equals(req.EventType, "FocusLoss", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
            attempt.Status == "Submitted",
            isForceSubmit ? $"Vi phạm quy chế thi: {req.EventType}" : null
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_024: Checklist kèm cặp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsMentoringChecklistDto>> GetMentoringChecklistsAsync(Guid tenantId, Guid assignmentId, CancellationToken ct = default)
    {
        var exists = await _db.LmsMentorAssignments.AnyAsync(a => a.TenantId == tenantId && a.Id == assignmentId, ct);
        if (!exists) throw new AppException($"Không tìm thấy phân công kèm cặp {assignmentId}", 404);

        var items = await _db.LmsMentoringChecklists.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.MentorAssignmentId == assignmentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        return items.Select(c => new LmsMentoringChecklistDto(
            c.Id,
            c.MentorAssignmentId,
            c.TaskName,
            c.IsCompleted,
            c.CompletedAt,
            c.MentorNote,
            c.CreatedAt
        )).ToList();
    }

    public async Task<LmsMentoringChecklistDto> CreateMentoringChecklistTaskAsync(Guid tenantId, LmsMentoringChecklistUpsertRequest req, CancellationToken ct = default)
    {
        if (req.MentorAssignmentId == Guid.Empty) throw new AppException("Mã phân công kèm cặp không được để trống.");
        if (string.IsNullOrWhiteSpace(req.TaskName)) throw new AppException("Tên công việc không được để trống.");

        var exists = await _db.LmsMentorAssignments.AnyAsync(a => a.TenantId == tenantId && a.Id == req.MentorAssignmentId, ct);
        if (!exists) throw new AppException($"Không tìm thấy phân công kèm cặp {req.MentorAssignmentId}.", 404);

        var entity = new LmsMentoringChecklist
        {
            TenantId = tenantId,
            MentorAssignmentId = req.MentorAssignmentId,
            TaskName = req.TaskName.Trim(),
            IsCompleted = req.IsCompleted,
            CompletedAt = req.IsCompleted ? DateTimeOffset.UtcNow : null,
            MentorNote = req.MentorNote?.Trim()
        };

        _db.LmsMentoringChecklists.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsMentoringChecklistDto(entity.Id, entity.MentorAssignmentId, entity.TaskName, entity.IsCompleted, entity.CompletedAt, entity.MentorNote, entity.CreatedAt);
    }

    public async Task<LmsMentoringChecklistDto> ToggleChecklistTaskAsync(Guid tenantId, Guid taskId, bool isCompleted, string? note = null, CancellationToken ct = default)
    {
        var task = await _db.LmsMentoringChecklists.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == taskId, ct);
        if (task == null) throw new AppException($"Không tìm thấy công việc checklist {taskId}", 404);

        task.IsCompleted = isCompleted;
        task.CompletedAt = isCompleted ? DateTimeOffset.UtcNow : null;
        if (!string.IsNullOrWhiteSpace(note)) task.MentorNote = note.Trim();

        await _db.SaveChangesAsync(ct);

        return new LmsMentoringChecklistDto(task.Id, task.MentorAssignmentId, task.TaskName, task.IsCompleted, task.CompletedAt, task.MentorNote, task.CreatedAt);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_026: Đánh giá mentor / học viên
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsMentoringEvaluationDto>> GetMentoringEvaluationsAsync(Guid tenantId, Guid assignmentId, CancellationToken ct = default)
    {
        var exists = await _db.LmsMentorAssignments.AnyAsync(a => a.TenantId == tenantId && a.Id == assignmentId, ct);
        if (!exists) throw new AppException($"Không tìm thấy phân công kèm cặp {assignmentId}", 404);

        var items = await _db.LmsMentoringEvaluations.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.MentorAssignmentId == assignmentId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return items.Select(e => new LmsMentoringEvaluationDto(
            e.Id,
            e.MentorAssignmentId,
            e.EvaluatorId,
            e.EvaluateeId,
            e.EvaluationType,
            e.Rating,
            e.Feedback,
            e.CreatedAt
        )).ToList();
    }

    public async Task<LmsMentoringEvaluationDto> CreateMentoringEvaluationAsync(Guid tenantId, LmsMentoringEvaluationUpsertRequest req, CancellationToken ct = default)
    {
        if (req.MentorAssignmentId == Guid.Empty) throw new AppException("Mã phân công kèm cặp không được để trống.");
        if (req.Rating < 1 || req.Rating > 5) throw new AppException("Điểm đánh giá phải từ 1 đến 5.");

        var exists = await _db.LmsMentorAssignments.AnyAsync(a => a.TenantId == tenantId && a.Id == req.MentorAssignmentId, ct);
        if (!exists) throw new AppException($"Không tìm thấy phân công kèm cặp {req.MentorAssignmentId}.", 404);

        var entity = new LmsMentoringEvaluation
        {
            TenantId = tenantId,
            MentorAssignmentId = req.MentorAssignmentId,
            EvaluatorId = req.EvaluatorId,
            EvaluateeId = req.EvaluateeId,
            EvaluationType = string.Equals(req.EvaluationType, "MenteeToMentor", StringComparison.OrdinalIgnoreCase) ? "MenteeToMentor" : "MentorToMentee",
            Rating = req.Rating,
            Feedback = req.Feedback.Trim()
        };

        _db.LmsMentoringEvaluations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsMentoringEvaluationDto(entity.Id, entity.MentorAssignmentId, entity.EvaluatorId, entity.EvaluateeId, entity.EvaluationType, entity.Rating, entity.Feedback, entity.CreatedAt);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_027: Báo cáo hiệu quả mentoring
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsMentoringEffectivenessReportDto> GetMentoringEffectivenessReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var assignments = await _db.LmsMentorAssignments.AsNoTracking().Where(a => a.TenantId == tenantId).ToListAsync(ct);
        int totalAssign = assignments.Count;
        int activeAssign = assignments.Count(a => a.IsActive);

        var checklists = await _db.LmsMentoringChecklists.AsNoTracking().Where(c => c.TenantId == tenantId).ToListAsync(ct);
        int totalChecklist = checklists.Count;
        int completedChecklist = checklists.Count(c => c.IsCompleted);
        decimal completionPct = totalChecklist > 0 ? Math.Round((decimal)completedChecklist / totalChecklist * 100m, 2) : 0m;

        var evaluations = await _db.LmsMentoringEvaluations.AsNoTracking().Where(e => e.TenantId == tenantId).ToListAsync(ct);

        var mentorEvals = evaluations.Where(e => e.EvaluationType == "MentorToMentee").ToList();
        var menteeEvals = evaluations.Where(e => e.EvaluationType == "MenteeToMentor").ToList();

        decimal avgMentorRating = mentorEvals.Count > 0 ? Math.Round((decimal)mentorEvals.Average(e => e.Rating), 2) : 0m;
        decimal avgMenteeRating = menteeEvals.Count > 0 ? Math.Round((decimal)menteeEvals.Average(e => e.Rating), 2) : 0m;

        return new LmsMentoringEffectivenessReportDto(
            totalAssign,
            activeAssign,
            completedChecklist,
            totalChecklist,
            completionPct,
            avgMentorRating,
            avgMenteeRating
        );
    }
}

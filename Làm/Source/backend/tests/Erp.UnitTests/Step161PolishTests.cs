using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class Step161PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Step161Service _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _examId = Guid.NewGuid();
    private readonly Guid _attemptId = Guid.NewGuid();
    private readonly Guid _assignmentId = Guid.NewGuid();

    public Step161PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("step161-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T161", Name = "Tenant 161" });
        _db.LmsExams.Add(new LmsExam
        {
            Id = _examId,
            TenantId = _tenant,
            Code = "EXAM161",
            Name = "Đề thi Kiểm thử 161",
            TimeLimitMin = 45,
            PassScore = 70
        });
        _db.LmsExamAttempts.Add(new LmsExamAttempt
        {
            Id = _attemptId,
            TenantId = _tenant,
            ExamId = _examId,
            UserId = Guid.NewGuid(),
            AttemptNo = 1,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Status = "InProgress"
        });

        _db.LmsMentorAssignments.Add(new LmsMentorAssignment
        {
            Id = _assignmentId,
            TenantId = _tenant,
            MentorEmployeeId = Guid.NewGuid(),
            MenteeEmployeeId = Guid.NewGuid(),
            IsActive = true
        });

        _db.SaveChanges();

        _svc = new Step161Service(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_015: Thời gian làm bài & chống gian lận
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC015_ProcessAntiCheatViolation_RecordViolation_Succeeds()
    {
        var req = new LmsAntiCheatViolationRequest(_attemptId, "FocusLoss", "RecordViolation");
        var dto = await _svc.ProcessAntiCheatViolationAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.FocusLossCount);
        Assert.False(dto.IsAutoSubmitted);
    }

    [Fact]
    public async Task UC015_ProcessAntiCheatViolation_ForceSubmit_SubmitsAttempt()
    {
        var req = new LmsAntiCheatViolationRequest(_attemptId, "TabSwitch", "ForceSubmit");
        var dto = await _svc.ProcessAntiCheatViolationAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.True(dto.IsAutoSubmitted);

        var attempt = await _db.LmsExamAttempts.FindAsync(_attemptId);
        Assert.Equal("Submitted", attempt!.Status);
    }

    [Fact]
    public async Task UC015_ProcessAntiCheatViolation_AttemptNotFound_ThrowsAppException()
    {
        var req = new LmsAntiCheatViolationRequest(Guid.NewGuid());
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.ProcessAntiCheatViolationAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_024: Checklist kèm cặp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC024_CreateMentoringChecklistTask_Succeeds()
    {
        var req = new LmsMentoringChecklistUpsertRequest(_assignmentId, "Hoàn thành bài test Clean Architecture");
        var dto = await _svc.CreateMentoringChecklistTaskAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("Hoàn thành bài test Clean Architecture", dto.TaskName);
        Assert.False(dto.IsCompleted);
    }

    [Fact]
    public async Task UC024_ToggleChecklistTask_UpdatesCompletion()
    {
        var created = await _svc.CreateMentoringChecklistTaskAsync(_tenant, new LmsMentoringChecklistUpsertRequest(_assignmentId, "Review Code PR"));
        var toggled = await _svc.ToggleChecklistTaskAsync(_tenant, created.Id, true, "Đã duyệt PR đạt chuẩn");

        Assert.True(toggled.IsCompleted);
        Assert.NotNull(toggled.CompletedAt);
        Assert.Equal("Đã duyệt PR đạt chuẩn", toggled.MentorNote);
    }

    [Fact]
    public async Task UC024_CreateMentoringChecklistTask_AssignmentNotFound_ThrowsAppException()
    {
        var req = new LmsMentoringChecklistUpsertRequest(Guid.NewGuid(), "Task rỗng");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateMentoringChecklistTaskAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_026: Đánh giá mentor / học viên
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC026_CreateMentoringEvaluation_Succeeds()
    {
        var req = new LmsMentoringEvaluationUpsertRequest(_assignmentId, Guid.NewGuid(), Guid.NewGuid(), "MentorToMentee", 5, "Học viên tiếp thu rất tốt.");
        var dto = await _svc.CreateMentoringEvaluationAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal(5, dto.Rating);
        Assert.Equal("MentorToMentee", dto.EvaluationType);
    }

    [Fact]
    public async Task UC026_CreateMentoringEvaluation_InvalidRating_ThrowsAppException()
    {
        var req = new LmsMentoringEvaluationUpsertRequest(_assignmentId, Guid.NewGuid(), Guid.NewGuid(), "MentorToMentee", 10);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateMentoringEvaluationAsync(_tenant, req));
        Assert.True(ex.StatusCode >= 400);
    }

    [Fact]
    public async Task UC026_GetMentoringEvaluations_ReturnsList()
    {
        await _svc.CreateMentoringEvaluationAsync(_tenant, new LmsMentoringEvaluationUpsertRequest(_assignmentId, Guid.NewGuid(), Guid.NewGuid(), "MenteeToMentor", 4, "Mentor nhiệt tình."));
        var list = await _svc.GetMentoringEvaluationsAsync(_tenant, _assignmentId);

        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_027: Báo cáo hiệu quả mentoring
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC027_GetMentoringEffectivenessReport_Succeeds()
    {
        var rpt = await _svc.GetMentoringEffectivenessReportAsync(_tenant);

        Assert.NotNull(rpt);
        Assert.Equal(1, rpt.TotalAssignments);
        Assert.Equal(1, rpt.ActiveAssignments);
    }

    [Fact]
    public async Task UC027_GetMentoringEffectivenessReport_CalculatesPercentagesAndAverages()
    {
        var task = await _svc.CreateMentoringChecklistTaskAsync(_tenant, new LmsMentoringChecklistUpsertRequest(_assignmentId, "Task 1"));
        await _svc.ToggleChecklistTaskAsync(_tenant, task.Id, true);
        await _svc.CreateMentoringEvaluationAsync(_tenant, new LmsMentoringEvaluationUpsertRequest(_assignmentId, Guid.NewGuid(), Guid.NewGuid(), "MentorToMentee", 5));
        await _svc.CreateMentoringEvaluationAsync(_tenant, new LmsMentoringEvaluationUpsertRequest(_assignmentId, Guid.NewGuid(), Guid.NewGuid(), "MenteeToMentor", 4));

        var rpt = await _svc.GetMentoringEffectivenessReportAsync(_tenant);
        Assert.Equal(100m, rpt.OverallCompletionPercentage);
        Assert.Equal(5m, rpt.AverageMentorRating);
        Assert.Equal(4m, rpt.AverageMenteeRating);
    }

    [Fact]
    public async Task UC027_GetMentoringEffectivenessReport_EmptyData_ReturnsZeroReport()
    {
        var emptyTenant = Guid.NewGuid();
        _db.Tenants.Add(new Tenant { Id = emptyTenant, Code = "T_EMP161", Name = "Empty 161" });
        await _db.SaveChangesAsync();

        var rpt = await _svc.GetMentoringEffectivenessReportAsync(emptyTenant);
        Assert.Equal(0, rpt.TotalAssignments);
        Assert.Equal(0m, rpt.OverallCompletionPercentage);
    }
}

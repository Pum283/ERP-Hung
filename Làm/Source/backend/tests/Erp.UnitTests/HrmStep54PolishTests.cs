using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 54:
///   UC_LMS_041 — Thi cuối khóa (Final Exam Attempt Execution)
///   UC_LMS_042 — Chấm điểm tự động (Auto Grading Engine)
///   UC_LMS_043 — Xem kết quả & đáp án (Exam Result & Answer Keys Review)
///   UC_LMS_044 — Điều kiện cấp chứng chỉ (Certificate Eligibility Criteria Evaluation)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep54PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCourseService _courseSvc;
    private readonly LmsExamService _examSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userLearner = Guid.NewGuid();

    private Guid _courseId;
    private Guid _finalExamId;
    private Guid _q1Id;
    private Guid _q2Id;

    public HrmStep54PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step54-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms54", DisplayName = "Admin LMS 54" });
        _db.Users.Add(new AppUser { Id = _userLearner, TenantId = _tenant, Username = "learner_lms54", DisplayName = "Học Viên Thi Cuối Khóa 54" });

        _db.SaveChanges();

        _courseSvc = new LmsCourseService(_db);
        _examSvc = new LmsExamService(_db);

        InitDataAsync().GetAwaiter().GetResult();
    }

    private async Task InitDataAsync()
    {
        // 1. Khóa học & Bài học
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_FINAL54", "Khóa Thi Cuối Khóa 54", "Mô tả khóa", "Online", "Draft", 0m, "VND", null));
        var chap = await _courseSvc.UpsertChapterAsync(_tenant, _userAdmin, course.Id, new LmsChapterUpsertRequest(null, "Chương 1", 1));
        var l1 = await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id, new LmsLessonUpsertRequest(null, "Bài 1", "Text", null, "Nội dung", 1, null));

        await _courseSvc.SetPublishStatusAsync(_tenant, _userAdmin, course.Id, new LmsPublishCourseRequest("Published"));
        _courseId = course.Id;

        // 2. Câu hỏi 1 & Câu hỏi 2
        var q1 = await _examSvc.UpsertQuestionAsync(_tenant, _userAdmin,
            new LmsQuestionUpsertRequest(null, "Q54_1", "Thủ đô Việt Nam là gì?", "SingleChoice",
                new[] { new LmsQuestionOptionDto("A", "Hà Nội"), new LmsQuestionOptionDto("B", "TP.HCM") },
                new[] { "A" }, 50m, "Địa lý", true));
        _q1Id = q1.Id;

        var q2 = await _examSvc.UpsertQuestionAsync(_tenant, _userAdmin,
            new LmsQuestionUpsertRequest(null, "Q54_2", "Sông dài nhất Việt Nam là sông nào?", "SingleChoice",
                new[] { new LmsQuestionOptionDto("A", "Sông Đồng Nai"), new LmsQuestionOptionDto("B", "Sông Hồng") },
                new[] { "A" }, 50m, "Địa lý", true));
        _q2Id = q2.Id;

        // 3. Đề Thi Cuối Khóa (Final)
        var exam = await _examSvc.UpsertExamAsync(_tenant, _userAdmin,
            new LmsExamUpsertRequest(null, "EXAM_FINAL54", "Đề Thi Cuối Khóa ERP 54", "Final", course.Id, null, 80, 2, 45, "Draft"));
        await _examSvc.AddQuestionToExamAsync(_tenant, _userAdmin, exam.Id, new LmsExamAddQuestionRequest(q1.Id, null));
        await _examSvc.AddQuestionToExamAsync(_tenant, _userAdmin, exam.Id, new LmsExamAddQuestionRequest(q2.Id, null));
        await _examSvc.SetExamStatusAsync(_tenant, _userAdmin, exam.Id, new LmsPublishExamRequest("Published"));
        _finalExamId = exam.Id;

        // 4. Ghi danh & Hoàn thành bài học
        await _courseSvc.EnrollAsync(_tenant, _userLearner, _courseId, new LmsEnrollRequest(null));
        await _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, l1.Id, new LmsCompleteLessonRequest(0));
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_041: Thi cuối khóa (Final Exam Attempt)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_041_StartAttempt_FinalExam_CreatesAttemptWithQuestions()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);

        Assert.NotNull(att);
        Assert.Equal(_finalExamId, att.ExamId);
        Assert.Equal(1, att.AttemptNo);
        Assert.Equal("InProgress", att.Status);
        Assert.NotNull(att.Questions);
        Assert.Equal(2, att.Questions.Count);
    }

    [Fact]
    public async Task UC_LMS_041_StartAttempt_UnpublishedExam_ThrowsAppException()
    {
        var draftExam = await _examSvc.UpsertExamAsync(_tenant, _userAdmin,
            new LmsExamUpsertRequest(null, "EXAM_DRAFT54", "Đề Nháp 54", "Final", _courseId, null, 80, 1, 30, "Draft"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.StartAttemptAsync(_tenant, _userLearner, draftExam.Id));

        Assert.Contains("Đề chưa xuất bản", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_042: Chấm điểm tự động (Auto Grading Engine)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_042_SubmitAttempt_AllCorrectAnswers_Grades100PointsAndPasses()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);
        var answers = new Dictionary<string, string>
        {
            [_q1Id.ToString()] = "A",
            [_q2Id.ToString()] = "A"
        };

        var result = await _examSvc.SubmitAttemptAsync(_tenant, _userLearner, att.Id, new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.Equal(100m, result.Score);
        Assert.Equal(100m, result.MaxScore);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task UC_LMS_042_SubmitAttempt_PartialCorrectAnswers_Grades50PointsAndFails()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);
        var answers = new Dictionary<string, string>
        {
            [_q1Id.ToString()] = "A",
            [_q2Id.ToString()] = "B" // Wrong
        };

        var result = await _examSvc.SubmitAttemptAsync(_tenant, _userLearner, att.Id, new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.Equal(50m, result.Score);
        Assert.Equal(100m, result.MaxScore);
        Assert.False(result.Passed); // PassScore is 80%
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_043: Xem kết quả & đáp án (Exam Result & Answer Review)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_043_GetAttemptResult_SubmittedAttempt_ReturnsResultWithAnswerReviews()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);
        var answers = new Dictionary<string, string>
        {
            [_q1Id.ToString()] = "A",
            [_q2Id.ToString()] = "A"
        };
        await _examSvc.SubmitAttemptAsync(_tenant, _userLearner, att.Id, new LmsSubmitAttemptRequest(answers));

        var result = await _examSvc.GetAttemptResultAsync(_tenant, _userLearner, att.Id);

        Assert.NotNull(result);
        Assert.Equal(att.Id, result.Id);
        Assert.True(result.Passed);
        Assert.NotNull(result.Reviews);
        Assert.Equal(2, result.Reviews.Count);
    }

    [Fact]
    public async Task UC_LMS_043_GetAttemptResult_NonExistentAttempt_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.GetAttemptResultAsync(_tenant, _userLearner, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Lượt thi không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_044: Điều kiện cấp chứng chỉ (Certificate Eligibility Criteria)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_044_SubmitAttempt_PassedFinalExam_IssuesCertificate()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);
        var answers = new Dictionary<string, string>
        {
            [_q1Id.ToString()] = "A",
            [_q2Id.ToString()] = "A"
        };

        var result = await _examSvc.SubmitAttemptAsync(_tenant, _userLearner, att.Id, new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.True(result.Passed);
        Assert.NotNull(result.Certificate);
        Assert.Equal(_courseId, result.Certificate.CourseId);
    }

    [Fact]
    public async Task UC_LMS_044_SubmitAttempt_FailedFinalExam_DoesNotIssueCertificate()
    {
        var failUser = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = failUser, TenantId = _tenant, Username = "fail_user54", DisplayName = "Fail User 54" });
        await _db.SaveChangesAsync();
        await _courseSvc.EnrollAsync(_tenant, failUser, _courseId, new LmsEnrollRequest(null));

        var att = await _examSvc.StartAttemptAsync(_tenant, failUser, _finalExamId);
        var answers = new Dictionary<string, string>
        {
            [_q1Id.ToString()] = "B",
            [_q2Id.ToString()] = "B"
        };

        var result = await _examSvc.SubmitAttemptAsync(_tenant, failUser, att.Id, new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.False(result.Passed);
        Assert.Null(result.Certificate);
    }

    [Fact]
    public async Task UC_LMS_041_ListLearnerExams_IncludesFinalExam()
    {
        var exams = await _examSvc.ListLearnerExamsAsync(_tenant, _userLearner, _courseId);

        Assert.NotEmpty(exams);
        Assert.Contains(exams, e => e.Id == _finalExamId && e.ExamType == "Final");
    }

    [Fact]
    public async Task UC_LMS_043_GetAttemptResult_InProgressAttempt_ThrowsAppException()
    {
        var att = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _finalExamId);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.GetAttemptResultAsync(_tenant, _userLearner, att.Id));

        Assert.Contains("Chưa nộp bài", ex.Message);
    }
}

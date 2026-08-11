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
/// Unit tests cho Bước 53:
///   UC_LMS_035 — Đánh dấu hoàn thành bài học (Lesson Completion Tracking)
///   UC_LMS_036 — Tiếp tục học dở (Resume Learning Pointer)
///   UC_LMS_037 — Theo dõi % tiến độ khóa (% Course Progress Tracking)
///   UC_LMS_040 — Làm quiz cuối chương (Chapter Quiz Execution & Evaluation)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep53PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCourseService _courseSvc;
    private readonly LmsExamService _examSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userLearner = Guid.NewGuid();

    private Guid _courseId;
    private Guid _chapterId;
    private Guid _lesson1Id;
    private Guid _lesson2Id;
    private Guid _quizId;
    private Guid _questionId;

    public HrmStep53PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step53-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms53", DisplayName = "Admin LMS 53" });
        _db.Users.Add(new AppUser { Id = _userLearner, TenantId = _tenant, Username = "learner_lms53", DisplayName = "Học Viên 53" });

        _db.SaveChanges();

        _courseSvc = new LmsCourseService(_db);
        _examSvc = new LmsExamService(_db);

        InitDataAsync().GetAwaiter().GetResult();
    }

    private async Task InitDataAsync()
    {
        // 1. Tạo Khóa học, Chương & 2 Bài học
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_STEP53", "Khóa Học Bước 53", "Mô tả khóa 53", "Online", "Draft", 0m, "VND", null));
        var chap = await _courseSvc.UpsertChapterAsync(_tenant, _userAdmin, course.Id, new LmsChapterUpsertRequest(null, "Chương 1", 1));
        var l1 = await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id, new LmsLessonUpsertRequest(null, "Bài 1: Khởi Đầu", "Video", "https://cdn.erp.vn/l1.mp4", null, 1, 300));
        var l2 = await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id, new LmsLessonUpsertRequest(null, "Bài 2: Thực Hành", "Text", null, "Nội dung bài 2", 2, null));

        await _courseSvc.SetPublishStatusAsync(_tenant, _userAdmin, course.Id, new LmsPublishCourseRequest("Published"));

        _courseId = course.Id;
        _chapterId = chap.Id;
        _lesson1Id = l1.Id;
        _lesson2Id = l2.Id;

        // 2. Tạo Quiz Chương
        var q = await _examSvc.UpsertQuestionAsync(_tenant, _userAdmin,
            new LmsQuestionUpsertRequest(null, "Q_STEP53_1", "Câu hỏi 1 + 1 = ?", "SingleChoice",
                new[] { new LmsQuestionOptionDto("A", "2"), new LmsQuestionOptionDto("B", "3") },
                new[] { "A" }, 10m, "Dễ", true));
        _questionId = q.Id;

        var quiz = await _examSvc.UpsertExamAsync(_tenant, _userAdmin,
            new LmsExamUpsertRequest(null, "QUIZ_CHAP1_53", "Quiz Kiểm Tra Chương 1", "ChapterQuiz", course.Id, chap.Id, 80, 2, 15, "Draft"));
        await _examSvc.AddQuestionToExamAsync(_tenant, _userAdmin, quiz.Id, new LmsExamAddQuestionRequest(q.Id, null));
        await _examSvc.SetExamStatusAsync(_tenant, _userAdmin, quiz.Id, new LmsPublishExamRequest("Published"));
        _quizId = quiz.Id;

        // Ghi danh học viên
        await _courseSvc.EnrollAsync(_tenant, _userLearner, _courseId, new LmsEnrollRequest(null));
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_035: Đánh dấu hoàn thành bài học (Lesson Completion)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_035_CompleteLesson_ValidLesson_MarksStatusCompleted()
    {
        var prog = await _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, _lesson1Id, new LmsCompleteLessonRequest(300));

        Assert.NotNull(prog);
        Assert.Equal(_lesson1Id, prog.LessonId);
        Assert.Equal("Completed", prog.Status);
    }

    [Fact]
    public async Task UC_LMS_035_CompleteLesson_NonExistentLesson_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, Guid.NewGuid(), new LmsCompleteLessonRequest(0)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Bài học không thuộc khóa này", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_036: Tiếp tục học dở (Resume Learning Pointer)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_036_GetLearn_InitialState_ResumesFirstLesson()
    {
        var learn = await _courseSvc.GetLearnAsync(_tenant, _userLearner, _courseId);

        Assert.NotNull(learn);
        Assert.Equal(_lesson1Id, learn.ResumeLessonId);
    }

    [Fact]
    public async Task UC_LMS_036_GetLearn_AfterCompletingLesson1_ResumesLesson2()
    {
        await _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, _lesson1Id, new LmsCompleteLessonRequest(300));

        var learn = await _courseSvc.GetLearnAsync(_tenant, _userLearner, _courseId);

        Assert.NotNull(learn);
        Assert.Equal(_lesson2Id, learn.ResumeLessonId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_037: Theo dõi % tiến độ khóa (% Course Progress Tracking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_037_GetLearn_ProgressIncreasesAsLessonsCompleted()
    {
        var learnBefore = await _courseSvc.GetLearnAsync(_tenant, _userLearner, _courseId);
        Assert.Empty(learnBefore.Progress);

        await _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, _lesson1Id, new LmsCompleteLessonRequest(300));
        await _courseSvc.CompleteLessonAsync(_tenant, _userLearner, _courseId, _lesson2Id, new LmsCompleteLessonRequest(0));

        var learnAfter = await _courseSvc.GetLearnAsync(_tenant, _userLearner, _courseId);
        Assert.Equal(2, learnAfter.Progress.Count);
        Assert.All(learnAfter.Progress, p => Assert.Equal("Completed", p.Status));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_040: Làm quiz cuối chương (Chapter Quiz Execution & Evaluation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_040_StartAttempt_ValidChapterQuiz_CreatesAttemptSuccessfully()
    {
        var attempt = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _quizId);

        Assert.NotNull(attempt);
        Assert.Equal(_quizId, attempt.ExamId);
        Assert.Equal(1, attempt.AttemptNo);
        Assert.Equal("InProgress", attempt.Status);
        Assert.NotEmpty(attempt.Questions);
    }

    [Fact]
    public async Task UC_LMS_040_SubmitAttempt_CorrectAnswer_PassesQuiz()
    {
        var attempt = await _examSvc.StartAttemptAsync(_tenant, _userLearner, _quizId);

        var answers = new Dictionary<string, string> { [_questionId.ToString()] = "A" };
        var result = await _examSvc.SubmitAttemptAsync(_tenant, _userLearner, attempt.Id,
            new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.True(result.Passed);
        Assert.Equal(10m, result.Score);
    }

    [Fact]
    public async Task UC_LMS_040_SubmitAttempt_WrongAnswer_FailsQuiz()
    {
        var anotherLearner = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = anotherLearner, TenantId = _tenant, Username = "learner_wrong53", DisplayName = "Learner Wrong 53" });
        await _db.SaveChangesAsync();
        await _courseSvc.EnrollAsync(_tenant, anotherLearner, _courseId, new LmsEnrollRequest(null));

        var attempt = await _examSvc.StartAttemptAsync(_tenant, anotherLearner, _quizId);

        var answers = new Dictionary<string, string> { [_questionId.ToString()] = "B" };
        var result = await _examSvc.SubmitAttemptAsync(_tenant, anotherLearner, attempt.Id,
            new LmsSubmitAttemptRequest(answers));

        Assert.NotNull(result);
        Assert.False(result.Passed);
        Assert.Equal(0m, result.Score);
    }

    [Fact]
    public async Task UC_LMS_040_StartAttempt_ExceedMaxAttempts_ThrowsAppException()
    {
        var repeatedUser = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = repeatedUser, TenantId = _tenant, Username = "learner_repeat53", DisplayName = "Learner Repeat 53" });
        await _db.SaveChangesAsync();
        await _courseSvc.EnrollAsync(_tenant, repeatedUser, _courseId, new LmsEnrollRequest(null));

        var wrongAns = new Dictionary<string, string> { [_questionId.ToString()] = "B" };

        // Attempt 1
        var att1 = await _examSvc.StartAttemptAsync(_tenant, repeatedUser, _quizId);
        await _examSvc.SubmitAttemptAsync(_tenant, repeatedUser, att1.Id, new LmsSubmitAttemptRequest(wrongAns));

        // Attempt 2
        var att2 = await _examSvc.StartAttemptAsync(_tenant, repeatedUser, _quizId);
        await _examSvc.SubmitAttemptAsync(_tenant, repeatedUser, att2.Id, new LmsSubmitAttemptRequest(wrongAns));

        // Attempt 3 -> Should throw
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.StartAttemptAsync(_tenant, repeatedUser, _quizId));

        Assert.Contains("Đã hết số lần thi cho phép", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_040_ListLearnerExams_ReturnsPublishedQuizzes()
    {
        var exams = await _examSvc.ListLearnerExamsAsync(_tenant, _userLearner, _courseId);

        Assert.NotEmpty(exams);
        Assert.Contains(exams, e => e.Id == _quizId);
    }
}

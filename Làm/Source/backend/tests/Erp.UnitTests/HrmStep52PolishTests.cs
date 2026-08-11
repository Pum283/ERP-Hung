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
/// Unit tests cho Bước 52:
///   UC_LMS_031 — Mua khóa / thanh toán online (Course Online Payment & Checkout)
///   UC_LMS_032 — Kích hoạt bằng mã voucher (Promo / Voucher Code Redemption)
///   UC_LMS_033 — Tự mở khóa sau thanh toán (Auto Unlock & Enrollment Status Transition)
///   UC_LMS_034 — Xem video / tài liệu (Learn Player, Content Viewing & Progress Tracking)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep52PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCourseService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _userLearner = Guid.NewGuid();

    private Guid _coursePaidId;
    private Guid _courseFreeId;

    public HrmStep52PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step52-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms52", DisplayName = "Admin LMS 52" });
        _db.Users.Add(new AppUser { Id = _userLearner, TenantId = _tenant, Username = "learner_lms52", DisplayName = "Học Viên Online 52" });

        _db.SaveChanges();

        _svc = new LmsCourseService(_db);

        InitCoursesAsync().GetAwaiter().GetResult();
    }

    private async Task InitCoursesAsync()
    {
        var cPaid = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_PAID52", "Khóa Trả Phí 52", "Mô tả trả phí", "Online", "Draft", 500000m, "VND", null));
        var ch1 = await _svc.UpsertChapterAsync(_tenant, _userAdmin, cPaid.Id, new LmsChapterUpsertRequest(null, "Chương 1", 1));
        await _svc.UpsertLessonAsync(_tenant, _userAdmin, ch1.Id, new LmsLessonUpsertRequest(null, "Bài 1", "Video", "https://cdn.erp.vn/v1.mp4", null, 1, 600));
        await _svc.SetPublishStatusAsync(_tenant, _userAdmin, cPaid.Id, new LmsPublishCourseRequest("Published"));
        _coursePaidId = cPaid.Id;

        var cFree = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_FREE52", "Khóa Miễn Phí 52", "Mô tả miễn phí", "Online", "Draft", 0m, "VND", null));
        var ch2 = await _svc.UpsertChapterAsync(_tenant, _userAdmin, cFree.Id, new LmsChapterUpsertRequest(null, "Chương Miễn Phí", 1));
        await _svc.UpsertLessonAsync(_tenant, _userAdmin, ch2.Id, new LmsLessonUpsertRequest(null, "Bài Mẫu", "Text", null, "Nội dung văn bản", 1, null));
        await _svc.SetPublishStatusAsync(_tenant, _userAdmin, cFree.Id, new LmsPublishCourseRequest("Published"));
        _courseFreeId = cFree.Id;
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_031: Mua khóa / thanh toán online (Online Payment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_031_Enroll_PaidCourse_SetsPaidAmountAndUnlocks()
    {
        var en = await _svc.EnrollAsync(_tenant, _userLearner, _coursePaidId, new LmsEnrollRequest(null));

        Assert.NotNull(en);
        Assert.Equal(500000m, en.PaidAmount);
        Assert.Equal("Unlocked", en.Status);
    }

    [Fact]
    public async Task UC_LMS_031_Enroll_UnpublishedCourse_ThrowsAppException()
    {
        var draftCourse = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_DRAFT52", "Khóa Nháp 52", null, "Online", "Draft", 100000m, "VND", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.EnrollAsync(_tenant, _userLearner, draftCourse.Id, new LmsEnrollRequest(null)));

        Assert.Contains("Khóa chưa xuất bản", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_032: Kích hoạt bằng mã voucher (Voucher Code Redemption)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_032_Enroll_ValidVoucher_SetsZeroAmountAndUnlocks()
    {
        var anotherUser = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = anotherUser, TenantId = _tenant, Username = "user_voucher52", DisplayName = "Voucher User 52" });
        await _db.SaveChangesAsync();

        var en = await _svc.EnrollAsync(_tenant, anotherUser, _coursePaidId, new LmsEnrollRequest("FREE"));

        Assert.NotNull(en);
        Assert.Equal(0m, en.PaidAmount);
        Assert.Equal("Unlocked", en.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_033: Tự mở khóa sau thanh toán (Auto Unlock & Status Transition)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_033_Enroll_FreeCourse_AutoUnlocksImmediately()
    {
        var en = await _svc.EnrollAsync(_tenant, _userLearner, _courseFreeId, new LmsEnrollRequest(null));

        Assert.NotNull(en);
        Assert.Equal(0m, en.PaidAmount);
        Assert.Equal("Unlocked", en.Status);
    }

    [Fact]
    public async Task UC_LMS_033_Enroll_AlreadyUnlockedCourse_ReturnsExistingEnrollment()
    {
        var en1 = await _svc.EnrollAsync(_tenant, _userLearner, _courseFreeId, new LmsEnrollRequest(null));
        var en2 = await _svc.EnrollAsync(_tenant, _userLearner, _courseFreeId, new LmsEnrollRequest(null));

        Assert.Equal(en1.Id, en2.Id);
        Assert.Equal("Unlocked", en2.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_034: Xem video / tài liệu (Learn Player & Progress Tracking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_034_GetLearn_UnlockedCourse_ReturnsLearnDetailsAndResumeLesson()
    {
        await _svc.EnrollAsync(_tenant, _userLearner, _coursePaidId, new LmsEnrollRequest(null));

        var learn = await _svc.GetLearnAsync(_tenant, _userLearner, _coursePaidId);

        Assert.NotNull(learn);
        Assert.Equal("CRS_PAID52", learn.Course.Code);
        Assert.NotEmpty(learn.Chapters);
        Assert.NotEmpty(learn.Lessons);
        Assert.NotNull(learn.ResumeLessonId);
    }

    [Fact]
    public async Task UC_LMS_034_GetLearn_NotEnrolledCourse_ThrowsAppException()
    {
        var nonEnrolledUser = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = nonEnrolledUser, TenantId = _tenant, Username = "user_noenroll52", DisplayName = "No Enroll 52" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.GetLearnAsync(_tenant, nonEnrolledUser, _coursePaidId));

        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("Chưa ghi danh khóa học", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_034_CompleteLesson_ValidLesson_UpdatesProgressToCompleted()
    {
        await _svc.EnrollAsync(_tenant, _userLearner, _coursePaidId, new LmsEnrollRequest(null));
        var learn = await _svc.GetLearnAsync(_tenant, _userLearner, _coursePaidId);
        var lessonId = learn.Lessons[0].Id;

        var prog = await _svc.CompleteLessonAsync(_tenant, _userLearner, _coursePaidId, lessonId, new LmsCompleteLessonRequest(600));

        Assert.NotNull(prog);
        Assert.Equal(lessonId, prog.LessonId);
        Assert.Equal("Completed", prog.Status);
    }

    [Fact]
    public async Task UC_LMS_034_CompleteLesson_NotEnrolled_ThrowsAppException()
    {
        var stranger = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = stranger, TenantId = _tenant, Username = "stranger52", DisplayName = "Stranger 52" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CompleteLessonAsync(_tenant, stranger, _coursePaidId, Guid.NewGuid(), new LmsCompleteLessonRequest(0)));

        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("Chưa ghi danh khóa học", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_031_ListCourses_ReturnsPublishedPaidAndFreeCourses()
    {
        var courses = await _svc.ListCoursesAsync(_tenant);

        Assert.NotNull(courses);
        Assert.Contains(courses, c => c.Code == "CRS_PAID52");
        Assert.Contains(courses, c => c.Code == "CRS_FREE52");
    }
}

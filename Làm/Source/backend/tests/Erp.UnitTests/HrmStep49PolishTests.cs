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
/// Unit tests cho Bước 49:
///   UC_LMS_006 — Upload tài liệu PDF / slide (Document / Slide Material Lesson)
///   UC_LMS_009 — Ẩn / xuất bản khóa học (Course Publish & Hide Status)
///   UC_LMS_014 — Cấu hình điểm đạt / số lần thi (Exam Pass Score & Max Attempts Config)
///   UC_LMS_016 — Mở lớp đào tạo offline (Offline Class Schedule & Capacity Creation)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep49PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCourseService _courseSvc;
    private readonly LmsExamService _examSvc;
    private readonly LmsClassService _classSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public HrmStep49PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step49-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms49", DisplayName = "Admin LMS 49" });

        _db.SaveChanges();

        _courseSvc = new LmsCourseService(_db);
        _examSvc = new LmsExamService(_db);
        _classSvc = new LmsClassService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_006: Upload tài liệu PDF / slide (Document Lesson Type)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_006_UpsertLesson_DocumentLessonType_CreatesDocumentLessonWithUrl()
    {
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_DOC49", "Khóa Tài Liệu 49", null, "Online", "Draft", 0m, "VND", null));
        var chap = await _courseSvc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương Tài Liệu", 1));

        var lesson = await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id,
            new LmsLessonUpsertRequest(null, "Bài 1: Slide Bài Giảng PDF", "Document", "https://cdn.erp.vn/docs/slide1.pdf", "Nội dung slide tổng quan", 1, null));

        Assert.NotNull(lesson);
        Assert.Equal("Document", lesson.LessonType);
        Assert.Equal("https://cdn.erp.vn/docs/slide1.pdf", lesson.ContentUrl);
        Assert.Equal("Nội dung slide tổng quan", lesson.Body);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_009: Ẩn / xuất bản khóa học (Publish & Hide Status)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_009_SetPublishStatus_PublishCourseWithLesson_UpdatesStatusToPublished()
    {
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_PUB49", "Khóa Xuất Bản 49", null, "Online", "Draft", 0m, "VND", null));
        var chap = await _courseSvc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương A", 1));
        await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id,
            new LmsLessonUpsertRequest(null, "Bài 1", "Text", null, "Nội dung", 1, null));

        var updated = await _courseSvc.SetPublishStatusAsync(_tenant, _userAdmin, course.Id,
            new LmsPublishCourseRequest("Published"));

        Assert.NotNull(updated);
        Assert.Equal("Published", updated.Status);
    }

    [Fact]
    public async Task UC_LMS_009_SetPublishStatus_PublishCourseWithoutLesson_ThrowsAppException()
    {
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_NOLESSON49", "Khóa Trống 49", null, "Online", "Draft", 0m, "VND", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _courseSvc.SetPublishStatusAsync(_tenant, _userAdmin, course.Id,
                new LmsPublishCourseRequest("Published")));

        Assert.Contains("Cần ít nhất 1 bài học", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_009_SetPublishStatus_HideCourse_UpdatesStatusToHidden()
    {
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_HIDE49", "Khóa Ẩn 49", null, "Online", "Draft", 0m, "VND", null));

        var updated = await _courseSvc.SetPublishStatusAsync(_tenant, _userAdmin, course.Id,
            new LmsPublishCourseRequest("Hidden"));

        Assert.NotNull(updated);
        Assert.Equal("Hidden", updated.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_014: Cấu hình điểm đạt / số lần thi (Exam Pass Score & Attempts Config)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_014_UpsertExam_ValidPassScoreAndAttempts_CreatesExamSuccessfully()
    {
        var exam = await _examSvc.UpsertExamAsync(_tenant, _userAdmin,
            new LmsExamUpsertRequest(null, "EXAM_PASS49", "Đề Thi Cuối Khóa 49", "Final", null, null, 80, 3, 45, "Draft"));

        Assert.NotNull(exam);
        Assert.Equal(80, exam.PassScore);
        Assert.Equal(3, exam.MaxAttempts);
    }

    [Fact]
    public async Task UC_LMS_014_UpsertExam_PassScoreOutOfRange_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.UpsertExamAsync(_tenant, _userAdmin,
                new LmsExamUpsertRequest(null, "EXAM_BADPASS49", "Đề Sai Điểm", "Final", null, null, 120, 3, 45, "Draft")));

        Assert.Contains("Điểm đạt 0–100", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_014_UpsertExam_ZeroMaxAttempts_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _examSvc.UpsertExamAsync(_tenant, _userAdmin,
                new LmsExamUpsertRequest(null, "EXAM_BADATT49", "Đề Sai Lần Thi", "Final", null, null, 70, 0, 45, "Draft")));

        Assert.Contains("Số lần thi tối thiểu 1", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_016: Mở lớp đào tạo offline (Offline Class Creation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_016_UpsertClass_ValidClassDetails_CreatesOfflineClassSuccessfully()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var endDate = startDate.AddDays(14);

        var cls = await _classSvc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLASS_OFF49", "Lớp An Toàn Lao Động 49", "An Toàn Lao Động K49", null, null, "Phòng 301 - Tòa B", startDate, endDate, "Open"));

        Assert.NotNull(cls);
        Assert.Equal("CLASS_OFF49", cls.Code);
        Assert.Equal("Open", cls.Status);
    }

    [Fact]
    public async Task UC_LMS_016_UpsertClass_EndDateBeforeStartDate_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var endDate = startDate.AddDays(-1);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _classSvc.UpsertClassAsync(_tenant, _userAdmin,
                new LmsTrainingClassUpsertRequest(null, "CLASS_BADDATE49", "Lớp Sai Ngày", "Khóa A", null, null, null, startDate, endDate, "Draft")));

        Assert.Contains("Ngày kết thúc phải sau ngày bắt đầu", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_016_ListClasses_ReturnsCreatedClass()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var endDate = startDate.AddDays(5);

        await _classSvc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLASS_LIST49", "Lớp Danh Sách 49", "Khóa List 49", null, null, null, startDate, endDate, "Draft"));

        var list = await _classSvc.ListClassesAsync(_tenant);

        Assert.NotEmpty(list);
        Assert.Contains(list, x => x.Code == "CLASS_LIST49");
    }
}

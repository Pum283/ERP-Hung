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
/// Unit tests cho Bước 48:
///   UC_LMS_002 — Danh mục khóa học (LMS Course Catalog)
///   UC_LMS_003 — Phân loại khóa (Online/Offline/Blended Modes)
///   UC_LMS_004 — Quản lý chương / bài học (Chapter & Lesson Management)
///   UC_LMS_005 — Upload video bài giảng (Lesson Video Media Upload)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep48PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCourseService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public HrmStep48PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step48-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms48", DisplayName = "Admin LMS 48" });

        _db.SaveChanges();

        _svc = new LmsCourseService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_002: Danh mục khóa học (Course Catalog)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_002_UpsertCourse_ValidParameters_CreatesCourseSuccessfully()
    {
        var course = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_CSHARP48", "Lập trình C# nâng cao 48", "Tóm tắt khóa học C#", "Online", "Draft", 0m, "VND", null));

        Assert.NotNull(course);
        Assert.Equal("CRS_CSHARP48", course.Code);
        Assert.Equal("Online", course.DeliveryMode);
    }

    [Fact]
    public async Task UC_LMS_002_UpsertCourse_DuplicateCode_ThrowsAppException()
    {
        await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_DUP48", "Khóa A", null, "Online", "Draft", 0m, "VND", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertCourseAsync(_tenant, _userAdmin,
                new LmsCourseUpsertRequest(null, null, "CRS_DUP48", "Khóa B", null, "Online", "Draft", 0m, "VND", null)));

        Assert.Contains("Mã khóa đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_002_UpsertCourse_InvalidPrice_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertCourseAsync(_tenant, _userAdmin,
                new LmsCourseUpsertRequest(null, null, "CRS_NEG48", "Khóa Giá Âm", null, "Online", "Draft", -100000m, "VND", null)));

        Assert.Contains("Giá khóa không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_003: Phân loại khóa (Online/Offline/Blended Delivery Modes)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_003_UpsertCourse_BlendedMode_CreatesCourseSuccessfully()
    {
        var course = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_BLEND48", "Khóa Kết Hợp Blended 48", null, "Blended", "Draft", 500000m, "VND", null));

        Assert.NotNull(course);
        Assert.Equal("Blended", course.DeliveryMode);
    }

    [Fact]
    public async Task UC_LMS_003_UpsertCourse_InvalidDeliveryMode_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertCourseAsync(_tenant, _userAdmin,
                new LmsCourseUpsertRequest(null, null, "CRS_BADMODE48", "Khóa Sai Mode", null, "HYBRID_UNKNOWN", "Draft", 0m, "VND", null)));

        Assert.Contains("Hình thức khóa không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_004: Quản lý chương / bài học (Chapter & Lesson Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_004_UpsertChapter_ValidTitle_CreatesChapterSuccessfully()
    {
        var course = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_CHAP48", "Khóa Chương 48", null, "Online", "Draft", 0m, "VND", null));

        var chap = await _svc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương 1: Giới thiệu cơ bản", 1));

        Assert.NotNull(chap);
        Assert.Equal("Chương 1: Giới thiệu cơ bản", chap.Title);
        Assert.Equal(1, chap.SortOrder);
    }

    [Fact]
    public async Task UC_LMS_004_UpsertChapter_NonExistentCourse_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertChapterAsync(_tenant, _userAdmin, Guid.NewGuid(),
                new LmsChapterUpsertRequest(null, "Chương 1", 1)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Khóa học không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_005: Upload video bài giảng (Lesson Video Media Upload)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_005_UpsertLesson_VideoLessonType_CreatesLessonWithVideoUrl()
    {
        var course = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_VID48", "Khóa Video 48", null, "Online", "Draft", 0m, "VND", null));
        var chap = await _svc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương Video", 1));

        var lesson = await _svc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id,
            new LmsLessonUpsertRequest(null, "Bài 1: Video Bài Giảng C#", "Video", "https://cdn.erp.vn/videos/lesson1.mp4", null, 1, 900));

        Assert.NotNull(lesson);
        Assert.Equal("Video", lesson.LessonType);
        Assert.Equal("https://cdn.erp.vn/videos/lesson1.mp4", lesson.ContentUrl);
        Assert.Equal(900, lesson.DurationSec);
    }

    [Fact]
    public async Task UC_LMS_005_UpsertLesson_InvalidLessonType_ThrowsAppException()
    {
        var course = await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_BADLES48", "Khóa Bài Học Sai", null, "Online", "Draft", 0m, "VND", null));
        var chap = await _svc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương A", 1));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id,
                new LmsLessonUpsertRequest(null, "Bài 1", "AUDIO_UNKNOWN", null, null, 1, null)));

        Assert.Contains("Loại bài học không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_002_ListCourses_ReturnsCreatedCoursesWithCounts()
    {
        await _svc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_LIST48", "Khóa Danh Sách 48", null, "Online", "Draft", 0m, "VND", null));

        var list = await _svc.ListCoursesAsync(_tenant);

        Assert.NotEmpty(list);
        Assert.Contains(list, x => x.Code == "CRS_LIST48");
    }
}

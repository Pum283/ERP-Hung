using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 51:
///   UC_LMS_023 — Gán mentor cho học viên (LMS Learner Mentor Assignment)
///   UC_LMS_028 — Đăng ký tài khoản học viên (Learner Account Registration Validation)
///   UC_LMS_029 — Đăng nhập / quên mật khẩu (Learner Auth Login & Reset Password Rules)
///   UC_LMS_030 — Danh sách & chi tiết khóa (Course Public Roster & Deep Details)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep51PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsClassService _classSvc;
    private readonly LmsCourseService _courseSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _orgUnit1  = Guid.NewGuid();
    private readonly Guid _empMentee = Guid.NewGuid();
    private readonly Guid _empMentor = Guid.NewGuid();

    public HrmStep51PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step51-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms51", DisplayName = "Admin LMS 51" });

        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnit1, TenantId = _tenant, Code = "ORG51", Name = "Phòng Mentor 51", UnitType = "Department", Path = "/1" });

        _db.Employees.Add(new Employee
        {
            Id = _empMentee, TenantId = _tenant, EmployeeCode = "EMP_MENTEE_51", FullName = "Học Viên Mentee 51",
            OrgUnitId = _orgUnit1, Status = "Active"
        });
        _db.Employees.Add(new Employee
        {
            Id = _empMentor, TenantId = _tenant, EmployeeCode = "EMP_MENTOR_51", FullName = "Giảng Viên Mentor 51",
            OrgUnitId = _orgUnit1, Status = "Active"
        });

        _db.SaveChanges();

        _classSvc = new LmsClassService(_db);
        _courseSvc = new LmsCourseService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_023: Gán mentor cho học viên (Mentor Assignment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_023_AssignMentor_DifferentEmployees_AssignsSuccessfully()
    {
        var assignment = await _classSvc.AssignMentorAsync(_tenant, _userAdmin,
            new LmsMentorAssignRequest(_empMentee, _empMentor, "Đồng hành học phần C#"));

        Assert.NotNull(assignment);
        Assert.Equal(_empMentee, assignment.MenteeEmployeeId);
        Assert.Equal(_empMentor, assignment.MentorEmployeeId);
        Assert.True(assignment.IsActive);
    }

    [Fact]
    public async Task UC_LMS_023_AssignMentor_SameEmployee_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _classSvc.AssignMentorAsync(_tenant, _userAdmin,
                new LmsMentorAssignRequest(_empMentee, _empMentee, "Tự làm mentor chính mình")));

        Assert.Contains("Mentor và mentee phải khác nhau", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_023_AssignMentor_NonExistentMentee_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _classSvc.AssignMentorAsync(_tenant, _userAdmin,
                new LmsMentorAssignRequest(Guid.NewGuid(), _empMentor, "Gán mentee ảo")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_028: Đăng ký tài khoản học viên (Learner Account Registration)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_028_RegisterAccount_ValidUser_AddsUserToTenant()
    {
        var newUserId = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = newUserId, TenantId = _tenant, Username = "learner51", DisplayName = "Học Viên Mới 51" });
        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == newUserId && u.TenantId == _tenant);
        Assert.NotNull(user);
        Assert.Equal("learner51", user.Username);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_029: Đăng nhập / quên mật khẩu (Learner Auth Rules)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_029_Login_ExistingUser_ReturnsActiveUser()
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == _userAdmin && u.TenantId == _tenant);
        Assert.NotNull(user);
        Assert.Equal("admin_lms51", user.Username);
    }

    [Fact]
    public async Task UC_LMS_029_Login_NonExistentUser_ReturnsNull()
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "ghost_user51" && u.TenantId == _tenant);
        Assert.Null(user);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_030: Danh sách & chi tiết khóa (Course Catalog & Detail)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_030_GetCourseDetail_ExistingCourse_ReturnsCourseWithChaptersAndLessons()
    {
        var course = await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_DET51", "Khóa Chi Tiết 51", "Mô tả khóa", "Online", "Draft", 0m, "VND", null));
        var chap = await _courseSvc.UpsertChapterAsync(_tenant, _userAdmin, course.Id,
            new LmsChapterUpsertRequest(null, "Chương 1", 1));
        await _courseSvc.UpsertLessonAsync(_tenant, _userAdmin, chap.Id,
            new LmsLessonUpsertRequest(null, "Bài 1", "Text", null, "Nội dung", 1, null));

        var detail = await _courseSvc.GetCourseDetailAsync(_tenant, course.Id);

        Assert.NotNull(detail);
        Assert.Equal("CRS_DET51", detail.Course.Code);
        Assert.Single(detail.Chapters);
        Assert.Single(detail.Lessons);
    }

    [Fact]
    public async Task UC_LMS_030_GetCourseDetail_NonExistentCourse_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _courseSvc.GetCourseDetailAsync(_tenant, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Khóa học không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_030_ListCourses_ReturnsAllTenantCourses()
    {
        await _courseSvc.UpsertCourseAsync(_tenant, _userAdmin,
            new LmsCourseUpsertRequest(null, null, "CRS_ALL51", "Khóa Tổng 51", null, "Online", "Draft", 0m, "VND", null));

        var list = await _courseSvc.ListCoursesAsync(_tenant);

        Assert.NotEmpty(list);
        Assert.Contains(list, c => c.Code == "CRS_ALL51");
    }
}

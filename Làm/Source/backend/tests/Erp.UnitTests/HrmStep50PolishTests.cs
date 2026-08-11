using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 50:
///   UC_LMS_017 — Gán giảng viên / địa điểm / lịch (Class Session & Instructor Assignment)
///   UC_LMS_018 — Tuyển sinh / ghi danh học viên (Learner Class Enrollment)
///   UC_LMS_019 — Điểm danh buổi học (Session Attendance Marking)
///   UC_LMS_022 — Đóng lớp & tổng kết (Class Closure & Summary)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep50PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsClassService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _orgUnit1  = Guid.NewGuid();
    private readonly Guid _empId1    = Guid.NewGuid();

    public HrmStep50PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step50-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms50", DisplayName = "Admin LMS 50" });

        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnit1, TenantId = _tenant, Code = "ORG50", Name = "Phòng Lớp 50", UnitType = "Department", Path = "/1" });

        _db.Employees.Add(new Employee
        {
            Id = _empId1, TenantId = _tenant, EmployeeCode = "EMP_S50_1", FullName = "Nguyễn Văn Học Viên 50",
            OrgUnitId = _orgUnit1, Status = "Active"
        });

        _db.SaveChanges();

        _svc = new LmsClassService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_017: Gán giảng viên / địa điểm / lịch (Session Assignment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_017_AddSession_ValidTopic_CreatesClassSessionSuccessfully()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_SESH50", "Lớp Lịch Học 50", "Khóa Lịch 50", null, "ThS. Nguyễn Văn A", "Phòng 101", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), "Open"));

        var sessionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var session = await _svc.AddSessionAsync(_tenant, _userAdmin, cls.Id,
            new LmsClassSessionCreateRequest(sessionDate, "Buổi 1: Tổng quan chương trình", new TimeOnly(9, 0), new TimeOnly(11, 30), 1));

        Assert.NotNull(session);
        Assert.Equal("Buổi 1: Tổng quan chương trình", session.Topic);
        Assert.Equal(sessionDate, session.SessionDate);
    }

    [Fact]
    public async Task UC_LMS_017_AddSession_ClosedClass_ThrowsAppException()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_CLOSED50", "Lớp Đã Đóng", "Khóa A", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "Draft"));
        await _svc.CloseClassAsync(_tenant, _userAdmin, cls.Id, new LmsClassCloseRequest("Đã hoàn thành khóa"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.AddSessionAsync(_tenant, _userAdmin, cls.Id,
                new LmsClassSessionCreateRequest(DateOnly.FromDateTime(DateTime.UtcNow), "Buổi 1", new TimeOnly(9, 0), new TimeOnly(11, 0), 1)));

        Assert.Contains("Lớp đã đóng", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_018: Tuyển sinh / ghi danh học viên (Learner Class Enrollment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_018_Enroll_ValidEmployee_EnrollsLearnerSuccessfully()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_ENROLL50", "Lớp Ghi Danh 50", "Khóa B", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));

        var enrollment = await _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1));

        Assert.NotNull(enrollment);
        Assert.Equal(_empId1, enrollment.EmployeeId);
        Assert.Equal("Enrolled", enrollment.Status);
    }

    [Fact]
    public async Task UC_LMS_018_Enroll_DuplicateEmployee_ThrowsAppException()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_DUPENROLL50", "Lớp Trùng Ghi Danh", "Khóa C", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));
        await _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1)));

        Assert.Contains("Học viên đã ghi danh", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_018_Enroll_NonExistentEmployee_ThrowsAppException()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_BADEMP50", "Lớp Học Viên Ảo", "Khóa D", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(Guid.NewGuid())));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_019: Điểm danh buổi học (Session Attendance Marking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_019_RecordAttendance_PresentStatus_MarksAttendanceSuccessfully()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_ATT50", "Lớp Điểm Danh 50", "Khóa E", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));
        var session = await _svc.AddSessionAsync(_tenant, _userAdmin, cls.Id,
            new LmsClassSessionCreateRequest(DateOnly.FromDateTime(DateTime.UtcNow), "Buổi Điểm Danh", new TimeOnly(8, 0), new TimeOnly(12, 0), 1));
        var enrollment = await _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1));

        var att = await _svc.RecordAttendanceAsync(_tenant, _userAdmin, session.Id,
            new LmsSessionAttendanceRequest(enrollment.Id, true, "Tham gia đầy đủ"));

        Assert.NotNull(att);
        Assert.True(att.Present);
    }

    [Fact]
    public async Task UC_LMS_019_RecordAttendance_NonExistentSession_ThrowsAppException()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_BADATT50", "Lớp Sai Buổi", "Khóa F", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));
        var enrollment = await _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.RecordAttendanceAsync(_tenant, _userAdmin, Guid.NewGuid(),
                new LmsSessionAttendanceRequest(enrollment.Id, true, null)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Buổi học không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_022: Đóng lớp & tổng kết (Class Closure & Summary)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_022_CloseClass_OpenClass_UpdatesStatusToClosed()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_CLOSE50", "Lớp Hoàn Thành 50", "Khóa G", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));

        var closed = await _svc.CloseClassAsync(_tenant, _userAdmin, cls.Id, new LmsClassCloseRequest("Lớp đã tốt nghiệp"));

        Assert.NotNull(closed);
        Assert.Equal("Closed", closed.Status);
    }

    [Fact]
    public async Task UC_LMS_022_CloseClass_NonExistentClass_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CloseClassAsync(_tenant, _userAdmin, Guid.NewGuid(), new LmsClassCloseRequest("Ghi chú")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Lớp không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_022_GetClassDetail_ReturnsDetailWithSessionsAndEnrollments()
    {
        var cls = await _svc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_DET50", "Lớp Chi Tiết 50", "Khóa H", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "Open"));
        await _svc.AddSessionAsync(_tenant, _userAdmin, cls.Id,
            new LmsClassSessionCreateRequest(DateOnly.FromDateTime(DateTime.UtcNow), "Buổi 1", new TimeOnly(8, 0), new TimeOnly(10, 0), 1));
        await _svc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empId1));

        var detail = await _svc.GetClassDetailAsync(_tenant, cls.Id);

        Assert.NotNull(detail);
        Assert.Equal("CLS_DET50", detail.Class.Code);
        Assert.Single(detail.Sessions);
        Assert.Single(detail.Enrollments);
    }
}

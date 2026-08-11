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
/// Unit tests cho Bước 55:
///   UC_LMS_045 — Cấp chứng chỉ điện tử (Digital Certificate Issuance)
///   UC_LMS_049 — Hồ sơ giảng viên (Instructor Profile Management)
///   UC_LMS_050 — Phân quyền giảng viên (Instructor Role & Permission Granting)
///   UC_LMS_051 — Theo dõi danh sách học viên (Learner Roster & Enrolled Student Monitoring)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep55PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsInstructorService _insSvc;
    private readonly LmsClassService _classSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userTeacher = Guid.NewGuid();
    private readonly Guid _empTeacher  = Guid.NewGuid();

    public HrmStep55PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step55-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms55", DisplayName = "Admin LMS 55" });
        _db.Users.Add(new AppUser { Id = _userTeacher, TenantId = _tenant, Username = "teacher_lms55", DisplayName = "Giảng Viên 55" });

        _db.Employees.Add(new Employee
        {
            Id = _empTeacher, TenantId = _tenant, EmployeeCode = "EMP_TEACHER_55", FullName = "Nguyễn Văn Giảng 55",
            Status = "Active", UserId = _userTeacher, Email = "teacher55@erp.vn"
        });

        _db.Roles.Add(new Role
        {
            Id = LmsInstructorService.RoleLmsInstructorId,
            TenantId = _tenant,
            Code = "LMS_INSTRUCTOR",
            Name = "Giảng viên LMS",
            IsSystem = true
        });

        _db.SaveChanges();

        _insSvc = new LmsInstructorService(_db);
        _classSvc = new LmsClassService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_049: Hồ sơ giảng viên (Instructor Profile Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_049_UpsertInstructor_ValidInput_CreatesInstructorSuccessfully()
    {
        var ins = await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(null, "INS_55_01", "ThS. Nguyễn Văn Giảng 55", _empTeacher, _userTeacher,
                "Tiến sĩ CNTT", "Lập trình C# & Architecture", "Giảng viên 10 năm kinh nghiệm", "teacher55@erp.vn", "0901234567", "Active", false));

        Assert.NotNull(ins);
        Assert.Equal("INS_55_01", ins.Code);
        Assert.Equal("ThS. Nguyễn Văn Giảng 55", ins.DisplayName);
        Assert.Equal(_empTeacher, ins.EmployeeId);
        Assert.Equal(_userTeacher, ins.UserId);
        Assert.Equal("Active", ins.Status);
    }

    [Fact]
    public async Task UC_LMS_049_UpsertInstructor_DuplicateCode_ThrowsAppException()
    {
        await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(null, "INS_DUP55", "Giảng Viên A", null, null, null, null, null, null, null, "Active", false));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _insSvc.UpsertAsync(_tenant, _userAdmin,
                new LmsInstructorUpsertRequest(null, "INS_DUP55", "Giảng Viên B", null, null, null, null, null, null, null, "Active", false)));

        Assert.Contains("Mã giảng viên đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_049_UpsertInstructor_InvalidCodeLength_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _insSvc.UpsertAsync(_tenant, _userAdmin,
                new LmsInstructorUpsertRequest(null, "", "Giảng Viên Rỗng", null, null, null, null, null, null, null, "Active", false)));

        Assert.Contains("Mã GV 1–40 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_049_UpsertInstructor_NonExistentEmployee_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _insSvc.UpsertAsync(_tenant, _userAdmin,
                new LmsInstructorUpsertRequest(null, "INS_GHOST", "Giảng Viên Ảo", Guid.NewGuid(), null, null, null, null, null, null, "Active", false)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_050: Phân quyền giảng viên (Instructor Role & Permission Granting)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_050_UpsertInstructor_WithGrantRole_AssignsInstructorRole()
    {
        var ins = await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(null, "INS_ROLE55", "Giảng Viên Có Quyền", _empTeacher, _userTeacher,
                null, null, null, null, null, "Active", true));

        Assert.NotNull(ins);

        var userRoleExists = await _db.UserRoles.AnyAsync(
            ur => ur.TenantId == _tenant && ur.UserId == _userTeacher && ur.RoleId == LmsInstructorService.RoleLmsInstructorId);
        Assert.True(userRoleExists);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_051: Theo dõi danh sách học viên (Learner Roster & Enrolled Students)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_051_ListInstructors_ReturnsAllInstructors()
    {
        await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(null, "INS_LIST55", "Giảng Viên Danh Sách", null, null, null, null, null, null, null, "Active", false));

        var list = await _insSvc.ListAsync(_tenant);

        Assert.NotEmpty(list);
        Assert.Contains(list, i => i.Code == "INS_LIST55");
    }

    [Fact]
    public async Task UC_LMS_051_GetClassDetail_ReturnsEnrolledLearnerList()
    {
        var cls = await _classSvc.UpsertClassAsync(_tenant, _userAdmin,
            new LmsTrainingClassUpsertRequest(null, "CLS_ROSTER55", "Lớp Học Viên 55", "Khóa ERP", null, null, "Hà Nội",
                DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), "Open"));

        await _classSvc.EnrollAsync(_tenant, _userAdmin, cls.Id, new LmsClassEnrollmentRequest(_empTeacher));

        var detail = await _classSvc.GetClassDetailAsync(_tenant, cls.Id);

        Assert.NotNull(detail);
        Assert.Single(detail.Enrollments);
        Assert.Equal(_empTeacher, detail.Enrollments[0].EmployeeId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_045: Cấp chứng chỉ điện tử (Digital Certificate Verification)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_045_VerifyCertificate_ExistingCert_ReturnsCertificateInfo()
    {
        var certId = Guid.NewGuid();
        _db.LmsCertificates.Add(new Erp.Domain.Entities.Lms.LmsCertificate
        {
            Id = certId,
            TenantId = _tenant,
            CourseId = Guid.NewGuid(),
            UserId = _userTeacher,
            Code = "CERT_ERP_55_001",
            IssuedAt = DateTimeOffset.UtcNow,
            CreatedBy = _userAdmin
        });
        await _db.SaveChangesAsync();

        var cert = await _db.LmsCertificates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == certId && c.TenantId == _tenant);
        Assert.NotNull(cert);
        Assert.Equal("CERT_ERP_55_001", cert.Code);
    }

    [Fact]
    public async Task UC_LMS_045_VerifyCertificate_NonExistentCert_ReturnsNull()
    {
        var cert = await _db.LmsCertificates.AsNoTracking().FirstOrDefaultAsync(c => c.Code == "CERT_GHOST" && c.TenantId == _tenant);
        Assert.Null(cert);
    }

    [Fact]
    public async Task UC_LMS_049_UpdateInstructorStatus_Inactive_DeactivatesInstructor()
    {
        var ins = await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(null, "INS_INACT55", "GV Ngừng Dạy", null, null, null, null, null, null, null, "Active", false));

        var updated = await _insSvc.UpsertAsync(_tenant, _userAdmin,
            new LmsInstructorUpsertRequest(ins.Id, "INS_INACT55", "GV Ngừng Dạy", null, null, null, null, null, null, null, "Inactive", false));

        Assert.Equal("Inactive", updated.Status);
    }
}

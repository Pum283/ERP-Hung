using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 38:
///   UC_HRM_143 — Báo cáo khen thưởng – kỷ luật (Reward & Discipline Summary Report)
///   UC_HRM_144 — Tạo đơn nghỉ việc (Create Offboarding Request / Resignation Form)
///   UC_HRM_145 — Cấu hình / kiểm tra báo trước (Notice Period Rule Check)
///   UC_HRM_146 — Duyệt đơn nghỉ việc (Approve Resignation / Offboarding Request)
/// 11 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep38PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmRewardDisciplineService _rewardSvc;
    private readonly HrmOffboardingService _offboardingSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep38PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step38-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit1, TenantId = _tenant,
            Code = "ORG_S38_1", Name = "Phòng Quản Lý 38", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_OFF38", Name = "Chuyên Viên Offboarding 38"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp38_1", DisplayName = "Hoàng Văn Nghỉ Việc 38" });
        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin38", DisplayName = "Admin Quản Lý 38" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S38_1", FullName = "Hoàng Văn Nghỉ Việc 38",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);
        _db.SaveChanges();

        _empId1 = emp1.Id;

        _rewardSvc = new HrmRewardDisciplineService(_db);
        _offboardingSvc = new HrmOffboardingService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_143: Báo cáo khen thưởng – kỷ luật (Reward & Discipline Summary)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC143_Report_ReturnsSummaryByKindForYear()
    {
        await _rewardSvc.CreateAsync(_tenant, _userEmp1, new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Khen thưởng năm 2026", DateOnly.FromDateTime(DateTime.UtcNow), "Reason", 1000000m, "Bonus", null, null));
        await _rewardSvc.CreateAsync(_tenant, _userEmp1, new RewardDisciplineCreateRequest(
            _empId1, "Discipline", "Kỷ luật năm 2026", DateOnly.FromDateTime(DateTime.UtcNow), "Reason", 200000m, "Deduction", null, null));

        var report = await _rewardSvc.ReportAsync(_tenant, DateTime.UtcNow.Year);

        Assert.NotNull(report);
        Assert.True(report.Count >= 2);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_144: Tạo đơn nghỉ việc (Create Offboarding Request)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC144_CreateOffboarding_ValidParameters_CreatesDraftCaseSuccessfully()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var res = await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Lý do cá nhân"));

        Assert.NotNull(res);
        Assert.Equal("Draft", res.Status);
        Assert.Equal(_empId1, res.EmployeeId);
        Assert.NotEmpty(res.Checklist);
    }

    [Fact]
    public async Task UC144_CreateOffboarding_DuplicateOpenCase_ThrowsAppException()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đơn 1"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _offboardingSvc.CreateAsync(_tenant, _userEmp1,
                new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đơn 2")));

        Assert.Contains("NV đã có hồ sơ offboarding đang mở", ex.Message);
    }

    [Fact]
    public async Task UC144_CreateOffboarding_NonExistentEmployee_ThrowsAppException()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _offboardingSvc.CreateAsync(_tenant, _userEmp1,
                new OffboardingCreateRequest(Guid.NewGuid(), reqDate, lastDay, "Personal", "Đơn sai NV")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_145: Cấu hình / kiểm tra báo trước (Notice Period Rule Check)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC145_UpsertSettings_ValidNoticeDays_UpdatesSettingsSuccessfully()
    {
        var settings = await _offboardingSvc.UpsertSettingsAsync(_tenant, _userAdmin,
            new OffboardingSettingUpsertRequest(45, true, true));

        Assert.NotNull(settings);
        Assert.Equal(45, settings.NoticeDays);
        Assert.True(settings.RequireChecklistComplete);
    }

    [Fact]
    public async Task UC145_UpsertSettings_InvalidNoticeDays_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _offboardingSvc.UpsertSettingsAsync(_tenant, _userAdmin,
                new OffboardingSettingUpsertRequest(400, true, true)));

        Assert.Contains("Số ngày báo trước 0–365", ex.Message);
    }

    [Fact]
    public async Task UC145_Submit_SatisfiedNotice_SetsNoticeSatisfiedToTrue()
    {
        await _offboardingSvc.UpsertSettingsAsync(_tenant, _userAdmin, new OffboardingSettingUpsertRequest(30, true, true));

        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35)); // 35 >= 30 days notice

        var created = await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đủ ngày báo trước"));

        var submitted = await _offboardingSvc.SubmitAsync(_tenant, _userEmp1, created.Id);

        Assert.Equal("Submitted", submitted.Status);
        Assert.True(submitted.NoticeSatisfied);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_146: Duyệt đơn nghỉ việc (Approve Offboarding Request)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC146_Approve_SubmittedRequest_SetsStatusToApproved()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var created = await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đợi duyệt"));
        await _offboardingSvc.SubmitAsync(_tenant, _userEmp1, created.Id);

        var approved = await _offboardingSvc.ApproveAsync(_tenant, _userAdmin, created.Id);

        Assert.Equal("Approved", approved.Status);
    }

    [Fact]
    public async Task UC146_Approve_DraftRequest_ThrowsAppException()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var created = await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đơn chưa nộp"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _offboardingSvc.ApproveAsync(_tenant, _userAdmin, created.Id));

        Assert.Contains("Chỉ duyệt đơn đã nộp", ex.Message);
    }

    [Fact]
    public async Task UC146_Reject_SubmittedRequest_SetsStatusToRejected()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var created = await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Đơn từ chối"));
        await _offboardingSvc.SubmitAsync(_tenant, _userEmp1, created.Id);

        var rejected = await _offboardingSvc.RejectAsync(_tenant, _userAdmin, created.Id,
            new OffboardingRejectRequest("Cần hoàn thành dự án gấp"));

        Assert.Equal("Rejected", rejected.Status);
    }
}

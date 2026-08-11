using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 39:
///   UC_HRM_147 — Checklist bàn giao (Offboarding Handover Checklist)
///   UC_HRM_148 — Thu hồi quyền hệ thống (System Access Revocation on Offboarding)
///   UC_HRM_149 — Quyết toán phép / lương nghỉ việc (Offboarding Final Settlement)
///   UC_HRM_150 — Phỏng vấn nghỉ việc & Hoàn tất (Exit Interview & Offboarding Completion)
/// 11 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep39PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmOffboardingService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep39PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step39-" + Guid.NewGuid())
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
            Code = "ORG_S39_1", Name = "Phòng Quản Lý 39", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_OFF39", Name = "Chuyên Viên Offboarding 39"
        });

        var user = new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp39_1", DisplayName = "Vũ Văn Bàn Giao 39", Status = UserStatus.Active };
        _db.Users.Add(user);
        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin39", DisplayName = "Admin Quản Lý 39", Status = UserStatus.Active });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S39_1", FullName = "Vũ Văn Bàn Giao 39",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.LeaveBalances.Add(new LeaveBalance
        {
            TenantId = _tenant, EmployeeId = emp1.Id, Year = DateTime.UtcNow.Year,
            Entitled = 12, Used = 4, Remaining = 8
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _svc = new HrmOffboardingService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<OffboardingCaseDto> CreateAndApproveCaseAsync()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        var created = await _svc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Personal", "Lý do xin thôi việc"));
        await _svc.SubmitAsync(_tenant, _userEmp1, created.Id);
        return await _svc.ApproveAsync(_tenant, _userAdmin, created.Id);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_147: Checklist bàn giao (Offboarding Handover Checklist)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC147_UpdateChecklist_ApprovedCase_UpdatesItemsAndSetsInProgress()
    {
        var approvedCase = await CreateAndApproveCaseAsync();
        var items = new List<OffboardingChecklistItemDto>
        {
            new OffboardingChecklistItemDto("assets", "Thu hồi laptop và thẻ từ", true),
            new OffboardingChecklistItemDto("docs", "Bàn giao tài liệu dự án", true),
            new OffboardingChecklistItemDto("knowledge", "Bàn giao quy trình vận hành", true),
            new OffboardingChecklistItemDto("access", "Thu hồi quyền hệ thống", true),
            new OffboardingChecklistItemDto("finance", "Thanh toán tạm ứng", true)
        };

        var updated = await _svc.UpdateChecklistAsync(_tenant, _userAdmin, approvedCase.Id,
            new OffboardingChecklistUpdateRequest(items));

        Assert.Equal("InProgress", updated.Status);
        Assert.True(updated.Checklist.All(x => x.Done));
    }

    [Fact]
    public async Task UC147_UpdateChecklist_RejectedCase_ThrowsAppException()
    {
        var approvedCase = await CreateAndApproveCaseAsync();
        // Reject case directly for test
        var entity = await _db.OffboardingCases.FirstAsync(x => x.Id == approvedCase.Id);
        entity.Status = "Rejected";
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpdateChecklistAsync(_tenant, _userAdmin, approvedCase.Id,
                new OffboardingChecklistUpdateRequest(new List<OffboardingChecklistItemDto>())));

        Assert.Contains("Không cập nhật checklist ở trạng thái hiện tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_148: Thu hồi quyền hệ thống (System Access Revocation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC148_RevokeAccess_ApprovedCase_SetsAccessRevokedAndDisablesAppUser()
    {
        var approvedCase = await CreateAndApproveCaseAsync();

        var revoked = await _svc.RevokeAccessAsync(_tenant, _userAdmin, approvedCase.Id);

        Assert.True(revoked.AccessRevoked);

        var user = await _db.Users.FirstAsync(x => x.Id == _userEmp1);
        Assert.Equal(UserStatus.Disabled, user.Status);
    }

    [Fact]
    public async Task UC148_RevokeAccess_DraftCase_ThrowsAppException()
    {
        var created = await _svc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), "Personal", "Lý do"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.RevokeAccessAsync(_tenant, _userAdmin, created.Id));

        Assert.Contains("Thu hồi quyền sau khi duyệt đơn", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_149: Quyết toán phép / lương nghỉ việc (Offboarding Final Settlement)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC149_Settle_ApprovedCase_CalculatesRemainingLeaveAndSettlementAmount()
    {
        var approvedCase = await CreateAndApproveCaseAsync();

        var settled = await _svc.SettleAsync(_tenant, _userAdmin, approvedCase.Id,
            new OffboardingSettleRequest(3200000m, 15000000m, "Thanh toán 8 ngày phép tồn"));

        Assert.Equal(8m, settled.LeaveDaysRemaining);
        Assert.Equal(3200000m, settled.LeaveSettlementAmount);
        Assert.Equal(15000000m, settled.FinalPayEstimate);
    }

    [Fact]
    public async Task UC149_Settle_DraftCase_ThrowsAppException()
    {
        var created = await _svc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), "Personal", "Lý do"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SettleAsync(_tenant, _userAdmin, created.Id,
                new OffboardingSettleRequest(1000000m, 5000000m, "Ghi chú")));

        Assert.Contains("Quyết toán sau khi duyệt đơn", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_150: Phỏng vấn nghỉ việc & Hoàn tất (Exit Interview & Completion)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC150_SaveInterview_ValidNotes_UpdatesInterviewNotesSuccessfully()
    {
        var approvedCase = await CreateAndApproveCaseAsync();

        var updated = await _svc.SaveInterviewAsync(_tenant, _userAdmin, approvedCase.Id,
            new OffboardingInterviewRequest("Nhân viên mong muốn tìm kiếm môi trường mới thử thách hơn."));

        Assert.Contains("tìm kiếm môi trường mới", updated.InterviewNotes);
    }

    [Fact]
    public async Task UC150_Complete_CompletedChecklist_UpdatesEmployeeStatusToResigned()
    {
        var approvedCase = await CreateAndApproveCaseAsync();
        var allDone = new List<OffboardingChecklistItemDto>
        {
            new OffboardingChecklistItemDto("assets", "Thu hồi laptop và thẻ từ", true),
            new OffboardingChecklistItemDto("docs", "Bàn giao tài liệu dự án", true),
            new OffboardingChecklistItemDto("knowledge", "Bàn giao quy trình vận hành", true),
            new OffboardingChecklistItemDto("access", "Thu hồi quyền hệ thống", true),
            new OffboardingChecklistItemDto("finance", "Thanh toán tạm ứng", true)
        };
        await _svc.UpdateChecklistAsync(_tenant, _userAdmin, approvedCase.Id, new OffboardingChecklistUpdateRequest(allDone));

        var completed = await _svc.CompleteAsync(_tenant, _userAdmin, approvedCase.Id);

        Assert.Equal("Completed", completed.Status);

        var emp = await _db.Employees.FirstAsync(x => x.Id == _empId1);
        Assert.Equal("Resigned", emp.Status);

        var statusChange = await _db.EmploymentStatusChanges.FirstOrDefaultAsync(x => x.EmployeeId == _empId1);
        Assert.NotNull(statusChange);
        Assert.Equal("Resigned", statusChange.ToStatus);
    }

    [Fact]
    public async Task UC150_Complete_IncompleteChecklistWithRequirement_ThrowsAppException()
    {
        await _svc.UpsertSettingsAsync(_tenant, _userAdmin, new OffboardingSettingUpsertRequest(30, true, true));
        var approvedCase = await CreateAndApproveCaseAsync(); // Checklist not all done

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CompleteAsync(_tenant, _userAdmin, approvedCase.Id));

        Assert.Contains("Checklist bàn giao chưa hoàn tất", ex.Message);
    }
}

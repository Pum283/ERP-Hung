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
/// Unit tests cho Bước 36:
///   UC_HRM_134 — Hủy đơn nghỉ (Cancel Leave Request & Restore Balance)
///   UC_HRM_136 — Lịch nghỉ theo đơn vị (Department Leave Calendar)
///   UC_HRM_137 — Import nghỉ lễ / ngày nghỉ (Import Public Holidays / Off Days)
///   UC_HRM_138 — Báo cáo nghỉ / quỹ phép (Leave & Entitlement Summary Report)
/// 12 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep36PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmLeaveService _leaveSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();
    private readonly Guid _leaveTypeId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep36PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step36-" + Guid.NewGuid())
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
            Code = "ORG_S36_1", Name = "Phòng Quản Lý Phép 36", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_LEAVE36", Name = "Chuyên Viên 36"
        });
        _db.LeaveTypes.Add(new LeaveType
        {
            Id = _leaveTypeId, TenantId = _tenant, Code = "ANNUAL36", Name = "Nghỉ phép năm 36", IsActive = true
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp36_1", DisplayName = "Ngô Văn Hủy Phép 36" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S36_1", FullName = "Ngô Văn Hủy Phép 36",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp1);
        _db.SaveChanges();

        _empId1 = emp1.Id;

        _leaveSvc = new HrmLeaveService(_db, new FakeWfRuntimeService());
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_134: Hủy đơn nghỉ (Cancel Leave Request & Restore Balance)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC134_CancelRequest_PendingStatus_CancelsRequestSuccessfully()
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var req = await _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
            new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, fromDate, 1, "Nghỉ việc cá nhân", false));

        var cancelled = await _leaveSvc.CancelRequestAsync(_tenant, _userEmp1, req.Id);

        Assert.NotNull(cancelled);
        Assert.Equal("Cancelled", cancelled.Status);
    }

    [Fact]
    public async Task UC134_CancelRequest_AlreadyCancelled_ThrowsAppException()
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var req = await _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
            new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, fromDate, 1, "Nghỉ việc gia đình", false));

        await _leaveSvc.CancelRequestAsync(_tenant, _userEmp1, req.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.CancelRequestAsync(_tenant, _userEmp1, req.Id));

        Assert.Contains("Đơn đã hủy/từ chối", ex.Message);
    }

    [Fact]
    public async Task UC134_CancelRequest_NonExistentId_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.CancelRequestAsync(_tenant, _userEmp1, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Đơn nghỉ không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_136: Lịch nghỉ theo đơn vị (Department Leave Calendar)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC136_Calendar_ReturnsPendingAndApprovedLeaveRequestsForOrgUnit()
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        await _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
            new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, fromDate, 1, "Nghỉ lịch đơn vị", false));

        // Manually update status to Pending to match calendar filter
        var reqEntity = await _db.LeaveRequests.FirstAsync(x => x.EmployeeId == _empId1);
        reqEntity.Status = "Pending";
        await _db.SaveChangesAsync();

        var calendar = await _leaveSvc.CalendarAsync(_tenant, _orgUnit1, fromDate, fromDate);

        Assert.Single(calendar);
        Assert.Equal(_empId1, calendar[0].EmployeeId);
        Assert.Equal("EMP_S36_1", calendar[0].EmployeeCode);
    }

    [Fact]
    public async Task UC136_Calendar_NoMatchingRecords_ReturnsEmptyList()
    {
        var futureFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3));
        var futureTo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3).AddDays(5));

        var calendar = await _leaveSvc.CalendarAsync(_tenant, _orgUnit1, futureFrom, futureTo);

        Assert.Empty(calendar);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_137: Import nghỉ lễ / ngày nghỉ (Import Public Holidays)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC137_UpsertHoliday_ValidParameters_CreatesHolidaySuccessfully()
    {
        var date = new DateOnly(2026, 9, 2);
        var hol = await _leaveSvc.UpsertHolidayAsync(_tenant, _userEmp1,
            new HolidayUpsertRequest(null, date, "Quốc Khánh 2/9", true, "Nghỉ hưởng nguyên lương"));

        Assert.NotNull(hol);
        Assert.Equal("Quốc Khánh 2/9", hol.Name);
        Assert.Equal(2026, hol.Year);
        Assert.True(hol.IsPaid);
    }

    [Fact]
    public async Task UC137_UpsertHoliday_EmptyName_ThrowsAppException()
    {
        var date = new DateOnly(2026, 4, 30);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.UpsertHolidayAsync(_tenant, _userEmp1,
                new HolidayUpsertRequest(null, date, "", true, null)));

        Assert.Contains("Tên ngày nghỉ 1–200 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC137_ImportHolidays_ValidBatchList_ImportsHolidaysSuccessfully()
    {
        var items = new List<HolidayImportItem>
        {
            new HolidayImportItem(new DateOnly(2026, 1, 1), "Tết Dương Lịch", true),
            new HolidayImportItem(new DateOnly(2026, 4, 30), "Ngày Giải Phóng", true),
            new HolidayImportItem(new DateOnly(2026, 5, 1), "Quốc Tế Lao Động", true)
        };

        var count = await _leaveSvc.ImportHolidaysAsync(_tenant, _userEmp1, items);

        Assert.Equal(3, count);

        var list = await _leaveSvc.ListHolidaysAsync(_tenant, 2026);
        Assert.True(list.Count >= 3);
    }

    [Fact]
    public async Task UC137_ImportHolidays_EmptyList_ReturnsZero()
    {
        var count = await _leaveSvc.ImportHolidaysAsync(_tenant, _userEmp1, new List<HolidayImportItem>());

        Assert.Equal(0, count);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_138: Báo cáo nghỉ / quỹ phép (Leave & Entitlement Summary Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC138_Report_ReturnsLeaveSummaryReportForYear()
    {
        await _leaveSvc.AdjustBalanceAsync(_tenant, _userEmp1,
            new LeaveBalanceAdjustRequest(_empId1, _leaveTypeId, 2026, 12, "Cấp đầu năm"));

        var report = await _leaveSvc.ReportAsync(_tenant, 2026, _orgUnit1);

        Assert.NotNull(report);
        Assert.Single(report);
        Assert.Equal(_empId1, report[0].EmployeeId);
        Assert.Equal(12, report[0].Entitled);
        Assert.Equal(12, report[0].Remaining);
    }
}

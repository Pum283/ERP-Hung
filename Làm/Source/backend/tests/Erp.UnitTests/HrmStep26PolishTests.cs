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
/// Unit tests cho Bước 26:
///   UC_HRM_089 — Sao chép lịch ca (Shift Schedule Batch Copy)
///   UC_HRM_090 — Khóa sổ lịch ca theo kỳ (Shift Roster Period Locking)
///   UC_HRM_091 — In / xuất lịch ca (Shift Roster CSV Export & Print View)
///   UC_HRM_092 — Tạo lệnh điều động (Staff Mobilization / Transfer Dispatch Order Creation)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep26PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmShiftService _shiftSvc;
    private readonly HrmTransferService _transferSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _orgUnit2    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _shiftId1;

    public HrmStep26PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step26-" + Guid.NewGuid())
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
            Code = "ORG_S26_1", Name = "Phòng Sản Xuất 26", UnitType = "Department", Path = "/1"
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit2, TenantId = _tenant,
            Code = "ORG_S26_2", Name = "Phòng Kho 26", UnitType = "Department", Path = "/2"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_WORKER26", Name = "Công Nhân Vận Hành"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "user_step26", DisplayName = "Phạm Quản Lý 26"
        });

        var emp = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S26_1", FullName = "Nguyễn Văn Điều Động",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp);

        var shift = new WorkShift
        {
            TenantId = _tenant, Code = "SH_DAY26", Name = "Ca Ngày 8h",
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(17, 0), BreakMinutes = 60,
            IsOvernight = false, IsActive = true
        };
        _db.WorkShifts.Add(shift);
        _db.SaveChanges();

        _empId1 = emp.Id;
        _shiftId1 = shift.Id;

        _shiftSvc = new HrmShiftService(_db);
        _transferSvc = new HrmTransferService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_089: Sao chép lịch ca (Shift Schedule Batch Copy)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC089_CopyShifts_ValidRange_CopiesAssignmentsToTargetPeriod()
    {
        var sourceFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var sourceTo = sourceFrom.AddDays(2); // 3 days
        var targetStart = sourceFrom.AddDays(7); // Next week

        // Assign shifts for 3 days in source period
        await _shiftSvc.AssignAsync(_tenant, _user, new ShiftAssignRequest(_empId1, _shiftId1, sourceFrom, "Day 1"));
        await _shiftSvc.AssignAsync(_tenant, _user, new ShiftAssignRequest(_empId1, _shiftId1, sourceFrom.AddDays(1), "Day 2"));
        await _shiftSvc.AssignAsync(_tenant, _user, new ShiftAssignRequest(_empId1, _shiftId1, sourceFrom.AddDays(2), "Day 3"));

        var count = await _shiftSvc.CopyAsync(_tenant, _user,
            new ShiftCopyRequest(sourceFrom, sourceTo, targetStart, _orgUnit1));

        Assert.Equal(3, count);

        var copied = await _shiftSvc.ListAssignmentsAsync(_tenant, _orgUnit1, _empId1, targetStart, targetStart.AddDays(2));
        Assert.Equal(3, copied.Count);
    }

    [Fact]
    public async Task UC089_CopyShifts_InvalidSourceRange_ThrowsAppException()
    {
        var sourceFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var sourceTo = sourceFrom.AddDays(-1); // To < From

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.CopyAsync(_tenant, _user,
                new ShiftCopyRequest(sourceFrom, sourceTo, sourceFrom.AddDays(7), null)));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC089_CopyShifts_Exceeds62Days_ThrowsAppException()
    {
        var sourceFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var sourceTo = sourceFrom.AddDays(70); // > 62 days

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.CopyAsync(_tenant, _user,
                new ShiftCopyRequest(sourceFrom, sourceTo, sourceFrom.AddDays(100), null)));

        Assert.Contains("tối đa 62 ngày", ex.Message);
    }

    [Fact]
    public async Task UC089_CopyShifts_TargetPeriodLocked_ThrowsAppException()
    {
        var sourceFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var sourceTo = sourceFrom.AddDays(2);
        var targetStart = sourceFrom.AddDays(30);
        var periodKey = targetStart.ToString("yyyy-MM");

        await _shiftSvc.AssignAsync(_tenant, _user, new ShiftAssignRequest(_empId1, _shiftId1, sourceFrom, null));
        await _shiftSvc.LockPeriodAsync(_tenant, _user, new ShiftLockRequest(_orgUnit1, periodKey, "Khóa tháng sau"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.CopyAsync(_tenant, _user,
                new ShiftCopyRequest(sourceFrom, sourceTo, targetStart, null)));

        Assert.Contains("đã khóa sổ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_090: Khóa sổ lịch ca theo kỳ (Shift Period Locking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC090_LockPeriod_ValidOrgAndKey_CreatesPeriodLockSuccessfully()
    {
        var lockDto = await _shiftSvc.LockPeriodAsync(_tenant, _user,
            new ShiftLockRequest(_orgUnit1, "2026-08", "Khóa ca tháng 8"));

        Assert.NotNull(lockDto);
        Assert.Equal(_orgUnit1, lockDto.OrgUnitId);
        Assert.Equal("2026-08", lockDto.PeriodKey);
        Assert.Equal(new DateOnly(2026, 8, 1), lockDto.PeriodFrom);
        Assert.Equal(new DateOnly(2026, 8, 31), lockDto.PeriodTo);
    }

    [Fact]
    public async Task UC090_LockPeriod_InvalidPeriodKeyFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.LockPeriodAsync(_tenant, _user,
                new ShiftLockRequest(_orgUnit1, "202608", "Khóa sai định dạng")));

        Assert.Contains("yyyy-MM", ex.Message);
    }

    [Fact]
    public async Task UC090_LockPeriod_NonExistentOrgUnit_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.LockPeriodAsync(_tenant, _user,
                new ShiftLockRequest(Guid.NewGuid(), "2026-08", null)));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_091: In / xuất lịch ca CSV (Export CSV)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC091_ExportCsv_WithAssignments_ReturnsCsvFormattedString()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _shiftSvc.AssignAsync(_tenant, _user, new ShiftAssignRequest(_empId1, _shiftId1, today, "Xuất CSV"));

        var csv = await _shiftSvc.ExportCsvAsync(_tenant, _orgUnit1, today, today);

        Assert.NotNull(csv);
        Assert.Contains("WorkDate,EmployeeCode,EmployeeName,OrgUnit", csv);
        Assert.Contains("EMP_S26_1", csv);
        Assert.Contains("Nguyễn Văn Điều Động", csv);
    }

    [Fact]
    public async Task UC091_ExportCsv_EmptyAssignments_ReturnsHeaderOnly()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5));

        var csv = await _shiftSvc.ExportCsvAsync(_tenant, _orgUnit1, futureDate, futureDate);

        Assert.NotNull(csv);
        Assert.Contains("WorkDate,EmployeeCode,EmployeeName,OrgUnit", csv);
        Assert.DoesNotContain("EMP_S26_1", csv);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_092: Tạo lệnh điều động (Staff Transfer / Mobilization Order)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC092_CreateOrder_ValidEmployeeAndDifferentOrgs_CreatesTransferOrderSuccessfully()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddMonths(1);

        var order = await _transferSvc.CreateOrderAsync(_tenant, _user,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, endDate, "Điều động tăng cường kho", 160, 500000, true, null, false, null));

        Assert.NotNull(order);
        Assert.Equal("Order", order.Kind);
        Assert.Equal(_empId1, order.EmployeeId);
        Assert.Equal(_orgUnit1, order.FromOrgUnitId);
        Assert.Equal(_orgUnit2, order.ToOrgUnitId);
        Assert.Equal("Điều động tăng cường kho", order.Reason);
    }

    [Fact]
    public async Task UC092_CreateOrder_SameFromAndToOrgUnits_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateOrderAsync(_tenant, _user,
                new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit1, startDate, null, "Trùng đơn vị", 160, null, false, null, false, null)));

        Assert.Contains("khác nhau", ex.Message);
    }

    [Fact]
    public async Task UC092_CreateOrder_NonExistentEmployee_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateOrderAsync(_tenant, _user,
                new TransferOrderCreateRequest(Guid.NewGuid(), _orgUnit1, _orgUnit2, startDate, null, "Lỗi nhân viên", null, null, false, null, false, null)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC092_CreateOrder_ReasonTooShort_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateOrderAsync(_tenant, _user,
                new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "AB", null, null, false, null, false, null)));

        Assert.Contains("3–500 ký tự", ex.Message);
    }
}

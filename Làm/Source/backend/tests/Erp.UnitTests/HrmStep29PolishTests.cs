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
/// Unit tests cho Bước 29:
///   UC_HRM_101 — Đăng ký thiết bị chấm (Biometric Device Registration & Sync)
///   UC_HRM_102 — Cấu hình geo-fence điểm chấm (Geo-fence Location Radius Configuration)
///   UC_HRM_103 — Cấu hình quy tắc đi trễ (Late Arrival Grace Period Rules Configuration)
///   UC_HRM_104 — Cấu hình mức trừ công khi trễ (Late Penalty Deduction Scale Configuration)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep29PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private string _empCode1 = "EMP_S29_1";

    public HrmStep29PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step29-" + Guid.NewGuid())
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
            Code = "ORG_S29_1", Name = "Phòng Vận Hành 29", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_OP29", Name = "Nhiệm Vụ Vận Hành 29"
        });
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "user_step29", DisplayName = "Phạm Quản Lý 29" });

        var emp = new Employee
        {
            TenantId = _tenant, EmployeeCode = _empCode1, FullName = "Hoàng Văn Chấm Công 29",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp);

        var dev = new AttendanceDevice
        {
            TenantId = _tenant, Code = "DEV_S29_1", Name = "Máy Vân Tay Cổng Cáp", DeviceType = "Fingerprint",
            OrgUnitId = _orgUnit1, IsActive = true
        };
        _db.AttendanceDevices.Add(dev);
        _db.SaveChanges();

        _empId1 = emp.Id;

        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_101: Đăng ký & đồng bộ thiết bị chấm (Device Registration & Sync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC101_SyncDevice_ValidPunchIn_SyncsAttendanceRecordSuccessfully()
    {
        var punchTime = DateTimeOffset.UtcNow;
        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem(_empCode1, punchTime, "In", "DEV_S29_1")
        };

        var result = await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        Assert.NotNull(result);
        Assert.Equal(1, result.Synced);
        Assert.Equal(0, result.SkippedUnknownEmployee);
        Assert.Equal(0, result.SkippedDuplicate);

        var today = DateOnly.FromDateTime(punchTime.LocalDateTime);
        var history = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(history);
        Assert.Equal("DeviceSync", history[0].CheckInMethod);
    }

    [Fact]
    public async Task UC101_SyncDevice_UnknownEmployee_IncrementsUnknownCounter()
    {
        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_NON_EXISTENT", DateTimeOffset.UtcNow, "In", "DEV_S29_1")
        };

        var result = await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        Assert.Equal(0, result.Synced);
        Assert.Equal(1, result.SkippedUnknownEmployee);
    }

    [Fact]
    public async Task UC101_SyncDevice_DuplicatePunchIn_IncrementsDuplicateCounter()
    {
        var punchTime1 = DateTimeOffset.UtcNow.AddHours(-2);
        var punchTime2 = DateTimeOffset.UtcNow.AddHours(-1); // Later punch time

        var items1 = new List<AttendanceDeviceSyncItem> { new AttendanceDeviceSyncItem(_empCode1, punchTime1, "In", "DEV_S29_1") };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items1));

        // Sync again with later punch time -> duplicate
        var items2 = new List<AttendanceDeviceSyncItem> { new AttendanceDeviceSyncItem(_empCode1, punchTime2, "In", "DEV_S29_1") };
        var result2 = await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items2));

        Assert.Equal(1, result2.SkippedDuplicate);
    }

    [Fact]
    public async Task UC101_SyncDevice_InvalidPunchType_IncrementsInvalidTypeCounter()
    {
        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem(_empCode1, DateTimeOffset.UtcNow, "UNKNOWN_PUNCH_TYPE", "DEV_S29_1")
        };

        var result = await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        Assert.Equal(0, result.Synced);
        Assert.Equal(1, result.SkippedInvalidType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_102: Cấu hình geo-fence điểm chấm (Geo-fence Configuration)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC102_UpsertGeoFence_ValidParameters_CreatesGeoFenceSuccessfully()
    {
        var gf = await _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
            new AttendanceGeoFenceUpsertRequest(null, "Chi Nhánh Tây Ninh", _orgUnit1, 11.31, 106.09, 250, true));

        Assert.NotNull(gf);
        Assert.Equal("Chi Nhánh Tây Ninh", gf.Name);
        Assert.Equal(11.31, gf.Latitude);
        Assert.Equal(106.09, gf.Longitude);
        Assert.Equal(250, gf.RadiusMeters);
    }

    [Fact]
    public async Task UC102_UpsertGeoFence_RadiusTooSmall_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
                new AttendanceGeoFenceUpsertRequest(null, "Điểm Siêu Nhỏ", _orgUnit1, 11.31, 106.09, 5, true)));

        Assert.Contains("10–50000 m", ex.Message);
    }

    [Fact]
    public async Task UC102_UpsertGeoFence_EmptyName_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
                new AttendanceGeoFenceUpsertRequest(null, "", _orgUnit1, 11.31, 106.09, 200, true)));

        Assert.Contains("1–100 ký tự", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_103: Cấu hình quy tắc đi trễ (Late Grace Period Rules)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC103_LateWithinGracePeriod_NoDeductionApplied()
    {
        // Policy: Grace = 15m
        await _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
            new AttendancePolicyUpsertRequest(true, true, true, false, 15, 30, 0.25m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 10)), TimeSpan.FromHours(7)); // 8:10 (late 10m <= 15m)

        var items = new List<AttendanceDeviceSyncItem> { new AttendanceDeviceSyncItem(_empCode1, checkInTime, "In", "DEV_S29_1") };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(0, rec.DeductedWorkUnit);
        Assert.Equal(1.0m, rec.WorkUnit);
    }

    [Fact]
    public async Task UC103_UpsertPolicy_LateGraceMinutesExceeds240_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
                new AttendancePolicyUpsertRequest(true, true, true, false, 300, 30, 0.25m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0))));

        Assert.Contains("Ân hạn trễ không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_104: Cấu hình mức trừ công khi trễ (Late Penalty Deduction Scale)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC104_LateBeyondGracePeriod_CalculatesDeductionCorrectly()
    {
        // Policy: Grace = 15m, DeductEvery = 30m, DeductWorkUnit = 0.25
        await _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
            new AttendancePolicyUpsertRequest(true, true, true, false, 15, 30, 0.25m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 45)), TimeSpan.FromHours(7)); // Late 45m (> 15m grace) -> late - grace = 30m -> 1 block -> deduct 0.25

        var items = new List<AttendanceDeviceSyncItem> { new AttendanceDeviceSyncItem(_empCode1, checkInTime, "In", "DEV_S29_1") };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(0.25m, rec.DeductedWorkUnit);
        Assert.Equal(0.75m, rec.WorkUnit);
    }

    [Fact]
    public async Task UC104_ExtremelyLate_DeductionCappedAtOneAndWorkUnitFlooredAtZero()
    {
        await _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
            new AttendancePolicyUpsertRequest(true, true, true, false, 15, 30, 0.5m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(14, 0)), TimeSpan.FromHours(7)); // Late 6 hours -> blocks * 0.5 > 1.0

        var items = new List<AttendanceDeviceSyncItem> { new AttendanceDeviceSyncItem(_empCode1, checkInTime, "In", "DEV_S29_1") };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(1.0m, rec.DeductedWorkUnit);
        Assert.Equal(0.0m, rec.WorkUnit);
    }

    [Fact]
    public async Task UC104_UpsertPolicy_InvalidDeductEveryMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
                new AttendancePolicyUpsertRequest(true, true, true, false, 15, 0, 0.25m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)))); // < 1m

        Assert.Contains("Bậc trừ công không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC104_UpsertPolicy_InvalidDeductWorkUnit_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
                new AttendancePolicyUpsertRequest(true, true, true, false, 15, 30, 1.2m, 14, 3, true, 30, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)))); // > 1.0

        Assert.Contains("Mức trừ công 0–1", ex.Message);
    }
}

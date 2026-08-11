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
/// Unit tests cho Bước 28:
///   UC_HRM_097 — Báo cáo giờ / chi phí điều động (Transfer Hours & Cost Report)
///   UC_HRM_098 — Cấu hình chấm vân tay / sinh trắc (Biometric / Fingerprint Attendance Device Configuration)
///   UC_HRM_099 — Cấu hình chấm theo GPS / Wi-Fi (GPS & Wi-Fi Attendance Geo-fence Configuration)
///   UC_HRM_100 — Cấu hình chấm bằng khuôn mặt / chính sách chấm công (Face Recognition & Attendance Policy Configuration)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep28PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmTransferService _transferSvc;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _orgUnit2    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep28PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step28-" + Guid.NewGuid())
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
            Code = "ORG_S28_1", Name = "Phòng Sản Xuất 28", UnitType = "Department", Path = "/1"
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit2, TenantId = _tenant,
            Code = "ORG_S28_2", Name = "Phòng Công Trường 28", UnitType = "Department", Path = "/2"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_ENG28", Name = "Kỹ Sư Công Trường 28"
        });
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "user_step28", DisplayName = "Trưởng Phòng Kỹ Thuật 28" });

        var emp = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S28_1", FullName = "Trần Văn Báo Cáo",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp);
        _db.SaveChanges();

        _empId1 = emp.Id;

        _transferSvc = new HrmTransferService(_db);
        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_097: Báo cáo giờ / chi phí điều động (Transfer Hours & Cost Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC097_SetActualHours_ValidHours_UpdatesOrderActualHoursSuccessfully()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _user,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Điều động sản xuất", 160, 500000, true, null, true, null));

        var updated = await _transferSvc.SetActualHoursAsync(_tenant, _user, order.Id, new TransferActualHoursRequest(168.5m));

        Assert.Equal(168.5m, updated.ActualHours);
    }

    [Fact]
    public async Task UC097_SetActualHours_NegativeHours_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _user,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lỗi giờ âm", 160, 500000, true, null, true, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.SetActualHoursAsync(_tenant, _user, order.Id, new TransferActualHoursRequest(-10m)));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC097_CostReport_AggregatesHoursAndCostByDestinationOrgUnit()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order1 = await _transferSvc.CreateOrderAsync(_tenant, _user,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lệnh 1", 100, 100000, true, null, true, null));
        await _transferSvc.SetActualHoursAsync(_tenant, _user, order1.Id, new TransferActualHoursRequest(110m));

        var report = await _transferSvc.CostReportAsync(_tenant, startDate.AddDays(-1), startDate.AddDays(30));

        Assert.NotEmpty(report);
        var row = report.First(x => x.OrgUnitId == _orgUnit2);
        Assert.Equal(1, row.OrderCount);
        Assert.Equal(100m, row.PlannedHours);
        Assert.Equal(110m, row.ActualHours);
        Assert.Equal(10000000m, row.EstimatedCost);
        Assert.Equal(11000000m, row.ActualCost);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_098: Cấu hình chấm vân tay / sinh trắc (Biometric Device Config)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC098_UpsertDevice_ValidBiometricDevice_CreatesDeviceSuccessfully()
    {
        var dev = await _attendanceSvc.UpsertDeviceAsync(_tenant, _user,
            new AttendanceDeviceUpsertRequest(null, "BIO_001", "Máy Chấm Vân Tay Cổng Chính", "Fingerprint", _orgUnit1, "SN-998877", true, "Thiết bị chính"));

        Assert.NotNull(dev);
        Assert.Equal("BIO_001", dev.Code);
        Assert.Equal("Máy Chấm Vân Tay Cổng Chính", dev.Name);
        Assert.Equal("Fingerprint", dev.DeviceType);
        Assert.True(dev.IsActive);
    }

    [Fact]
    public async Task UC098_UpsertDevice_EmptyCode_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertDeviceAsync(_tenant, _user,
                new AttendanceDeviceUpsertRequest(null, "", "Tên thiết bị", "Fingerprint", null, null, true, null)));

        Assert.Contains("1–40 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC098_UpsertDevice_DuplicateCode_ThrowsAppException()
    {
        await _attendanceSvc.UpsertDeviceAsync(_tenant, _user,
            new AttendanceDeviceUpsertRequest(null, "BIO_DUP", "Máy Vân Tay 1", "Fingerprint", null, null, true, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertDeviceAsync(_tenant, _user,
                new AttendanceDeviceUpsertRequest(null, "BIO_DUP", "Máy Vân Tay 2", "Fingerprint", null, null, true, null)));

        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC098_ListDevices_ReturnsActiveDevices()
    {
        await _attendanceSvc.UpsertDeviceAsync(_tenant, _user,
            new AttendanceDeviceUpsertRequest(null, "BIO_LIST1", "Máy 1", "Fingerprint", null, null, true, null));

        var list = await _attendanceSvc.ListDevicesAsync(_tenant);

        Assert.Single(list);
        Assert.Equal("BIO_LIST1", list[0].Code);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_099: Cấu hình chấm theo GPS / Wi-Fi (GPS & Geo-fence Config)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC099_UpsertGeoFence_ValidGpsGeoFence_CreatesGeoFenceSuccessfully()
    {
        var gf = await _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
            new AttendanceGeoFenceUpsertRequest(null, "Văn Phòng Trụ Sở Chính", _orgUnit1, 10.762622, 106.660172, 100, true));

        Assert.NotNull(gf);
        Assert.Equal("Văn Phòng Trụ Sở Chính", gf.Name);
        Assert.Equal(10.762622, gf.Latitude);
        Assert.Equal(106.660172, gf.Longitude);
        Assert.Equal(100, gf.RadiusMeters);
        Assert.True(gf.IsActive);
    }

    [Fact]
    public async Task UC099_UpsertGeoFence_RadiusTooSmall_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
                new AttendanceGeoFenceUpsertRequest(null, "Điểm Chấm Nhỏ", _orgUnit1, 10.76, 106.66, 5, true))); // < 10m

        Assert.Contains("10–50000 m", ex.Message);
    }

    [Fact]
    public async Task UC099_UpsertGeoFence_EmptyName_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
                new AttendanceGeoFenceUpsertRequest(null, "  ", _orgUnit1, 10.76, 106.66, 100, true)));

        Assert.Contains("1–100 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC099_ListGeoFences_ReturnsGeoFencesSuccessfully()
    {
        await _attendanceSvc.UpsertGeoFenceAsync(_tenant, _user,
            new AttendanceGeoFenceUpsertRequest(null, "Kho Hàng Trung Tâm", _orgUnit2, 10.80, 106.70, 200, true));

        var list = await _attendanceSvc.ListGeoFencesAsync(_tenant);

        Assert.Single(list);
        Assert.Equal("Kho Hàng Trung Tâm", list[0].Name);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_100: Cấu hình chấm bằng khuôn mặt & Chính sách chấm công
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC100_UpsertPolicy_EnableFaceAndFingerprint_UpdatesPolicySuccessfully()
    {
        var policy = await _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
            new AttendancePolicyUpsertRequest(true, true, true, true, 15, 30, 0.5m, 12, 7, true, 30, true, true, new TimeOnly(8, 0), new TimeOnly(17, 0)));

        Assert.NotNull(policy);
        Assert.True(policy.EnableFingerprint);
        Assert.True(policy.EnableApp);
        Assert.True(policy.EnableGeoFence);
        Assert.Equal(15, policy.LateGraceMinutes);
        Assert.Equal(30, policy.LateDeductEveryMinutes);
        Assert.Equal(0.5m, policy.LateDeductWorkUnit);
    }

    [Fact]
    public async Task UC100_UpsertPolicy_InvalidLateGraceMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
                new AttendancePolicyUpsertRequest(true, true, false, false, 300, 30, 0.5m, 12, 7, false, 0, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)))); // > 240m

        Assert.Contains("Ân hạn trễ không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC100_UpsertPolicy_InvalidDeductWorkUnit_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UpsertPolicyAsync(_tenant, _user,
                new AttendancePolicyUpsertRequest(true, true, false, false, 15, 30, 1.5m, 12, 7, false, 0, false, false, new TimeOnly(8, 0), new TimeOnly(17, 0)))); // > 1.0

        Assert.Contains("Mức trừ công 0–1", ex.Message);
    }

    [Fact]
    public async Task UC100_GetPolicy_DefaultPolicy_ReturnsInitializedPolicy()
    {
        var policy = await _attendanceSvc.GetPolicyAsync(_tenant);

        Assert.NotNull(policy);
        Assert.True(policy.LateGraceMinutes >= 0);
    }
}

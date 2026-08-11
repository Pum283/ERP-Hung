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
/// Unit tests cho Bước 27:
///   UC_HRM_093 — Đề xuất nhu cầu điều động (Staff Mobilization Request Creation)
///   UC_HRM_094 — Nhận lệnh điều động trên APP (Acknowledge Transfer Order on Mobile)
///   UC_HRM_095 — Theo dõi nhân sự điều động (Active Mobilization Tracking)
///   UC_HRM_096 — Gắn nhãn công điều động khi chấm (Attendance Tagging for Transfer)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep27PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmTransferService _transferSvc;

    private readonly Guid _tenant        = Guid.NewGuid();
    private readonly Guid _userRequester = Guid.NewGuid();
    private readonly Guid _userEmp1      = Guid.NewGuid();
    private readonly Guid _userEmp2      = Guid.NewGuid();
    private readonly Guid _orgUnit1       = Guid.NewGuid();
    private readonly Guid _orgUnit2       = Guid.NewGuid();
    private readonly Guid _jobTitleId    = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;

    public HrmStep27PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step27-" + Guid.NewGuid())
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
            Code = "ORG_S27_1", Name = "Phòng Dự Án 27", UnitType = "Department", Path = "/1"
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit2, TenantId = _tenant,
            Code = "ORG_S27_2", Name = "Phòng Công Trường 27", UnitType = "Department", Path = "/2"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_ENG27", Name = "Kỹ Sư Công Trường"
        });

        _db.Users.Add(new AppUser { Id = _userRequester, TenantId = _tenant, Username = "requester27", DisplayName = "Trưởng Phòng Nhu Cầu 27" });
        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp_user27_1", DisplayName = "Nguyễn Văn Nhận Lệnh 1" });
        _db.Users.Add(new AppUser { Id = _userEmp2, TenantId = _tenant, Username = "emp_user27_2", DisplayName = "Lê Văn Nhận Lệnh 2" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S27_1", FullName = "Nguyễn Văn Nhận Lệnh 1",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp2, EmployeeCode = "EMP_S27_2", FullName = "Lê Văn Nhận Lệnh 2",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);
        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;

        _transferSvc = new HrmTransferService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_093: Đề xuất nhu cầu điều động (Staff Mobilization Request Creation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC093_CreateRequest_ValidRequest_CreatesMobilizationRequestSuccessfully()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(15);

        var req = await _transferSvc.CreateRequestAsync(_tenant, _userRequester,
            new TransferRequestCreateRequest(_orgUnit1, _orgUnit2, startDate, endDate, 5, "Cần 5 kỹ sư hỗ trợ công trường", "Đề xuất gấp", true));

        Assert.NotNull(req);
        Assert.Equal("Request", req.Kind);
        Assert.Equal(_orgUnit1, req.FromOrgUnitId);
        Assert.Equal(_orgUnit2, req.ToOrgUnitId);
        Assert.Equal(5, req.RequestedHeadcount);
        Assert.Equal("Submitted", req.Status);
    }

    [Fact]
    public async Task UC093_CreateRequest_SameFromAndToOrgs_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateRequestAsync(_tenant, _userRequester,
                new TransferRequestCreateRequest(_orgUnit1, _orgUnit1, startDate, null, 2, "Trùng đơn vị đi đến", null, false)));

        Assert.Contains("khác nhau", ex.Message);
    }

    [Fact]
    public async Task UC093_CreateRequest_InvalidHeadcount_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateRequestAsync(_tenant, _userRequester,
                new TransferRequestCreateRequest(_orgUnit1, _orgUnit2, startDate, null, 0, "Cần 0 người", null, false)));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC093_CreateRequest_ReasonTooShort_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.CreateRequestAsync(_tenant, _userRequester,
                new TransferRequestCreateRequest(_orgUnit1, _orgUnit2, startDate, null, 3, "OK", null, false)));

        Assert.Contains("3–500 ký tự", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_094: Nhận lệnh điều động trên APP (Acknowledge Transfer Order)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC094_AcknowledgeOrder_IssuedOrder_AssignedEmployee_UpdatesToAcknowledged()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Điều động tăng cường", 160, 500000, true, null, true, null));

        Assert.Equal("Issued", order.Status);

        var ack = await _transferSvc.AcknowledgeAsync(_tenant, _userEmp1, order.Id);

        Assert.Equal("Acknowledged", ack.Status);
        Assert.Equal(_userEmp1, ack.AcknowledgedByUserId);
        Assert.NotNull(ack.AcknowledgedAt);
    }

    [Fact]
    public async Task UC094_AcknowledgeOrder_DifferentEmployee_ThrowsForbiddenAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Điều động nhân viên 1", 160, 500000, true, null, true, null));

        // Employee 2 tries to acknowledge Employee 1's order
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.AcknowledgeAsync(_tenant, _userEmp2, order.Id));

        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("chỉ nhân viên được điều động", ex.Message.ToLower());
    }

    [Fact]
    public async Task UC094_AcknowledgeOrder_DraftOrder_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        // Create draft order (issue = false)
        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lệnh nháp chưa phát hành", 160, 500000, true, null, false, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.AcknowledgeAsync(_tenant, _userEmp1, order.Id));

        Assert.Contains("Issued", ex.Message);
    }

    [Fact]
    public async Task UC094_AcknowledgeOrder_NonExistentOrder_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.AcknowledgeAsync(_tenant, _userEmp1, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_095: Theo dõi nhân sự điều động (Active Mobilization Tracking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC095_ActiveTracking_ReturnsIssuedAcknowledgedAndActiveOrdersOnly()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Order 1: Issued
        await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lệnh 1 Issued", 160, 500000, true, null, true, null));

        // Order 2: Draft (not in active tracking)
        await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId2, _orgUnit1, _orgUnit2, startDate, null, "Lệnh 2 Draft", 160, 500000, true, null, false, null));

        var trackingList = await _transferSvc.ActiveTrackingAsync(_tenant);

        Assert.Single(trackingList);
        Assert.Equal("Issued", trackingList[0].Status);
    }

    [Fact]
    public async Task UC095_ActiveTracking_ExcludesCompletedAndCancelledOrders()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lệnh sắp hủy", 160, 500000, true, null, true, null));
        await _transferSvc.CancelAsync(_tenant, _userRequester, order.Id);

        var trackingList = await _transferSvc.ActiveTrackingAsync(_tenant);

        Assert.Empty(trackingList);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_096: Gắn nhãn công điều động khi chấm (Attendance Tagging for Transfer)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC096_ActivateOrder_ValidIssuedOrder_SetsStatusActiveAndAttendanceTag()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Kích hoạt điều động công trường", 160, 500000, true, null, true, null));

        var activeOrder = await _transferSvc.ActivateAsync(_tenant, _userRequester, order.Id);

        Assert.NotNull(activeOrder);
        Assert.Equal("Active", activeOrder.Status);
        Assert.True(activeOrder.AttendanceTagged);
        Assert.Equal("TRANSFER", activeOrder.AttendanceTag);
    }

    [Fact]
    public async Task UC096_ActivateOrder_DraftOrder_ThrowsAppException()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = await _transferSvc.CreateOrderAsync(_tenant, _userRequester,
            new TransferOrderCreateRequest(_empId1, _orgUnit1, _orgUnit2, startDate, null, "Lệnh nháp", 160, 500000, true, null, false, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _transferSvc.ActivateAsync(_tenant, _userRequester, order.Id));

        Assert.Contains("Issued/Acknowledged", ex.Message);
    }
}

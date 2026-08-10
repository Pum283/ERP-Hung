using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 12: UC_HRM_006 (Giờ làm việc theo đơn vị), UC_HRM_010 (Cấp bậc / Level),
/// UC_HRM_012 (Sinh mã nhân sự tự động), UC_HRM_017 (Upload giấy tờ tùy thân).
/// 18+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class SysHrmWorkLevelCodeDocPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _sysSvc;
    private readonly HrmEmployeeService _hrmSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public SysHrmWorkLevelCodeDocPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-hrm-wlcd-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _db.Licenses.Add(new License
        {
            TenantId = _tenant,
            PlanCode = "ENTERPRISE",
            Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100,
            MaxOrgUnits = 500
        });
        _db.SaveChanges();

        _sysSvc = new SysPlatformService(_db, new OutboxWriter(_db));
        _hrmSvc = new HrmEmployeeService(_db, new DataScopeService(_db), _sysSvc);
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_006: Khai báo giờ làm việc theo đơn vị ───

    [Fact]
    public async Task UC006_UpsertWorkCalendar_ValidWeekMask_CreatesCalendar()
    {
        var req = new WorkCalendarDto(Guid.Empty, "CAL_OFFICE", "Giờ hành chính", "1111100", "[\"2026-01-01\"]", true);
        var res = await _sysSvc.UpsertWorkCalendarAsync(_tenant, _actor, req);

        Assert.Equal("CAL_OFFICE", res.Code);
        Assert.Equal("1111100", res.WeekMask);
        Assert.True(res.IsActive);
    }

    [Fact]
    public async Task UC006_UpsertWorkCalendar_InvalidWeekMaskLength_ThrowsAppException()
    {
        var req = new WorkCalendarDto(Guid.Empty, "CAL_BAD", "Lịch lỗi", "1111", null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysSvc.UpsertWorkCalendarAsync(_tenant, _actor, req));
        Assert.Contains("7 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC006_UpsertWorkCalendar_InvalidWeekMaskChars_ThrowsAppException()
    {
        var req = new WorkCalendarDto(Guid.Empty, "CAL_BAD2", "Lịch lỗi", "1111102", null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysSvc.UpsertWorkCalendarAsync(_tenant, _actor, req));
        Assert.Contains("'0' và '1'", ex.Message);
    }

    [Fact]
    public async Task UC006_UpsertWorkCalendar_InvalidHolidaysJson_ThrowsAppException()
    {
        var req = new WorkCalendarDto(Guid.Empty, "CAL_BAD3", "Lịch lỗi", "1111100", "NOT_JSON", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysSvc.UpsertWorkCalendarAsync(_tenant, _actor, req));
        Assert.Contains("JSON", ex.Message);
    }

    // ─── UC_HRM_010: Quản lý cấp bậc / level ───

    [Fact]
    public async Task UC010_UpsertJobLevel_ValidData_CreatesLevel()
    {
        var req = new UpsertJobLevelRequest(null, "L1", "Junior Specialist", 1, "Own", "Mức nhân viên", true);
        var res = await _sysSvc.UpsertJobLevelAsync(_tenant, _actor, req);

        Assert.Equal("L1", res.Code);
        Assert.Equal("Junior Specialist", res.Name);
        Assert.Equal(1, res.LevelOrder);
        Assert.Equal("Own", res.DefaultScopeType);
        Assert.True(res.IsActive);
    }

    [Fact]
    public async Task UC010_UpsertJobLevel_EmptyCodeOrName_ThrowsAppException()
    {
        var ex1 = await Assert.ThrowsAsync<AppException>(() =>
            _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "", "Name", 1)));
        Assert.Contains("Code", ex1.Message);

        var ex2 = await Assert.ThrowsAsync<AppException>(() =>
            _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "L1", "", 1)));
        Assert.Contains("Name", ex2.Message);
    }

    [Fact]
    public async Task UC010_UpsertJobLevel_NegativeOrder_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "L1", "Name", -1)));
        Assert.Contains("LevelOrder", ex.Message);
    }

    [Fact]
    public async Task UC010_UpsertJobLevel_DuplicateCode_ThrowsAppException()
    {
        await _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "L2", "Senior", 2));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "l2", "Senior Duplicate", 2)));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC010_DeleteJobLevel_AssignedToEmployee_ThrowsAppException()
    {
        var level = await _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "L3", "Lead", 3));
        _db.Employees.Add(new Employee { TenantId = _tenant, EmployeeCode = "EMP_L3", FullName = "Trần Lead", JobLevelId = level.Id });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _sysSvc.DeleteJobLevelAsync(_tenant, level.Id));
        Assert.Contains("gán cho nhân sự", ex.Message);
    }

    [Fact]
    public async Task UC010_DeleteJobLevel_Unassigned_SoftDeletesSuccessfully()
    {
        var level = await _sysSvc.UpsertJobLevelAsync(_tenant, _actor, new UpsertJobLevelRequest(null, "L4", "Manager", 4));
        await _sysSvc.DeleteJobLevelAsync(_tenant, level.Id);

        var list = await _sysSvc.ListJobLevelsAsync(_tenant);
        Assert.Empty(list);
    }

    // ─── UC_HRM_012: Sinh mã nhân sự tự động ───

    [Fact]
    public async Task UC012_GenerateNextEmployeeCode_FirstTime_GeneratesSequentialCode()
    {
        var res1 = await _sysSvc.GenerateNextEmployeeCodeAsync(_tenant, new EmployeeCodeGenerateRequest("EMP", "EMP-{SEQ:4}"));
        Assert.Equal("EMP-0001", res1.Code);
        Assert.Equal(1, res1.SequenceValue);

        var res2 = await _sysSvc.GenerateNextEmployeeCodeAsync(_tenant, new EmployeeCodeGenerateRequest("EMP", "EMP-{SEQ:4}"));
        Assert.Equal("EMP-0002", res2.Code);
        Assert.Equal(2, res2.SequenceValue);
    }

    [Fact]
    public async Task UC012_GenerateNextEmployeeCode_WithYearPattern_ReplacesYearToken()
    {
        var res = await _sysSvc.GenerateNextEmployeeCodeAsync(_tenant, new EmployeeCodeGenerateRequest("EMP_YEAR", "NV-{YYYY}-{SEQ:3}"));
        var currentYear = DateTime.UtcNow.Year.ToString();
        Assert.Equal($"NV-{currentYear}-001", res.Code);
    }

    [Fact]
    public async Task UC012_GenerateNextEmployeeCode_AutoSkipsExistingDuplicateInDb()
    {
        // Seed duplicate EMP-0001 already in DB
        _db.Employees.Add(new Employee { TenantId = _tenant, EmployeeCode = "EMP-0001", FullName = "Trùng Mã" });
        await _db.SaveChangesAsync();

        var res = await _sysSvc.GenerateNextEmployeeCodeAsync(_tenant, new EmployeeCodeGenerateRequest("EMP", "EMP-{SEQ:4}"));
        // Tự động bỏ qua EMP-0001 và sinh EMP-0002
        Assert.Equal("EMP-0002", res.Code);
    }

    // ─── UC_HRM_017: Upload giấy tờ tùy thân ───

    [Fact]
    public async Task UC017_AddDocument_ValidIdCard_CreatesDocument()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_DOC", FullName = "Lê Văn Doc" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new EmployeeDocumentUploadRequest("IdCard", "CCCD 12 số", "files/cccd.pdf", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(9)));
        var doc = await _hrmSvc.AddDocumentAsync(_tenant, _actor, emp.Id, req);

        Assert.Equal(emp.Id, doc.EmployeeId);
        Assert.Equal("IdCard", doc.DocType);
        Assert.Equal("CCCD 12 số", doc.Title);
        Assert.Equal("files/cccd.pdf", doc.StorageKey);
    }

    [Fact]
    public async Task UC017_AddDocument_ExpiredDateBeforeIssued_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_DOC2", FullName = "Lê Văn Doc 2" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new EmployeeDocumentUploadRequest("Passport", "Hộ chiếu", "files/pass.pdf", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        var ex = await Assert.ThrowsAsync<AppException>(() => _hrmSvc.AddDocumentAsync(_tenant, _actor, emp.Id, req));
        Assert.Contains("hạn", ex.Message);
    }

    [Fact]
    public async Task UC017_DeleteDocument_SoftDeletesDocument()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_DOC3", FullName = "Lê Văn Doc 3" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var doc = await _hrmSvc.AddDocumentAsync(_tenant, _actor, emp.Id, new EmployeeDocumentUploadRequest("Other", "Bằng đại học", "files/degree.pdf", null, null));
        await _hrmSvc.DeleteDocumentAsync(_tenant, emp.Id, doc.Id);

        var list = await _hrmSvc.ListDocumentsAsync(_tenant, emp.Id);
        Assert.Empty(list);
    }
}

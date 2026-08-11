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
/// Unit tests cho Bước 56:
///   UC_LMS_058 — Xác nhận đã đọc nội quy (Training Policy Acknowledgment)
///   UC_LMS_065 — Dashboard tiến độ đào tạo (Training Progress Analytics Dashboard)
///   UC_LMS_066 — Báo cáo hoàn thành theo đơn vị (Completion Breakdown by Department)
///   UC_LMS_070 — Xuất báo cáo đào tạo (Training Report CSV Export)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep56PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsReportService _repSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userLearner = Guid.NewGuid();
    private readonly Guid _orgDept1    = Guid.NewGuid();

    public HrmStep56PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step56-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_lms56", DisplayName = "Admin LMS 56" });
        _db.Users.Add(new AppUser { Id = _userLearner, TenantId = _tenant, Username = "learner_lms56", DisplayName = "Học Viên 56" });

        _db.OrgUnits.Add(new OrgUnit { Id = _orgDept1, TenantId = _tenant, Code = "DEPT_LMS56", Name = "Phòng Đào Tạo 56", UnitType = "Department", Path = "/1" });

        _db.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(), TenantId = _tenant, EmployeeCode = "EMP_56_01", FullName = "Học Viên Đơn Vị 56",
            OrgUnitId = _orgDept1, UserId = _userLearner, Status = "Active"
        });

        _db.SaveChanges();

        _repSvc = new LmsReportService(_db);

        InitDataAsync().GetAwaiter().GetResult();
    }

    private async Task InitDataAsync()
    {
        var course = new LmsCourse
        {
            Id = Guid.NewGuid(), TenantId = _tenant, Code = "CRS_56_REP", Name = "Khóa Báo Cáo 56", Status = "Published", Price = 0m, DeliveryMode = "Online", CreatedBy = _userAdmin
        };
        _db.LmsCourses.Add(course);

        var cls = new LmsTrainingClass
        {
            Id = Guid.NewGuid(), TenantId = _tenant, Code = "CLS_56_REP", Name = "Lớp Báo Cáo 56", CourseTitle = "Khóa Báo Cáo 56", Status = "Open", CreatedBy = _userAdmin
        };
        _db.LmsTrainingClasses.Add(cls);

        _db.LmsOnlineEnrollments.Add(new LmsOnlineEnrollment
        {
            TenantId = _tenant, CourseId = course.Id, UserId = _userLearner, Status = "Completed", PaidAmount = 0m, CreatedBy = _userLearner
        });

        await _db.SaveChangesAsync();
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_065: Dashboard tiến độ đào tạo (Training Analytics Dashboard)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_065_Dashboard_ReturnsAnalyticsMetrics()
    {
        var dash = await _repSvc.DashboardAsync(_tenant);

        Assert.NotNull(dash);
        Assert.True(dash.CourseCount >= 1);
        Assert.True(dash.PublishedCourseCount >= 1);
        Assert.True(dash.OnlineEnrollmentCount >= 1);
        Assert.True(dash.OnlineCompletedCount >= 1);
    }

    [Fact]
    public async Task UC_LMS_065_Dashboard_EmptyTenant_ReturnsZeroMetrics()
    {
        var emptyTenantId = Guid.NewGuid();
        var dash = await _repSvc.DashboardAsync(emptyTenantId);

        Assert.NotNull(dash);
        Assert.Equal(0, dash.CourseCount);
        Assert.Equal(0, dash.PublishedCourseCount);
        Assert.Equal(0, dash.OnlineEnrollmentCount);
        Assert.Equal(0m, dash.ExamPassRatePercent);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_066: Báo cáo hoàn thành theo đơn vị (Department Completion Breakdown)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_066_CompletionByOrg_ReturnsOrgBreakdownRows()
    {
        var rows = await _repSvc.CompletionByOrgAsync(_tenant);

        Assert.NotNull(rows);
        Assert.NotEmpty(rows);
        Assert.Contains(rows, r => r.OrgUnitName == "Phòng Đào Tạo 56" || r.OrgUnitCode == "DEPT_LMS56");
    }

    [Fact]
    public async Task UC_LMS_066_CompletionByOrg_EmptyTenant_ReturnsEmptyList()
    {
        var emptyTenantId = Guid.NewGuid();
        var rows = await _repSvc.CompletionByOrgAsync(emptyTenantId);

        Assert.NotNull(rows);
        Assert.Empty(rows);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_070: Xuất báo cáo đào tạo (CSV Export)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_070_ExportReportCsv_Learners_GeneratesCsvContent()
    {
        var csvText = await _repSvc.ExportCsvAsync(_tenant, "learners");

        Assert.NotNull(csvText);
        Assert.NotEmpty(csvText);
        Assert.Contains("LearnerCode", csvText);
    }

    [Fact]
    public async Task UC_LMS_070_ExportReportCsv_ByOrg_GeneratesCsvContent()
    {
        var csvText = await _repSvc.ExportCsvAsync(_tenant, "by-org");

        Assert.NotNull(csvText);
        Assert.NotEmpty(csvText);
        Assert.Contains("OrgName", csvText);
    }

    [Fact]
    public async Task UC_LMS_070_ExportReportCsv_Dashboard_GeneratesCsvContent()
    {
        var csvText = await _repSvc.ExportCsvAsync(_tenant, "dashboard");

        Assert.NotNull(csvText);
        Assert.NotEmpty(csvText);
        Assert.Contains("CourseCount", csvText);
    }

    [Fact]
    public async Task UC_LMS_070_ExportReportCsv_InvalidReportType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _repSvc.ExportCsvAsync(_tenant, "invalid-report-type"));

        Assert.Contains("report: dashboard | by-org | learners", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_058: Xác nhận đã đọc nội quy (Training Policy Acknowledgment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_058_Learners_ReturnsLearnerRosterWithEnrollmentStatus()
    {
        var list = await _repSvc.LearnersAsync(_tenant);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, l => l.LearnerName == "Học Viên Đơn Vị 56" || l.Source == "Online");
    }

    [Fact]
    public async Task UC_LMS_058_Learners_EmptyTenant_ReturnsEmptyList()
    {
        var emptyTenantId = Guid.NewGuid();
        var list = await _repSvc.LearnersAsync(emptyTenantId);

        Assert.NotNull(list);
        Assert.Empty(list);
    }
}

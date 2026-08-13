using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsPathTrackingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsPathTrackingService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _course1Id = Guid.NewGuid();
    private readonly Guid _course2Id = Guid.NewGuid();

    public LmsPathTrackingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-path-tracking-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T165", Name = "Tenant 165" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _course1Id,
            TenantId = _tenant,
            Code = "CRS-ONBOARD-01",
            Name = "Khóa Đào tạo Nội quy & Văn hóa Công ty",
            Price = 0m
        });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _course2Id,
            TenantId = _tenant,
            Code = "CRS-DEV-01",
            Name = "Khóa Đào tạo Lập trình C# Domain-Driven Design",
            Price = 0m
        });

        _db.Set<LmsAcknowledgement>().Add(new LmsAcknowledgement
        {
            TenantId = _tenant,
            EmployeeId = _userId,
            DocumentTitle = "Quy định An toàn Thông tin Q3/2026",
            AcknowledgedAt = DateTimeOffset.UtcNow
        });

        _db.SaveChanges();

        _svc = new LmsPathTrackingService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_060: Báo cáo tỷ lệ xác nhận
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC060_GetAcknowledgementReport_ReturnsComplianceRates()
    {
        var list = await _svc.GetAcknowledgementReportAsync(_tenant);
        Assert.NotEmpty(list);
        Assert.Contains(list, r => r.TotalEmployees > 0 && r.ComplianceRatePct >= 0m);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_061: Gán lộ trình theo chức danh
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC061_CreateAndGetLearningPath_Succeeds()
    {
        var req = new LmsLearningPathUpsertRequest(
            Title: "Lộ trình Đào tạo Backend Developer Junior",
            JobTitle: "Backend Developer",
            Description: "Lộ trình chuẩn 30 ngày cho lập trình viên backend mới nhận việc",
            TargetDaysToComplete: 30,
            IsActive: true,
            CourseIds: new List<Guid> { _course1Id, _course2Id }
        );

        var created = await _svc.CreateLearningPathAsync(_tenant, req);

        Assert.NotNull(created);
        Assert.Equal("Lộ trình Đào tạo Backend Developer Junior", created.Title);
        Assert.Equal("Backend Developer", created.JobTitle);
        Assert.Equal(2, created.Items.Count);

        var list = await _svc.GetLearningPathsAsync(_tenant, "Backend Developer");
        Assert.Single(list);
        Assert.Equal(created.Id, list[0].Id);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_062: Tự gán khóa bắt buộc khi nhận việc
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC062_AutoAssignOnHire_AssignsMandatoryCourses()
    {
        // Khởi tạo lộ trình trước
        var pathReq = new LmsLearningPathUpsertRequest(
            Title: "Lộ trình Onboarding Nhân viên Kho",
            JobTitle: "Warehouse Staff",
            TargetDaysToComplete: 14,
            CourseIds: new List<Guid> { _course1Id }
        );
        var path = await _svc.CreateLearningPathAsync(_tenant, pathReq);

        var res = await _svc.AutoAssignOnHireAsync(_tenant, _userId, "Warehouse Staff");

        Assert.NotNull(res);
        Assert.Equal(_userId, res.UserId);
        Assert.Equal("Warehouse Staff", res.JobTitle);
        Assert.Equal(path.Id, res.AssignedPathId);
        Assert.Single(res.AssignedCourseIds);
        Assert.Contains("Đã tự động gán lộ trình", res.Message);

        var enrollmentExists = await _db.LmsOnlineEnrollments.AnyAsync(e => e.TenantId == _tenant && e.UserId == _userId && e.CourseId == _course1Id);
        Assert.True(enrollmentExists);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_063: Theo dõi hoàn thành lộ trình
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC063_GetUserLearningPathProgress_ReturnsProgressList()
    {
        // Gán lộ trình cho nhân viên
        await _svc.AutoAssignOnHireAsync(_tenant, _userId, "Software Engineer");

        var progressList = await _svc.GetUserLearningPathProgressAsync(_tenant, _userId);

        Assert.NotEmpty(progressList);
        Assert.Equal(_userId, progressList[0].UserId);
        Assert.Equal("Software Engineer", progressList[0].JobTitle);
        Assert.Equal("InProgress", progressList[0].Status);
    }
}

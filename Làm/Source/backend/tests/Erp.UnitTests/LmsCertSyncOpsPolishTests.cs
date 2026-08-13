using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsCertSyncOpsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsCertSyncOpsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _instructorId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _lessonId = Guid.NewGuid();
    private readonly Guid _certId = Guid.NewGuid();

    public LmsCertSyncOpsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-cert-sync-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T163", Name = "Tenant 163" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Code = "CRS163",
            Name = "Khóa học Lập trình Domain-Driven Design",
            Price = 2000000m,
            Currency = "VND"
        });
        _db.LmsLessons.Add(new LmsLesson
        {
            Id = _lessonId,
            TenantId = _tenant,
            ChapterId = Guid.NewGuid(),
            Title = "Bài tập thực hành Aggregate Root"
        });
        _db.LmsCertificates.Add(new LmsCertificate
        {
            Id = _certId,
            TenantId = _tenant,
            CourseId = _courseId,
            UserId = _userId,
            Code = "CERT-DDD-2026",
            IssuedAt = DateTimeOffset.UtcNow.AddDays(-5),
            Status = "Active"
        });
        _db.LmsOnlineEnrollments.Add(new LmsOnlineEnrollment
        {
            TenantId = _tenant,
            CourseId = _courseId,
            UserId = _userId,
            Status = "Active"
        });

        _db.SaveChanges();

        _svc = new LmsCertSyncOpsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_048: Đồng bộ chứng chỉ sang HRM
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC048_SyncCertificateToHrm_Succeeds()
    {
        var res = await _svc.SyncCertificateToHrmAsync(_tenant, _certId);

        Assert.NotNull(res);
        Assert.True(res.IsSynced);
        Assert.Equal("CERT-DDD-2026", res.CertificateCode);

        var skill = await _db.HrmEmployeeSkills.FirstOrDefaultAsync(s => s.TenantId == _tenant && s.EmployeeId == _userId);
        Assert.NotNull(skill);
        Assert.Equal("CERT-DDD-2026", skill.CertificateRef);
    }

    [Fact]
    public async Task UC048_SyncCertificateToHrm_RevokedCert_ThrowsAppException()
    {
        var revokedCertId = Guid.NewGuid();
        _db.LmsCertificates.Add(new LmsCertificate
        {
            Id = revokedCertId,
            TenantId = _tenant,
            CourseId = _courseId,
            UserId = _userId,
            Code = "CERT-REVOKED",
            Status = "Revoked"
        });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.SyncCertificateToHrmAsync(_tenant, revokedCertId));
        Assert.True(ex.StatusCode >= 400);
    }

    [Fact]
    public async Task UC048_SyncCertificateToHrm_CertNotFound_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.SyncCertificateToHrmAsync(_tenant, Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_052: Phản hồi bài tập
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC052_CreateAssignmentFeedback_Succeeds()
    {
        var req = new LmsAssignmentFeedbackUpsertRequest(_lessonId, _userId, "https://github.com/org/ddd-assignment", 95m, "Bài tập thiết kế rất tốt!");
        var dto = await _svc.CreateAssignmentFeedbackAsync(_tenant, _instructorId, req);

        Assert.NotNull(dto);
        Assert.Equal(95m, dto.Score);
        Assert.Equal("Graded", dto.Status);
    }

    [Fact]
    public async Task UC052_CreateAssignmentFeedback_InvalidScore_ThrowsAppException()
    {
        var req = new LmsAssignmentFeedbackUpsertRequest(_lessonId, _userId, "https://github.com", 150m);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAssignmentFeedbackAsync(_tenant, _instructorId, req));
        Assert.True(ex.StatusCode >= 400);
    }

    [Fact]
    public async Task UC052_CreateAssignmentFeedback_LessonNotFound_ThrowsAppException()
    {
        var req = new LmsAssignmentFeedbackUpsertRequest(Guid.NewGuid(), _userId, "https://github.com");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAssignmentFeedbackAsync(_tenant, _instructorId, req));
        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_053: Thống kê doanh thu theo khóa
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC053_GetCourseRevenueStats_Succeeds()
    {
        var list = await _svc.GetCourseRevenueStatsAsync(_tenant);

        Assert.NotEmpty(list);
        var item = list.First(c => c.CourseId == _courseId);
        Assert.Equal(1, item.PaidEnrollments);
        Assert.Equal(2000000m, item.GrossRevenue);
    }

    [Fact]
    public async Task UC053_GetCourseRevenueStats_CalculatesRevenueCorrectly()
    {
        _db.LmsOnlineEnrollments.Add(new LmsOnlineEnrollment { TenantId = _tenant, CourseId = _courseId, UserId = Guid.NewGuid(), Status = "Completed" });
        await _db.SaveChangesAsync();

        var list = await _svc.GetCourseRevenueStatsAsync(_tenant);
        var item = list.First(c => c.CourseId == _courseId);

        Assert.Equal(2, item.PaidEnrollments);
        Assert.Equal(4000000m, item.GrossRevenue);
    }

    [Fact]
    public async Task UC053_GetCourseRevenueStats_EmptyCourses_ReturnsEmptyList()
    {
        var emptyTenant = Guid.NewGuid();
        var list = await _svc.GetCourseRevenueStatsAsync(emptyTenant);
        Assert.Empty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_054: Chống chia sẻ tài khoản
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC054_ValidateAccountSession_NormalIp_AllowsSession()
    {
        var req = new LmsSessionValidationRequest("DEV-CHROME-001", "14.225.1.1");
        var dto = await _svc.ValidateAccountSessionAsync(_tenant, _userId, req);

        Assert.NotNull(dto);
        Assert.False(dto.IsSharingDetected);
        Assert.Equal("Allowed", dto.ActionTaken);
    }

    [Fact]
    public async Task UC054_ValidateAccountSession_SuspiciousIp_DetectsAccountSharing()
    {
        var req = new LmsSessionValidationRequest("DEV-CHROME-002", "192.168.99.50");
        var dto = await _svc.ValidateAccountSessionAsync(_tenant, _userId, req);

        Assert.NotNull(dto);
        Assert.True(dto.IsSharingDetected);
        Assert.Equal("ForceLogoutPreviousSession", dto.ActionTaken);
    }

    [Fact]
    public async Task UC054_ValidateAccountSession_MissingDeviceId_ThrowsAppException()
    {
        var req = new LmsSessionValidationRequest("", "14.225.1.1");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.ValidateAccountSessionAsync(_tenant, _userId, req));
        Assert.True(ex.StatusCode >= 400);
    }
}

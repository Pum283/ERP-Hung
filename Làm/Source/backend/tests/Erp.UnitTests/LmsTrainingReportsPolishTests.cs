using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsTrainingReportsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsTrainingReportsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _examId = Guid.NewGuid();

    public LmsTrainingReportsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-training-reports-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T166", Name = "Tenant 166" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Code = "CRS-ANALYTICS",
            Name = "Khóa Đào tạo Phân tích Dữ liệu & Báo cáo LMS",
            Price = 1500000m
        });
        _db.LmsExams.Add(new LmsExam
        {
            Id = _examId,
            TenantId = _tenant,
            CourseId = _courseId,
            Name = "Bài thi Tổng kết Đánh giá Năng lực LMS",
            PassScore = 70m
        });
        _db.LmsExamAttempts.Add(new LmsExamAttempt
        {
            TenantId = _tenant,
            ExamId = _examId,
            UserId = _userId,
            Score = 85m,
            Passed = true,
            SubmittedAt = DateTimeOffset.UtcNow
        });

        _db.SaveChanges();

        _svc = new LmsTrainingReportsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_064: Cảnh báo quá hạn đào tạo
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC064_TriggerOverdueCheck_Succeeds()
    {
        var alerts = await _svc.TriggerOverdueCheckAsync(_tenant);
        Assert.NotEmpty(alerts);
        Assert.Contains(alerts, a => a.OverdueDays >= 1);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_067: Báo cáo điểm thi / tỷ lệ đạt
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC067_GetExamAnalyticsReport_ReturnsValidAnalytics()
    {
        var report = await _svc.GetExamAnalyticsReportAsync(_tenant, _examId);
        Assert.NotEmpty(report);
        Assert.Equal(_examId, report[0].ExamId);
        Assert.True(report[0].PassRatePct >= 0m);
        Assert.Equal(85m, report[0].AverageScore);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_068: Báo cáo học viên bỏ dở
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC068_GetDropoutAnalyticsReport_ReturnsDropoutStats()
    {
        var report = await _svc.GetDropoutAnalyticsReportAsync(_tenant, _courseId);
        Assert.NotEmpty(report);
        Assert.Equal(_courseId, report[0].CourseId);
        Assert.True(report[0].DropoutRatePct >= 0m);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_069: Báo cáo hiệu quả khóa
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC069_GetCourseEngagementReport_ReturnsEngagementMetrics()
    {
        var report = await _svc.GetCourseEngagementReportAsync(_tenant, _courseId);
        Assert.NotEmpty(report);
        Assert.Equal(_courseId, report[0].CourseId);
        Assert.True(report[0].CompletionRatePct > 0m);
        Assert.True(report[0].AverageRating >= 4.0m);
    }
}

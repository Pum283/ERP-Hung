using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LmsContentCompliancePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LmsContentComplianceService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _lessonId = Guid.NewGuid();

    public LmsContentCompliancePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("lms-content-compliance-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T164", Name = "Tenant 164" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Code = "CRS164-SAFETY",
            Name = "Khóa Đào tạo An toàn Lao động & Tuân thủ Ca",
            Price = 0m,
            Currency = "VND"
        });
        _db.LmsLessons.Add(new LmsLesson
        {
            Id = _lessonId,
            TenantId = _tenant,
            ChapterId = Guid.NewGuid(),
            Title = "Video Hướng dẫn Quy trình An toàn Nhà máy"
        });

        _db.SaveChanges();

        _svc = new LmsContentComplianceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_055: Chặn tải video
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC055_GetVideoProtectionConfig_Default_ReturnsValidConfig()
    {
        var config = await _svc.GetVideoProtectionConfigAsync(_tenant, _lessonId);
        Assert.NotNull(config);
        Assert.Equal(_lessonId, config.LessonId);
        Assert.True(config.IsDownloadBlocked);
        Assert.True(config.WatermarkEnabled);
    }

    [Fact]
    public async Task UC055_UpdateVideoProtectionConfig_Succeeds()
    {
        var req = new LmsVideoProtectionUpdateRequest(
            _lessonId,
            IsDownloadBlocked: true,
            WatermarkEnabled: true,
            WatermarkText: "EMP-164 WATERMARK",
            SignedUrlExpiryMinutes: 60,
            AllowedRoles: "Admin"
        );

        var updated = await _svc.UpdateVideoProtectionConfigAsync(_tenant, req);

        Assert.NotNull(updated);
        Assert.True(updated.IsDownloadBlocked);
        Assert.Equal("EMP-164 WATERMARK", updated.WatermarkText);
        Assert.Equal(60, updated.SignedUrlExpiryMinutes);
    }

    [Fact]
    public async Task UC055_GenerateProtectedPlaybackUrl_ReturnsSignedTokenAndStreamUrl()
    {
        var res = await _svc.GenerateProtectedPlaybackUrlAsync(_tenant, _userId, _lessonId);
        Assert.NotNull(res);
        Assert.Contains("nodownload=1", res.StreamUrl);
        Assert.NotEmpty(res.SignedToken);
        Assert.True(res.ExpiresAt > DateTimeOffset.UtcNow);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_056 & UC_LMS_057: Khảo sát hiểu bài & Khảo sát tuân thủ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC056_CreateAndGetSurveys_Succeeds()
    {
        var surveyReq = new LmsSurveyUpsertRequest("Khảo sát Hiểu bài An toàn Lao động", "Comprehension", _courseId, true, true);
        var created = await _svc.CreateSurveyAsync(_tenant, surveyReq);

        Assert.NotNull(created);
        Assert.Equal("Khảo sát Hiểu bài An toàn Lao động", created.Title);

        var list = await _svc.GetSurveysAsync(_tenant, "Comprehension");
        Assert.Single(list);
        Assert.Equal(created.Id, list[0].Id);
    }

    [Fact]
    public async Task UC057_SubmitSurveyResponse_Passed_ReturnsSuccessResult()
    {
        var surveyReq = new LmsSurveyUpsertRequest("Khảo sát Tuân thủ Phòng cháy Chữa cháy", "Compliance", _courseId, true, false);
        var survey = await _svc.CreateSurveyAsync(_tenant, surveyReq);

        var subReq = new LmsSurveySubmissionRequest(
            survey.Id,
            "{\"calculatedScore\": 85.5, \"q1\": 1, \"q2\": 1}",
            TargetPassingScore: 70m
        );

        var res = await _svc.SubmitSurveyResponseAsync(_tenant, _userId, subReq);

        Assert.NotNull(res);
        Assert.True(res.IsPassed);
        Assert.Equal(85.5m, res.Score);
        Assert.Contains("đạt yêu cầu", res.StatusMessage);
    }

    [Fact]
    public async Task UC057_SubmitSurveyResponse_Failed_ReturnsFailedStatus()
    {
        var surveyReq = new LmsSurveyUpsertRequest("Khảo sát Tuân thủ An toàn Điện", "Compliance", _courseId, true, false);
        var survey = await _svc.CreateSurveyAsync(_tenant, surveyReq);

        var subReq = new LmsSurveySubmissionRequest(
            survey.Id,
            "{\"calculatedScore\": 50.0, \"q1\": 0, \"q2\": 1}",
            TargetPassingScore: 70m
        );

        var res = await _svc.SubmitSurveyResponseAsync(_tenant, _userId, subReq);

        Assert.NotNull(res);
        Assert.False(res.IsPassed);
        Assert.Equal(50.0m, res.Score);
        Assert.Contains("chưa đạt tiêu chuẩn", res.StatusMessage);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_059: Bắt buộc hoàn thành trước ca
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC059_EvaluateShiftTrainingGate_Incomplete_BlocksWorkEntry()
    {
        var checkReq = new LmsShiftGateCheckRequest(
            _userId,
            "SHIFT-01-MORNING",
            DateTime.UtcNow.Date,
            DateTimeOffset.UtcNow.AddHours(1),
            _courseId
        );

        var res = await _svc.EvaluateShiftTrainingGateAsync(_tenant, checkReq);

        Assert.NotNull(res);
        Assert.False(res.IsMandatoryCompleted);
        Assert.True(res.IsWorkEntryBlocked);
        Assert.Equal("Blocked", res.GateStatus);
        Assert.Contains("CHẶN VÀO CA", res.Message);
    }

    [Fact]
    public async Task UC059_EvaluateShiftTrainingGate_Completed_AllowsWorkEntry()
    {
        // Giả lập học viên đã hoàn thành khóa học
        _db.LmsOnlineEnrollments.Add(new LmsOnlineEnrollment
        {
            TenantId = _tenant,
            CourseId = _courseId,
            UserId = _userId,
            Status = "Completed"
        });
        _db.SaveChanges();

        var checkReq = new LmsShiftGateCheckRequest(
            _userId,
            "SHIFT-01-MORNING",
            DateTime.UtcNow.Date,
            DateTimeOffset.UtcNow.AddHours(1),
            _courseId
        );

        var res = await _svc.EvaluateShiftTrainingGateAsync(_tenant, checkReq);

        Assert.NotNull(res);
        Assert.True(res.IsMandatoryCompleted);
        Assert.False(res.IsWorkEntryBlocked);
        Assert.Equal("Passed", res.GateStatus);
        Assert.Contains("Được phép đăng nhập làm việc", res.Message);
    }
}

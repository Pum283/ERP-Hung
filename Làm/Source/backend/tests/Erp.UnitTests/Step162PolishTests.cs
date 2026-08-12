using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class Step162PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Step162Service _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _certId = Guid.NewGuid();
    private const string CertCode = "CERT-2026-X999";

    public Step162PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("step162-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T162", Name = "Tenant 162" });
        _db.LmsCourses.Add(new LmsCourse
        {
            Id = _courseId,
            TenantId = _tenant,
            Code = "CRS162",
            Name = "Khóa học Lập trình Microservices & Event-Driven"
        });
        _db.LmsCertificates.Add(new LmsCertificate
        {
            Id = _certId,
            TenantId = _tenant,
            CourseId = _courseId,
            UserId = _userId,
            Code = CertCode,
            IssuedAt = DateTimeOffset.UtcNow.AddDays(-30),
            Status = "Active",
            ScoreAtIssue = 95m
        });

        _db.SaveChanges();

        _svc = new Step162Service(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_038: Nhắc học tiếp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC038_CreateStudyReminder_Succeeds()
    {
        var req = new LmsStudyReminderUpsertRequest(_courseId, "Daily", "Nhắc nhở học tập hàng ngày.");
        var dto = await _svc.CreateStudyReminderAsync(_tenant, _userId, req);

        Assert.NotNull(dto);
        Assert.Equal("Daily", dto.Frequency);
        Assert.Equal("Khóa học Lập trình Microservices & Event-Driven", dto.CourseName);
    }

    [Fact]
    public async Task UC038_CreateStudyReminder_CourseNotFound_ThrowsAppException()
    {
        var req = new LmsStudyReminderUpsertRequest(Guid.NewGuid());
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateStudyReminderAsync(_tenant, _userId, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC038_GetStudyReminders_ReturnsList()
    {
        await _svc.CreateStudyReminderAsync(_tenant, _userId, new LmsStudyReminderUpsertRequest(_courseId));
        var list = await _svc.GetStudyRemindersAsync(_tenant, _userId);

        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_039: Diễn đàn / bình luận
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC039_CreateForumTopic_Succeeds()
    {
        var req = new LmsForumTopicUpsertRequest(_courseId, "Hỏi về Saga Pattern", "Làm sao triển khai Choreography Saga?", true);
        var dto = await _svc.CreateForumTopicAsync(_tenant, _userId, req);

        Assert.NotNull(dto);
        Assert.Equal("Hỏi về Saga Pattern", dto.Title);
        Assert.True(dto.IsPinned);
    }

    [Fact]
    public async Task UC039_CreateForumTopic_MissingTitle_ThrowsAppException()
    {
        var req = new LmsForumTopicUpsertRequest(_courseId, "", "Nội dung thảo luận");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateForumTopicAsync(_tenant, _userId, req));
        Assert.True(ex.StatusCode >= 400);
    }

    [Fact]
    public async Task UC039_GetForumTopics_ReturnsList()
    {
        await _svc.CreateForumTopicAsync(_tenant, _userId, new LmsForumTopicUpsertRequest(_courseId, "Topic 1", "Content 1"));
        var list = await _svc.GetForumTopicsAsync(_tenant, _courseId);

        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_046: Mã xác thực chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC046_VerifyCertificate_ValidCode_ReturnsValidResult()
    {
        var res = await _svc.VerifyCertificateAsync(_tenant, CertCode);

        Assert.NotNull(res);
        Assert.True(res.IsValid);
        Assert.Equal("Active", res.Status);
        Assert.Equal(CertCode, res.Code);
    }

    [Fact]
    public async Task UC046_VerifyCertificate_InvalidCode_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.VerifyCertificateAsync(_tenant, "INVALID-CODE"));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC046_VerifyCertificate_RevokedCertificate_ReturnsInvalidStatus()
    {
        await _svc.RevokeCertificateAsync(_tenant, _userId, new LmsRevokeCertificateRequest(_certId, "Vi phạm bản quyền"));
        var res = await _svc.VerifyCertificateAsync(_tenant, CertCode);

        Assert.False(res.IsValid);
        Assert.Equal("Revoked", res.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_047: Thu hồi chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC047_RevokeCertificate_Succeeds()
    {
        var req = new LmsRevokeCertificateRequest(_certId, "Gian lận trong bài kiểm tra cuối khóa");
        var dto = await _svc.RevokeCertificateAsync(_tenant, _userId, req);

        Assert.NotNull(dto);
        Assert.Equal("Gian lận trong bài kiểm tra cuối khóa", dto.RevocationReason);

        var cert = await _db.LmsCertificates.FindAsync(_certId);
        Assert.Equal("Revoked", cert!.Status);
    }

    [Fact]
    public async Task UC047_RevokeCertificate_CertificateNotFound_ThrowsAppException()
    {
        var req = new LmsRevokeCertificateRequest(Guid.NewGuid(), "Lý do sai");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.RevokeCertificateAsync(_tenant, _userId, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC047_RevokeCertificate_MissingReason_ThrowsAppException()
    {
        var req = new LmsRevokeCertificateRequest(_certId, "   ");
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.RevokeCertificateAsync(_tenant, _userId, req));
        Assert.True(ex.StatusCode >= 400);
    }
}

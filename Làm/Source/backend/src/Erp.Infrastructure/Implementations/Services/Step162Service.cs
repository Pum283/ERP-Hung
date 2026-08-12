using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class Step162Service : IStep162Service
{
    private readonly AppDbContext _db;

    public Step162Service(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_038: Nhắc học tiếp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsStudyReminderDto>> GetStudyRemindersAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var items = await _db.LmsStudyReminders.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var courseIds = items.Select(r => r.CourseId).Distinct().ToList();
        var courses = await _db.LmsCourses.AsNoTracking()
            .Where(c => c.TenantId == tenantId && courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return items.Select(r => new LmsStudyReminderDto(
            r.Id,
            r.UserId,
            r.CourseId,
            courses.TryGetValue(r.CourseId, out var name) ? name : null,
            r.Frequency,
            r.LastRemindedAt,
            r.Message,
            r.IsActive,
            r.CreatedAt
        )).ToList();
    }

    public async Task<LmsStudyReminderDto> CreateStudyReminderAsync(Guid tenantId, Guid userId, LmsStudyReminderUpsertRequest req, CancellationToken ct = default)
    {
        if (req.CourseId == Guid.Empty) throw new AppException("Khóa học không được để trống.");

        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        if (course == null) throw new AppException($"Không tìm thấy khóa học {req.CourseId}.", 404);

        var entity = new LmsStudyReminder
        {
            TenantId = tenantId,
            UserId = userId,
            CourseId = req.CourseId,
            Frequency = NormalizeFrequency(req.Frequency),
            Message = string.IsNullOrWhiteSpace(req.Message) ? "Bạn còn bài học chưa hoàn thành, hãy vào học tiếp nhé!" : req.Message.Trim(),
            IsActive = true,
            LastRemindedAt = DateTimeOffset.UtcNow
        };

        _db.LmsStudyReminders.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsStudyReminderDto(entity.Id, entity.UserId, entity.CourseId, course.Name, entity.Frequency, entity.LastRemindedAt, entity.Message, entity.IsActive, entity.CreatedAt);
    }

    private static string NormalizeFrequency(string freq)
    {
        var valid = new[] { "Daily", "Weekly", "Custom" };
        var found = valid.FirstOrDefault(v => string.Equals(v, freq, StringComparison.OrdinalIgnoreCase));
        return found ?? "Daily";
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_039: Diễn đàn / bình luận
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsForumTopicDto>> GetForumTopicsAsync(Guid tenantId, Guid courseId, CancellationToken ct = default)
    {
        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == courseId, ct);
        if (course == null) throw new AppException($"Không tìm thấy khóa học {courseId}", 404);

        var items = await _db.LmsForumTopics.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.CourseId == courseId)
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return items.Select(t => new LmsForumTopicDto(
            t.Id,
            t.CourseId,
            course.Name,
            t.AuthorId,
            "Học viên / Giảng viên",
            t.Title,
            t.Content,
            t.ReplyCount,
            t.IsPinned,
            t.CreatedAt
        )).ToList();
    }

    public async Task<LmsForumTopicDto> CreateForumTopicAsync(Guid tenantId, Guid authorId, LmsForumTopicUpsertRequest req, CancellationToken ct = default)
    {
        if (req.CourseId == Guid.Empty) throw new AppException("Khóa học không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Tiêu đề thảo luận không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Content)) throw new AppException("Nội dung thảo luận không được để trống.");

        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        if (course == null) throw new AppException($"Không tìm thấy khóa học {req.CourseId}.", 404);

        var entity = new LmsForumTopic
        {
            TenantId = tenantId,
            CourseId = req.CourseId,
            AuthorId = authorId,
            Title = req.Title.Trim(),
            Content = req.Content.Trim(),
            ReplyCount = 0,
            IsPinned = req.IsPinned
        };

        _db.LmsForumTopics.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsForumTopicDto(entity.Id, entity.CourseId, course.Name, entity.AuthorId, "Học viên", entity.Title, entity.Content, entity.ReplyCount, entity.IsPinned, entity.CreatedAt);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_046: Mã xác thực chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsCertificateVerificationResultDto> VerifyCertificateAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new AppException("Mã xác thực không được để trống.");

        var cert = await _db.LmsCertificates.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code.ToLower() == code.Trim().ToLower(), ct);

        if (cert == null) throw new AppException($"Mã chứng chỉ '{code}' không tồn tại hoặc không hợp lệ.", 404);

        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == cert.CourseId, ct);
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == cert.UserId, ct);

        bool isValid = string.Equals(cert.Status, "Active", StringComparison.OrdinalIgnoreCase);

        return new LmsCertificateVerificationResultDto(
            cert.Id,
            cert.Code,
            cert.CourseId,
            course?.Name,
            cert.UserId,
            employee?.FullName ?? "Học viên ERP",
            cert.IssuedAt,
            cert.Status,
            isValid,
            cert.ScoreAtIssue
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_047: Thu hồi chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsCertificateRevocationDto> RevokeCertificateAsync(Guid tenantId, Guid revokedByUserId, LmsRevokeCertificateRequest req, CancellationToken ct = default)
    {
        if (req.CertificateId == Guid.Empty) throw new AppException("Mã chứng chỉ không được để trống.");
        if (string.IsNullOrWhiteSpace(req.RevocationReason)) throw new AppException("Lý do thu hồi chứng chỉ không được để trống.");

        var cert = await _db.LmsCertificates.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.CertificateId, ct);
        if (cert == null) throw new AppException($"Không tìm thấy chứng chỉ {req.CertificateId}.", 404);

        cert.Status = "Revoked";

        var revocation = new LmsCertificateRevocation
        {
            TenantId = tenantId,
            CertificateId = cert.Id,
            RevocationReason = req.RevocationReason.Trim(),
            RevokedAt = DateTimeOffset.UtcNow,
            RevokedByUserId = revokedByUserId
        };

        _db.LmsCertificateRevocations.Add(revocation);
        await _db.SaveChangesAsync(ct);

        return new LmsCertificateRevocationDto(
            revocation.Id,
            revocation.CertificateId,
            cert.Code,
            revocation.RevocationReason,
            revocation.RevokedAt,
            revocation.RevokedByUserId,
            revocation.CreatedAt
        );
    }
}

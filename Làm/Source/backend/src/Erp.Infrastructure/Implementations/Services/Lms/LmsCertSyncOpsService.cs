using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LmsCertSyncOpsService : ILmsCertSyncOpsService
{
    private readonly AppDbContext _db;

    public LmsCertSyncOpsService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_048: Đồng bộ chứng chỉ sang HRM
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsHrmCertificateSyncResultDto> SyncCertificateToHrmAsync(Guid tenantId, Guid certificateId, CancellationToken ct = default)
    {
        var cert = await _db.LmsCertificates.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == certificateId, ct);
        if (cert == null) throw new AppException($"Không tìm thấy chứng chỉ {certificateId}", 404);

        if (string.Equals(cert.Status, "Revoked", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Không thể đồng bộ chứng chỉ đã bị thu hồi sang hồ sơ nhân sự HRM.");

        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == cert.CourseId, ct);
        string skillName = course != null ? $"{course.Name} (LMS Certified)" : $"Chứng chỉ {cert.Code}";

        // Đồng bộ sang bảng HrmEmployeeSkill
        var existingSkill = await _db.HrmEmployeeSkills.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.EmployeeId == cert.UserId && s.SkillName == skillName, ct);
        if (existingSkill == null)
        {
            _db.HrmEmployeeSkills.Add(new HrmEmployeeSkill
            {
                TenantId = tenantId,
                EmployeeId = cert.UserId,
                SkillName = skillName,
                ProficiencyLevel = "Expert",
                CertificateRef = cert.Code
            });
        }
        else
        {
            existingSkill.CertificateRef = cert.Code;
            existingSkill.ProficiencyLevel = "Expert";
        }

        await _db.SaveChangesAsync(ct);

        return new LmsHrmCertificateSyncResultDto(
            cert.Id,
            cert.Code,
            cert.UserId,
            skillName,
            true,
            DateTimeOffset.UtcNow,
            $"Đã đồng bộ thành công chứng chỉ {cert.Code} vào hồ sơ kỹ năng HRM của học viên."
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_052: Phản hồi bài tập
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsAssignmentFeedbackDto>> GetAssignmentFeedbacksAsync(Guid tenantId, Guid lessonId, CancellationToken ct = default)
    {
        var lessonExists = await _db.LmsLessons.AnyAsync(l => l.TenantId == tenantId && l.Id == lessonId, ct);
        if (!lessonExists) throw new AppException($"Không tìm thấy bài học {lessonId}", 404);

        var items = await _db.LmsAssignmentFeedbacks.AsNoTracking()
            .Where(f => f.TenantId == tenantId && f.LessonId == lessonId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        return items.Select(f => new LmsAssignmentFeedbackDto(
            f.Id,
            f.LessonId,
            f.StudentUserId,
            f.InstructorUserId,
            f.SubmissionUrl,
            f.Score,
            f.FeedbackComment,
            f.Status,
            f.CreatedAt
        )).ToList();
    }

    public async Task<LmsAssignmentFeedbackDto> CreateAssignmentFeedbackAsync(Guid tenantId, Guid instructorUserId, LmsAssignmentFeedbackUpsertRequest req, CancellationToken ct = default)
    {
        if (req.LessonId == Guid.Empty) throw new AppException("Mã bài học không được để trống.");
        if (req.StudentUserId == Guid.Empty) throw new AppException("Mã học viên không được để trống.");
        if (string.IsNullOrWhiteSpace(req.SubmissionUrl)) throw new AppException("Đường dẫn bài nộp không được để trống.");
        if (req.Score < 0 || req.Score > 100) throw new AppException("Điểm số phải từ 0 đến 100.");

        var lessonExists = await _db.LmsLessons.AnyAsync(l => l.TenantId == tenantId && l.Id == req.LessonId, ct);
        if (!lessonExists) throw new AppException($"Không tìm thấy bài học {req.LessonId}.", 404);

        var entity = new LmsAssignmentFeedback
        {
            TenantId = tenantId,
            LessonId = req.LessonId,
            StudentUserId = req.StudentUserId,
            InstructorUserId = instructorUserId,
            SubmissionUrl = req.SubmissionUrl.Trim(),
            Score = req.Score,
            FeedbackComment = req.FeedbackComment.Trim(),
            Status = string.IsNullOrWhiteSpace(req.Status) ? "Graded" : req.Status.Trim()
        };

        _db.LmsAssignmentFeedbacks.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsAssignmentFeedbackDto(entity.Id, entity.LessonId, entity.StudentUserId, entity.InstructorUserId, entity.SubmissionUrl, entity.Score, entity.FeedbackComment, entity.Status, entity.CreatedAt);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_053: Thống kê doanh thu theo khóa
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsCourseRevenueStatDto>> GetCourseRevenueStatsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var courses = await _db.LmsCourses.AsNoTracking().Where(c => c.TenantId == tenantId).ToListAsync(ct);
        var enrollments = await _db.LmsOnlineEnrollments.AsNoTracking().Where(e => e.TenantId == tenantId).ToListAsync(ct);

        var result = new List<LmsCourseRevenueStatDto>();

        foreach (var course in courses)
        {
            var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();
            int total = courseEnrollments.Count;
            int paidCount = courseEnrollments.Count(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase));
            decimal gross = paidCount * course.Price;

            result.Add(new LmsCourseRevenueStatDto(
                course.Id,
                course.Name,
                course.Price,
                total,
                paidCount,
                gross,
                course.Currency
            ));
        }

        return result.OrderByDescending(r => r.GrossRevenue).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_054: Chống chia sẻ tài khoản
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsAccountSharingGuardDto> ValidateAccountSessionAsync(Guid tenantId, Guid userId, LmsSessionValidationRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.DeviceId)) throw new AppException("Mã thiết bị không được để trống.");
        if (string.IsNullOrWhiteSpace(req.IpAddress)) throw new AppException("Địa chỉ IP không được để trống.");

        // Giả lập kiểm soát phiên làm việc đăng nhập đồng thời
        bool isSuspiciousIp = req.IpAddress.StartsWith("10.99.") || req.IpAddress.StartsWith("192.168.99.");
        bool isSharingDetected = isSuspiciousIp;

        string actionTaken = isSharingDetected ? "ForceLogoutPreviousSession" : "Allowed";
        string reason = isSharingDetected
            ? "Phát hiện truy cập đồng thời từ thiết bị / IP lạ khác vị trí thường lệ."
            : "Phiên đăng nhập hợp lệ.";

        return new LmsAccountSharingGuardDto(
            userId,
            req.DeviceId.Trim(),
            req.IpAddress.Trim(),
            isSharingDetected ? 2 : 1,
            isSharingDetected,
            actionTaken,
            reason
        );
    }
}

using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LmsContentComplianceService : ILmsContentComplianceService
{
    private readonly AppDbContext _db;

    public LmsContentComplianceService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_055: Chặn tải video
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsVideoProtectionDto> GetVideoProtectionConfigAsync(Guid tenantId, Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _db.LmsLessons.AsNoTracking().FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == lessonId, ct);
        if (lesson == null) throw new AppException($"Không tìm thấy bài học {lessonId}", 404);

        var config = await _db.LmsVideoProtections.AsNoTracking().FirstOrDefaultAsync(vp => vp.TenantId == tenantId && vp.LessonId == lessonId, ct);

        if (config == null)
        {
            // Trả về cấu hình mặc định nếu chưa lưu trong DB
            return new LmsVideoProtectionDto(
                Guid.Empty,
                lessonId,
                IsDownloadBlocked: true,
                WatermarkEnabled: true,
                WatermarkText: "Confidential - LMS Protection",
                SignedUrlExpiryMinutes: 120,
                AllowedRoles: "Instructor,Admin"
            );
        }

        return new LmsVideoProtectionDto(
            config.Id,
            config.LessonId,
            config.IsDownloadBlocked,
            config.WatermarkEnabled,
            config.WatermarkText,
            config.SignedUrlExpiryMinutes,
            config.AllowedRoles
        );
    }

    public async Task<LmsVideoProtectionDto> UpdateVideoProtectionConfigAsync(Guid tenantId, LmsVideoProtectionUpdateRequest req, CancellationToken ct = default)
    {
        if (req.LessonId == Guid.Empty) throw new AppException("Mã bài học không được để trống.");

        var lesson = await _db.LmsLessons.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == req.LessonId, ct);
        if (lesson == null) throw new AppException($"Không tìm thấy bài học {req.LessonId}", 404);

        var config = await _db.LmsVideoProtections.FirstOrDefaultAsync(vp => vp.TenantId == tenantId && vp.LessonId == req.LessonId, ct);

        if (config == null)
        {
            config = new LmsVideoProtection
            {
                TenantId = tenantId,
                LessonId = req.LessonId,
                IsDownloadBlocked = req.IsDownloadBlocked,
                WatermarkEnabled = req.WatermarkEnabled,
                WatermarkText = string.IsNullOrWhiteSpace(req.WatermarkText) ? "Protected Video Content" : req.WatermarkText,
                SignedUrlExpiryMinutes = req.SignedUrlExpiryMinutes > 0 ? req.SignedUrlExpiryMinutes : 120,
                AllowedRoles = string.IsNullOrWhiteSpace(req.AllowedRoles) ? "Instructor,Admin" : req.AllowedRoles
            };
            _db.LmsVideoProtections.Add(config);
        }
        else
        {
            config.IsDownloadBlocked = req.IsDownloadBlocked;
            config.WatermarkEnabled = req.WatermarkEnabled;
            config.WatermarkText = string.IsNullOrWhiteSpace(req.WatermarkText) ? config.WatermarkText : req.WatermarkText;
            config.SignedUrlExpiryMinutes = req.SignedUrlExpiryMinutes > 0 ? req.SignedUrlExpiryMinutes : config.SignedUrlExpiryMinutes;
            config.AllowedRoles = string.IsNullOrWhiteSpace(req.AllowedRoles) ? config.AllowedRoles : req.AllowedRoles;
        }

        await _db.SaveChangesAsync(ct);

        return new LmsVideoProtectionDto(
            config.Id,
            config.LessonId,
            config.IsDownloadBlocked,
            config.WatermarkEnabled,
            config.WatermarkText,
            config.SignedUrlExpiryMinutes,
            config.AllowedRoles
        );
    }

    public async Task<LmsVideoPlaybackUrlDto> GenerateProtectedPlaybackUrlAsync(Guid tenantId, Guid userId, Guid lessonId, string userRole = "Learner", CancellationToken ct = default)
    {
        var lesson = await _db.LmsLessons.AsNoTracking().FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == lessonId, ct);
        if (lesson == null) throw new AppException($"Không tìm thấy bài học {lessonId}", 404);

        var config = await _db.LmsVideoProtections.AsNoTracking().FirstOrDefaultAsync(vp => vp.TenantId == tenantId && vp.LessonId == lessonId, ct);

        bool isBlocked = config?.IsDownloadBlocked ?? true;
        bool watermarkEnabled = config?.WatermarkEnabled ?? true;
        string watermarkText = config?.WatermarkText ?? $"User:{userId.ToString()[..8]}";
        int expiryMinutes = config?.SignedUrlExpiryMinutes ?? 120;

        string signedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{tenantId}:{userId}:{lessonId}:{DateTimeOffset.UtcNow.Ticks}"));
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        string streamUrl = $"https://stream.erp-hung.vn/lms/video/{lessonId}?token={signedToken}&nodownload={(isBlocked ? 1 : 0)}";

        return new LmsVideoPlaybackUrlDto(
            lessonId,
            streamUrl,
            signedToken,
            expiresAt,
            isBlocked,
            watermarkEnabled,
            watermarkText
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_056 & UC_LMS_057: Khảo sát hiểu bài & Khảo sát tuân thủ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsSurveyDto>> GetSurveysAsync(Guid tenantId, string? surveyType = null, CancellationToken ct = default)
    {
        var query = _db.LmsSurveys.AsNoTracking().Where(s => s.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(surveyType))
        {
            query = query.Where(s => s.SurveyType == surveyType);
        }

        var items = await query.ToListAsync(ct);

        return items.Select(s => new LmsSurveyDto(
            s.Id,
            s.Title,
            s.SurveyType,
            s.CourseId,
            s.IsMandatory,
            s.MustCompleteBeforeShift
        )).ToList();
    }

    public async Task<LmsSurveyDto> CreateSurveyAsync(Guid tenantId, LmsSurveyUpsertRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Tiêu đề khảo sát không được để trống.");

        var survey = new LmsSurvey
        {
            TenantId = tenantId,
            Title = req.Title.Trim(),
            SurveyType = string.IsNullOrWhiteSpace(req.SurveyType) ? "Comprehension" : req.SurveyType,
            CourseId = req.CourseId,
            IsMandatory = req.IsMandatory,
            MustCompleteBeforeShift = req.MustCompleteBeforeShift
        };

        _db.LmsSurveys.Add(survey);
        await _db.SaveChangesAsync(ct);

        return new LmsSurveyDto(
            survey.Id,
            survey.Title,
            survey.SurveyType,
            survey.CourseId,
            survey.IsMandatory,
            survey.MustCompleteBeforeShift
        );
    }

    public async Task<LmsSurveyResultDto> SubmitSurveyResponseAsync(Guid tenantId, Guid userId, LmsSurveySubmissionRequest req, CancellationToken ct = default)
    {
        var survey = await _db.LmsSurveys.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == req.SurveyId, ct);
        if (survey == null) throw new AppException($"Không tìm thấy khảo sát {req.SurveyId}", 404);

        decimal score = 100m;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(req.AnswersJson) ? "{}" : req.AnswersJson);
            if (doc.RootElement.TryGetProperty("calculatedScore", out var scoreElem))
            {
                score = scoreElem.GetDecimal();
            }
        }
        catch
        {
            score = 100m;
        }

        bool isPassed = score >= req.TargetPassingScore;

        var response = new LmsSurveyResponse
        {
            TenantId = tenantId,
            SurveyId = req.SurveyId,
            StudentUserId = userId,
            AnswersJson = req.AnswersJson,
            Score = score,
            IsPassed = isPassed,
            SubmittedAt = DateTimeOffset.UtcNow
        };

        _db.LmsSurveyResponses.Add(response);
        await _db.SaveChangesAsync(ct);

        string message = isPassed
            ? $"Hoàn thành khảo sát [{survey.Title}] đạt yêu cầu ({score:F1}%)."
            : $"Khảo sát [{survey.Title}] chưa đạt tiêu chuẩn ({score:F1}% < {req.TargetPassingScore:F1}%).";

        return new LmsSurveyResultDto(
            response.Id,
            survey.Id,
            userId,
            score,
            isPassed,
            response.SubmittedAt,
            message
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_059: Bắt buộc hoàn thành trước ca
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsShiftGateEvaluationResultDto> EvaluateShiftTrainingGateAsync(Guid tenantId, LmsShiftGateCheckRequest req, CancellationToken ct = default)
    {
        if (req.EmployeeId == Guid.Empty) throw new AppException("Mã nhân viên không được để trống.");
        if (req.MandatoryCourseId == Guid.Empty) throw new AppException("Mã khóa học bắt buộc không được để trống.");

        // Kiểm tra tiến độ khóa học bắt buộc của nhân viên
        var isCompleted = await _db.LmsOnlineEnrollments.AsNoTracking().AnyAsync(
            e => e.TenantId == tenantId && e.UserId == req.EmployeeId && e.CourseId == req.MandatoryCourseId && e.Status == "Completed",
            ct
        );

        // Nếu chưa hoàn thành và thời gian ca bắt đầu từ bây giờ hoặc đã đến giờ ca
        bool isWorkEntryBlocked = !isCompleted;
        string gateStatus = isCompleted ? "Passed" : "Blocked";
        string message = isCompleted
            ? "Đã hoàn thành khóa học bắt buộc trước ca. Được phép đăng nhập làm việc."
            : "CHẶN VÀO CA: Nhân viên chưa hoàn thành khóa học đào tạo bắt buộc trước giờ ca làm việc!";

        var record = await _db.LmsShiftTrainingGates.FirstOrDefaultAsync(
            g => g.TenantId == tenantId && g.EmployeeId == req.EmployeeId && g.ShiftId == req.ShiftId && g.ShiftDate == req.ShiftDate.Date,
            ct
        );

        if (record == null)
        {
            record = new LmsShiftTrainingGate
            {
                TenantId = tenantId,
                EmployeeId = req.EmployeeId,
                ShiftId = req.ShiftId,
                ShiftDate = req.ShiftDate.Date,
                ShiftStartTime = req.ShiftStartTime,
                CourseId = req.MandatoryCourseId,
                IsMandatoryCompleted = isCompleted,
                IsWorkEntryBlocked = isWorkEntryBlocked,
                GateStatus = gateStatus
            };
            _db.LmsShiftTrainingGates.Add(record);
        }
        else
        {
            record.IsMandatoryCompleted = isCompleted;
            record.IsWorkEntryBlocked = isWorkEntryBlocked;
            record.GateStatus = gateStatus;
        }

        await _db.SaveChangesAsync(ct);

        return new LmsShiftGateEvaluationResultDto(
            req.EmployeeId,
            req.ShiftId,
            isCompleted,
            isWorkEntryBlocked,
            gateStatus,
            message
        );
    }
}

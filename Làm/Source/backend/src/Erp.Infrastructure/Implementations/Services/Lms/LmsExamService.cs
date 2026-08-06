using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Lms;

public sealed class LmsExamService : ILmsExamService
{
    private static readonly HashSet<string> QuestionTypes =
        new(StringComparer.OrdinalIgnoreCase) { "SingleChoice", "TrueFalse" };
    private static readonly HashSet<string> ExamTypes =
        new(StringComparer.OrdinalIgnoreCase) { "ChapterQuiz", "Final" };
    private static readonly HashSet<string> ExamStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Draft", "Published", "Archived" };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _db;

    public LmsExamService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LmsQuestionDto>> ListQuestionsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LmsQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return list.Select(MapQuestion).ToList();
    }

    public async Task<LmsQuestionDto> UpsertQuestionAsync(
        Guid tenantId, Guid userId, LmsQuestionUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var stem = (req.Stem ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã câu hỏi 1–40 ký tự.");
        if (stem.Length is < 1 or > 2000) throw new AppException("Nội dung câu hỏi 1–2000 ký tự.");
        var type = string.IsNullOrWhiteSpace(req.QuestionType) ? "SingleChoice" : req.QuestionType.Trim();
        if (!QuestionTypes.Contains(type)) throw new AppException("Loại câu hỏi không hợp lệ.");
        if (req.Options is null || req.Options.Count < 2) throw new AppException("Cần ít nhất 2 đáp án.");
        if (req.CorrectKeys is null || req.CorrectKeys.Count < 1) throw new AppException("Cần ít nhất 1 đáp án đúng.");
        if (req.Points <= 0) throw new AppException("Điểm câu hỏi phải > 0.");

        var optionKeys = req.Options.Select(o => o.Key.Trim().ToUpperInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var k in req.CorrectKeys)
            if (!optionKeys.Contains(k.Trim()))
                throw new AppException($"Đáp án đúng '{k}' không nằm trong options.");

        if (string.Equals(type, "TrueFalse", StringComparison.OrdinalIgnoreCase) && req.Options.Count != 2)
            throw new AppException("TrueFalse cần đúng 2 đáp án.");

        LmsQuestion entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsQuestions.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Câu hỏi không tồn tại.", 404);
        }
        else
        {
            if (await _db.LmsQuestions.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã câu hỏi đã tồn tại.");
            entity = new LmsQuestion { TenantId = tenantId, CreatedBy = userId };
            _db.LmsQuestions.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsQuestions.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã câu hỏi đã tồn tại.");

        var options = req.Options.Select(o => new LmsQuestionOptionDto(
            o.Key.Trim().ToUpperInvariant(), (o.Text ?? "").Trim())).ToList();
        var corrects = req.CorrectKeys.Select(k => k.Trim().ToUpperInvariant()).Distinct().ToList();

        entity.Code = code;
        entity.Stem = stem;
        entity.QuestionType = type;
        entity.OptionsJson = JsonSerializer.Serialize(options, JsonOpts);
        entity.CorrectKeysJson = JsonSerializer.Serialize(corrects, JsonOpts);
        entity.Points = req.Points;
        entity.Tag = string.IsNullOrWhiteSpace(req.Tag) ? null : req.Tag.Trim();
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapQuestion(entity);
    }

    public async Task<IReadOnlyList<LmsExamDto>> ListExamsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var exams = await _db.LmsExams.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return await MapExamListAsync(tenantId, exams, ct);
    }

    public async Task<LmsExamDto> UpsertExamAsync(
        Guid tenantId, Guid userId, LmsExamUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã đề 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên đề 1–200 ký tự.");
        var examType = string.IsNullOrWhiteSpace(req.ExamType) ? "Final" : req.ExamType.Trim();
        if (!ExamTypes.Contains(examType)) throw new AppException("Loại đề không hợp lệ.");
        if (req.PassScore is < 0 or > 100) throw new AppException("Điểm đạt 0–100.");
        if (req.MaxAttempts < 1) throw new AppException("Số lần thi tối thiểu 1.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim();
        if (!ExamStatuses.Contains(status)) throw new AppException("Trạng thái đề không hợp lệ.");

        if (req.CourseId is Guid courseId)
        {
            var ok = await _db.LmsCourses.AnyAsync(
                x => x.Id == courseId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Khóa học không tồn tại.", 404);
        }

        if (string.Equals(examType, "ChapterQuiz", StringComparison.OrdinalIgnoreCase))
        {
            if (req.ChapterId is not Guid chapterId)
                throw new AppException("Quiz chương cần ChapterId.");
            var ch = await _db.LmsChapters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == chapterId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Chương không tồn tại.", 404);
            if (req.CourseId is Guid cid && ch.CourseId != cid)
                throw new AppException("Chương không thuộc khóa đã chọn.");
        }

        LmsExam entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsExams.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Đề thi không tồn tại.", 404);
        }
        else
        {
            if (await _db.LmsExams.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã đề đã tồn tại.");
            entity = new LmsExam { TenantId = tenantId, CreatedBy = userId, Status = "Draft" };
            _db.LmsExams.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsExams.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã đề đã tồn tại.");

        entity.Code = code;
        entity.Name = name;
        entity.ExamType = examType;
        entity.CourseId = req.CourseId;
        entity.ChapterId = req.ChapterId;
        entity.PassScore = req.PassScore;
        entity.MaxAttempts = req.MaxAttempts;
        entity.TimeLimitMin = req.TimeLimitMin;
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var mapped = await MapExamListAsync(tenantId, [entity], ct);
        return mapped[0];
    }

    public async Task<LmsExamDetailDto> GetExamDetailAsync(
        Guid tenantId, Guid examId, CancellationToken ct = default)
    {
        var exam = await _db.LmsExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == examId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);

        var links = await _db.LmsExamQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExamId == examId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);
        var qIds = links.Select(l => l.QuestionId).ToList();
        var questions = qIds.Count == 0
            ? new Dictionary<Guid, LmsQuestion>()
            : await _db.LmsQuestions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && qIds.Contains(x.Id) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, ct);

        var items = links.Select(l =>
        {
            questions.TryGetValue(l.QuestionId, out var q);
            var pts = l.PointsOverride ?? q?.Points ?? 0;
            return new LmsExamQuestionItemDto(
                l.Id, l.QuestionId, q?.Code ?? "?", q?.Stem ?? "(đã xóa)",
                q?.QuestionType ?? "?", l.SortOrder, pts);
        }).ToList();

        var examDto = (await MapExamListAsync(tenantId, [exam], ct))[0];
        return new LmsExamDetailDto(examDto, items);
    }

    public async Task<LmsExamDto> SetExamStatusAsync(
        Guid tenantId, Guid userId, Guid examId, LmsPublishExamRequest req, CancellationToken ct = default)
    {
        var status = (req.Status ?? "").Trim();
        if (!ExamStatuses.Contains(status)) throw new AppException("Trạng thái đề không hợp lệ.");

        var entity = await _db.LmsExams.FirstOrDefaultAsync(
            x => x.Id == examId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);

        if (string.Equals(status, "Published", StringComparison.OrdinalIgnoreCase))
        {
            var count = await _db.LmsExamQuestions.CountAsync(
                x => x.TenantId == tenantId && x.ExamId == examId && !x.IsDeleted, ct);
            if (count < 1) throw new AppException("Cần ít nhất 1 câu hỏi trước khi xuất bản.");
            if (entity.CourseId is null) throw new AppException("Đề Published cần gắn khóa học.");
        }

        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapExamListAsync(tenantId, [entity], ct))[0];
    }

    public async Task<LmsExamQuestionItemDto> AddQuestionToExamAsync(
        Guid tenantId, Guid userId, Guid examId, LmsExamAddQuestionRequest req, CancellationToken ct = default)
    {
        var exam = await _db.LmsExams.FirstOrDefaultAsync(
            x => x.Id == examId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);

        var question = await _db.LmsQuestions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.QuestionId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Câu hỏi không tồn tại.", 404);
        if (!question.IsActive) throw new AppException("Câu hỏi đang ngưng.");

        if (await _db.LmsExamQuestions.AnyAsync(
                x => x.TenantId == tenantId && x.ExamId == examId && x.QuestionId == req.QuestionId && !x.IsDeleted, ct))
            throw new AppException("Câu hỏi đã có trong đề.");

        var maxOrder = await _db.LmsExamQuestions
            .Where(x => x.TenantId == tenantId && x.ExamId == examId && !x.IsDeleted)
            .Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? 0;

        var link = new LmsExamQuestion
        {
            TenantId = tenantId,
            ExamId = examId,
            QuestionId = req.QuestionId,
            SortOrder = maxOrder + 1,
            PointsOverride = req.PointsOverride,
            CreatedBy = userId
        };
        _db.LmsExamQuestions.Add(link);
        exam.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var pts = link.PointsOverride ?? question.Points;
        return new LmsExamQuestionItemDto(
            link.Id, question.Id, question.Code, question.Stem, question.QuestionType, link.SortOrder, pts);
    }

    public async Task<IReadOnlyList<LmsLearnerExamDto>> ListLearnerExamsAsync(
        Guid tenantId, Guid userId, Guid courseId, CancellationToken ct = default)
    {
        var unlocked = await _db.LmsOnlineEnrollments.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId
                 && !x.IsDeleted && (x.Status == "Unlocked" || x.Status == "Completed"), ct);
        if (!unlocked) throw new AppException("Chưa mở khóa khóa học.", 403);

        var exams = await _db.LmsExams.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Published" && x.CourseId == courseId)
            .OrderBy(x => x.ExamType).ThenBy(x => x.Code)
            .ToListAsync(ct);

        var result = new List<LmsLearnerExamDto>();
        foreach (var e in exams)
        {
            var attempts = await _db.LmsExamAttempts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ExamId == e.Id && x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.AttemptNo)
                .ToListAsync(ct);
            var used = attempts.Count;
            var last = attempts.FirstOrDefault(a => a.Status == "Submitted");
            var inProgress = attempts.Any(a => a.Status == "InProgress");
            result.Add(new LmsLearnerExamDto(
                e.Id, e.Code, e.Name, e.ExamType, e.ChapterId, e.PassScore, e.MaxAttempts, used,
                !inProgress && used < e.MaxAttempts,
                last?.Passed,
                last is null ? null : last.MaxScore == 0 ? 0 : Math.Round(100m * last.Score / last.MaxScore, 1)));
        }
        return result;
    }

    public async Task<LmsAttemptDto> StartAttemptAsync(
        Guid tenantId, Guid userId, Guid examId, CancellationToken ct = default)
    {
        var exam = await _db.LmsExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == examId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);
        if (exam.Status != "Published") throw new AppException("Đề chưa xuất bản.");
        if (exam.CourseId is not Guid courseId) throw new AppException("Đề chưa gắn khóa.");

        var enrollment = await _db.LmsOnlineEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId && !x.IsDeleted
                     && (x.Status == "Unlocked" || x.Status == "Completed"), ct)
            ?? throw new AppException("Chưa mở khóa khóa học.", 403);

        var existingOpen = await _db.LmsExamAttempts.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ExamId == examId && x.UserId == userId
                     && x.Status == "InProgress" && !x.IsDeleted, ct);
        if (existingOpen is not null)
            return await BuildAttemptDtoAsync(tenantId, existingOpen, includeQuestions: true, ct);

        var used = await _db.LmsExamAttempts.CountAsync(
            x => x.TenantId == tenantId && x.ExamId == examId && x.UserId == userId && !x.IsDeleted, ct);
        if (used >= exam.MaxAttempts) throw new AppException("Đã hết số lần thi cho phép.");

        var attempt = new LmsExamAttempt
        {
            TenantId = tenantId,
            ExamId = examId,
            UserId = userId,
            EnrollmentId = enrollment.Id,
            AttemptNo = used + 1,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "InProgress",
            AnswersJson = "{}",
            CreatedBy = userId
        };
        _db.LmsExamAttempts.Add(attempt);
        await _db.SaveChangesAsync(ct);
        return await BuildAttemptDtoAsync(tenantId, attempt, includeQuestions: true, ct);
    }

    public async Task<LmsAttemptResultDto> SubmitAttemptAsync(
        Guid tenantId, Guid userId, Guid attemptId, LmsSubmitAttemptRequest req, CancellationToken ct = default)
    {
        var attempt = await _db.LmsExamAttempts.FirstOrDefaultAsync(
            x => x.Id == attemptId && x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Lượt thi không tồn tại.", 404);
        if (attempt.Status == "Submitted")
            return await GetAttemptResultAsync(tenantId, userId, attemptId, ct);

        var exam = await _db.LmsExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attempt.ExamId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);

        var links = await _db.LmsExamQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExamId == exam.Id && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);
        var qIds = links.Select(l => l.QuestionId).ToList();
        var questions = await _db.LmsQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && qIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);

        var answers = req.Answers ?? new Dictionary<string, string>();
        decimal score = 0, maxScore = 0;
        var reviews = new List<LmsAnswerReviewDto>();

        foreach (var link in links)
        {
            if (!questions.TryGetValue(link.QuestionId, out var q)) continue;
            var pts = link.PointsOverride ?? q.Points;
            maxScore += pts;
            answers.TryGetValue(link.QuestionId.ToString(), out var raw);
            raw ??= answers.FirstOrDefault(kv =>
                string.Equals(kv.Key, link.QuestionId.ToString(), StringComparison.OrdinalIgnoreCase)).Value;
            var yourKey = string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToUpperInvariant();
            var correct = ParseKeys(q.CorrectKeysJson);
            var isCorrect = yourKey is not null
                && correct.Count == 1
                && correct.Contains(yourKey, StringComparer.OrdinalIgnoreCase);
            // Multi-key: exact set match if ever needed — SingleChoice/TrueFalse = 1 key
            if (!isCorrect && yourKey is not null && correct.Count > 1)
            {
                var yours = yourKey.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToUpperInvariant()).OrderBy(x => x).ToList();
                isCorrect = yours.SequenceEqual(correct.Select(x => x.ToUpperInvariant()).OrderBy(x => x));
            }
            var earned = isCorrect ? pts : 0;
            score += earned;
            reviews.Add(new LmsAnswerReviewDto(
                q.Id, q.Stem, yourKey, correct, isCorrect, earned, pts));
        }

        var pct = maxScore == 0 ? 0 : Math.Round(100m * score / maxScore, 1);
        var passed = pct >= exam.PassScore;

        attempt.AnswersJson = JsonSerializer.Serialize(answers, JsonOpts);
        attempt.Score = score;
        attempt.MaxScore = maxScore;
        attempt.Passed = passed;
        attempt.Status = "Submitted";
        attempt.SubmittedAt = DateTimeOffset.UtcNow;
        attempt.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        LmsCertificateDto? cert = null;
        if (passed && string.Equals(exam.ExamType, "Final", StringComparison.OrdinalIgnoreCase)
            && exam.CourseId is Guid courseId)
        {
            cert = await TryIssueCertificateAsync(tenantId, userId, courseId, attempt, pct, ct);
        }

        return new LmsAttemptResultDto(
            attempt.Id, attempt.ExamId, attempt.AttemptNo, score, maxScore, passed, exam.PassScore, reviews, cert);
    }

    public async Task<LmsAttemptResultDto> GetAttemptResultAsync(
        Guid tenantId, Guid userId, Guid attemptId, CancellationToken ct = default)
    {
        var attempt = await _db.LmsExamAttempts.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == attemptId && x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Lượt thi không tồn tại.", 404);
        if (attempt.Status != "Submitted") throw new AppException("Chưa nộp bài.");

        var exam = await _db.LmsExams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attempt.ExamId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đề thi không tồn tại.", 404);

        var links = await _db.LmsExamQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExamId == exam.Id && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);
        var qIds = links.Select(l => l.QuestionId).ToList();
        var questions = await _db.LmsQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && qIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);

        var answers = ParseAnswers(attempt.AnswersJson);
        var reviews = new List<LmsAnswerReviewDto>();
        foreach (var link in links)
        {
            if (!questions.TryGetValue(link.QuestionId, out var q)) continue;
            var pts = link.PointsOverride ?? q.Points;
            answers.TryGetValue(link.QuestionId.ToString(), out var raw);
            var yourKey = string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToUpperInvariant();
            var correct = ParseKeys(q.CorrectKeysJson);
            var isCorrect = yourKey is not null && correct.Contains(yourKey, StringComparer.OrdinalIgnoreCase)
                && correct.Count == 1;
            reviews.Add(new LmsAnswerReviewDto(
                q.Id, q.Stem, yourKey, correct, isCorrect, isCorrect ? pts : 0, pts));
        }

        LmsCertificateDto? cert = null;
        if (exam.CourseId is Guid courseId)
        {
            var c = await _db.LmsCertificates.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId
                         && x.Status == "Active" && !x.IsDeleted, ct);
            if (c is not null)
            {
                var courseName = await _db.LmsCourses.AsNoTracking()
                    .Where(x => x.Id == courseId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? "";
                cert = new LmsCertificateDto(
                    c.Id, c.CourseId, courseName, c.UserId, c.Code, c.IssuedAt, c.Status, c.ScoreAtIssue);
            }
        }

        return new LmsAttemptResultDto(
            attempt.Id, attempt.ExamId, attempt.AttemptNo, attempt.Score, attempt.MaxScore,
            attempt.Passed, exam.PassScore, reviews, cert);
    }

    public async Task<IReadOnlyList<LmsCertificateDto>> ListMyCertificatesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var list = await _db.LmsCertificates.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.IssuedAt)
            .ToListAsync(ct);
        if (list.Count == 0) return Array.Empty<LmsCertificateDto>();

        var courseIds = list.Select(c => c.CourseId).Distinct().ToList();
        var names = await _db.LmsCourses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && courseIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return list.Select(c => new LmsCertificateDto(
            c.Id, c.CourseId, names.GetValueOrDefault(c.CourseId) ?? "", c.UserId,
            c.Code, c.IssuedAt, c.Status, c.ScoreAtIssue)).ToList();
    }

    private async Task<LmsCertificateDto?> TryIssueCertificateAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsExamAttempt attempt, decimal pct, CancellationToken ct)
    {
        var existing = await _db.LmsCertificates.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId
                     && x.Status == "Active" && !x.IsDeleted, ct);
        if (existing is not null)
        {
            var name0 = await _db.LmsCourses.AsNoTracking()
                .Where(x => x.Id == courseId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? "";
            return new LmsCertificateDto(
                existing.Id, existing.CourseId, name0, existing.UserId,
                existing.Code, existing.IssuedAt, existing.Status, existing.ScoreAtIssue);
        }

        // Điều kiện: hoàn thành bài học (nếu có) + đậu Final
        var chapterIds = await _db.LmsChapters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CourseId == courseId && !x.IsDeleted)
            .Select(x => x.Id).ToListAsync(ct);
        var totalLessons = chapterIds.Count == 0 ? 0 : await _db.LmsLessons.CountAsync(
            x => x.TenantId == tenantId && chapterIds.Contains(x.ChapterId) && !x.IsDeleted, ct);
        if (totalLessons > 0 && attempt.EnrollmentId is Guid enId)
        {
            var done = await _db.LmsLessonProgresses.CountAsync(
                x => x.TenantId == tenantId && x.EnrollmentId == enId && !x.IsDeleted && x.Status == "Completed", ct);
            if (done < totalLessons)
                return null; // đậu thi nhưng chưa xong bài — chưa cấp
        }

        var code = $"CERT-{DateTime.UtcNow:yyMM}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var cert = new LmsCertificate
        {
            TenantId = tenantId,
            CourseId = courseId,
            UserId = userId,
            EnrollmentId = attempt.EnrollmentId,
            FinalAttemptId = attempt.Id,
            Code = code,
            IssuedAt = DateTimeOffset.UtcNow,
            Status = "Active",
            ScoreAtIssue = pct,
            CreatedBy = userId
        };
        _db.LmsCertificates.Add(cert);

        if (attempt.EnrollmentId is Guid enrollmentId)
        {
            var en = await _db.LmsOnlineEnrollments.FirstOrDefaultAsync(
                x => x.Id == enrollmentId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (en is not null) en.Status = "Completed";
        }

        await _db.SaveChangesAsync(ct);
        var courseName = await _db.LmsCourses.AsNoTracking()
            .Where(x => x.Id == courseId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? "";
        return new LmsCertificateDto(
            cert.Id, cert.CourseId, courseName, cert.UserId, cert.Code, cert.IssuedAt, cert.Status, cert.ScoreAtIssue);
    }

    private async Task<LmsAttemptDto> BuildAttemptDtoAsync(
        Guid tenantId, LmsExamAttempt attempt, bool includeQuestions, CancellationToken ct)
    {
        IReadOnlyList<LmsTakeQuestionDto>? qs = null;
        if (includeQuestions)
        {
            var links = await _db.LmsExamQuestions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ExamId == attempt.ExamId && !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(ct);
            var qIds = links.Select(l => l.QuestionId).ToList();
            var questions = await _db.LmsQuestions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && qIds.Contains(x.Id) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, ct);
            qs = links.Select(l =>
            {
                questions.TryGetValue(l.QuestionId, out var q);
                var opts = q is null ? Array.Empty<LmsQuestionOptionDto>() : ParseOptions(q.OptionsJson);
                return new LmsTakeQuestionDto(
                    l.QuestionId, q?.Stem ?? "?", q?.QuestionType ?? "SingleChoice",
                    opts, l.PointsOverride ?? q?.Points ?? 0, l.SortOrder);
            }).ToList();
        }

        return new LmsAttemptDto(
            attempt.Id, attempt.ExamId, attempt.AttemptNo, attempt.Status,
            attempt.StartedAt, attempt.SubmittedAt, attempt.Score, attempt.MaxScore, attempt.Passed, qs);
    }

    private async Task<IReadOnlyList<LmsExamDto>> MapExamListAsync(
        Guid tenantId, List<LmsExam> exams, CancellationToken ct)
    {
        if (exams.Count == 0) return Array.Empty<LmsExamDto>();
        var ids = exams.Select(e => e.Id).ToList();
        var counts = await _db.LmsExamQuestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ExamId) && !x.IsDeleted)
            .GroupBy(x => x.ExamId)
            .Select(g => new { ExamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ExamId, x => x.Count, ct);

        var courseIds = exams.Where(e => e.CourseId.HasValue).Select(e => e.CourseId!.Value).Distinct().ToList();
        var courses = courseIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.LmsCourses.AsNoTracking()
                .Where(x => x.TenantId == tenantId && courseIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var chapterIds = exams.Where(e => e.ChapterId.HasValue).Select(e => e.ChapterId!.Value).Distinct().ToList();
        var chapters = chapterIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.LmsChapters.AsNoTracking()
                .Where(x => x.TenantId == tenantId && chapterIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Title, ct);

        return exams.Select(e => new LmsExamDto(
            e.Id, e.Code, e.Name, e.ExamType, e.CourseId,
            e.CourseId is Guid cid ? courses.GetValueOrDefault(cid) : null,
            e.ChapterId, e.ChapterId is Guid chid ? chapters.GetValueOrDefault(chid) : null,
            e.PassScore, e.MaxAttempts, e.TimeLimitMin, e.Status,
            counts.GetValueOrDefault(e.Id))).ToList();
    }

    private static LmsQuestionDto MapQuestion(LmsQuestion q) =>
        new(q.Id, q.Code, q.Stem, q.QuestionType, ParseOptions(q.OptionsJson), ParseKeys(q.CorrectKeysJson),
            q.Points, q.Tag, q.IsActive);

    private static IReadOnlyList<LmsQuestionOptionDto> ParseOptions(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<LmsQuestionOptionDto>>(json, JsonOpts)
                   ?? new List<LmsQuestionOptionDto>();
        }
        catch { return Array.Empty<LmsQuestionOptionDto>(); }
    }

    private static IReadOnlyList<string> ParseKeys(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? new List<string>();
        }
        catch { return Array.Empty<string>(); }
    }

    private static Dictionary<string, string> ParseAnswers(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOpts)
                   ?? new Dictionary<string, string>();
        }
        catch { return new Dictionary<string, string>(); }
    }
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class HrmLmsEvalCatalogService : IHrmLmsEvalCatalogService
{
    private readonly AppDbContext _db;

    public HrmLmsEvalCatalogService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_181: Tổng hợp kết quả đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<HrmEvaluationSummaryReportDto> GetEvaluationSummaryReportAsync(Guid tenantId, Guid cycleId, CancellationToken ct = default)
    {
        var cycle = await _db.HrmEvaluationCycles.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == cycleId, ct);
        if (cycle == null) throw new AppException($"Không tìm thấy kỳ đánh giá {cycleId}", 404);

        var evaluations = await _db.HrmManagerEvaluations.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.EvaluationCycleId == cycleId)
            .ToListAsync(ct);

        if (evaluations.Count == 0)
        {
            return new HrmEvaluationSummaryReportDto(
                cycleId,
                cycle.CycleName,
                0,
                0m,
                0m,
                new List<HrmGradeDistributionDto>
                {
                    new("A", 0, 0m), new("B", 0, 0m), new("C", 0, 0m), new("D", 0, 0m)
                }
            );
        }

        int total = evaluations.Count;
        decimal avgKpi = Math.Round(evaluations.Average(e => e.KpiScore), 2);
        decimal avgCompetency = Math.Round(evaluations.Average(e => e.CompetencyScore), 2);

        var grades = new[] { "A", "B", "C", "D" };
        var dists = grades.Select(g =>
        {
            int cnt = evaluations.Count(e => string.Equals(e.FinalGrade, g, StringComparison.OrdinalIgnoreCase));
            decimal pct = total > 0 ? Math.Round((decimal)cnt / total * 100m, 2) : 0m;
            return new HrmGradeDistributionDto(g, cnt, pct);
        }).ToList();

        return new HrmEvaluationSummaryReportDto(
            cycleId,
            cycle.CycleName,
            total,
            avgKpi,
            avgCompetency,
            dists
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_007: Gắn tag kỹ năng / vị trí
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsCourseSkillTagDto>> GetCourseSkillTagsAsync(Guid tenantId, Guid? courseId = null, CancellationToken ct = default)
    {
        var query = _db.LmsCourseSkillTags.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (courseId.HasValue && courseId.Value != Guid.Empty) query = query.Where(t => t.CourseId == courseId.Value);

        var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

        var courseIds = items.Select(t => t.CourseId).Distinct().ToList();
        var courses = await _db.LmsCourses.AsNoTracking()
            .Where(c => c.TenantId == tenantId && courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        return items.Select(t => new LmsCourseSkillTagDto(
            t.Id,
            t.CourseId,
            courses.TryGetValue(t.CourseId, out var name) ? name : null,
            t.TagName,
            t.TagType,
            t.RelatedRefId,
            t.CreatedAt
        )).ToList();
    }

    public async Task<LmsCourseSkillTagDto> CreateCourseSkillTagAsync(Guid tenantId, LmsCourseSkillTagUpsertRequest req, CancellationToken ct = default)
    {
        if (req.CourseId == Guid.Empty) throw new AppException("Khóa học không được để trống.");
        if (string.IsNullOrWhiteSpace(req.TagName)) throw new AppException("Tên tag không được để trống.");

        var courseExists = await _db.LmsCourses.AnyAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        if (!courseExists) throw new AppException($"Không tìm thấy khóa học {req.CourseId}.", 404);

        var entity = new LmsCourseSkillTag
        {
            TenantId = tenantId,
            CourseId = req.CourseId,
            TagName = req.TagName.Trim(),
            TagType = NormalizeTagType(req.TagType),
            RelatedRefId = req.RelatedRefId
        };

        _db.LmsCourseSkillTags.Add(entity);
        await _db.SaveChangesAsync(ct);

        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        return new LmsCourseSkillTagDto(entity.Id, entity.CourseId, course?.Name, entity.TagName, entity.TagType, entity.RelatedRefId, entity.CreatedAt);
    }

    public async Task DeleteCourseSkillTagAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.LmsCourseSkillTags.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy tag {id}", 404);

        _db.LmsCourseSkillTags.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static string NormalizeTagType(string type)
    {
        var valid = new[] { "Skill", "Position", "General" };
        var found = valid.FirstOrDefault(v => string.Equals(v, type, StringComparison.OrdinalIgnoreCase));
        return found ?? "Skill";
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_008: Phiên bản nội dung khóa học
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<LmsCourseVersionDto>> GetCourseVersionsAsync(Guid tenantId, Guid courseId, CancellationToken ct = default)
    {
        var course = await _db.LmsCourses.AsNoTracking().FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == courseId, ct);
        if (course == null) throw new AppException($"Không tìm thấy khóa học {courseId}", 404);

        var items = await _db.LmsCourseVersions.AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.CourseId == courseId)
            .OrderByDescending(v => v.PublishedAt)
            .ToListAsync(ct);

        return items.Select(v => new LmsCourseVersionDto(
            v.Id,
            v.CourseId,
            course.Name,
            v.VersionNumber,
            v.Changelog,
            v.IsPublished,
            v.PublishedAt,
            v.CreatedAt
        )).ToList();
    }

    public async Task<LmsCourseVersionDto> CreateCourseVersionAsync(Guid tenantId, LmsCourseVersionUpsertRequest req, CancellationToken ct = default)
    {
        if (req.CourseId == Guid.Empty) throw new AppException("Khóa học không được để trống.");
        if (string.IsNullOrWhiteSpace(req.VersionNumber)) throw new AppException("Số phiên bản không được để trống.");

        var course = await _db.LmsCourses.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        if (course == null) throw new AppException($"Không tìm thấy khóa học {req.CourseId}.", 404);

        var entity = new LmsCourseVersion
        {
            TenantId = tenantId,
            CourseId = req.CourseId,
            VersionNumber = req.VersionNumber.Trim(),
            Changelog = req.Changelog.Trim(),
            IsPublished = req.IsPublished,
            PublishedAt = DateTimeOffset.UtcNow
        };

        _db.LmsCourseVersions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LmsCourseVersionDto(entity.Id, entity.CourseId, course.Name, entity.VersionNumber, entity.Changelog, entity.IsPublished, entity.PublishedAt, entity.CreatedAt);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_013: Tạo đề thi random
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<LmsRandomExamResult> GenerateRandomExamAsync(Guid tenantId, LmsRandomExamRequest req, CancellationToken ct = default)
    {
        if (req.CourseId == Guid.Empty) throw new AppException("Khóa học không được để trống.");
        if (string.IsNullOrWhiteSpace(req.ExamTitle)) throw new AppException("Tên đề thi không được để trống.");
        if (req.TotalQuestions <= 0) throw new AppException("Số lượng câu hỏi phải lớn hơn 0.");

        var courseExists = await _db.LmsCourses.AnyAsync(c => c.TenantId == tenantId && c.Id == req.CourseId, ct);
        if (!courseExists) throw new AppException($"Không tìm thấy khóa học {req.CourseId}.", 404);

        var allQuestions = await _db.LmsQuestions.AsNoTracking()
            .Where(q => q.TenantId == tenantId)
            .Select(q => q.Id)
            .ToListAsync(ct);

        var selectedIds = allQuestions.OrderBy(_ => Guid.NewGuid()).Take(req.TotalQuestions).ToList();

        var exam = new LmsExam
        {
            TenantId = tenantId,
            CourseId = req.CourseId,
            Code = $"EXAM-RND-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Name = req.ExamTitle.Trim(),
            PassScore = req.PassingScore,
            TimeLimitMin = req.DurationMinutes,
            Status = "Published"
        };

        _db.LmsExams.Add(exam);
        await _db.SaveChangesAsync(ct);

        for (int i = 0; i < selectedIds.Count; i++)
        {
            _db.LmsExamQuestions.Add(new LmsExamQuestion
            {
                TenantId = tenantId,
                ExamId = exam.Id,
                QuestionId = selectedIds[i],
                SortOrder = i + 1,
                PointsOverride = 1m
            });
        }

        if (selectedIds.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return new LmsRandomExamResult(
            exam.Id,
            exam.Name,
            exam.CourseId ?? req.CourseId,
            selectedIds.Count,
            exam.PassScore,
            exam.TimeLimitMin ?? 45,
            selectedIds
        );
    }
}

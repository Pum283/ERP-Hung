using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Lms;

public sealed class LmsCourseService : ILmsCourseService
{
    private static readonly HashSet<string> DeliveryModes =
        new(StringComparer.OrdinalIgnoreCase) { "Online", "Offline", "Blended" };
    private static readonly HashSet<string> CourseStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Draft", "Published", "Hidden" };
    private static readonly HashSet<string> LessonTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Video", "Document", "Text" };

    private readonly AppDbContext _db;

    public LmsCourseService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LmsProgramDto>> ListProgramsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        return await _db.LmsPrograms.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new LmsProgramDto(x.Id, x.Code, x.Name, x.Description, x.Status))
            .ToListAsync(ct);
    }

    public async Task<LmsProgramDto> UpsertProgramAsync(
        Guid tenantId, Guid userId, LmsProgramUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã CTĐT 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên CTĐT 1–200 ký tự.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Trạng thái CTĐT không hợp lệ.");

        LmsProgram entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsPrograms.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Chương trình không tồn tại.", 404);
        }
        else
        {
            if (await _db.LmsPrograms.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã CTĐT đã tồn tại.");
            entity = new LmsProgram { TenantId = tenantId, CreatedBy = userId };
            _db.LmsPrograms.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsPrograms.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã CTĐT đã tồn tại.");

        entity.Code = code;
        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new LmsProgramDto(entity.Id, entity.Code, entity.Name, entity.Description, entity.Status);
    }

    public async Task<IReadOnlyList<LmsCourseDto>> ListCoursesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var courses = await _db.LmsCourses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        if (courses.Count == 0) return Array.Empty<LmsCourseDto>();

        var programIds = courses.Where(c => c.ProgramId.HasValue).Select(c => c.ProgramId!.Value).Distinct().ToList();
        var programs = programIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.LmsPrograms.AsNoTracking()
                .Where(x => x.TenantId == tenantId && programIds.Contains(x.Id) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var ids = courses.Select(c => c.Id).ToList();
        var chapters = await _db.LmsChapters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.CourseId) && !x.IsDeleted)
            .Select(x => new { x.Id, x.CourseId })
            .ToListAsync(ct);
        var chapterIds = chapters.Select(c => c.Id).ToList();
        var lessonCountsByChapter = chapterIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await _db.LmsLessons.AsNoTracking()
                .Where(x => x.TenantId == tenantId && chapterIds.Contains(x.ChapterId) && !x.IsDeleted)
                .GroupBy(x => x.ChapterId)
                .Select(g => new { ChapterId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ChapterId, x => x.Count, ct);

        var chapterCountByCourse = chapters.GroupBy(c => c.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());
        var lessonCountByCourse = chapters.GroupBy(c => c.CourseId)
            .ToDictionary(g => g.Key, g => g.Sum(ch => lessonCountsByChapter.GetValueOrDefault(ch.Id)));

        return courses.Select(c => MapCourse(
            c,
            c.ProgramId is Guid pid ? programs.GetValueOrDefault(pid) : null,
            chapterCountByCourse.GetValueOrDefault(c.Id),
            lessonCountByCourse.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<LmsCourseDto> UpsertCourseAsync(
        Guid tenantId, Guid userId, LmsCourseUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã khóa 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên khóa 1–200 ký tự.");
        var mode = string.IsNullOrWhiteSpace(req.DeliveryMode) ? "Online" : req.DeliveryMode.Trim();
        if (!DeliveryModes.Contains(mode)) throw new AppException("Hình thức khóa không hợp lệ.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim();
        if (!CourseStatuses.Contains(status)) throw new AppException("Trạng thái khóa không hợp lệ.");
        if (req.Price < 0) throw new AppException("Giá khóa không hợp lệ.");

        if (req.ProgramId is Guid programId)
        {
            var ok = await _db.LmsPrograms.AnyAsync(
                x => x.Id == programId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Chương trình không tồn tại.", 404);
        }

        LmsCourse entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsCourses.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Khóa học không tồn tại.", 404);
        }
        else
        {
            if (await _db.LmsCourses.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã khóa đã tồn tại.");
            entity = new LmsCourse { TenantId = tenantId, CreatedBy = userId, Status = "Draft" };
            _db.LmsCourses.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsCourses.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã khóa đã tồn tại.");

        entity.ProgramId = req.ProgramId;
        entity.Code = code;
        entity.Name = name;
        entity.Summary = string.IsNullOrWhiteSpace(req.Summary) ? null : req.Summary.Trim();
        entity.DeliveryMode = mode;
        entity.Status = status;
        entity.Price = req.Price;
        entity.Currency = string.IsNullOrWhiteSpace(req.Currency) ? "VND" : req.Currency.Trim().ToUpperInvariant();
        entity.CoverUrl = string.IsNullOrWhiteSpace(req.CoverUrl) ? null : req.CoverUrl.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var detail = await GetCourseDetailAsync(tenantId, entity.Id, ct);
        return detail.Course;
    }

    public async Task<LmsCourseDetailDto> GetCourseDetailAsync(
        Guid tenantId, Guid courseId, CancellationToken ct = default)
    {
        var course = await _db.LmsCourses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == courseId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khóa học không tồn tại.", 404);

        string? programName = null;
        if (course.ProgramId is Guid pid)
            programName = await _db.LmsPrograms.AsNoTracking()
                .Where(x => x.Id == pid && x.TenantId == tenantId && !x.IsDeleted)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(ct);

        var chapters = await _db.LmsChapters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CourseId == courseId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Title)
            .ToListAsync(ct);
        var chapterIds = chapters.Select(c => c.Id).ToList();
        var lessons = chapterIds.Count == 0
            ? new List<LmsLesson>()
            : await _db.LmsLessons.AsNoTracking()
                .Where(x => x.TenantId == tenantId && chapterIds.Contains(x.ChapterId) && !x.IsDeleted)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Title)
                .ToListAsync(ct);

        var lessonCountByChapter = lessons.GroupBy(l => l.ChapterId)
            .ToDictionary(g => g.Key, g => g.Count());

        return new LmsCourseDetailDto(
            MapCourse(course, programName, chapters.Count, lessons.Count),
            chapters.Select(c => new LmsChapterDto(
                c.Id, c.CourseId, c.Title, c.SortOrder, lessonCountByChapter.GetValueOrDefault(c.Id))).ToList(),
            lessons.Select(MapLesson).ToList());
    }

    public async Task<LmsCourseDto> SetPublishStatusAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsPublishCourseRequest req, CancellationToken ct = default)
    {
        var status = (req.Status ?? "").Trim();
        if (!CourseStatuses.Contains(status)) throw new AppException("Trạng thái khóa không hợp lệ.");

        var entity = await _db.LmsCourses.FirstOrDefaultAsync(
            x => x.Id == courseId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khóa học không tồn tại.", 404);

        if (string.Equals(status, "Published", StringComparison.OrdinalIgnoreCase))
        {
            var hasLesson = await (
                from ch in _db.LmsChapters.AsNoTracking()
                join ls in _db.LmsLessons.AsNoTracking() on ch.Id equals ls.ChapterId
                where ch.TenantId == tenantId && ch.CourseId == courseId && !ch.IsDeleted && !ls.IsDeleted
                select ls.Id).AnyAsync(ct);
            if (!hasLesson) throw new AppException("Cần ít nhất 1 bài học trước khi xuất bản.");
        }

        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var detail = await GetCourseDetailAsync(tenantId, courseId, ct);
        return detail.Course;
    }

    public async Task<LmsChapterDto> UpsertChapterAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsChapterUpsertRequest req, CancellationToken ct = default)
    {
        var courseOk = await _db.LmsCourses.AnyAsync(
            x => x.Id == courseId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!courseOk) throw new AppException("Khóa học không tồn tại.", 404);

        var title = (req.Title ?? "").Trim();
        if (title.Length is < 1 or > 300) throw new AppException("Tiêu đề chương 1–300 ký tự.");

        LmsChapter entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsChapters.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.CourseId == courseId && !x.IsDeleted, ct)
                ?? throw new AppException("Chương không tồn tại.", 404);
        }
        else
        {
            var maxOrder = await _db.LmsChapters
                .Where(x => x.TenantId == tenantId && x.CourseId == courseId && !x.IsDeleted)
                .Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? 0;
            entity = new LmsChapter
            {
                TenantId = tenantId,
                CourseId = courseId,
                CreatedBy = userId,
                SortOrder = req.SortOrder ?? maxOrder + 1
            };
            _db.LmsChapters.Add(entity);
        }

        entity.Title = title;
        if (req.SortOrder is int so) entity.SortOrder = so;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var lessonCount = await _db.LmsLessons.CountAsync(
            x => x.TenantId == tenantId && x.ChapterId == entity.Id && !x.IsDeleted, ct);
        return new LmsChapterDto(entity.Id, entity.CourseId, entity.Title, entity.SortOrder, lessonCount);
    }

    public async Task<LmsLessonDto> UpsertLessonAsync(
        Guid tenantId, Guid userId, Guid chapterId, LmsLessonUpsertRequest req, CancellationToken ct = default)
    {
        var chapter = await _db.LmsChapters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == chapterId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Chương không tồn tại.", 404);

        var title = (req.Title ?? "").Trim();
        if (title.Length is < 1 or > 300) throw new AppException("Tiêu đề bài 1–300 ký tự.");
        var type = string.IsNullOrWhiteSpace(req.LessonType) ? "Text" : req.LessonType.Trim();
        if (!LessonTypes.Contains(type)) throw new AppException("Loại bài học không hợp lệ.");
        if ((string.Equals(type, "Video", StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, "Document", StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(req.ContentUrl))
            throw new AppException("Video/Document cần URL nội dung.");

        LmsLesson entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsLessons.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.ChapterId == chapterId && !x.IsDeleted, ct)
                ?? throw new AppException("Bài học không tồn tại.", 404);
        }
        else
        {
            var maxOrder = await _db.LmsLessons
                .Where(x => x.TenantId == tenantId && x.ChapterId == chapterId && !x.IsDeleted)
                .Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? 0;
            entity = new LmsLesson
            {
                TenantId = tenantId,
                ChapterId = chapterId,
                CreatedBy = userId,
                SortOrder = req.SortOrder ?? maxOrder + 1
            };
            _db.LmsLessons.Add(entity);
        }

        entity.Title = title;
        entity.LessonType = type;
        entity.ContentUrl = string.IsNullOrWhiteSpace(req.ContentUrl) ? null : req.ContentUrl.Trim();
        entity.Body = string.IsNullOrWhiteSpace(req.Body) ? null : req.Body.Trim();
        if (req.SortOrder is int so) entity.SortOrder = so;
        entity.DurationSec = req.DurationSec;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        _ = chapter; // keep chapter load for existence check
        return MapLesson(entity);
    }

    public async Task<IReadOnlyList<LmsCatalogCourseDto>> ListCatalogAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var courses = await _db.LmsCourses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Published")
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        if (courses.Count == 0) return Array.Empty<LmsCatalogCourseDto>();

        var ids = courses.Select(c => c.Id).ToList();
        var chapterIds = await _db.LmsChapters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.CourseId) && !x.IsDeleted)
            .Select(x => new { x.Id, x.CourseId })
            .ToListAsync(ct);
        var allChapterIds = chapterIds.Select(c => c.Id).ToList();
        var lessons = allChapterIds.Count == 0
            ? new List<(Guid ChapterId, Guid Id)>()
            : (await _db.LmsLessons.AsNoTracking()
                .Where(x => x.TenantId == tenantId && allChapterIds.Contains(x.ChapterId) && !x.IsDeleted)
                .Select(x => new { x.ChapterId, x.Id })
                .ToListAsync(ct)).Select(x => (x.ChapterId, x.Id)).ToList();

        var lessonCountByCourse = chapterIds
            .GroupBy(c => c.CourseId)
            .ToDictionary(
                g => g.Key,
                g => lessons.Count(l => g.Any(ch => ch.Id == l.ChapterId)));

        var enrollments = await _db.LmsOnlineEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && ids.Contains(x.CourseId) && !x.IsDeleted)
            .ToListAsync(ct);
        var enrollByCourse = enrollments.ToDictionary(x => x.CourseId);

        var progressPct = new Dictionary<Guid, decimal>();
        foreach (var en in enrollments)
        {
            var total = lessonCountByCourse.GetValueOrDefault(en.CourseId);
            if (total == 0) { progressPct[en.CourseId] = 0; continue; }
            var done = await _db.LmsLessonProgresses.CountAsync(
                x => x.TenantId == tenantId && x.EnrollmentId == en.Id && !x.IsDeleted && x.Status == "Completed", ct);
            progressPct[en.CourseId] = Math.Round(100m * done / total, 1);
        }

        return courses.Select(c =>
        {
            enrollByCourse.TryGetValue(c.Id, out var en);
            return new LmsCatalogCourseDto(
                c.Id, c.Code, c.Name, c.Summary, c.DeliveryMode, c.Price, c.Currency, c.CoverUrl,
                lessonCountByCourse.GetValueOrDefault(c.Id),
                en?.Status,
                progressPct.GetValueOrDefault(c.Id));
        }).ToList();
    }

    public async Task<LmsOnlineEnrollmentDto> EnrollAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsEnrollRequest req, CancellationToken ct = default)
    {
        var course = await _db.LmsCourses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == courseId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khóa học không tồn tại.", 404);
        if (!string.Equals(course.Status, "Published", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Khóa chưa xuất bản.");

        var existing = await _db.LmsOnlineEnrollments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId && !x.IsDeleted, ct);
        if (existing is not null && existing.Status is "Unlocked" or "Completed")
            return await MapEnrollmentAsync(tenantId, existing, ct);

        var voucher = (req.VoucherCode ?? "").Trim().ToUpperInvariant();
        var freeByVoucher = voucher is "FREE" or "DEMO100";
        var amount = freeByVoucher || course.Price <= 0 ? 0 : course.Price;

        if (existing is null)
        {
            existing = new LmsOnlineEnrollment
            {
                TenantId = tenantId,
                CourseId = courseId,
                UserId = userId,
                CreatedBy = userId,
                Status = "Pending"
            };
            _db.LmsOnlineEnrollments.Add(existing);
        }

        // Mock thanh toán / voucher → tự mở khóa (UC_LMS_031, 032, 033)
        existing.PaidAmount = amount;
        existing.PaidAt = DateTimeOffset.UtcNow;
        existing.Status = "Unlocked";
        existing.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await MapEnrollmentAsync(tenantId, existing, ct);
    }

    public async Task<LmsLearnCourseDto> GetLearnAsync(
        Guid tenantId, Guid userId, Guid courseId, CancellationToken ct = default)
    {
        var enrollment = await _db.LmsOnlineEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Chưa ghi danh khóa học.", 403);
        if (enrollment.Status is not ("Unlocked" or "Completed"))
            throw new AppException("Khóa chưa được mở — hãy mua / kích hoạt trước.", 403);

        var detail = await GetCourseDetailAsync(tenantId, courseId, ct);
        var progress = await _db.LmsLessonProgresses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EnrollmentId == enrollment.Id && !x.IsDeleted)
            .Select(x => new LmsLessonProgressDto(x.LessonId, x.Status, x.CompletedAt, x.LastPositionSec))
            .ToListAsync(ct);

        var completed = progress.Where(p => p.Status == "Completed").Select(p => p.LessonId).ToHashSet();
        Guid? resume = enrollment.LastLessonId;
        if (resume is null || completed.Contains(resume.Value))
        {
            resume = detail.Lessons
                .OrderBy(l => detail.Chapters.FirstOrDefault(c => c.Id == l.ChapterId)?.SortOrder ?? 0)
                .ThenBy(l => l.SortOrder)
                .Select(l => (Guid?)l.Id)
                .FirstOrDefault(id => id is Guid g && !completed.Contains(g));
        }

        var enDto = await MapEnrollmentAsync(tenantId, enrollment, ct, detail.Lessons.Count);
        return new LmsLearnCourseDto(detail.Course, detail.Chapters, detail.Lessons, enDto, progress, resume);
    }

    public async Task<LmsLessonProgressDto> CompleteLessonAsync(
        Guid tenantId, Guid userId, Guid courseId, Guid lessonId, LmsCompleteLessonRequest req,
        CancellationToken ct = default)
    {
        var enrollment = await _db.LmsOnlineEnrollments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.CourseId == courseId && x.UserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Chưa ghi danh khóa học.", 403);
        if (enrollment.Status is not ("Unlocked" or "Completed"))
            throw new AppException("Khóa chưa được mở.", 403);

        var lessonOk = await (
            from ls in _db.LmsLessons.AsNoTracking()
            join ch in _db.LmsChapters.AsNoTracking() on ls.ChapterId equals ch.Id
            where ls.Id == lessonId && ls.TenantId == tenantId && !ls.IsDeleted
                  && ch.CourseId == courseId && !ch.IsDeleted
            select ls.Id).AnyAsync(ct);
        if (!lessonOk) throw new AppException("Bài học không thuộc khóa này.", 404);

        var progress = await _db.LmsLessonProgresses.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EnrollmentId == enrollment.Id && x.LessonId == lessonId && !x.IsDeleted, ct);
        if (progress is null)
        {
            progress = new LmsLessonProgress
            {
                TenantId = tenantId,
                EnrollmentId = enrollment.Id,
                LessonId = lessonId,
                CreatedBy = userId
            };
            _db.LmsLessonProgresses.Add(progress);
        }

        progress.Status = "Completed";
        progress.CompletedAt = DateTimeOffset.UtcNow;
        progress.LastPositionSec = req.LastPositionSec;
        progress.UpdatedBy = userId;
        enrollment.LastLessonId = lessonId;
        enrollment.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var totalLessons = await (
            from ch in _db.LmsChapters.AsNoTracking()
            join ls in _db.LmsLessons.AsNoTracking() on ch.Id equals ls.ChapterId
            where ch.TenantId == tenantId && ch.CourseId == courseId && !ch.IsDeleted && !ls.IsDeleted
            select ls.Id).CountAsync(ct);
        var done = await _db.LmsLessonProgresses.CountAsync(
            x => x.TenantId == tenantId && x.EnrollmentId == enrollment.Id && !x.IsDeleted && x.Status == "Completed", ct);
        if (totalLessons > 0 && done >= totalLessons && enrollment.Status != "Completed")
        {
            enrollment.Status = "Completed";
            await _db.SaveChangesAsync(ct);
        }

        return new LmsLessonProgressDto(progress.LessonId, progress.Status, progress.CompletedAt, progress.LastPositionSec);
    }

    private async Task<LmsOnlineEnrollmentDto> MapEnrollmentAsync(
        Guid tenantId, LmsOnlineEnrollment en, CancellationToken ct, int? totalLessons = null)
    {
        var total = totalLessons ?? await (
            from ch in _db.LmsChapters.AsNoTracking()
            join ls in _db.LmsLessons.AsNoTracking() on ch.Id equals ls.ChapterId
            where ch.TenantId == tenantId && ch.CourseId == en.CourseId && !ch.IsDeleted && !ls.IsDeleted
            select ls.Id).CountAsync(ct);
        var done = total == 0 ? 0 : await _db.LmsLessonProgresses.CountAsync(
            x => x.TenantId == tenantId && x.EnrollmentId == en.Id && !x.IsDeleted && x.Status == "Completed", ct);
        var pct = total == 0 ? 0 : Math.Round(100m * done / total, 1);
        return new LmsOnlineEnrollmentDto(
            en.Id, en.CourseId, en.UserId, en.Status, en.PaidAmount, en.PaidAt, en.LastLessonId, pct);
    }

    private static LmsCourseDto MapCourse(LmsCourse c, string? programName, int chapters, int lessons) =>
        new(c.Id, c.ProgramId, programName, c.Code, c.Name, c.Summary, c.DeliveryMode, c.Status,
            c.Price, c.Currency, c.CoverUrl, chapters, lessons);

    private static LmsLessonDto MapLesson(LmsLesson l) =>
        new(l.Id, l.ChapterId, l.Title, l.LessonType, l.ContentUrl, l.Body, l.SortOrder, l.DurationSec);
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Domain.Entities.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Lms;

public sealed class LmsClassService : ILmsClassService
{
    private static readonly HashSet<string> ValidClassStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Draft", "Open", "InProgress", "Closed" };

    private readonly AppDbContext _db;

    public LmsClassService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LmsTrainingClassDto>> ListClassesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var classes = await _db.LmsTrainingClasses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);

        if (classes.Count == 0) return Array.Empty<LmsTrainingClassDto>();

        var ids = classes.Select(x => x.Id).ToList();
        var sessionCounts = await _db.LmsClassSessions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ClassId) && !x.IsDeleted)
            .GroupBy(x => x.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, ct);
        var enrollCounts = await _db.LmsClassEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ClassId) && !x.IsDeleted)
            .GroupBy(x => x.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, ct);

        return classes.Select(c => MapClass(c,
            sessionCounts.GetValueOrDefault(c.Id),
            enrollCounts.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<LmsTrainingClassDto> UpsertClassAsync(
        Guid tenantId, Guid userId, LmsTrainingClassUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        var courseTitle = (req.CourseTitle ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã lớp 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên lớp 1–200 ký tự.");
        if (courseTitle.Length is < 1 or > 200) throw new AppException("Tên khóa 1–200 ký tự.");
        if (req.EndDate < req.StartDate) throw new AppException("Ngày kết thúc phải sau ngày bắt đầu.");

        var status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim();
        if (!ValidClassStatuses.Contains(status)) throw new AppException("Trạng thái lớp không hợp lệ.");

        LmsTrainingClass entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsTrainingClasses.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Lớp không tồn tại.", 404);
            if (entity.Status == "Closed") throw new AppException("Lớp đã đóng — không chỉnh sửa.");
        }
        else
        {
            if (await _db.LmsTrainingClasses.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã lớp đã tồn tại.");
            entity = new LmsTrainingClass { TenantId = tenantId, CreatedBy = userId, Status = "Draft" };
            _db.LmsTrainingClasses.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsTrainingClasses.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã lớp đã tồn tại.");

        entity.Code = code;
        entity.Name = name;
        entity.CourseTitle = courseTitle;
        entity.InstructorId = req.InstructorId;
        if (req.InstructorId is Guid iid)
        {
            var instr = await _db.LmsInstructors.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == iid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Giảng viên không tồn tại.", 404);
            entity.InstructorName = instr.DisplayName;
        }
        else
        {
            entity.InstructorName = string.IsNullOrWhiteSpace(req.InstructorName) ? null : req.InstructorName.Trim();
        }
        entity.Location = string.IsNullOrWhiteSpace(req.Location) ? null : req.Location.Trim();
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        if (entity.Status != "Closed") entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var sc = await _db.LmsClassSessions.CountAsync(
            x => x.TenantId == tenantId && x.ClassId == entity.Id && !x.IsDeleted, ct);
        var ec = await _db.LmsClassEnrollments.CountAsync(
            x => x.TenantId == tenantId && x.ClassId == entity.Id && !x.IsDeleted, ct);
        return MapClass(entity, sc, ec);
    }

    public async Task<LmsClassDetailDto> GetClassDetailAsync(
        Guid tenantId, Guid classId, CancellationToken ct = default)
    {
        var cls = await _db.LmsTrainingClasses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == classId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lớp không tồn tại.", 404);

        var sessions = await _db.LmsClassSessions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.SessionDate)
            .ToListAsync(ct);

        var enrollments = await _db.LmsClassEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted)
            .OrderBy(x => x.EnrolledAt)
            .ToListAsync(ct);

        var sessionIds = sessions.Select(x => x.Id).ToList();
        var attendance = sessionIds.Count == 0
            ? new List<LmsSessionAttendance>()
            : await _db.LmsSessionAttendances.AsNoTracking()
                .Where(x => x.TenantId == tenantId && sessionIds.Contains(x.SessionId) && !x.IsDeleted)
                .ToListAsync(ct);

        var empIds = enrollments.Select(x => x.EmployeeId).Distinct().ToList();
        var emps = empIds.Count == 0
            ? new Dictionary<Guid, (string EmployeeCode, string FullName)>()
            : await _db.Employees.AsNoTracking()
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.EmployeeCode, x.FullName), ct);

        var sc = sessions.Count;
        var ec = enrollments.Count;
        return new LmsClassDetailDto(
            MapClass(cls, sc, ec),
            sessions.Select(MapSession).ToList(),
            enrollments.Select(e =>
            {
                emps.TryGetValue(e.EmployeeId, out var emp);
                return MapEnrollment(e, emp.EmployeeCode, emp.FullName);
            }).ToList(),
            attendance.Select(MapAttendance).ToList());
    }

    public async Task<LmsClassSessionDto> AddSessionAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassSessionCreateRequest req, CancellationToken ct = default)
    {
        var cls = await RequireOpenClassAsync(tenantId, classId, ct);
        var topic = (req.Topic ?? "").Trim();
        if (topic.Length is < 1 or > 300) throw new AppException("Chủ đề buổi học 1–300 ký tự.");

        var sortOrder = req.SortOrder ?? await _db.LmsClassSessions
            .Where(x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted)
            .Select(x => (int?)x.SortOrder).MaxAsync(ct) + 1 ?? 1;

        var entity = new LmsClassSession
        {
            TenantId = tenantId,
            ClassId = classId,
            SessionDate = req.SessionDate,
            Topic = topic,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            SortOrder = sortOrder,
            CreatedBy = userId
        };
        _db.LmsClassSessions.Add(entity);

        if (cls.Status == "Open" || cls.Status == "Draft")
        {
            cls.Status = "InProgress";
            cls.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);
        return MapSession(entity);
    }

    public async Task<LmsClassEnrollmentDto> EnrollAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassEnrollmentRequest req, CancellationToken ct = default)
    {
        var cls = await RequireOpenClassAsync(tenantId, classId, ct);
        if (cls.Status == "Closed") throw new AppException("Lớp đã đóng — không ghi danh.");

        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);

        var existing = await _db.LmsClassEnrollments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ClassId == classId && x.EmployeeId == req.EmployeeId && !x.IsDeleted, ct);

        if (existing is null)
        {
            existing = new LmsClassEnrollment
            {
                TenantId = tenantId,
                ClassId = classId,
                EmployeeId = req.EmployeeId,
                Status = "Enrolled",
                EnrolledAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };
            _db.LmsClassEnrollments.Add(existing);
        }
        else if (existing.Status == "Dropped")
        {
            existing.Status = "Enrolled";
            existing.EnrolledAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = userId;
        }
        else
        {
            throw new AppException("Học viên đã ghi danh.");
        }

        if (cls.Status == "Draft") cls.Status = "Open";
        cls.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapEnrollment(existing, emp.EmployeeCode, emp.FullName);
    }

    public async Task<LmsTrainingClassDto> CloseClassAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassCloseRequest req, CancellationToken ct = default)
    {
        var cls = await _db.LmsTrainingClasses.FirstOrDefaultAsync(
            x => x.Id == classId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lớp không tồn tại.", 404);

        if (cls.Status == "Closed") throw new AppException("Lớp đã đóng.");

        cls.Status = "Closed";
        cls.SummaryNote = string.IsNullOrWhiteSpace(req.SummaryNote) ? null : req.SummaryNote.Trim();
        cls.UpdatedBy = userId;

        var enrollments = await _db.LmsClassEnrollments
            .Where(x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted && x.Status == "Enrolled")
            .ToListAsync(ct);
        foreach (var e in enrollments)
        {
            e.Status = "Completed";
            e.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);

        var sc = await _db.LmsClassSessions.CountAsync(
            x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted, ct);
        var ec = await _db.LmsClassEnrollments.CountAsync(
            x => x.TenantId == tenantId && x.ClassId == classId && !x.IsDeleted, ct);
        return MapClass(cls, sc, ec);
    }

    public async Task<LmsSessionAttendanceDto> RecordAttendanceAsync(
        Guid tenantId, Guid userId, Guid sessionId, LmsSessionAttendanceRequest req, CancellationToken ct = default)
    {
        var session = await _db.LmsClassSessions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Buổi học không tồn tại.", 404);

        await RequireOpenClassAsync(tenantId, session.ClassId, ct, allowClosed: false);

        var enrollment = await _db.LmsClassEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EnrollmentId && x.TenantId == tenantId
                                      && x.ClassId == session.ClassId && !x.IsDeleted, ct)
            ?? throw new AppException("Ghi danh không hợp lệ.", 404);

        var entity = await _db.LmsSessionAttendances.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SessionId == sessionId
                 && x.EnrollmentId == req.EnrollmentId && !x.IsDeleted, ct);

        if (entity is null)
        {
            entity = new LmsSessionAttendance
            {
                TenantId = tenantId,
                SessionId = sessionId,
                EnrollmentId = req.EnrollmentId,
                CreatedBy = userId
            };
            _db.LmsSessionAttendances.Add(entity);
        }

        entity.Present = req.Present;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapAttendance(entity);
    }

    public async Task<IReadOnlyList<LmsMentorAssignmentDto>> ListMentorsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.LmsMentorAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        if (rows.Count == 0) return Array.Empty<LmsMentorAssignmentDto>();

        var empIds = rows.SelectMany(x => new[] { x.MenteeEmployeeId, x.MentorEmployeeId }).Distinct().ToList();
        var emps = await _db.Employees.AsNoTracking()
            .Where(x => empIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => (x.EmployeeCode, x.FullName), ct);

        return rows.Select(r =>
        {
            emps.TryGetValue(r.MenteeEmployeeId, out var mentee);
            emps.TryGetValue(r.MentorEmployeeId, out var mentor);
            return new LmsMentorAssignmentDto(
                r.Id, r.MenteeEmployeeId, mentee.EmployeeCode, mentee.FullName,
                r.MentorEmployeeId, mentor.EmployeeCode, mentor.FullName, r.Note, r.IsActive);
        }).ToList();
    }

    public async Task<LmsMentorAssignmentDto> AssignMentorAsync(
        Guid tenantId, Guid userId, LmsMentorAssignRequest req, CancellationToken ct = default)
    {
        if (req.MenteeEmployeeId == req.MentorEmployeeId)
            throw new AppException("Mentor và mentee phải khác nhau.");

        var mentee = await RequireEmployeeAsync(tenantId, req.MenteeEmployeeId, ct);
        var mentor = await RequireEmployeeAsync(tenantId, req.MentorEmployeeId, ct);

        var existing = await _db.LmsMentorAssignments
            .Where(x => x.TenantId == tenantId && x.MenteeEmployeeId == req.MenteeEmployeeId
                        && x.MentorEmployeeId == req.MentorEmployeeId && !x.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            existing = new LmsMentorAssignment
            {
                TenantId = tenantId,
                MenteeEmployeeId = req.MenteeEmployeeId,
                MentorEmployeeId = req.MentorEmployeeId,
                IsActive = true,
                CreatedBy = userId
            };
            _db.LmsMentorAssignments.Add(existing);
        }
        else
        {
            existing.IsActive = true;
            existing.UpdatedBy = userId;
        }

        existing.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        await _db.SaveChangesAsync(ct);

        return new LmsMentorAssignmentDto(
            existing.Id, existing.MenteeEmployeeId, mentee.EmployeeCode, mentee.FullName,
            existing.MentorEmployeeId, mentor.EmployeeCode, mentor.FullName,
            existing.Note, existing.IsActive);
    }

    private async Task<LmsTrainingClass> RequireOpenClassAsync(
        Guid tenantId, Guid classId, CancellationToken ct, bool allowClosed = false)
    {
        var cls = await _db.LmsTrainingClasses.FirstOrDefaultAsync(
            x => x.Id == classId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lớp không tồn tại.", 404);

        if (!allowClosed && cls.Status == "Closed")
            throw new AppException("Lớp đã đóng.");
        return cls;
    }

    private async Task<(string EmployeeCode, string FullName)> RequireEmployeeAsync(
        Guid tenantId, Guid employeeId, CancellationToken ct)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        return (emp.EmployeeCode, emp.FullName);
    }

    private static LmsTrainingClassDto MapClass(LmsTrainingClass x, int sessionCount, int enrollmentCount) =>
        new(x.Id, x.Code, x.Name, x.CourseTitle, x.InstructorId, x.InstructorName, x.Location,
            x.StartDate, x.EndDate, x.Status, x.SummaryNote, sessionCount, enrollmentCount);

    private static LmsClassSessionDto MapSession(LmsClassSession x) =>
        new(x.Id, x.ClassId, x.SessionDate, x.Topic, x.StartTime, x.EndTime, x.SortOrder);

    private static LmsClassEnrollmentDto MapEnrollment(LmsClassEnrollment x, string code, string name) =>
        new(x.Id, x.ClassId, x.EmployeeId, code, name, x.Status, x.EnrolledAt);

    private static LmsSessionAttendanceDto MapAttendance(LmsSessionAttendance x) =>
        new(x.Id, x.SessionId, x.EnrollmentId, x.Present, x.Note);
}

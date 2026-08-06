using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Lms;

public sealed class LmsReportService : ILmsReportService
{
    private readonly AppDbContext _db;
    public LmsReportService(AppDbContext db) => _db = db;

    public async Task<LmsDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var courseCount = await _db.LmsCourses.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        var published = await _db.LmsCourses.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Published", ct);
        var openCls = await _db.LmsTrainingClasses.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Open" || x.Status == "InProgress"), ct);
        var closedCls = await _db.LmsTrainingClasses.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Closed", ct);
        var offTotal = await _db.LmsClassEnrollments.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted, ct);
        var offDone = await _db.LmsClassEnrollments.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Completed", ct);
        var onTotal = await _db.LmsOnlineEnrollments.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted, ct);
        var onDone = await _db.LmsOnlineEnrollments.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Completed", ct);
        var certs = await _db.LmsCertificates.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);
        var instructors = await _db.LmsInstructors.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);

        var enrollIds = await _db.LmsOnlineEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.CourseId }).Take(2000).ToListAsync(ct);
        decimal avgProgress = 0;
        if (enrollIds.Count > 0)
        {
            var cids = enrollIds.Select(x => x.CourseId).Distinct().ToList();
            var lessonCounts = await (
                from l in _db.LmsLessons.AsNoTracking()
                join ch in _db.LmsChapters.AsNoTracking() on l.ChapterId equals ch.Id
                where cids.Contains(ch.CourseId) && !l.IsDeleted && !ch.IsDeleted
                group l by ch.CourseId into g
                select new { CourseId = g.Key, C = g.Count() }
            ).ToDictionaryAsync(x => x.CourseId, x => x.C, ct);
            var eids = enrollIds.Select(x => x.Id).ToList();
            var doneProg = await _db.LmsLessonProgresses.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && eids.Contains(x.EnrollmentId)
                            && x.Status == "Completed")
                .GroupBy(x => x.EnrollmentId)
                .Select(g => new { g.Key, C = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.C, ct);
            var sum = 0m;
            foreach (var e in enrollIds)
            {
                var total = lessonCounts.GetValueOrDefault(e.CourseId);
                if (total <= 0) continue;
                sum += Math.Min(100m, 100m * doneProg.GetValueOrDefault(e.Id) / total);
            }
            avgProgress = decimal.Round(sum / enrollIds.Count, 1);
        }

        var submitted = await _db.LmsExamAttempts.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Submitted", ct);
        var passed = await _db.LmsExamAttempts.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Submitted" && x.Passed, ct);
        var passRate = submitted == 0 ? 0 : decimal.Round(100m * passed / submitted, 1);

        return new LmsDashboardDto(
            courseCount, published, openCls, closedCls,
            offTotal, offDone, onTotal, onDone, certs, instructors,
            avgProgress, passRate);
    }

    public async Task<IReadOnlyList<LmsCompletionByOrgRowDto>> CompletionByOrgAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var orgs = await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);
        var employees = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.UserId, x.OrgUnitId })
            .ToListAsync(ct);
        var empById = employees.ToDictionary(x => x.Id);
        var empByUser = employees.Where(x => x.UserId.HasValue)
            .GroupBy(x => x.UserId!.Value).ToDictionary(g => g.Key, g => g.First());

        var off = await _db.LmsClassEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var on = await _db.LmsOnlineEnrollments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);

        var bag = new Dictionary<Guid?, (int ot, int oc, int nt, int nc)>();
        void Acc(Guid? orgId, bool online, bool completed)
        {
            bag.TryGetValue(orgId, out var t);
            if (online) { t.nt++; if (completed) t.nc++; }
            else { t.ot++; if (completed) t.oc++; }
            bag[orgId] = t;
        }

        foreach (var e in off)
        {
            empById.TryGetValue(e.EmployeeId, out var emp);
            Acc(emp?.OrgUnitId, false, e.Status == "Completed");
        }
        foreach (var e in on)
        {
            empByUser.TryGetValue(e.UserId, out var emp);
            Acc(emp?.OrgUnitId, true, e.Status == "Completed");
        }

        return bag.Select(kv =>
        {
            orgs.TryGetValue(kv.Key ?? Guid.Empty, out var org);
            var total = kv.Value.ot + kv.Value.nt;
            var done = kv.Value.oc + kv.Value.nc;
            var rate = total == 0 ? 0 : decimal.Round(100m * done / total, 1);
            return new LmsCompletionByOrgRowDto(
                kv.Key,
                org?.Code ?? (kv.Key is null ? "—" : "?"),
                org?.Name ?? (kv.Key is null ? "(Không gắn đơn vị)" : "—"),
                kv.Value.ot, kv.Value.oc, kv.Value.nt, kv.Value.nc, rate);
        }).OrderByDescending(x => x.CompletionRatePercent).ThenBy(x => x.OrgUnitName).ToList();
    }

    public async Task<IReadOnlyList<LmsLearnerRowDto>> LearnersAsync(
        Guid tenantId, Guid? classId = null, Guid? courseId = null, Guid? instructorId = null,
        CancellationToken ct = default)
    {
        var rows = new List<LmsLearnerRowDto>();

        var classesQ = _db.LmsTrainingClasses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (classId is Guid cid) classesQ = classesQ.Where(x => x.Id == cid);
        if (instructorId is Guid iid) classesQ = classesQ.Where(x => x.InstructorId == iid);
        var classes = await classesQ.ToListAsync(ct);
        var classIds = classes.Select(x => x.Id).ToList();
        var classMap = classes.ToDictionary(x => x.Id);

        if (classIds.Count > 0 && courseId is null)
        {
            var enrolls = await _db.LmsClassEnrollments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && classIds.Contains(x.ClassId))
                .OrderByDescending(x => x.EnrolledAt).Take(500).ToListAsync(ct);
            var eids = enrolls.Select(x => x.EmployeeId).Distinct().ToList();
            var emps = await _db.Employees.AsNoTracking().Where(x => eids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
            var ouids = emps.Values.Select(x => x.OrgUnitId).Distinct().ToList();
            var orgs = await _db.OrgUnits.AsNoTracking().Where(x => ouids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
            var sessionCounts = await _db.LmsClassSessions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && classIds.Contains(x.ClassId))
                .GroupBy(x => x.ClassId)
                .Select(g => new { g.Key, C = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.C, ct);
            var enrollIds = enrolls.Select(x => x.Id).ToList();
            var present = await _db.LmsSessionAttendances.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && enrollIds.Contains(x.EnrollmentId) && x.Present)
                .GroupBy(x => x.EnrollmentId)
                .Select(g => new { g.Key, C = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.C, ct);

            foreach (var e in enrolls)
            {
                classMap.TryGetValue(e.ClassId, out var cls);
                emps.TryGetValue(e.EmployeeId, out var emp);
                var totalS = sessionCounts.GetValueOrDefault(e.ClassId);
                var presentS = present.GetValueOrDefault(e.Id);
                var pct = totalS <= 0 ? (e.Status == "Completed" ? 100m : 0m)
                    : decimal.Round(100m * presentS / totalS, 1);
                rows.Add(new LmsLearnerRowDto(
                    "Offline", e.ClassId, cls?.Code, cls?.Name,
                    null, null, cls?.CourseTitle,
                    e.EmployeeId, emp?.UserId,
                    emp?.EmployeeCode ?? "", emp?.FullName ?? "",
                    emp is null ? null : orgs.GetValueOrDefault(emp.OrgUnitId),
                    e.Status, e.EnrolledAt, pct, presentS, totalS));
            }
        }

        if (classId is null)
        {
            var onQ = _db.LmsOnlineEnrollments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted);
            if (courseId is Guid crid) onQ = onQ.Where(x => x.CourseId == crid);
            var onEnrolls = await onQ.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync(ct);
            if (onEnrolls.Count > 0)
            {
                var courseIds = onEnrolls.Select(x => x.CourseId).Distinct().ToList();
                var courses = await _db.LmsCourses.AsNoTracking().Where(x => courseIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, ct);
                var lessonCounts = await (
                    from l in _db.LmsLessons.AsNoTracking()
                    join ch in _db.LmsChapters.AsNoTracking() on l.ChapterId equals ch.Id
                    where courseIds.Contains(ch.CourseId) && !l.IsDeleted && !ch.IsDeleted
                    group l by ch.CourseId into g
                    select new { CourseId = g.Key, C = g.Count() }
                ).ToDictionaryAsync(x => x.CourseId, x => x.C, ct);
                var oeids = onEnrolls.Select(x => x.Id).ToList();
                var doneProg = await _db.LmsLessonProgresses.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && oeids.Contains(x.EnrollmentId)
                                && x.Status == "Completed")
                    .GroupBy(x => x.EnrollmentId)
                    .Select(g => new { g.Key, C = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.C, ct);
                var uids = onEnrolls.Select(x => x.UserId).Distinct().ToList();
                var users = await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, ct);
                var emps = await _db.Employees.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.UserId != null && uids.Contains(x.UserId.Value))
                    .ToListAsync(ct);
                var empByUser = emps.GroupBy(x => x.UserId!.Value).ToDictionary(g => g.Key, g => g.First());
                var ouids = emps.Select(x => x.OrgUnitId).Distinct().ToList();
                var orgs = await _db.OrgUnits.AsNoTracking().Where(x => ouids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

                foreach (var e in onEnrolls)
                {
                    courses.TryGetValue(e.CourseId, out var course);
                    users.TryGetValue(e.UserId, out var user);
                    empByUser.TryGetValue(e.UserId, out var emp);
                    var totalL = lessonCounts.GetValueOrDefault(e.CourseId);
                    var doneL = doneProg.GetValueOrDefault(e.Id);
                    var pct = e.Status == "Completed" ? 100m
                        : totalL <= 0 ? 0m : decimal.Round(100m * doneL / totalL, 1);
                    rows.Add(new LmsLearnerRowDto(
                        "Online", null, null, null,
                        e.CourseId, course?.Code, course?.Name,
                        emp?.Id, e.UserId,
                        emp?.EmployeeCode ?? user?.Username ?? "",
                        emp?.FullName ?? user?.DisplayName ?? user?.Username ?? "",
                        emp is null ? null : orgs.GetValueOrDefault(emp.OrgUnitId),
                        e.Status, e.CreatedAt, pct, null, null));
                }
            }
        }

        return rows;
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, Guid? classId = null, Guid? courseId = null,
        Guid? instructorId = null, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "dashboard" or "065")
        {
            var d = await DashboardAsync(tenantId, ct);
            sb.AppendLine("CourseCount,PublishedCourses,OpenClasses,ClosedClasses,OfflineEnroll,OfflineCompleted,OnlineEnroll,OnlineCompleted,Certificates,Instructors,AvgOnlineProgress,ExamPassRate");
            sb.AppendLine($"{d.CourseCount},{d.PublishedCourseCount},{d.OpenClassCount},{d.ClosedClassCount},{d.OfflineEnrollmentCount},{d.OfflineCompletedCount},{d.OnlineEnrollmentCount},{d.OnlineCompletedCount},{d.ActiveCertificateCount},{d.InstructorCount},{N(d.AvgOnlineProgressPercent)},{N(d.ExamPassRatePercent)}");
            return sb.ToString();
        }
        if (kind is "by-org" or "066")
        {
            var rows = await CompletionByOrgAsync(tenantId, ct);
            sb.AppendLine("OrgCode,OrgName,OfflineTotal,OfflineCompleted,OnlineTotal,OnlineCompleted,CompletionRate");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.OrgUnitCode)},{Csv(r.OrgUnitName)},{r.OfflineTotal},{r.OfflineCompleted},{r.OnlineTotal},{r.OnlineCompleted},{N(r.CompletionRatePercent)}");
            return sb.ToString();
        }
        if (kind is "learners" or "051" or "roster")
        {
            var rows = await LearnersAsync(tenantId, classId, courseId, instructorId, ct);
            sb.AppendLine("Source,Class,Course,LearnerCode,LearnerName,Org,Status,EnrolledAt,ProgressPercent,PresentSessions,TotalSessions");
            foreach (var r in rows)
                sb.AppendLine($"{Csv(r.Source)},{Csv(r.ClassCode)},{Csv(r.CourseName)},{Csv(r.LearnerCode)},{Csv(r.LearnerName)},{Csv(r.OrgUnitName)},{Csv(r.Status)},{r.EnrolledAt:yyyy-MM-dd},{N(r.ProgressPercent)},{r.PresentSessions},{r.TotalSessions}");
            return sb.ToString();
        }
        throw new AppException("report: dashboard | by-org | learners.");
    }

    private static string Csv(string? s)
    {
        var v = s ?? "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static string N(decimal n) => n.ToString(CultureInfo.InvariantCulture);
}

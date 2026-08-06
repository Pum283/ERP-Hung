using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmShiftService : IHrmShiftService
{
    private readonly AppDbContext _db;

    public HrmShiftService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<WorkShiftDto>> ListTemplatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.WorkShifts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return rows.Select(MapTemplate).ToList();
    }

    public async Task<WorkShiftDto> UpsertTemplateAsync(
        Guid tenantId, Guid userId, WorkShiftUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã ca 1–40 ký tự.");
        if (name.Length is < 1 or > 100) throw new AppException("Tên ca 1–100 ký tự.");
        if (req.BreakMinutes is < 0 or > 600) throw new AppException("Giờ nghỉ không hợp lệ.");

        var overnight = req.IsOvernight ?? (req.EndTime <= req.StartTime);

        WorkShift entity;
        if (req.Id is Guid id)
        {
            entity = await _db.WorkShifts.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Mẫu ca không tồn tại.", 404);
        }
        else
        {
            if (await _db.WorkShifts.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã ca đã tồn tại.");
            entity = new WorkShift { TenantId = tenantId, CreatedBy = userId };
            _db.WorkShifts.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.WorkShifts.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã ca đã tồn tại.");

        entity.Code = code;
        entity.Name = name;
        entity.StartTime = req.StartTime;
        entity.EndTime = req.EndTime;
        entity.BreakMinutes = req.BreakMinutes;
        entity.IsOvernight = overnight;
        entity.IsActive = req.IsActive;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapTemplate(entity);
    }

    public async Task<IReadOnlyList<ShiftAssignmentDto>> ListAssignmentsAsync(
        Guid tenantId, Guid? orgUnitId, Guid? employeeId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var q = from a in _db.ShiftAssignments.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on a.EmployeeId equals e.Id
                where a.TenantId == tenantId && !a.IsDeleted && !e.IsDeleted
                select new { a, e };

        if (orgUnitId is Guid ou) q = q.Where(x => x.e.OrgUnitId == ou);
        if (employeeId is Guid emp) q = q.Where(x => x.a.EmployeeId == emp);
        if (from is DateOnly f) q = q.Where(x => x.a.WorkDate >= f);
        if (to is DateOnly t) q = q.Where(x => x.a.WorkDate <= t);

        var rows = await q.OrderBy(x => x.a.WorkDate).ThenBy(x => x.e.EmployeeCode).ToListAsync(ct);
        return await MapAssignmentsAsync(rows.Select(x => x.a).ToList(), ct);
    }

    public async Task<IReadOnlyList<ShiftAssignmentDto>> MyAssignmentsAsync(
        Guid tenantId, Guid userId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy hồ sơ nhân viên gắn tài khoản.", 404);
        return await ListAssignmentsAsync(tenantId, null, emp.Id, from, to, ct);
    }

    public async Task<ShiftAssignmentDto> AssignAsync(
        Guid tenantId, Guid userId, ShiftAssignRequest req, CancellationToken ct = default)
    {
        var emp = await RequireEmployeeAsync(tenantId, req.EmployeeId, ct);
        await EnsureShiftActiveAsync(tenantId, req.WorkShiftId, ct);
        await EnsureNotLockedAsync(tenantId, emp.OrgUnitId, req.WorkDate, ct);

        var existing = await _db.ShiftAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == req.EmployeeId
                 && x.WorkDate == req.WorkDate && !x.IsDeleted, ct);

        if (existing is null)
        {
            existing = new ShiftAssignment
            {
                TenantId = tenantId,
                EmployeeId = req.EmployeeId,
                WorkDate = req.WorkDate,
                CreatedBy = userId,
                Status = "Scheduled"
            };
            _db.ShiftAssignments.Add(existing);
        }
        else if (existing.Status == "Cancelled")
        {
            existing.Status = "Scheduled";
        }

        existing.WorkShiftId = req.WorkShiftId;
        existing.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        existing.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAssignmentsAsync(new[] { existing }, ct))[0];
    }

    public async Task<IReadOnlyList<ShiftAssignmentDto>> AssignRangeAsync(
        Guid tenantId, Guid userId, ShiftAssignRangeRequest req, CancellationToken ct = default)
    {
        if (req.EmployeeIds is null || req.EmployeeIds.Count == 0)
            throw new AppException("Cần ít nhất một nhân viên.");
        if (req.To < req.From) throw new AppException("Khoảng ngày không hợp lệ.");
        if (req.To.DayNumber - req.From.DayNumber > 62)
            throw new AppException("Chỉ xếp tối đa 62 ngày một lần.");

        await EnsureShiftActiveAsync(tenantId, req.WorkShiftId, ct);
        var weekdays = req.Weekdays is { Count: > 0 }
            ? req.Weekdays.ToHashSet()
            : null;

        var created = new List<ShiftAssignment>();
        foreach (var empId in req.EmployeeIds.Distinct())
        {
            var emp = await RequireEmployeeAsync(tenantId, empId, ct);
            for (var d = req.From; d <= req.To; d = d.AddDays(1))
            {
                if (weekdays is not null && !weekdays.Contains((int)d.DayOfWeek))
                    continue;
                await EnsureNotLockedAsync(tenantId, emp.OrgUnitId, d, ct);

                var existing = await _db.ShiftAssignments.FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.EmployeeId == empId
                         && x.WorkDate == d && !x.IsDeleted, ct);
                if (existing is null)
                {
                    existing = new ShiftAssignment
                    {
                        TenantId = tenantId,
                        EmployeeId = empId,
                        WorkDate = d,
                        CreatedBy = userId,
                        Status = "Scheduled"
                    };
                    _db.ShiftAssignments.Add(existing);
                }
                else
                {
                    existing.Status = "Scheduled";
                }

                existing.WorkShiftId = req.WorkShiftId;
                existing.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
                existing.UpdatedBy = userId;
                created.Add(existing);
            }
        }

        await _db.SaveChangesAsync(ct);
        return await MapAssignmentsAsync(created, ct);
    }

    public async Task SwapAsync(Guid tenantId, Guid userId, ShiftSwapRequest req, CancellationToken ct = default)
    {
        var a = await _db.ShiftAssignments.FirstOrDefaultAsync(
            x => x.Id == req.AssignmentAId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lịch ca A không tồn tại.", 404);
        var b = await _db.ShiftAssignments.FirstOrDefaultAsync(
            x => x.Id == req.AssignmentBId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lịch ca B không tồn tại.", 404);

        if (a.Status != "Scheduled" || b.Status != "Scheduled")
            throw new AppException("Chỉ đổi ca khi cả hai đang Scheduled.");
        if (a.WorkDate != b.WorkDate)
            throw new AppException("Chỉ đổi ca cùng ngày.");

        var empA = await RequireEmployeeAsync(tenantId, a.EmployeeId, ct);
        var empB = await RequireEmployeeAsync(tenantId, b.EmployeeId, ct);
        await EnsureNotLockedAsync(tenantId, empA.OrgUnitId, a.WorkDate, ct);
        await EnsureNotLockedAsync(tenantId, empB.OrgUnitId, b.WorkDate, ct);

        (a.WorkShiftId, b.WorkShiftId) = (b.WorkShiftId, a.WorkShiftId);
        a.UpdatedBy = userId;
        b.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(Guid tenantId, Guid userId, Guid assignmentId, CancellationToken ct = default)
    {
        var a = await _db.ShiftAssignments.FirstOrDefaultAsync(
            x => x.Id == assignmentId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Lịch ca không tồn tại.", 404);
        var emp = await RequireEmployeeAsync(tenantId, a.EmployeeId, ct);
        await EnsureNotLockedAsync(tenantId, emp.OrgUnitId, a.WorkDate, ct);
        a.Status = "Cancelled";
        a.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> CopyAsync(Guid tenantId, Guid userId, ShiftCopyRequest req, CancellationToken ct = default)
    {
        if (req.SourceTo < req.SourceFrom) throw new AppException("Khoảng nguồn không hợp lệ.");
        var span = req.SourceTo.DayNumber - req.SourceFrom.DayNumber;
        if (span > 62) throw new AppException("Chỉ sao chép tối đa 62 ngày.");

        var q = _db.ShiftAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.Status == "Scheduled"
                        && x.WorkDate >= req.SourceFrom && x.WorkDate <= req.SourceTo);

        if (req.OrgUnitId is Guid ou)
        {
            var empIds = await _db.Employees.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.OrgUnitId == ou && !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync(ct);
            q = q.Where(x => empIds.Contains(x.EmployeeId));
        }

        var source = await q.ToListAsync(ct);
        var count = 0;
        foreach (var s in source)
        {
            var targetDate = req.TargetStart.AddDays(s.WorkDate.DayNumber - req.SourceFrom.DayNumber);
            var emp = await RequireEmployeeAsync(tenantId, s.EmployeeId, ct);
            await EnsureNotLockedAsync(tenantId, emp.OrgUnitId, targetDate, ct);

            var existing = await _db.ShiftAssignments.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EmployeeId == s.EmployeeId
                     && x.WorkDate == targetDate && !x.IsDeleted, ct);
            if (existing is null)
            {
                existing = new ShiftAssignment
                {
                    TenantId = tenantId,
                    EmployeeId = s.EmployeeId,
                    WorkDate = targetDate,
                    CreatedBy = userId,
                    Status = "Scheduled"
                };
                _db.ShiftAssignments.Add(existing);
            }
            else
            {
                existing.Status = "Scheduled";
            }

            existing.WorkShiftId = s.WorkShiftId;
            existing.Note = s.Note;
            existing.UpdatedBy = userId;
            count++;
        }

        await _db.SaveChangesAsync(ct);
        return count;
    }

    public async Task<IReadOnlyList<ShiftPeriodLockDto>> ListLocksAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.ShiftPeriodLocks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.PeriodKey)
            .ToListAsync(ct);

        var ouIds = rows.Select(x => x.OrgUnitId).Distinct().ToList();
        var userIds = rows.Select(x => x.LockedByUserId).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking()
            .Where(x => ouIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = await _db.Users.AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return rows.Select(x => new ShiftPeriodLockDto(
            x.Id, x.OrgUnitId, ous.GetValueOrDefault(x.OrgUnitId, "?"), x.PeriodKey,
            x.PeriodFrom, x.PeriodTo, x.LockedByUserId, users.GetValueOrDefault(x.LockedByUserId, "?"),
            x.LockedAt, x.Note)).ToList();
    }

    public async Task<ShiftPeriodLockDto> LockPeriodAsync(
        Guid tenantId, Guid userId, ShiftLockRequest req, CancellationToken ct = default)
    {
        var key = (req.PeriodKey ?? "").Trim();
        if (!DateOnly.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var from))
            throw new AppException("PeriodKey phải dạng yyyy-MM.");

        var to = from.AddMonths(1).AddDays(-1);
        if (!await _db.OrgUnits.AnyAsync(x => x.Id == req.OrgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Đơn vị không hợp lệ.", 404);

        var entity = await _db.ShiftPeriodLocks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.OrgUnitId == req.OrgUnitId
                 && x.PeriodKey == key && !x.IsDeleted, ct);
        if (entity is null)
        {
            entity = new ShiftPeriodLock
            {
                TenantId = tenantId,
                OrgUnitId = req.OrgUnitId,
                PeriodKey = key,
                CreatedBy = userId
            };
            _db.ShiftPeriodLocks.Add(entity);
        }

        entity.PeriodFrom = from;
        entity.PeriodTo = to;
        entity.LockedByUserId = userId;
        entity.LockedAt = DateTimeOffset.UtcNow;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var locks = await ListLocksAsync(tenantId, ct);
        return locks.First(x => x.Id == entity.Id);
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var rows = await ListAssignmentsAsync(tenantId, orgUnitId, null, from, to, ct);
        var sb = new StringBuilder();
        sb.AppendLine("WorkDate,EmployeeCode,EmployeeName,OrgUnit,ShiftCode,ShiftName,Start,End,Status");
        foreach (var r in rows.Where(x => x.Status == "Scheduled"))
        {
            sb.Append(r.WorkDate.ToString("yyyy-MM-dd")).Append(',')
                .Append(Csv(r.EmployeeCode)).Append(',')
                .Append(Csv(r.EmployeeName)).Append(',')
                .Append(Csv(r.OrgUnitName)).Append(',')
                .Append(Csv(r.ShiftCode)).Append(',')
                .Append(Csv(r.ShiftName)).Append(',')
                .Append(r.StartTime.ToString("HH:mm")).Append(',')
                .Append(r.EndTime.ToString("HH:mm")).Append(',')
                .Append(r.Status).AppendLine();
        }
        return sb.ToString();
    }

    private static string Csv(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

    private static WorkShiftDto MapTemplate(WorkShift x) => new(
        x.Id, x.Code, x.Name, x.StartTime, x.EndTime, x.BreakMinutes, x.IsOvernight, x.IsActive, x.Note);

    private async Task<IReadOnlyList<ShiftAssignmentDto>> MapAssignmentsAsync(
        IReadOnlyList<ShiftAssignment> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<ShiftAssignmentDto>();
        var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
        var shiftIds = rows.Select(x => x.WorkShiftId).Distinct().ToList();
        var emps = await _db.Employees.AsNoTracking()
            .Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var shifts = await _db.WorkShifts.AsNoTracking()
            .Where(x => shiftIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var ouIds = emps.Values.Select(x => x.OrgUnitId).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking()
            .Where(x => ouIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.Select(a =>
        {
            emps.TryGetValue(a.EmployeeId, out var e);
            shifts.TryGetValue(a.WorkShiftId, out var s);
            var ouId = e?.OrgUnitId ?? Guid.Empty;
            return new ShiftAssignmentDto(
                a.Id, a.EmployeeId, e?.EmployeeCode ?? "?", e?.FullName ?? "?",
                ouId, ous.GetValueOrDefault(ouId, "?"),
                a.WorkShiftId, s?.Code ?? "?", s?.Name ?? "?",
                s?.StartTime ?? default, s?.EndTime ?? default,
                a.WorkDate, a.Status, a.Note);
        }).ToList();
    }

    private async Task<Employee> RequireEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct)
    {
        return await _db.Employees.FirstOrDefaultAsync(
                   x => x.Id == employeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
               ?? throw new AppException("Nhân viên không tồn tại.", 404);
    }

    private async Task EnsureShiftActiveAsync(Guid tenantId, Guid shiftId, CancellationToken ct)
    {
        var ok = await _db.WorkShifts.AnyAsync(
            x => x.Id == shiftId && x.TenantId == tenantId && x.IsActive && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Mẫu ca không hợp lệ hoặc đã ngưng.", 404);
    }

    private async Task EnsureNotLockedAsync(Guid tenantId, Guid orgUnitId, DateOnly date, CancellationToken ct)
    {
        var locked = await _db.ShiftPeriodLocks.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && x.OrgUnitId == orgUnitId && !x.IsDeleted
                 && x.PeriodFrom <= date && x.PeriodTo >= date, ct);
        if (locked) throw new AppException("Kỳ lịch ca đã khóa sổ — không chỉnh sửa.");
    }
}

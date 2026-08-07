using System.Globalization;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmAttendanceService : IHrmAttendanceService
{
    private readonly AppDbContext _db;

    public HrmAttendanceService(AppDbContext db) => _db = db;

    public async Task<AttendancePolicyDto> GetPolicyAsync(Guid tenantId, CancellationToken ct = default)
        => MapPolicy(await EnsurePolicyAsync(tenantId, ct));

    public async Task<AttendancePolicyDto> UpsertPolicyAsync(
        Guid tenantId, Guid userId, AttendancePolicyUpsertRequest req, CancellationToken ct = default)
    {
        var p = await EnsurePolicyAsync(tenantId, ct);
        if (req.LateGraceMinutes is < 0 or > 240) throw new AppException("Ân hạn trễ không hợp lệ.");
        if (req.LateDeductEveryMinutes is < 1 or > 480) throw new AppException("Bậc trừ công không hợp lệ.");
        if (req.LateDeductWorkUnit is < 0 or > 1) throw new AppException("Mức trừ công 0–1.");
        if (req.ForgotCheckoutHours is < 1 or > 48) throw new AppException("Giờ quên checkout không hợp lệ.");
        if (req.AdjustDeadlineDays is < 0 or > 60) throw new AppException("Hạn điều chỉnh không hợp lệ.");

        p.EnableFingerprint = req.EnableFingerprint;
        p.EnableApp = req.EnableApp;
        p.EnableQr = req.EnableQr;
        p.EnableGeoFence = req.EnableGeoFence;
        p.LateGraceMinutes = req.LateGraceMinutes;
        p.LateDeductEveryMinutes = req.LateDeductEveryMinutes;
        p.LateDeductWorkUnit = req.LateDeductWorkUnit;
        p.ForgotCheckoutHours = req.ForgotCheckoutHours;
        p.AdjustDeadlineDays = req.AdjustDeadlineDays;
        p.EnableOt = req.EnableOt;
        p.OtAfterMinutes = Math.Clamp(req.OtAfterMinutes, 0, 480);
        p.EnableNightShiftRule = req.EnableNightShiftRule;
        p.EnableHolidayRule = req.EnableHolidayRule;
        p.DefaultShiftStart = req.DefaultShiftStart;
        p.DefaultShiftEnd = req.DefaultShiftEnd;
        p.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapPolicy(p);
    }

    public async Task<IReadOnlyList<AttendanceDeviceDto>> ListDevicesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.AttendanceDevices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        var ouIds = rows.Where(x => x.OrgUnitId is not null).Select(x => x.OrgUnitId!.Value).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return rows.Select(d => new AttendanceDeviceDto(
            d.Id, d.Code, d.Name, d.DeviceType, d.OrgUnitId,
            d.OrgUnitId is Guid o ? ous.GetValueOrDefault(o) : null,
            d.SerialNo, d.IsActive, d.Note)).ToList();
    }

    public async Task<AttendanceDeviceDto> UpsertDeviceAsync(
        Guid tenantId, Guid userId, AttendanceDeviceUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.Name ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã thiết bị 1–40 ký tự.");
        if (name.Length is < 1 or > 100) throw new AppException("Tên thiết bị 1–100 ký tự.");

        AttendanceDevice e;
        if (req.Id is Guid id)
        {
            e = await _db.AttendanceDevices.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Thiết bị không tồn tại.", 404);
        }
        else
        {
            if (await _db.AttendanceDevices.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã thiết bị đã tồn tại.");
            e = new AttendanceDevice { TenantId = tenantId, CreatedBy = userId };
            _db.AttendanceDevices.Add(e);
        }

        e.Code = code;
        e.Name = name;
        e.DeviceType = string.IsNullOrWhiteSpace(req.DeviceType) ? "Fingerprint" : req.DeviceType.Trim();
        e.OrgUnitId = req.OrgUnitId;
        e.SerialNo = string.IsNullOrWhiteSpace(req.SerialNo) ? null : req.SerialNo.Trim();
        e.IsActive = req.IsActive;
        e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await ListDevicesAsync(tenantId, ct)).First(x => x.Id == e.Id);
    }

    public async Task<IReadOnlyList<AttendanceGeoFenceDto>> ListGeoFencesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.AttendanceGeoFences.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(ct);
        var ouIds = rows.Where(x => x.OrgUnitId is not null).Select(x => x.OrgUnitId!.Value).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return rows.Select(g => new AttendanceGeoFenceDto(
            g.Id, g.Name, g.OrgUnitId, g.OrgUnitId is Guid o ? ous.GetValueOrDefault(o) : null,
            g.Latitude, g.Longitude, g.RadiusMeters, g.IsActive)).ToList();
    }

    public async Task<AttendanceGeoFenceDto> UpsertGeoFenceAsync(
        Guid tenantId, Guid userId, AttendanceGeoFenceUpsertRequest req, CancellationToken ct = default)
    {
        var name = (req.Name ?? "").Trim();
        if (name.Length is < 1 or > 100) throw new AppException("Tên điểm chấm 1–100 ký tự.");
        if (req.RadiusMeters is < 10 or > 50000) throw new AppException("Bán kính 10–50000 m.");

        AttendanceGeoFence e;
        if (req.Id is Guid id)
        {
            e = await _db.AttendanceGeoFences.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Geo-fence không tồn tại.", 404);
        }
        else
        {
            e = new AttendanceGeoFence { TenantId = tenantId, CreatedBy = userId };
            _db.AttendanceGeoFences.Add(e);
        }

        e.Name = name;
        e.OrgUnitId = req.OrgUnitId;
        e.Latitude = req.Latitude;
        e.Longitude = req.Longitude;
        e.RadiusMeters = req.RadiusMeters;
        e.IsActive = req.IsActive;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await ListGeoFencesAsync(tenantId, ct)).First(x => x.Id == e.Id);
    }

    public async Task<AttendanceRecordDto> CheckInAsync(
        Guid tenantId, Guid userId, AttendancePunchRequest req, CancellationToken ct = default)
    {
        var emp = await RequireMyEmployeeAsync(tenantId, userId, ct);
        var policy = await EnsurePolicyAsync(tenantId, ct);
        EnsureMethodAllowed(policy, req.Method);
        var now = DateTimeOffset.UtcNow;
        var workDate = DateOnly.FromDateTime(now.LocalDateTime);
        await EnsureNotLockedAsync(tenantId, workDate, ct);
        await EnsureGeoAsync(tenantId, policy, emp.OrgUnitId, req.Latitude, req.Longitude, ct);

        var rec = await GetOrCreateRecordAsync(tenantId, emp.Id, workDate, userId, ct);
        if (rec.CheckInAt is not null) throw new AppException("Đã check-in trong ngày.");

        rec.CheckInAt = now;
        rec.CheckInMethod = NormalizeMethod(req.Method);
        rec.DeviceId = req.DeviceId;
        rec.Latitude = req.Latitude;
        rec.Longitude = req.Longitude;
        rec.Note = string.IsNullOrWhiteSpace(req.Note) ? rec.Note : req.Note.Trim();
        rec.Status = "Open";
        rec.Tag = await ResolveTagAsync(tenantId, emp.Id, workDate, ct);
        await ApplyLateAndWorkAsync(tenantId, rec, policy, ct);
        rec.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRecordsAsync(new[] { rec }, ct))[0];
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(
        Guid tenantId, Guid userId, AttendancePunchRequest req, CancellationToken ct = default)
    {
        var emp = await RequireMyEmployeeAsync(tenantId, userId, ct);
        var policy = await EnsurePolicyAsync(tenantId, ct);
        EnsureMethodAllowed(policy, req.Method);
        var now = DateTimeOffset.UtcNow;
        var workDate = DateOnly.FromDateTime(now.LocalDateTime);
        await EnsureNotLockedAsync(tenantId, workDate, ct);
        await EnsureGeoAsync(tenantId, policy, emp.OrgUnitId, req.Latitude, req.Longitude, ct);

        var rec = await _db.AttendanceRecords.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == emp.Id && x.WorkDate == workDate && !x.IsDeleted, ct)
            ?? throw new AppException("Chưa check-in — không thể check-out.");
        if (rec.CheckInAt is null) throw new AppException("Chưa check-in.");
        if (rec.CheckOutAt is not null) throw new AppException("Đã check-out.");

        rec.CheckOutAt = now;
        rec.CheckOutMethod = NormalizeMethod(req.Method);
        rec.Status = "Closed";
        if (policy.EnableOt) await ApplyOtAsync(tenantId, rec, policy, ct);
        rec.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRecordsAsync(new[] { rec }, ct))[0];
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> MyHistoryAsync(
        Guid tenantId, Guid userId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var emp = await RequireMyEmployeeAsync(tenantId, userId, ct);
        return await BoardAsync(tenantId, null, from, to, ct, emp.Id);
    }

    public Task<IReadOnlyList<AttendanceRecordDto>> BoardAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
        => BoardAsync(tenantId, orgUnitId, from, to, ct, null);

    private async Task<IReadOnlyList<AttendanceRecordDto>> BoardAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct, Guid? employeeId)
    {
        var q = from r in _db.AttendanceRecords.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on r.EmployeeId equals e.Id
                where r.TenantId == tenantId && !r.IsDeleted && !e.IsDeleted
                select new { r, e };
        if (orgUnitId is Guid ou) q = q.Where(x => x.e.OrgUnitId == ou);
        if (employeeId is Guid eid) q = q.Where(x => x.r.EmployeeId == eid);
        if (from is DateOnly f) q = q.Where(x => x.r.WorkDate >= f);
        if (to is DateOnly t) q = q.Where(x => x.r.WorkDate <= t);

        var rows = await q.OrderByDescending(x => x.r.WorkDate).ThenBy(x => x.e.EmployeeCode).Take(500).ToListAsync(ct);
        return await MapRecordsAsync(rows.Select(x => x.r).ToList(), ct);
    }

    public async Task<IReadOnlyList<AttendanceMissingAlertDto>> MissingAlertsAsync(
        Guid tenantId, DateOnly? date, CancellationToken ct = default)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Now);
        var policy = await EnsurePolicyAsync(tenantId, ct);
        var emps = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Active" || x.Status == "Probation"))
            .ToListAsync(ct);
        var recs = await _db.AttendanceRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkDate == d && !x.IsDeleted)
            .ToDictionaryAsync(x => x.EmployeeId, ct);
        var ous = await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var alerts = new List<AttendanceMissingAlertDto>();
        var now = DateTimeOffset.UtcNow;
        foreach (var e in emps)
        {
            recs.TryGetValue(e.Id, out var r);
            if (r is null)
            {
                alerts.Add(new AttendanceMissingAlertDto(
                    e.Id, e.EmployeeCode, e.FullName, e.OrgUnitId,
                    ous.GetValueOrDefault(e.OrgUnitId, "?"), d, "MissingCheckIn"));
                continue;
            }

            if (r.CheckInAt is not null && r.CheckOutAt is null
                && (now - r.CheckInAt.Value).TotalHours >= policy.ForgotCheckoutHours)
            {
                alerts.Add(new AttendanceMissingAlertDto(
                    e.Id, e.EmployeeCode, e.FullName, e.OrgUnitId,
                    ous.GetValueOrDefault(e.OrgUnitId, "?"), d, "MissingCheckout"));
            }
            else if (r.Status == "Missing")
            {
                alerts.Add(new AttendanceMissingAlertDto(
                    e.Id, e.EmployeeCode, e.FullName, e.OrgUnitId,
                    ous.GetValueOrDefault(e.OrgUnitId, "?"), d, "Missing"));
            }
        }

        return alerts;
    }

    public async Task<int> MarkMissingAsync(Guid tenantId, Guid userId, DateOnly date, CancellationToken ct = default)
    {
        await EnsureNotLockedAsync(tenantId, date, ct);
        var emps = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Active" || x.Status == "Probation"))
            .Select(x => x.Id).ToListAsync(ct);
        var existing = await _db.AttendanceRecords
            .Where(x => x.TenantId == tenantId && x.WorkDate == date && !x.IsDeleted)
            .Select(x => x.EmployeeId).ToListAsync(ct);
        var missing = emps.Except(existing).ToList();
        foreach (var eid in missing)
        {
            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                TenantId = tenantId,
                EmployeeId = eid,
                WorkDate = date,
                Status = "Missing",
                WorkUnit = 0,
                CreatedBy = userId,
                UpdatedBy = userId
            });
        }

        // Quên checkout
        var policy = await EnsurePolicyAsync(tenantId, ct);
        var opens = await _db.AttendanceRecords
            .Where(x => x.TenantId == tenantId && x.WorkDate == date && !x.IsDeleted
                        && x.CheckInAt != null && x.CheckOutAt == null)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var r in opens.Where(x => (now - x.CheckInAt!.Value).TotalHours >= policy.ForgotCheckoutHours))
        {
            r.Status = "MissingCheckout";
            r.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);
        return missing.Count + opens.Count(x => x.Status == "MissingCheckout");
    }

    public async Task<AttendanceDeviceSyncResult> SyncDeviceAsync(
        Guid tenantId, Guid userId, AttendanceDeviceSyncRequest req, CancellationToken ct = default)
    {
        if (req.Items is null || req.Items.Count == 0)
            return new AttendanceDeviceSyncResult(0, 0, 0, 0, 0, 0);

        var policy = await EnsurePolicyAsync(tenantId, ct);
        int synced = 0, unknown = 0, locked = 0, dup = 0, invalid = 0;
        var items = req.Items.Take(500).ToList();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.EmployeeCode))
            {
                unknown++;
                continue;
            }

            var emp = await _db.Employees.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EmployeeCode == item.EmployeeCode.Trim() && !x.IsDeleted, ct);
            if (emp is null)
            {
                unknown++;
                continue;
            }

            var workDate = DateOnly.FromDateTime(item.PunchedAt.LocalDateTime);
            if (await IsLockedAsync(tenantId, workDate, ct))
            {
                locked++;
                continue;
            }

            Guid? deviceId = null;
            if (!string.IsNullOrWhiteSpace(item.DeviceCode))
            {
                var dev = await _db.AttendanceDevices.AsNoTracking().FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Code == item.DeviceCode.Trim().ToUpperInvariant() && !x.IsDeleted, ct);
                deviceId = dev?.Id;
            }

            var rec = await GetOrCreateRecordAsync(tenantId, emp.Id, workDate, userId, ct);
            var type = (item.PunchType ?? "").Trim().ToLowerInvariant();
            if (type is "in" or "checkin")
            {
                if (rec.CheckInAt is not null && item.PunchedAt >= rec.CheckInAt)
                {
                    dup++;
                    continue;
                }
                rec.CheckInAt = item.PunchedAt;
                rec.CheckInMethod = "DeviceSync";
                rec.DeviceId = deviceId;
                rec.Status = rec.CheckOutAt is null ? "Open" : "Closed";
                await ApplyLateAndWorkAsync(tenantId, rec, policy, ct);
                synced++;
            }
            else if (type is "out" or "checkout")
            {
                if (rec.CheckInAt is null)
                {
                    invalid++;
                    continue;
                }
                if (rec.CheckOutAt is not null && item.PunchedAt <= rec.CheckOutAt)
                {
                    dup++;
                    continue;
                }
                rec.CheckOutAt = item.PunchedAt;
                rec.CheckOutMethod = "DeviceSync";
                rec.DeviceId = deviceId;
                rec.Status = "Closed";
                if (policy.EnableOt) await ApplyOtAsync(tenantId, rec, policy, ct);
                synced++;
            }
            else
            {
                invalid++;
                continue;
            }

            rec.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);
        return new AttendanceDeviceSyncResult(synced, unknown, locked, dup, invalid, items.Count);
    }

    public async Task<int> RecalcOtAsync(
        Guid tenantId, Guid userId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var policy = await EnsurePolicyAsync(tenantId, ct);
        if (!policy.EnableOt) return 0;
        var rows = await _db.AttendanceRecords
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.WorkDate >= from && x.WorkDate <= to
                        && x.CheckInAt != null && x.CheckOutAt != null)
            .ToListAsync(ct);
        foreach (var r in rows)
        {
            await ApplyOtAsync(tenantId, r, policy, ct);
            r.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);
        return rows.Count;
    }

    public async Task<IReadOnlyList<AttendanceAdjustDto>> ListAdjustsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.AttendanceAdjustRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapAdjustsAsync(rows, ct);
    }

    public async Task<AttendanceAdjustDto> CreateAdjustAsync(
        Guid tenantId, Guid userId, AttendanceAdjustCreateRequest req, CancellationToken ct = default)
    {
        var policy = await EnsurePolicyAsync(tenantId, ct);
        var deadline = DateOnly.FromDateTime(DateTime.Now).AddDays(-policy.AdjustDeadlineDays);
        if (req.WorkDate < deadline) throw new AppException("Quá hạn xin điều chỉnh công.");
        await EnsureNotLockedAsync(tenantId, req.WorkDate, ct);

        if (!await _db.Employees.AnyAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Nhân viên không hợp lệ.", 404);
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length is < 3 or > 500) throw new AppException("Lý do 3–500 ký tự.");

        var e = new AttendanceAdjustRequest
        {
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            WorkDate = req.WorkDate,
            RequestedCheckInAt = req.RequestedCheckInAt,
            RequestedCheckOutAt = req.RequestedCheckOutAt,
            Reason = reason,
            EvidenceStorageKey = string.IsNullOrWhiteSpace(req.EvidenceStorageKey) ? null : req.EvidenceStorageKey.Trim(),
            Status = req.Submit ? "Submitted" : "Draft",
            RequestedByUserId = userId,
            CreatedBy = userId
        };
        var rec = await _db.AttendanceRecords.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == req.EmployeeId && x.WorkDate == req.WorkDate && !x.IsDeleted, ct);
        e.AttendanceRecordId = rec?.Id;
        _db.AttendanceAdjustRequests.Add(e);
        await _db.SaveChangesAsync(ct);
        return (await MapAdjustsAsync(new[] { e }, ct))[0];
    }

    public async Task<AttendanceAdjustDto> DecideAdjustAsync(
        Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default)
    {
        var e = await _db.AttendanceAdjustRequests.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu điều chỉnh không tồn tại.", 404);
        if (e.Status != "Submitted") throw new AppException("Chỉ duyệt phiếu Submitted.");
        await EnsureNotLockedAsync(tenantId, e.WorkDate, ct);

        e.Status = approve ? "Approved" : "Rejected";
        e.DecidedByUserId = userId;
        e.DecidedAt = DateTimeOffset.UtcNow;
        e.UpdatedBy = userId;

        if (approve)
        {
            var policy = await EnsurePolicyAsync(tenantId, ct);
            var rec = await GetOrCreateRecordAsync(tenantId, e.EmployeeId, e.WorkDate, userId, ct);
            if (e.RequestedCheckInAt is not null) { rec.CheckInAt = e.RequestedCheckInAt; rec.CheckInMethod = "Manual"; }
            if (e.RequestedCheckOutAt is not null) { rec.CheckOutAt = e.RequestedCheckOutAt; rec.CheckOutMethod = "Manual"; }
            rec.Status = rec.CheckOutAt is null ? (rec.CheckInAt is null ? "Missing" : "Open") : "Closed";
            if (rec.Status == "Adjusted" || approve) rec.Status = rec.CheckOutAt is null ? "Open" : "Adjusted";
            await ApplyLateAndWorkAsync(tenantId, rec, policy, ct);
            if (policy.EnableOt && rec.CheckOutAt is not null) await ApplyOtAsync(tenantId, rec, policy, ct);
            e.AttendanceRecordId = rec.Id;
        }

        await _db.SaveChangesAsync(ct);
        return (await MapAdjustsAsync(new[] { e }, ct))[0];
    }

    public async Task<IReadOnlyList<AttendancePeriodLockDto>> ListLocksAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.AttendancePeriodLocks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.PeriodKey).ToListAsync(ct);
        var uids = rows.Select(x => x.LockedByUserId).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        return rows.Select(x => new AttendancePeriodLockDto(
            x.Id, x.PeriodKey, x.PeriodFrom, x.PeriodTo, x.IsLocked,
            x.LockedByUserId, users.GetValueOrDefault(x.LockedByUserId, "?"), x.LockedAt, x.Note)).ToList();
    }

    public async Task<AttendancePeriodLockDto> LockPeriodAsync(
        Guid tenantId, Guid userId, AttendanceLockRequest req, CancellationToken ct = default)
    {
        var key = (req.PeriodKey ?? "").Trim();
        if (!DateOnly.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var from))
            throw new AppException("PeriodKey phải dạng yyyy-MM.");
        var to = from.AddMonths(1).AddDays(-1);

        var e = await _db.AttendancePeriodLocks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PeriodKey == key && !x.IsDeleted, ct);
        if (e is null)
        {
            e = new AttendancePeriodLock
            {
                TenantId = tenantId, PeriodKey = key, CreatedBy = userId
            };
            _db.AttendancePeriodLocks.Add(e);
        }

        e.PeriodFrom = from;
        e.PeriodTo = to;
        e.IsLocked = true;
        e.LockedByUserId = userId;
        e.LockedAt = DateTimeOffset.UtcNow;
        e.UnlockedByUserId = null;
        e.UnlockedAt = null;
        e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await ListLocksAsync(tenantId, ct)).First(x => x.Id == e.Id);
    }

    public async Task<AttendancePeriodLockDto> UnlockPeriodAsync(
        Guid tenantId, Guid userId, string periodKey, CancellationToken ct = default)
    {
        var e = await _db.AttendancePeriodLocks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PeriodKey == periodKey && !x.IsDeleted, ct)
            ?? throw new AppException("Kỳ khóa không tồn tại.", 404);
        e.IsLocked = false;
        e.UnlockedByUserId = userId;
        e.UnlockedAt = DateTimeOffset.UtcNow;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await ListLocksAsync(tenantId, ct)).First(x => x.Id == e.Id);
    }

    public async Task ConfirmRecordAsync(Guid tenantId, Guid userId, Guid recordId, CancellationToken ct = default)
    {
        var r = await _db.AttendanceRecords.FirstOrDefaultAsync(
            x => x.Id == recordId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Bản ghi công không tồn tại.", 404);
        r.IsConfirmed = true;
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    private static AttendancePolicyDto MapPolicy(AttendancePolicy p) => new(
        p.EnableFingerprint, p.EnableApp, p.EnableQr, p.EnableGeoFence,
        p.LateGraceMinutes, p.LateDeductEveryMinutes, p.LateDeductWorkUnit,
        p.ForgotCheckoutHours, p.AdjustDeadlineDays, p.EnableOt, p.OtAfterMinutes,
        p.EnableNightShiftRule, p.EnableHolidayRule, p.DefaultShiftStart, p.DefaultShiftEnd);

    private async Task<AttendancePolicy> EnsurePolicyAsync(Guid tenantId, CancellationToken ct)
    {
        var p = await _db.AttendancePolicies.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        if (p is not null) return p;
        p = new AttendancePolicy { TenantId = tenantId };
        _db.AttendancePolicies.Add(p);
        await _db.SaveChangesAsync(ct);
        return p;
    }

    private async Task<Employee> RequireMyEmployeeAsync(Guid tenantId, Guid userId, CancellationToken ct)
        => await _db.Employees.FirstOrDefaultAsync(
               x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct)
           ?? throw new AppException("Không tìm thấy hồ sơ nhân viên gắn tài khoản.", 404);

    private static void EnsureMethodAllowed(AttendancePolicy p, string? method)
    {
        var m = NormalizeMethod(method);
        if (m == "App" && !p.EnableApp) throw new AppException("Chấm APP đang tắt.");
        if (m == "Qr" && !p.EnableQr) throw new AppException("Chấm QR đang tắt.");
        if (m == "Fingerprint" && !p.EnableFingerprint) throw new AppException("Chấm vân tay đang tắt.");
    }

    private static string NormalizeMethod(string? method)
    {
        var m = (method ?? "App").Trim();
        return m.ToLowerInvariant() switch
        {
            "app" => "App",
            "qr" => "Qr",
            "fingerprint" or "bio" => "Fingerprint",
            "devicesync" => "DeviceSync",
            "manual" => "Manual",
            _ => m
        };
    }

    private async Task EnsureGeoAsync(
        Guid tenantId, AttendancePolicy policy, Guid orgUnitId,
        double? lat, double? lng, CancellationToken ct)
    {
        if (!policy.EnableGeoFence) return;
        if (lat is null || lng is null) throw new AppException("Cần tọa độ khi bật geo-fence.");
        var fences = await _db.AttendanceGeoFences.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted
                        && (x.OrgUnitId == null || x.OrgUnitId == orgUnitId))
            .ToListAsync(ct);
        if (fences.Count == 0) return;
        var ok = fences.Any(f => HaversineMeters(f.Latitude, f.Longitude, lat.Value, lng.Value) <= f.RadiusMeters);
        if (!ok) throw new AppException("Ngoài vùng geo-fence cho phép.");
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        static double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLon = Rad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * R * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private async Task EnsureNotLockedAsync(Guid tenantId, DateOnly date, CancellationToken ct)
    {
        if (await IsLockedAsync(tenantId, date, ct))
            throw new AppException("Bảng công kỳ này đã khóa.");
    }

    private async Task<bool> IsLockedAsync(Guid tenantId, DateOnly date, CancellationToken ct)
        => await _db.AttendancePeriodLocks.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.IsLocked
                 && x.PeriodFrom <= date && x.PeriodTo >= date, ct);

    private async Task<AttendanceRecord> GetOrCreateRecordAsync(
        Guid tenantId, Guid employeeId, DateOnly workDate, Guid userId, CancellationToken ct)
    {
        var rec = await _db.AttendanceRecords.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == employeeId && x.WorkDate == workDate && !x.IsDeleted, ct);
        if (rec is not null) return rec;
        // Bản ghi vừa Add trong cùng batch (chưa SaveChanges) — tránh tạo trùng khi sync máy.
        rec = _db.AttendanceRecords.Local.FirstOrDefault(
            x => x.TenantId == tenantId && x.EmployeeId == employeeId && x.WorkDate == workDate && !x.IsDeleted);
        if (rec is not null) return rec;
        rec = new AttendanceRecord
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            WorkDate = workDate,
            Status = "Open",
            CreatedBy = userId
        };
        _db.AttendanceRecords.Add(rec);
        return rec;
    }

    private async Task<string?> ResolveTagAsync(Guid tenantId, Guid employeeId, DateOnly date, CancellationToken ct)
    {
        var t = await _db.StaffTransfers.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.Kind == "Order"
                 && x.EmployeeId == employeeId && x.AttendanceTagged
                 && x.StartDate <= date && (x.EndDate == null || x.EndDate >= date)
                 && (x.Status == "Active" || x.Status == "Acknowledged" || x.Status == "Issued"), ct);
        return t is null ? null : (string.IsNullOrWhiteSpace(t.AttendanceTag) ? "TRANSFER" : t.AttendanceTag);
    }

    private async Task ApplyLateAndWorkAsync(
        Guid tenantId, AttendanceRecord rec, AttendancePolicy policy, CancellationToken ct)
    {
        if (rec.CheckInAt is null) return;
        var (start, _) = await ResolveShiftWindowAsync(tenantId, rec.EmployeeId, rec.WorkDate, policy, ct);
        var localIn = rec.CheckInAt.Value.ToLocalTime();
        var scheduled = rec.WorkDate.ToDateTime(start, DateTimeKind.Local);
        var grace = scheduled.AddMinutes(policy.LateGraceMinutes);
        var late = localIn.DateTime > grace
            ? (int)Math.Ceiling((localIn.DateTime - scheduled).TotalMinutes)
            : 0;
        if (late < 0) late = 0;
        rec.LateMinutes = late;
        if (late <= policy.LateGraceMinutes)
        {
            rec.DeductedWorkUnit = 0;
            rec.WorkUnit = 1;
            return;
        }

        var blocks = (int)Math.Ceiling((double)(late - policy.LateGraceMinutes) / policy.LateDeductEveryMinutes);
        rec.DeductedWorkUnit = Math.Min(1, blocks * policy.LateDeductWorkUnit);
        rec.WorkUnit = Math.Max(0, 1 - rec.DeductedWorkUnit);
    }

    private async Task ApplyOtAsync(
        Guid tenantId, AttendanceRecord rec, AttendancePolicy policy, CancellationToken ct)
    {
        if (rec.CheckOutAt is null) { rec.OtMinutes = 0; return; }
        var (_, end) = await ResolveShiftWindowAsync(tenantId, rec.EmployeeId, rec.WorkDate, policy, ct);
        var localOut = rec.CheckOutAt.Value.ToLocalTime();
        var scheduledEnd = rec.WorkDate.ToDateTime(end, DateTimeKind.Local);
        if (end <= policy.DefaultShiftStart && policy.EnableNightShiftRule)
            scheduledEnd = scheduledEnd.AddDays(1);
        var otStart = scheduledEnd.AddMinutes(policy.OtAfterMinutes);
        rec.OtMinutes = localOut.DateTime > otStart
            ? (int)Math.Floor((localOut.DateTime - scheduledEnd).TotalMinutes)
            : 0;
        if (rec.OtMinutes < 0) rec.OtMinutes = 0;
    }

    private async Task<(TimeOnly start, TimeOnly end)> ResolveShiftWindowAsync(
        Guid tenantId, Guid employeeId, DateOnly date, AttendancePolicy policy, CancellationToken ct)
    {
        var asg = await _db.ShiftAssignments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == employeeId
                                      && x.WorkDate == date && x.Status == "Scheduled" && !x.IsDeleted, ct);
        if (asg is not null)
        {
            var shift = await _db.WorkShifts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == asg.WorkShiftId && !x.IsDeleted, ct);
            if (shift is not null) return (shift.StartTime, shift.EndTime);
        }

        return (policy.DefaultShiftStart, policy.DefaultShiftEnd);
    }

    private async Task<IReadOnlyList<AttendanceRecordDto>> MapRecordsAsync(
        IReadOnlyList<AttendanceRecord> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<AttendanceRecordDto>();
        var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
        var emps = await _db.Employees.AsNoTracking().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var ouIds = emps.Values.Select(x => x.OrgUnitId).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return rows.Select(r =>
        {
            emps.TryGetValue(r.EmployeeId, out var e);
            var ou = e?.OrgUnitId ?? Guid.Empty;
            return new AttendanceRecordDto(
                r.Id, r.EmployeeId, e?.EmployeeCode ?? "?", e?.FullName ?? "?",
                ou, ous.GetValueOrDefault(ou, "?"), r.WorkDate,
                r.CheckInAt, r.CheckOutAt, r.CheckInMethod, r.CheckOutMethod,
                r.LateMinutes, r.DeductedWorkUnit, r.OtMinutes, r.WorkUnit,
                r.Status, r.Tag, r.Note, r.IsConfirmed);
        }).ToList();
    }

    private async Task<IReadOnlyList<AttendanceAdjustDto>> MapAdjustsAsync(
        IReadOnlyList<AttendanceAdjustRequest> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<AttendanceAdjustDto>();
        var empIds = rows.Select(x => x.EmployeeId).Distinct().ToList();
        var userIds = rows.Select(x => x.RequestedByUserId).Distinct().ToList();
        var emps = await _db.Employees.AsNoTracking().Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var users = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        return rows.Select(a =>
        {
            emps.TryGetValue(a.EmployeeId, out var e);
            return new AttendanceAdjustDto(
                a.Id, a.EmployeeId, e?.EmployeeCode ?? "?", e?.FullName ?? "?", a.WorkDate,
                a.RequestedCheckInAt, a.RequestedCheckOutAt, a.Reason, a.EvidenceStorageKey, a.Status,
                a.RequestedByUserId, users.GetValueOrDefault(a.RequestedByUserId, "?"), a.CreatedAt);
        }).ToList();
    }
}

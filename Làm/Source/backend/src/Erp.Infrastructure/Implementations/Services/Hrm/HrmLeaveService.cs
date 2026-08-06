using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmLeaveService : IHrmLeaveService
{
    private readonly AppDbContext _db;
    private readonly IWfRuntimeService _wf;

    public HrmLeaveService(AppDbContext db, IWfRuntimeService wf)
    {
        _db = db;
        _wf = wf;
    }

    public async Task<IReadOnlyList<LeaveBalanceDto>> ListBalancesAsync(
        Guid tenantId, Guid currentUserId, Guid? employeeId, CancellationToken ct = default)
    {
        var myEmp = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == currentUserId && !x.IsDeleted)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);

        var q = from b in _db.LeaveBalances.AsNoTracking()
                join lt in _db.LeaveTypes.AsNoTracking() on b.LeaveTypeId equals lt.Id
                where b.TenantId == tenantId && !b.IsDeleted
                select new { b, lt.Name };

        if (employeeId is Guid eid)
            q = q.Where(x => x.b.EmployeeId == eid);
        else if (myEmp is Guid me)
            q = q.Where(x => x.b.EmployeeId == me);

        return await q.OrderBy(x => x.Name)
            .Select(x => new LeaveBalanceDto(x.b.Id, x.b.EmployeeId, x.b.LeaveTypeId, x.Name, x.b.Year, x.b.Entitled, x.b.Used, x.b.Remaining))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> ListRequestsAsync(
        Guid tenantId, Guid currentUserId, Guid? employeeId, CancellationToken ct = default)
    {
        var myEmp = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == currentUserId && !x.IsDeleted)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);

        var q = from lr in _db.LeaveRequests.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on lr.EmployeeId equals e.Id
                join lt in _db.LeaveTypes.AsNoTracking() on lr.LeaveTypeId equals lt.Id
                where lr.TenantId == tenantId && !lr.IsDeleted
                select new { lr, e.FullName, TypeName = lt.Name, e.UserId };

        if (employeeId is Guid eid)
            q = q.Where(x => x.lr.EmployeeId == eid);
        else if (myEmp is Guid me)
            q = q.Where(x => x.lr.EmployeeId == me || x.lr.RequestedByUserId == currentUserId);

        return await q.OrderByDescending(x => x.lr.CreatedAt)
            .Select(x => new LeaveRequestDto(
                x.lr.Id, x.lr.EmployeeId, x.FullName, x.lr.LeaveTypeId, x.TypeName,
                x.lr.FromDate, x.lr.ToDate, x.lr.Days, x.lr.Reason, x.lr.Status, x.lr.WfInstanceId))
            .ToListAsync(ct);
    }

    public async Task<LeaveRequestDto> CreateAndOptionallySubmitAsync(
        Guid tenantId, Guid userId, LeaveRequestCreateRequest req, CancellationToken ct = default)
    {
        var empId = req.EmployeeId
            ?? await _db.Employees.Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
                .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct)
            ?? throw new AppException("Không tìm thấy hồ sơ nhân sự gắn user.");

        if (req.Days <= 0) throw new AppException("Số ngày phải > 0.");
        if (req.ToDate < req.FromDate) throw new AppException("Đến ngày phải ≥ từ ngày.");

        _ = await _db.LeaveTypes.FirstOrDefaultAsync(x => x.Id == req.LeaveTypeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Loại nghỉ không hợp lệ.");

        var leave = new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = empId,
            LeaveTypeId = req.LeaveTypeId,
            FromDate = req.FromDate,
            ToDate = req.ToDate,
            Days = req.Days,
            Reason = req.Reason,
            Status = "Draft",
            RequestedByUserId = userId,
            CreatedBy = userId
        };
        _db.LeaveRequests.Add(leave);
        await _db.SaveChangesAsync(ct);

        if (req.Submit)
        {
            var year = req.FromDate.Year;
            var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EmployeeId == empId && x.LeaveTypeId == req.LeaveTypeId && x.Year == year && !x.IsDeleted, ct);
            if (bal is not null && bal.Remaining < req.Days)
                throw new AppException($"Quỹ phép còn {bal.Remaining} ngày, không đủ.");

            var instanceId = await _wf.StartAsync(
                tenantId, "LEAVE_APPROVE", "HRM", "leave_request", leave.Id, userId, null, ct);
            leave.Status = "Pending";
            leave.WfInstanceId = instanceId;
            await _db.SaveChangesAsync(ct);
        }

        var empName = await _db.Employees.Where(x => x.Id == empId).Select(x => x.FullName).FirstAsync(ct);
        var typeName = await _db.LeaveTypes.Where(x => x.Id == req.LeaveTypeId).Select(x => x.Name).FirstAsync(ct);
        return new LeaveRequestDto(leave.Id, leave.EmployeeId, empName, leave.LeaveTypeId, typeName,
            leave.FromDate, leave.ToDate, leave.Days, leave.Reason, leave.Status, leave.WfInstanceId);
    }

    public async Task<IReadOnlyList<LeaveEntitlementRuleDto>> ListEntitlementRulesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.LeaveEntitlementRules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.LeaveTypeId).ToListAsync(ct);
        var typeIds = rows.Select(x => x.LeaveTypeId).Distinct().ToList();
        var etIds = rows.Where(x => x.EmployeeTypeId is not null).Select(x => x.EmployeeTypeId!.Value).Distinct().ToList();
        var types = await _db.LeaveTypes.AsNoTracking().Where(x => typeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var ets = etIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.EmployeeTypes.AsNoTracking().Where(x => etIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return rows.Select(r => new LeaveEntitlementRuleDto(
            r.Id, r.LeaveTypeId, types.GetValueOrDefault(r.LeaveTypeId, "?"),
            r.EmployeeTypeId, r.EmployeeTypeId is Guid e ? ets.GetValueOrDefault(e) : null,
            r.DaysPerYear, r.IsActive, r.Note)).ToList();
    }

    public async Task<LeaveEntitlementRuleDto> UpsertEntitlementRuleAsync(
        Guid tenantId, Guid userId, LeaveEntitlementRuleUpsertRequest req, CancellationToken ct = default)
    {
        if (req.DaysPerYear is < 0 or > 366) throw new AppException("Số ngày quỹ không hợp lệ.");
        if (!await _db.LeaveTypes.AnyAsync(x => x.Id == req.LeaveTypeId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Loại nghỉ không hợp lệ.", 404);
        if (req.EmployeeTypeId is Guid etid
            && !await _db.EmployeeTypes.AnyAsync(x => x.Id == etid && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Loại NS không hợp lệ.", 404);

        LeaveEntitlementRule e;
        if (req.Id is Guid id)
        {
            e = await _db.LeaveEntitlementRules.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Rule không tồn tại.", 404);
        }
        else
        {
            e = new LeaveEntitlementRule { TenantId = tenantId, CreatedBy = userId };
            _db.LeaveEntitlementRules.Add(e);
        }

        e.LeaveTypeId = req.LeaveTypeId;
        e.EmployeeTypeId = req.EmployeeTypeId;
        e.DaysPerYear = req.DaysPerYear;
        e.IsActive = req.IsActive;
        e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await ListEntitlementRulesAsync(tenantId, ct)).First(x => x.Id == e.Id);
    }

    public async Task<LeaveBalanceDto> AdjustBalanceAsync(
        Guid tenantId, Guid userId, LeaveBalanceAdjustRequest req, CancellationToken ct = default)
    {
        if (req.Entitled < 0 || req.Entitled > 366) throw new AppException("Entitled không hợp lệ.");
        if (!await _db.Employees.AnyAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Nhân viên không hợp lệ.", 404);
        if (!await _db.LeaveTypes.AnyAsync(x => x.Id == req.LeaveTypeId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Loại nghỉ không hợp lệ.", 404);

        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == req.EmployeeId
                 && x.LeaveTypeId == req.LeaveTypeId && x.Year == req.Year && !x.IsDeleted, ct);
        if (bal is null)
        {
            bal = new LeaveBalance
            {
                TenantId = tenantId,
                EmployeeId = req.EmployeeId,
                LeaveTypeId = req.LeaveTypeId,
                Year = req.Year,
                Used = 0,
                CreatedBy = userId
            };
            _db.LeaveBalances.Add(bal);
        }

        bal.Entitled = req.Entitled;
        bal.Remaining = bal.Entitled - bal.Used;
        if (bal.Remaining < 0) bal.Remaining = 0;
        bal.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var typeName = await _db.LeaveTypes.Where(x => x.Id == bal.LeaveTypeId).Select(x => x.Name).FirstAsync(ct);
        return new LeaveBalanceDto(bal.Id, bal.EmployeeId, bal.LeaveTypeId, typeName,
            bal.Year, bal.Entitled, bal.Used, bal.Remaining);
    }

    public async Task<int> AllocateYearAsync(
        Guid tenantId, Guid userId, LeaveAllocateYearRequest req, CancellationToken ct = default)
    {
        var year = req.Year is >= 2000 and <= 2100 ? req.Year : DateTime.UtcNow.Year;
        var emps = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Active" || x.Status == "Probation")
                        && (req.EmployeeTypeId == null || x.EmployeeTypeId == req.EmployeeTypeId))
            .ToListAsync(ct);
        var types = await _db.LeaveTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted
                        && (req.LeaveTypeId == null || x.Id == req.LeaveTypeId))
            .ToListAsync(ct);
        var rules = await _db.LeaveEntitlementRules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted).ToListAsync(ct);

        var n = 0;
        foreach (var emp in emps)
        foreach (var lt in types)
        {
            var days = ResolveEntitlement(lt, emp.EmployeeTypeId, rules);
            var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EmployeeId == emp.Id
                     && x.LeaveTypeId == lt.Id && x.Year == year && !x.IsDeleted, ct);
            if (bal is null)
            {
                bal = new LeaveBalance
                {
                    TenantId = tenantId,
                    EmployeeId = emp.Id,
                    LeaveTypeId = lt.Id,
                    Year = year,
                    Used = 0,
                    CreatedBy = userId
                };
                _db.LeaveBalances.Add(bal);
            }

            bal.Entitled = days;
            bal.Remaining = Math.Max(0, bal.Entitled - bal.Used);
            bal.UpdatedBy = userId;
            n++;
        }

        await _db.SaveChangesAsync(ct);
        return n;
    }

    public async Task<LeaveRequestDto> CancelRequestAsync(
        Guid tenantId, Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var leave = await _db.LeaveRequests.FirstOrDefaultAsync(
            x => x.Id == requestId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đơn nghỉ không tồn tại.", 404);

        if (leave.Status is "Cancelled" or "Rejected")
            throw new AppException("Đơn đã hủy/từ chối.");
        if (leave.Status is not ("Draft" or "Pending" or "Approved"))
            throw new AppException("Không hủy được trạng thái hiện tại.");

        if (leave.Status == "Approved")
        {
            var year = leave.FromDate.Year;
            var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EmployeeId == leave.EmployeeId
                     && x.LeaveTypeId == leave.LeaveTypeId && x.Year == year && !x.IsDeleted, ct);
            if (bal is not null)
            {
                bal.Used = Math.Max(0, bal.Used - leave.Days);
                bal.Remaining = bal.Entitled - bal.Used;
                bal.UpdatedBy = userId;
            }
        }

        leave.Status = "Cancelled";
        leave.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var empName = await _db.Employees.Where(x => x.Id == leave.EmployeeId).Select(x => x.FullName).FirstAsync(ct);
        var typeName = await _db.LeaveTypes.Where(x => x.Id == leave.LeaveTypeId).Select(x => x.Name).FirstAsync(ct);
        return new LeaveRequestDto(leave.Id, leave.EmployeeId, empName, leave.LeaveTypeId, typeName,
            leave.FromDate, leave.ToDate, leave.Days, leave.Reason, leave.Status, leave.WfInstanceId);
    }

    public async Task<IReadOnlyList<LeaveCalendarItemDto>> CalendarAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var q = from lr in _db.LeaveRequests.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on lr.EmployeeId equals e.Id
                join lt in _db.LeaveTypes.AsNoTracking() on lr.LeaveTypeId equals lt.Id
                where lr.TenantId == tenantId && !lr.IsDeleted
                      && (lr.Status == "Approved" || lr.Status == "Pending")
                select new { lr, e, TypeName = lt.Name };

        if (orgUnitId is Guid ou) q = q.Where(x => x.e.OrgUnitId == ou);
        if (from is DateOnly f) q = q.Where(x => x.lr.ToDate >= f);
        if (to is DateOnly t) q = q.Where(x => x.lr.FromDate <= t);

        var rows = await q.OrderBy(x => x.lr.FromDate).Take(500).ToListAsync(ct);
        var ouIds = rows.Select(x => x.e.OrgUnitId).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return rows.Select(r => new LeaveCalendarItemDto(
            r.lr.Id, r.e.Id, r.e.EmployeeCode, r.e.FullName, r.e.OrgUnitId,
            ous.GetValueOrDefault(r.e.OrgUnitId, "?"), r.lr.LeaveTypeId, r.TypeName,
            r.lr.FromDate, r.lr.ToDate, r.lr.Days, r.lr.Status)).ToList();
    }

    public async Task<IReadOnlyList<HolidayDto>> ListHolidaysAsync(
        Guid tenantId, int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        return await _db.Holidays.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Year == y)
            .OrderBy(x => x.Date)
            .Select(x => new HolidayDto(x.Id, x.Date, x.Name, x.IsPaid, x.Year, x.Note))
            .ToListAsync(ct);
    }

    public async Task<HolidayDto> UpsertHolidayAsync(
        Guid tenantId, Guid userId, HolidayUpsertRequest req, CancellationToken ct = default)
    {
        var name = (req.Name ?? "").Trim();
        if (name.Length is < 1 or > 200) throw new AppException("Tên ngày nghỉ 1–200 ký tự.");

        Holiday e;
        if (req.Id is Guid id)
        {
            e = await _db.Holidays.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Ngày nghỉ không tồn tại.", 404);
        }
        else
        {
            e = new Holiday { TenantId = tenantId, CreatedBy = userId };
            _db.Holidays.Add(e);
        }

        e.Date = req.Date;
        e.Year = req.Date.Year;
        e.Name = name;
        e.IsPaid = req.IsPaid;
        e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new HolidayDto(e.Id, e.Date, e.Name, e.IsPaid, e.Year, e.Note);
    }

    public async Task<int> ImportHolidaysAsync(
        Guid tenantId, Guid userId, IReadOnlyList<HolidayImportItem> items, CancellationToken ct = default)
    {
        if (items is null || items.Count == 0) return 0;
        var n = 0;
        foreach (var item in items.Take(366))
        {
            var name = (item.Name ?? "").Trim();
            if (name.Length == 0) continue;
            var existing = await _db.Holidays.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Date == item.Date && !x.IsDeleted, ct);
            if (existing is null)
            {
                existing = new Holiday { TenantId = tenantId, CreatedBy = userId };
                _db.Holidays.Add(existing);
            }

            existing.Date = item.Date;
            existing.Year = item.Date.Year;
            existing.Name = name;
            existing.IsPaid = item.IsPaid;
            existing.UpdatedBy = userId;
            n++;
        }

        await _db.SaveChangesAsync(ct);
        return n;
    }

    public async Task<IReadOnlyList<LeaveReportRowDto>> ReportAsync(
        Guid tenantId, int year, Guid? orgUnitId, CancellationToken ct = default)
    {
        var q = from b in _db.LeaveBalances.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on b.EmployeeId equals e.Id
                join lt in _db.LeaveTypes.AsNoTracking() on b.LeaveTypeId equals lt.Id
                where b.TenantId == tenantId && !b.IsDeleted && b.Year == year && !e.IsDeleted
                select new { b, e, TypeName = lt.Name };
        if (orgUnitId is Guid ou) q = q.Where(x => x.e.OrgUnitId == ou);

        var rows = await q.OrderBy(x => x.e.EmployeeCode).ThenBy(x => x.TypeName).ToListAsync(ct);
        var ouIds = rows.Select(x => x.e.OrgUnitId).Distinct().ToList();
        var ous = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var approvedCounts = await _db.LeaveRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Approved"
                        && x.FromDate.Year == year)
            .GroupBy(x => new { x.EmployeeId, x.LeaveTypeId })
            .Select(g => new { g.Key.EmployeeId, g.Key.LeaveTypeId, Cnt = g.Count() })
            .ToListAsync(ct);
        var cntMap = approvedCounts.ToDictionary(x => (x.EmployeeId, x.LeaveTypeId), x => x.Cnt);

        return rows.Select(r => new LeaveReportRowDto(
            r.e.Id, r.e.EmployeeCode, r.e.FullName, r.e.OrgUnitId,
            ous.GetValueOrDefault(r.e.OrgUnitId, "?"),
            r.b.LeaveTypeId, r.TypeName, r.b.Year, r.b.Entitled, r.b.Used, r.b.Remaining,
            cntMap.GetValueOrDefault((r.e.Id, r.b.LeaveTypeId)))).ToList();
    }

    private static decimal ResolveEntitlement(
        LeaveType lt, Guid? employeeTypeId, List<LeaveEntitlementRule> rules)
    {
        var specific = rules.FirstOrDefault(r =>
            r.LeaveTypeId == lt.Id && r.EmployeeTypeId == employeeTypeId);
        if (specific is not null) return specific.DaysPerYear;
        var generic = rules.FirstOrDefault(r => r.LeaveTypeId == lt.Id && r.EmployeeTypeId is null);
        if (generic is not null) return generic.DaysPerYear;
        return lt.DefaultDaysPerYear;
    }
}

public sealed class HrmContractService : IHrmContractService
{
    private readonly AppDbContext _db;

    public HrmContractService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ContractDto>> ListAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default)
    {
        var q = from c in _db.Contracts.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on c.EmployeeId equals e.Id
                where c.TenantId == tenantId && !c.IsDeleted
                select new { c, e.FullName };
        if (employeeId is Guid eid) q = q.Where(x => x.c.EmployeeId == eid);

        return await q.OrderByDescending(x => x.c.StartDate)
            .Select(x => new ContractDto(x.c.Id, x.c.EmployeeId, x.FullName, x.c.ContractNo, x.c.ContractType, x.c.StartDate, x.c.EndDate, x.c.Status, x.c.ParentContractId, x.c.BaseSalary, x.c.ScanFileId))
            .ToListAsync(ct);
    }

    public async Task<ContractDto> UpsertAsync(Guid tenantId, Guid? actorId, ContractUpsertRequest req, CancellationToken ct = default)
    {
        _ = await _db.Employees.FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);

        Contract entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Contracts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Hợp đồng không tồn tại.", 404);
        }
        else
        {
            if (await _db.Contracts.AnyAsync(x => x.TenantId == tenantId && x.ContractNo == req.ContractNo.Trim() && !x.IsDeleted, ct))
                throw new AppException("Số HĐ đã tồn tại.");
            entity = new Contract { TenantId = tenantId, CreatedBy = actorId };
            _db.Contracts.Add(entity);
        }

        entity.EmployeeId = req.EmployeeId;
        entity.ContractNo = req.ContractNo.Trim();
        entity.ContractType = req.ContractType;
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.Status = req.Status;
        entity.ParentContractId = req.ParentContractId;
        entity.BaseSalary = req.BaseSalary;
        entity.ScanFileId = req.ScanFileId;
        entity.UpdatedBy = actorId;
        await _db.SaveChangesAsync(ct);

        return await MapAsync(entity, ct);
    }

    public async Task<ContractDto> RenewAsync(Guid tenantId, Guid? actorId, Guid id, ContractRenewRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Contracts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Hợp đồng không tồn tại.", 404);
        entity.EndDate = req.NewEndDate;
        if (req.BaseSalary is decimal s) entity.BaseSalary = s;
        entity.Status = "Active";
        entity.UpdatedBy = actorId;
        await _db.SaveChangesAsync(ct);
        return await MapAsync(entity, ct);
    }

    public async Task<ContractDto> TerminateAsync(Guid tenantId, Guid? actorId, Guid id, ContractTerminateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Contracts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Hợp đồng không tồn tại.", 404);
        entity.Status = "Terminated";
        entity.EndDate = req.TerminateDate;
        entity.UpdatedBy = actorId;
        await _db.SaveChangesAsync(ct);
        return await MapAsync(entity, ct);
    }

    public async Task<IReadOnlyList<ContractDto>> ListExpiringAsync(Guid tenantId, int withinDays, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(Math.Clamp(withinDays, 1, 365));
        var q = from c in _db.Contracts.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on c.EmployeeId equals e.Id
                where c.TenantId == tenantId && !c.IsDeleted && c.Status == "Active"
                      && c.EndDate != null && c.EndDate >= today && c.EndDate <= until
                orderby c.EndDate
                select new ContractDto(c.Id, c.EmployeeId, e.FullName, c.ContractNo, c.ContractType, c.StartDate, c.EndDate, c.Status, c.ParentContractId, c.BaseSalary, c.ScanFileId);
        return await q.ToListAsync(ct);
    }

    private async Task<ContractDto> MapAsync(Contract entity, CancellationToken ct)
    {
        var name = await _db.Employees.Where(x => x.Id == entity.EmployeeId).Select(x => x.FullName).FirstAsync(ct);
        return new ContractDto(entity.Id, entity.EmployeeId, name, entity.ContractNo, entity.ContractType, entity.StartDate, entity.EndDate, entity.Status, entity.ParentContractId, entity.BaseSalary, entity.ScanFileId);
    }
}

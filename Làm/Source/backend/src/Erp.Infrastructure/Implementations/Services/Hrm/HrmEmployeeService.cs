using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Mod;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmEmployeeService : IHrmEmployeeService
{
    private readonly AppDbContext _db;
    private readonly IDataScopeService _scope;
    private readonly ISysPlatformService _platform;

    public HrmEmployeeService(AppDbContext db, IDataScopeService scope, ISysPlatformService platform)
    {
        _db = db;
        _scope = scope;
        _platform = platform;
    }

    public async Task<IReadOnlyList<EmployeeDto>> ListAsync(
        Guid tenantId, Guid currentUserId, string? q, CancellationToken ct = default)
    {
        var scope = await _scope.GetUserScopeContextAsync(currentUserId, ct);
        var myEmpId = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == currentUserId && !x.IsDeleted)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);

        var query = _db.Employees.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);

        query = scope.Scope switch
        {
            ScopeType.Own => query.Where(e => e.UserId == currentUserId),
            ScopeType.Team => query.Where(e =>
                e.UserId == currentUserId || (myEmpId != null && e.ManagerEmployeeId == myEmpId)),
            ScopeType.Department => query.Where(e =>
                e.DepartmentId != null && scope.AccessibleDepartmentIds.Contains(e.DepartmentId.Value)),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(e =>
                e.FullName.Contains(term)
                || e.EmployeeCode.Contains(term)
                || (e.Email != null && e.Email.Contains(term)));
        }

        var rows = await (
            from e in query
            join o in _db.OrgUnits.AsNoTracking() on e.OrgUnitId equals o.Id into oj
            from o in oj.DefaultIfEmpty()
            join d in _db.Departments.AsNoTracking() on e.DepartmentId equals d.Id into dj
            from d in dj.DefaultIfEmpty()
            join jl in _db.JobLevels.AsNoTracking() on e.JobLevelId equals jl.Id into jlj
            from jl in jlj.DefaultIfEmpty()
            join jt in _db.JobTitles.AsNoTracking() on e.JobTitleId equals jt.Id into jtj
            from jt in jtj.DefaultIfEmpty()
            join et in _db.EmployeeTypes.AsNoTracking() on e.EmployeeTypeId equals et.Id into etj
            from et in etj.DefaultIfEmpty()
            join mgr in _db.Employees.AsNoTracking() on e.ManagerEmployeeId equals mgr.Id into mgrj
            from mgr in mgrj.DefaultIfEmpty()
            orderby e.EmployeeCode
            select new EmployeeDto(
                e.Id, e.EmployeeCode, e.UserId, e.FullName, e.Dob, e.Gender, e.Email, e.Phone,
                e.OrgUnitId, o != null ? o.Name : null,
                e.DepartmentId, d != null ? d.Name : null,
                e.JobLevelId, jl != null ? jl.Name : null,
                e.JobTitleId, jt != null ? jt.Name : null,
                e.EmployeeTypeId, et != null ? et.Name : null,
                e.ManagerEmployeeId, mgr != null ? mgr.FullName : null,
                e.Status, e.HireDate, e.TerminateDate)
        ).ToListAsync(ct);

        return rows;
    }

    public async Task<EmployeeDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);

        return await MapOneAsync(e, ct);
    }

    public async Task<EmployeeDto> GetWithScopeAsync(Guid tenantId, Guid currentUserId, Guid id, CancellationToken ct = default)
    {
        var scope = await _scope.GetUserScopeContextAsync(currentUserId, ct);
        var e = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);

        if (scope.Scope == ScopeType.Own && e.UserId != currentUserId)
        {
            var myEmpId = await _db.Employees.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (e.Id != myEmpId)
                throw new AppException("Bạn không có quyền truy cập hồ sơ nhân sự này.", 403);
        }
        else if (scope.Scope == ScopeType.Department && e.DepartmentId.HasValue && !scope.AccessibleDepartmentIds.Contains(e.DepartmentId.Value))
        {
            throw new AppException("Bạn không có quyền truy cập hồ sơ nhân sự này.", 403);
        }

        return await MapOneAsync(e, ct);
    }

    public async Task<EmployeeDto> UpsertAsync(Guid tenantId, Guid? actorId, EmployeeUpsertRequest req, CancellationToken ct = default)
    {
        Employee entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct)
                     ?? throw new AppException("Nhân viên không tồn tại.", 404);

            if (entity.IsDeleted || entity.Status is "Terminated" or "Resigned" or "Locked" or "Retired")
                throw new AppException("Hồ sơ nhân sự đã bị khóa do nghỉ việc, không thể chỉnh sửa thông tin.", 400);
        }
        else
        {
            var code = string.IsNullOrWhiteSpace(req.EmployeeCode)
                ? await _platform.NextNumberAsync(tenantId, "HRM.EMP", ct)
                : req.EmployeeCode.Trim();
            if (await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.EmployeeCode == code && !x.IsDeleted, ct))
                throw new AppException("Mã nhân viên đã tồn tại.");
            entity = new Employee { TenantId = tenantId, CreatedBy = actorId, EmployeeCode = code };
            _db.Employees.Add(entity);
        }

        if (req.Id is Guid existingId
            && !string.IsNullOrWhiteSpace(req.EmployeeCode)
            && !string.Equals(entity.EmployeeCode, req.EmployeeCode.Trim(), StringComparison.OrdinalIgnoreCase)
            && await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.EmployeeCode == req.EmployeeCode.Trim() && x.Id != existingId && !x.IsDeleted, ct))
            throw new AppException("Mã nhân viên đã tồn tại.");

        if (!string.IsNullOrWhiteSpace(req.EmployeeCode))
            entity.EmployeeCode = req.EmployeeCode.Trim();
        entity.UserId = req.UserId;
        entity.FullName = req.FullName.Trim();
        entity.Dob = req.Dob;
        entity.Gender = req.Gender;
        entity.Email = req.Email;
        entity.Phone = req.Phone;
        entity.OrgUnitId = req.OrgUnitId;
        entity.DepartmentId = req.DepartmentId;
        entity.JobLevelId = req.JobLevelId;
        entity.JobTitleId = req.JobTitleId;
        entity.EmployeeTypeId = req.EmployeeTypeId;
        entity.ManagerEmployeeId = req.ManagerEmployeeId;
        entity.Status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        entity.HireDate = req.HireDate;
        entity.TerminateDate = req.TerminateDate;
        entity.UpdatedBy = actorId;

        if (req.UserId is Guid uid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == uid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (user is not null)
            {
                user.EmployeeId = entity.Id;
                user.DepartmentId = req.DepartmentId;
                user.JobLevelId = req.JobLevelId;
                user.PrimaryOrgUnitId = req.OrgUnitId;
                user.DisplayName = req.FullName.Trim();
                user.Email = req.Email;
                user.Phone = req.Phone;
            }
        }

        await _db.SaveChangesAsync(ct);
        return await MapOneAsync(entity, ct);
    }

    public async Task<IReadOnlyList<JobTitleDto>> ListJobTitlesAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.JobTitles.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .Select(x => new JobTitleDto(x.Id, x.Code, x.Name, x.DefaultJobLevelId, x.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EmployeeTypeDto>> ListEmployeeTypesAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.EmployeeTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new EmployeeTypeDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LeaveTypeDto>> ListLeaveTypesAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.LeaveTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new LeaveTypeDto(x.Id, x.Code, x.Name, x.IsPaid, x.DefaultDaysPerYear, x.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ContractDto>> ListContractsAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default)
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

    public async Task<EmployeeDto> ChangeStatusAsync(Guid tenantId, Guid actorId, Guid employeeId, ChangeEmploymentStatusRequest req, CancellationToken ct = default)
    {
        var e = await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId && x.TenantId == tenantId, ct)
                ?? throw new AppException("Nhân viên không tồn tại.", 404);

        var from = e.Status;
        var to = req.ToStatus.Trim();
        var effectiveDate = req.EffectiveDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : req.EffectiveDate;

        if (to is "Terminated" or "Resigned" or "Locked" or "Retired" or "Inactive")
        {
            if (!e.TerminateDate.HasValue) e.TerminateDate = effectiveDate;
            e.IsDeleted = true;
            e.DeletedAt = DateTimeOffset.UtcNow;
            e.Status = to == "Locked" ? "Inactive" : to;
        }
        else
        {
            if (e.IsDeleted)
            {
                e.IsDeleted = false;
                e.DeletedAt = null;
            }
            e.Status = to;
        }

        _db.EmploymentStatusChanges.Add(new EmploymentStatusChange
        {
            TenantId = tenantId, EmployeeId = e.Id, FromStatus = from, ToStatus = e.Status,
            EffectiveDate = effectiveDate, Reason = req.Reason,
            OrgUnitId = req.OrgUnitId ?? e.OrgUnitId, DepartmentId = req.DepartmentId ?? e.DepartmentId,
            JobTitleId = req.JobTitleId ?? e.JobTitleId, CreatedBy = actorId
        });

        if (req.OrgUnitId is Guid o) e.OrgUnitId = o;
        if (req.DepartmentId is Guid d) e.DepartmentId = d;
        if (req.JobTitleId is Guid jt) e.JobTitleId = jt;

        await _db.SaveChangesAsync(ct);
        return await MapOneAsync(e, ct);
    }

    public async Task<IReadOnlyList<EmploymentStatusChangeDto>> ListStatusHistoryAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
        => await _db.EmploymentStatusChanges.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EmployeeId == employeeId && !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveDate)
            .Select(x => new EmploymentStatusChangeDto(x.Id, x.EmployeeId, x.FromStatus, x.ToStatus, x.EffectiveDate, x.Reason, x.OrgUnitId, x.DepartmentId, x.JobTitleId))
            .ToListAsync(ct);

    public async Task<byte[]> ExportEmployeesCsvAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default)
    {
        var rows = await ListAsync(tenantId, currentUserId, null, ct);
        var sb = new StringBuilder();
        sb.Append("\uFEFF");
        sb.AppendLine("Mã NV,Họ và tên,Email,Số điện thoại,Trạng thái,Đơn vị,Bộ phận,Chức danh,Cấp bậc,Ngày vào làm");
        foreach (var e in rows)
            sb.AppendLine($"{Escape(e.EmployeeCode)},{Escape(e.FullName)},{Escape(e.Email)},{Escape(e.Phone)},{Escape(e.Status)},{Escape(e.OrgUnitName)},{Escape(e.DepartmentName)},{Escape(e.JobTitleName)},{Escape(e.JobLevelName)},{e.HireDate:yyyy-MM-dd}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }


    public async Task<IReadOnlyList<EmployeeDocumentDto>> ListDocumentsAsync(
        Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        await EnsureEmployeeAsync(tenantId, employeeId, ct);
        return await _db.EmployeeDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EmployeeId == employeeId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EmployeeDocumentDto(
                x.Id, x.EmployeeId, x.DocType, x.Title, x.StorageKey, x.IssuedOn, x.ExpiresOn, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<EmployeeDocumentDto> AddDocumentAsync(
        Guid tenantId, Guid? actorId, Guid employeeId, EmployeeDocumentUploadRequest req, CancellationToken ct = default)
    {
        await EnsureEmployeeAsync(tenantId, employeeId, ct);
        var title = (req.Title ?? "").Trim();
        var key = (req.StorageKey ?? "").Trim();
        var docType = string.IsNullOrWhiteSpace(req.DocType) ? "Other" : req.DocType.Trim();
        if (title.Length == 0 || key.Length == 0) throw new AppException("Thiếu tiêu đề hoặc file.");
        if (req.IssuedOn.HasValue && req.ExpiresOn.HasValue && req.ExpiresOn.Value <= req.IssuedOn.Value)
            throw new AppException("Ngày hết hạn giấy tờ phải sau ngày cấp.");
        var e = new EmployeeDocument
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            DocType = docType.Length > 40 ? docType[..40] : docType,
            Title = title.Length > 200 ? title[..200] : title,
            StorageKey = key.Length > 500 ? key[..500] : key,
            IssuedOn = req.IssuedOn,
            ExpiresOn = req.ExpiresOn,
            CreatedBy = actorId
        };
        _db.EmployeeDocuments.Add(e);
        await _db.SaveChangesAsync(ct);
        return new EmployeeDocumentDto(e.Id, e.EmployeeId, e.DocType, e.Title, e.StorageKey, e.IssuedOn, e.ExpiresOn, e.CreatedAt);
    }

    public async Task DeleteDocumentAsync(Guid tenantId, Guid employeeId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await _db.EmployeeDocuments
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == employeeId && x.Id == documentId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy giấy tờ.");
        doc.IsDeleted = true;
        doc.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ─── UC_HRM_034 — Điều chuyển đơn vị / bộ phận ───

    public async Task<EmployeeDto> TransferEmployeeAsync(
        Guid tenantId, Guid actorId, Guid employeeId, EmployeeTransferRequest req, CancellationToken ct = default)
    {
        var e = await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Nhân viên không tồn tại.", 404);

        if (e.IsDeleted || e.Status is "Terminated" or "Resigned" or "Locked" or "Retired")
            throw new AppException("Không thể điều chuyển nhân sự đã bị khóa do nghỉ việc.", 400);

        var targetOrgId = req.OrgUnitId ?? e.OrgUnitId;
        var targetDeptId = req.DepartmentId ?? e.DepartmentId;
        var targetJtId = req.JobTitleId ?? e.JobTitleId;
        var targetJlId = req.JobLevelId ?? e.JobLevelId;

        if (targetOrgId == e.OrgUnitId && targetDeptId == e.DepartmentId && targetJtId == e.JobTitleId && targetJlId == e.JobLevelId)
            throw new AppException("Đơn vị, Bộ phận, Chức danh hoặc Cấp bậc mới phải khác với thông tin hiện tại.", 400);

        if (req.OrgUnitId.HasValue && !await _db.OrgUnits.AnyAsync(x => x.TenantId == tenantId && x.Id == req.OrgUnitId.Value && !x.IsDeleted, ct))
            throw new AppException("Đơn vị tổ chức mới không tồn tại.", 404);

        if (req.DepartmentId.HasValue)
        {
            var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == req.DepartmentId.Value && !x.IsDeleted, ct)
                       ?? throw new AppException("Bộ phận mới không tồn tại.", 404);
            if (dept.OrgUnitId != targetOrgId)
                throw new AppException("Bộ phận mới không thuộc Đơn vị tổ chức đã chọn.", 400);
        }

        var effectiveDate = req.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var reason = string.IsNullOrWhiteSpace(req.Reason) ? "Điều chuyển nhân sự" : req.Reason.Trim();

        _db.EmploymentStatusChanges.Add(new EmploymentStatusChange
        {
            TenantId = tenantId,
            EmployeeId = e.Id,
            FromStatus = e.Status,
            ToStatus = "Transferred",
            EffectiveDate = effectiveDate,
            Reason = reason,
            OrgUnitId = targetOrgId,
            DepartmentId = targetDeptId,
            JobTitleId = targetJtId,
            CreatedBy = actorId
        });

        e.OrgUnitId = targetOrgId;
        e.DepartmentId = targetDeptId;
        e.JobTitleId = targetJtId;
        e.JobLevelId = targetJlId;
        e.UpdatedBy = actorId;

        if (e.UserId is Guid uid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == uid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (user is not null)
            {
                user.PrimaryOrgUnitId = targetOrgId;
                user.DepartmentId = targetDeptId;
                user.JobLevelId = targetJlId;
            }
        }

        await _db.SaveChangesAsync(ct);
        return await MapOneAsync(e, ct);
    }

    // ─── UC_HRM_036 — Cảnh báo sắp hết hạn thử việc ───

    public async Task<IReadOnlyList<ProbationExpiringEmployeeDto>> ListExpiringProbationEmployeesAsync(
        Guid tenantId, int daysAhead = 15, CancellationToken ct = default)
    {
        if (daysAhead <= 0) daysAhead = 15;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thresholdDate = today.AddDays(daysAhead);

        var probationEmployees = await (
            from e in _db.Employees.AsNoTracking()
            join o in _db.OrgUnits.AsNoTracking() on e.OrgUnitId equals o.Id into oj from o in oj.DefaultIfEmpty()
            join d in _db.Departments.AsNoTracking() on e.DepartmentId equals d.Id into dj from d in dj.DefaultIfEmpty()
            join jt in _db.JobTitles.AsNoTracking() on e.JobTitleId equals jt.Id into jtj from jt in jtj.DefaultIfEmpty()
            where e.TenantId == tenantId && !e.IsDeleted && e.Status == "Probation"
            select new { e, OrgName = o != null ? o.Name : null, DeptName = d != null ? d.Name : null, JtName = jt != null ? jt.Name : null }
        ).ToListAsync(ct);

        var list = new List<ProbationExpiringEmployeeDto>();

        foreach (var item in probationEmployees)
        {
            var e = item.e;
            var hireDate = e.HireDate ?? DateOnly.FromDateTime(e.CreatedAt.DateTime);

            var contractEnd = await _db.Contracts.AsNoTracking()
                .Where(c => c.TenantId == tenantId && c.EmployeeId == e.Id && !c.IsDeleted && c.ContractType == "Probation")
                .Select(c => c.EndDate)
                .FirstOrDefaultAsync(ct);

            var probEndDate = contractEnd ?? hireDate.AddDays(60);
            var daysRemaining = probEndDate.DayNumber - today.DayNumber;

            if (probEndDate <= thresholdDate)
            {
                list.Add(new ProbationExpiringEmployeeDto(
                    e.Id, e.EmployeeCode, e.FullName, hireDate, probEndDate, daysRemaining,
                    item.OrgName, item.DeptName, item.JtName));
            }
        }

        return list.OrderBy(x => x.DaysRemaining).ToList();
    }

    private async Task EnsureEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct)
    {
        if (!await _db.Employees.AnyAsync(x => x.TenantId == tenantId && x.Id == employeeId && !x.IsDeleted, ct))
            throw new AppException("Không tìm thấy nhân viên.");
    }

    private static string Escape(string? s)
    {
        s ??= "";
        return s.Contains(',') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
    }

    private async Task<EmployeeDto> MapOneAsync(Employee e, CancellationToken ct)
    {
        var org = await _db.OrgUnits.AsNoTracking().Where(x => x.Id == e.OrgUnitId).Select(x => x.Name).FirstOrDefaultAsync(ct);
        var dept = e.DepartmentId is Guid did
            ? await _db.Departments.AsNoTracking().Where(x => x.Id == did).Select(x => x.Name).FirstOrDefaultAsync(ct)
            : null;
        var jl = e.JobLevelId is Guid jlid
            ? await _db.JobLevels.AsNoTracking().Where(x => x.Id == jlid).Select(x => x.Name).FirstOrDefaultAsync(ct)
            : null;
        var jt = e.JobTitleId is Guid jtid
            ? await _db.JobTitles.AsNoTracking().Where(x => x.Id == jtid).Select(x => x.Name).FirstOrDefaultAsync(ct)
            : null;
        var et = e.EmployeeTypeId is Guid etid
            ? await _db.EmployeeTypes.AsNoTracking().Where(x => x.Id == etid).Select(x => x.Name).FirstOrDefaultAsync(ct)
            : null;
        var mgr = e.ManagerEmployeeId is Guid mid
            ? await _db.Employees.AsNoTracking().Where(x => x.Id == mid).Select(x => x.FullName).FirstOrDefaultAsync(ct)
            : null;

        return new EmployeeDto(
            e.Id, e.EmployeeCode, e.UserId, e.FullName, e.Dob, e.Gender, e.Email, e.Phone,
            e.OrgUnitId, org, e.DepartmentId, dept, e.JobLevelId, jl, e.JobTitleId, jt,
            e.EmployeeTypeId, et, e.ManagerEmployeeId, mgr, e.Status, e.HireDate, e.TerminateDate);
    }
}

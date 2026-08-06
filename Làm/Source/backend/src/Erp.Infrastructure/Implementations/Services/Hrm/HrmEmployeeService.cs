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

    public async Task<EmployeeDto> UpsertAsync(Guid tenantId, Guid? actorId, EmployeeUpsertRequest req, CancellationToken ct = default)
    {
        Employee entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Nhân viên không tồn tại.", 404);
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
        var e = await _db.Employees.FirstOrDefaultAsync(x => x.Id == employeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Nhân viên không tồn tại.", 404);
        var from = e.Status;
        var to = req.ToStatus.Trim();
        _db.EmploymentStatusChanges.Add(new EmploymentStatusChange
        {
            TenantId = tenantId, EmployeeId = e.Id, FromStatus = from, ToStatus = to,
            EffectiveDate = req.EffectiveDate, Reason = req.Reason,
            OrgUnitId = req.OrgUnitId ?? e.OrgUnitId, DepartmentId = req.DepartmentId ?? e.DepartmentId,
            JobTitleId = req.JobTitleId ?? e.JobTitleId, CreatedBy = actorId
        });
        e.Status = to;
        if (req.OrgUnitId is Guid o) e.OrgUnitId = o;
        if (req.DepartmentId is Guid d) e.DepartmentId = d;
        if (req.JobTitleId is Guid jt) e.JobTitleId = jt;
        if (to is "Terminated" or "Resigned" or "Inactive")
        {
            e.TerminateDate ??= req.EffectiveDate;
            e.IsDeleted = to is "Terminated" or "Resigned"; // khóa hồ sơ nghỉ — soft
            if (e.IsDeleted) e.DeletedAt = DateTimeOffset.UtcNow;
        }
        // "khóa hồ sơ đã nghỉ": dùng Status Inactive + IsDeleted
        if (to == "Locked")
        {
            e.Status = "Inactive";
            e.IsDeleted = true;
            e.DeletedAt = DateTimeOffset.UtcNow;
        }
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
        sb.AppendLine("employeeCode,fullName,email,phone,status,orgUnit,department,jobTitle,hireDate");
        foreach (var e in rows)
            sb.AppendLine($"{e.EmployeeCode},{Escape(e.FullName)},{e.Email},{e.Phone},{e.Status},{e.OrgUnitName},{e.DepartmentName},{e.JobTitleName},{e.HireDate}");
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

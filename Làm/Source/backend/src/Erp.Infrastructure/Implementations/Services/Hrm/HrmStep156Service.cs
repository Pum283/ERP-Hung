using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmStep156Service : IHrmStep156Service
{
    private readonly AppDbContext _db;

    public HrmStep156Service(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_005: Quản lý bộ phận trong đơn vị
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmDepartmentDto>> GetDepartmentsAsync(Guid tenantId, Guid? orgUnitId = null, CancellationToken ct = default)
    {
        var query = _db.Departments.AsNoTracking().Where(d => d.TenantId == tenantId);
        if (orgUnitId.HasValue && orgUnitId.Value != Guid.Empty)
        {
            query = query.Where(d => d.OrgUnitId == orgUnitId.Value);
        }

        var deps = await query.OrderBy(d => d.SortOrder).ThenBy(d => d.Name).ToListAsync(ct);
        var orgUnits = await _db.OrgUnits.AsNoTracking().Where(o => o.TenantId == tenantId).ToDictionaryAsync(o => o.Id, o => o.Name, ct);
        var users = await _db.Users.AsNoTracking().Where(u => u.TenantId == tenantId).ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
        var depDict = deps.ToDictionary(d => d.Id, d => d.Name);

        return deps.Select(d => new HrmDepartmentDto(
            d.Id,
            d.Code,
            d.Name,
            d.ParentId,
            d.ParentId.HasValue && depDict.TryGetValue(d.ParentId.Value, out var parentName) ? parentName : null,
            d.OrgUnitId,
            orgUnits.TryGetValue(d.OrgUnitId, out var orgName) ? orgName : null,
            d.ManagerUserId,
            d.ManagerUserId.HasValue && users.TryGetValue(d.ManagerUserId.Value, out var mgrName) ? mgrName : null,
            d.Path,
            d.SortOrder,
            d.IsActive,
            d.CreatedAt
        )).ToList();
    }

    public async Task<HrmDepartmentDto> GetDepartmentByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetDepartmentsAsync(tenantId, null, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy bộ phận {id}", 404);
        return item;
    }

    public async Task<HrmDepartmentDto> CreateDepartmentAsync(Guid tenantId, HrmDepartmentUpsertRequest req, CancellationToken ct = default)
    {
        ValidateDepartmentRequest(req);

        var codeExists = await _db.Departments.AnyAsync(d => d.TenantId == tenantId && d.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã bộ phận đã tồn tại.");

        if (req.ParentId.HasValue)
        {
            var parentExists = await _db.Departments.AnyAsync(d => d.TenantId == tenantId && d.Id == req.ParentId.Value, ct);
            if (!parentExists) throw new AppException("Bộ phận cha không tồn tại.", 404);
        }

        var entity = new Department
        {
            TenantId = tenantId,
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            ParentId = req.ParentId,
            OrgUnitId = req.OrgUnitId,
            ManagerUserId = req.ManagerUserId,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive,
            Path = ""
        };

        _db.Departments.Add(entity);
        await _db.SaveChangesAsync(ct);

        entity.Path = req.ParentId.HasValue ? $"{req.ParentId}/{entity.Id}" : entity.Id.ToString();
        await _db.SaveChangesAsync(ct);

        return await GetDepartmentByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmDepartmentDto> UpdateDepartmentAsync(Guid tenantId, Guid id, HrmDepartmentUpsertRequest req, CancellationToken ct = default)
    {
        ValidateDepartmentRequest(req);

        var entity = await _db.Departments.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy bộ phận {id}", 404);

        var codeExists = await _db.Departments.AnyAsync(d => d.TenantId == tenantId && d.Id != id && d.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã bộ phận đã tồn tại.");

        if (req.ParentId.HasValue)
        {
            if (req.ParentId.Value == id) throw new AppException("Bộ phận cha không hợp lệ (gây ra đệ quy).");
            var parent = await _db.Departments.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == req.ParentId.Value, ct);
            if (parent == null) throw new AppException("Bộ phận cha không tồn tại.", 404);
            if (!string.IsNullOrEmpty(parent.Path) && parent.Path.Contains(id.ToString()))
            {
                throw new AppException("Bộ phận cha không hợp lệ (gây ra đệ quy).");
            }
        }

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.ParentId = req.ParentId;
        entity.OrgUnitId = req.OrgUnitId;
        entity.ManagerUserId = req.ManagerUserId;
        entity.SortOrder = req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.Path = req.ParentId.HasValue ? $"{req.ParentId}/{entity.Id}" : entity.Id.ToString();

        await _db.SaveChangesAsync(ct);
        return await GetDepartmentByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteDepartmentAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Departments.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy bộ phận {id}", 404);

        var hasChildren = await _db.Departments.AnyAsync(d => d.TenantId == tenantId && d.ParentId == id, ct);
        if (hasChildren) throw new AppException("Không thể xóa bộ phận đang chứa bộ phận con.");

        _db.Departments.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateDepartmentRequest(HrmDepartmentUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã bộ phận không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new AppException("Tên bộ phận không được để trống.");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_008: Quản lý vị trí công việc
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<JobPositionDto>> GetJobPositionsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var positions = await _db.JobTitles.AsNoTracking()
            .Where(j => j.TenantId == tenantId)
            .OrderBy(j => j.SortOrder)
            .ThenBy(j => j.Name)
            .ToListAsync(ct);

        var levelIds = positions.Where(p => p.DefaultJobLevelId.HasValue).Select(p => p.DefaultJobLevelId!.Value).Distinct().ToList();
        var levels = await _db.JobLevels.AsNoTracking()
            .Where(l => l.TenantId == tenantId && levelIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        return positions.Select(p => new JobPositionDto(
            p.Id,
            p.Code,
            p.Name,
            p.DefaultJobLevelId,
            p.DefaultJobLevelId.HasValue && levels.TryGetValue(p.DefaultJobLevelId.Value, out var lvlName) ? lvlName : null,
            p.SortOrder,
            p.IsActive,
            p.CreatedAt
        )).ToList();
    }

    public async Task<JobPositionDto> GetJobPositionByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetJobPositionsAsync(tenantId, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy vị trí công việc {id}", 404);
        return item;
    }

    public async Task<JobPositionDto> CreateJobPositionAsync(Guid tenantId, JobPositionUpsertRequest req, CancellationToken ct = default)
    {
        ValidateJobPositionRequest(req);

        var codeExists = await _db.JobTitles.AnyAsync(j => j.TenantId == tenantId && j.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã vị trí công việc đã tồn tại.");

        var entity = new JobTitle
        {
            TenantId = tenantId,
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            DefaultJobLevelId = req.DefaultJobLevelId,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive
        };

        _db.JobTitles.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetJobPositionByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<JobPositionDto> UpdateJobPositionAsync(Guid tenantId, Guid id, JobPositionUpsertRequest req, CancellationToken ct = default)
    {
        ValidateJobPositionRequest(req);

        var entity = await _db.JobTitles.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy vị trí công việc {id}", 404);

        var codeExists = await _db.JobTitles.AnyAsync(j => j.TenantId == tenantId && j.Id != id && j.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã vị trí công việc đã tồn tại.");

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.DefaultJobLevelId = req.DefaultJobLevelId;
        entity.SortOrder = req.SortOrder;
        entity.IsActive = req.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetJobPositionByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteJobPositionAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.JobTitles.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy vị trí công việc {id}", 404);

        _db.JobTitles.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateJobPositionRequest(JobPositionUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã vị trí công việc không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new AppException("Tên vị trí công việc không được để trống.");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_011: Định nghĩa trung tâm chi phí NS
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<HrmCostCenterDto>> GetCostCentersAsync(Guid tenantId, Guid? orgUnitId = null, CancellationToken ct = default)
    {
        var query = _db.HrmCostCenters.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (orgUnitId.HasValue && orgUnitId.Value != Guid.Empty)
        {
            query = query.Where(c => c.OrgUnitId == orgUnitId.Value);
        }

        var items = await query.OrderBy(c => c.Code).ToListAsync(ct);
        var orgUnits = await _db.OrgUnits.AsNoTracking().Where(o => o.TenantId == tenantId).ToDictionaryAsync(o => o.Id, o => o.Name, ct);

        return items.Select(c => new HrmCostCenterDto(
            c.Id,
            c.Code,
            c.Name,
            c.OrgUnitId,
            c.OrgUnitId.HasValue && orgUnits.TryGetValue(c.OrgUnitId.Value, out var orgName) ? orgName : null,
            c.AllocationPercentage,
            c.IsActive,
            c.CreatedAt
        )).ToList();
    }

    public async Task<HrmCostCenterDto> GetCostCenterByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetCostCentersAsync(tenantId, null, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy trung tâm chi phí {id}", 404);
        return item;
    }

    public async Task<HrmCostCenterDto> CreateCostCenterAsync(Guid tenantId, HrmCostCenterUpsertRequest req, CancellationToken ct = default)
    {
        ValidateCostCenterRequest(req);

        var codeExists = await _db.HrmCostCenters.AnyAsync(c => c.TenantId == tenantId && c.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã trung tâm chi phí đã tồn tại.");

        var entity = new HrmCostCenter
        {
            TenantId = tenantId,
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            OrgUnitId = req.OrgUnitId,
            AllocationPercentage = req.AllocationPercentage,
            IsActive = req.IsActive
        };

        _db.HrmCostCenters.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetCostCenterByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<HrmCostCenterDto> UpdateCostCenterAsync(Guid tenantId, Guid id, HrmCostCenterUpsertRequest req, CancellationToken ct = default)
    {
        ValidateCostCenterRequest(req);

        var entity = await _db.HrmCostCenters.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy trung tâm chi phí {id}", 404);

        var codeExists = await _db.HrmCostCenters.AnyAsync(c => c.TenantId == tenantId && c.Id != id && c.Code.ToLower() == req.Code.Trim().ToLower(), ct);
        if (codeExists) throw new AppException("Mã trung tâm chi phí đã tồn tại.");

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.OrgUnitId = req.OrgUnitId;
        entity.AllocationPercentage = req.AllocationPercentage;
        entity.IsActive = req.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetCostCenterByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteCostCenterAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HrmCostCenters.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy trung tâm chi phí {id}", 404);

        _db.HrmCostCenters.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateCostCenterRequest(HrmCostCenterUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) throw new AppException("Mã trung tâm chi phí không được để trống.");
        if (string.IsNullOrWhiteSpace(req.Name)) throw new AppException("Tên trung tâm chi phí không được để trống.");
        if (req.AllocationPercentage < 0m || req.AllocationPercentage > 100m)
        {
            throw new AppException("Tỷ lệ phân bổ chi phí phải từ 0% đến 100%.");
        }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_023: Quản lý người thân / liên hệ khẩn
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmployeeRelativeDto>> GetRelativesAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _db.EmployeeRelatives.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (employeeId.HasValue && employeeId.Value != Guid.Empty)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }

        var items = await query.OrderByDescending(r => r.IsEmergencyContact).ThenBy(r => r.FullName).ToListAsync(ct);
        var empIds = items.Select(r => r.EmployeeId).Distinct().ToList();
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.EmployeeCode} - {e.FullName}", ct);

        return items.Select(r => new EmployeeRelativeDto(
            r.Id,
            r.EmployeeId,
            employees.TryGetValue(r.EmployeeId, out var empName) ? empName : null,
            r.FullName,
            r.Relationship,
            r.Phone,
            r.Address,
            r.IsEmergencyContact,
            r.IsTaxDependent,
            r.IdNumber,
            r.CreatedAt
        )).ToList();
    }

    public async Task<EmployeeRelativeDto> GetRelativeByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var list = await GetRelativesAsync(tenantId, null, ct);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) throw new AppException($"Không tìm thấy thông tin người thân {id}", 404);
        return item;
    }

    public async Task<EmployeeRelativeDto> CreateRelativeAsync(Guid tenantId, EmployeeRelativeUpsertRequest req, CancellationToken ct = default)
    {
        ValidateRelativeRequest(req);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        var entity = new EmployeeRelative
        {
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            FullName = req.FullName.Trim(),
            Relationship = NormalizeRelationship(req.Relationship),
            Phone = req.Phone?.Trim(),
            Address = req.Address?.Trim(),
            IsEmergencyContact = req.IsEmergencyContact,
            IsTaxDependent = req.IsTaxDependent,
            IdNumber = req.IdNumber?.Trim()
        };

        _db.EmployeeRelatives.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetRelativeByIdAsync(tenantId, entity.Id, ct);
    }

    public async Task<EmployeeRelativeDto> UpdateRelativeAsync(Guid tenantId, Guid id, EmployeeRelativeUpsertRequest req, CancellationToken ct = default)
    {
        ValidateRelativeRequest(req);

        var entity = await _db.EmployeeRelatives.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy thông tin người thân {id}", 404);

        var empExists = await _db.Employees.AnyAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (!empExists) throw new AppException($"Không tìm thấy nhân sự {req.EmployeeId}.", 404);

        entity.EmployeeId = req.EmployeeId;
        entity.FullName = req.FullName.Trim();
        entity.Relationship = NormalizeRelationship(req.Relationship);
        entity.Phone = req.Phone?.Trim();
        entity.Address = req.Address?.Trim();
        entity.IsEmergencyContact = req.IsEmergencyContact;
        entity.IsTaxDependent = req.IsTaxDependent;
        entity.IdNumber = req.IdNumber?.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetRelativeByIdAsync(tenantId, id, ct);
    }

    public async Task DeleteRelativeAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.EmployeeRelatives.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, ct);
        if (entity == null) throw new AppException($"Không tìm thấy thông tin người thân {id}", 404);

        _db.EmployeeRelatives.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateRelativeRequest(EmployeeRelativeUpsertRequest req)
    {
        if (req.EmployeeId == Guid.Empty) throw new AppException("Nhân sự không được để trống.");
        if (string.IsNullOrWhiteSpace(req.FullName)) throw new AppException("Họ tên người thân không được để trống.");
    }

    private static string NormalizeRelationship(string relationship)
    {
        var valid = new[] { "Spouse", "Child", "Parent", "Sibling", "Other" };
        var rel = valid.FirstOrDefault(v => string.Equals(v, relationship, StringComparison.OrdinalIgnoreCase));
        return rel ?? "Other";
    }
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Lms;

public sealed class LmsInstructorService : ILmsInstructorService
{
    public static readonly Guid RoleLmsInstructorId = Guid.Parse("33333333-3333-3333-3333-333333333303");

    private readonly AppDbContext _db;
    public LmsInstructorService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LmsInstructorDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LmsInstructors.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        if (list.Count == 0) return Array.Empty<LmsInstructorDto>();
        return await MapMany(tenantId, list, ct);
    }

    public async Task<LmsInstructorDto> UpsertAsync(
        Guid tenantId, Guid userId, LmsInstructorUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.DisplayName ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã GV 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên GV 1–200 ký tự.");

        Guid? employeeId = req.EmployeeId;
        Guid? linkedUserId = req.UserId;
        string? email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        string? phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();

        if (employeeId is Guid eid)
        {
            var emp = await _db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == eid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Nhân viên không tồn tại.", 404);
            linkedUserId ??= emp.UserId;
            email ??= emp.Email;
            phone ??= emp.Phone;
            if (string.IsNullOrWhiteSpace(name)) name = emp.FullName;
        }

        if (linkedUserId is Guid uid)
        {
            var exists = await _db.Users.AsNoTracking()
                .AnyAsync(x => x.Id == uid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!exists) throw new AppException("User liên kết không tồn tại.", 404);
        }

        LmsInstructor entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LmsInstructors.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Giảng viên không tồn tại.", 404);
        }
        else
        {
            if (await _db.LmsInstructors.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã giảng viên đã tồn tại.");
            entity = new LmsInstructor { TenantId = tenantId, CreatedBy = userId };
            _db.LmsInstructors.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.LmsInstructors.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã giảng viên đã tồn tại.");

        entity.Code = code;
        entity.DisplayName = name;
        entity.EmployeeId = employeeId;
        entity.UserId = linkedUserId;
        entity.Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim();
        entity.Specialty = string.IsNullOrWhiteSpace(req.Specialty) ? null : req.Specialty.Trim();
        entity.Bio = string.IsNullOrWhiteSpace(req.Bio) ? null : req.Bio.Trim();
        entity.Email = email;
        entity.Phone = phone;
        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var st = req.Status.Trim();
            if (st is not ("Active" or "Inactive")) throw new AppException("Status: Active | Inactive.");
            entity.Status = st;
        }
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.GrantInstructorRole)
            await GrantRoleInternalAsync(tenantId, userId, entity, ct);

        return (await MapMany(tenantId, new[] { entity }, ct))[0];
    }

    public async Task<LmsInstructorDto> SetStatusAsync(
        Guid tenantId, Guid userId, Guid id, string status, CancellationToken ct = default)
    {
        var st = (status ?? "").Trim();
        if (st is not ("Active" or "Inactive")) throw new AppException("Status: Active | Inactive.");
        var entity = await _db.LmsInstructors.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Giảng viên không tồn tại.", 404);
        entity.Status = st;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapMany(tenantId, new[] { entity }, ct))[0];
    }

    public async Task GrantRoleAsync(Guid tenantId, Guid actorId, Guid instructorId, CancellationToken ct = default)
    {
        var entity = await _db.LmsInstructors.FirstOrDefaultAsync(
            x => x.Id == instructorId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Giảng viên không tồn tại.", 404);
        await GrantRoleInternalAsync(tenantId, actorId, entity, ct);
    }

    private async Task GrantRoleInternalAsync(
        Guid tenantId, Guid actorId, LmsInstructor entity, CancellationToken ct)
    {
        if (entity.UserId is not Guid uid)
            throw new AppException("Giảng viên chưa liên kết User — không gán role được.");

        var role = await _db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == RoleLmsInstructorId && !x.IsDeleted, ct)
            ?? await _db.Roles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == "LMS_INSTRUCTOR" && !x.IsDeleted, ct)
            ?? throw new AppException("Role LMS_INSTRUCTOR chưa được seed.");

        var existing = await _db.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == uid && x.RoleId == role.Id && !x.IsDeleted, ct);
        if (existing is null)
        {
            _db.UserRoles.Add(new UserRole
            {
                TenantId = tenantId,
                UserId = uid,
                RoleId = role.Id,
                IsActive = true,
                AssignedBy = actorId,
                CreatedBy = actorId
            });
        }
        else if (!existing.IsActive)
        {
            existing.IsActive = true;
            existing.RevokedAt = null;
            existing.UpdatedBy = actorId;
        }

        entity.RoleGranted = true;
        entity.UpdatedBy = actorId;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<LmsInstructorDto>> MapMany(
        Guid tenantId, IReadOnlyList<LmsInstructor> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var eids = list.Where(x => x.EmployeeId.HasValue).Select(x => x.EmployeeId!.Value).Distinct().ToList();
        var emps = eids.Count == 0
            ? new Dictionary<Guid, (string Code, string Name)>()
            : await _db.Employees.AsNoTracking()
                .Where(x => eids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (Code: x.EmployeeCode, Name: x.FullName), ct);
        var classCounts = await _db.LmsTrainingClasses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.InstructorId != null && ids.Contains(x.InstructorId.Value))
            .GroupBy(x => x.InstructorId!.Value)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(x =>
        {
            emps.TryGetValue(x.EmployeeId ?? Guid.Empty, out var e);
            return new LmsInstructorDto(
                x.Id, x.Code, x.DisplayName, x.EmployeeId,
                x.EmployeeId is null ? null : e.Code,
                x.EmployeeId is null ? null : e.Name,
                x.UserId, x.Title, x.Specialty, x.Bio, x.Email, x.Phone,
                x.Status, x.RoleGranted, classCounts.GetValueOrDefault(x.Id));
        }).ToList();
    }
}

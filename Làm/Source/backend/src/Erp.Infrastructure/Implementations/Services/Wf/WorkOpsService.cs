using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mod;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Domain.Entities.Wf;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Wf;

public sealed class WorkOpsService : IWorkOpsService
{
    private readonly AppDbContext _db;

    public WorkOpsService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<WorkTypeDto>> ListTypesAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.WorkTypes.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new WorkTypeDto(x.Id, x.Code, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<WorkTypeDto> UpsertTypeAsync(Guid tenantId, Guid? actorId, WorkTypeDto req, CancellationToken ct = default)
    {
        WorkType e;
        if (req.Id != Guid.Empty && await _db.WorkTypes.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.WorkTypes.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new WorkType { TenantId = tenantId, CreatedBy = actorId }; _db.WorkTypes.Add(e); }
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new WorkTypeDto(e.Id, e.Code, e.Name, e.IsActive);
    }

    public async Task<IReadOnlyList<WorkProjectDto>> ListProjectsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.WorkProjects.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new WorkProjectDto(x.Id, x.Code, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<WorkProjectDto> UpsertProjectAsync(Guid tenantId, Guid? actorId, WorkProjectDto req, CancellationToken ct = default)
    {
        WorkProject e;
        if (req.Id != Guid.Empty && await _db.WorkProjects.AnyAsync(x => x.Id == req.Id, ct))
            e = await _db.WorkProjects.FirstAsync(x => x.Id == req.Id && x.TenantId == tenantId, ct);
        else { e = new WorkProject { TenantId = tenantId, CreatedBy = actorId }; _db.WorkProjects.Add(e); }
        e.Code = req.Code.Trim(); e.Name = req.Name.Trim(); e.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new WorkProjectDto(e.Id, e.Code, e.Name, e.IsActive);
    }

    public async Task<IReadOnlyList<WorkItemDto>> ListItemsAsync(Guid tenantId, Guid userId, string? status, Guid? assigneeId, CancellationToken ct = default)
    {
        var q = _db.WorkItems.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        if (assigneeId is Guid a) q = q.Where(x => x.AssigneeUserId == a);
        return await q.OrderByDescending(x => x.CreatedAt).Take(200)
            .Select(x => new WorkItemDto(x.Id, x.Kind, x.Title, x.Description, x.ProjectId, x.AssigneeUserId, x.ReporterUserId, x.DueAt, x.Status, x.Priority))
            .ToListAsync(ct);
    }

    public async Task<WorkItemDto> UpsertItemAsync(Guid tenantId, Guid actorId, WorkItemUpsertRequest req, CancellationToken ct = default)
    {
        WorkItem e;
        if (req.Id is Guid id)
            e = await _db.WorkItems.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Work item không tồn tại.", 404);
        else
        {
            e = new WorkItem { TenantId = tenantId, ReporterUserId = actorId, CreatedBy = actorId };
            _db.WorkItems.Add(e);
        }
        e.Kind = req.Kind; e.Title = req.Title.Trim(); e.Description = req.Description;
        e.ProjectId = req.ProjectId; e.AssigneeUserId = req.AssigneeUserId; e.DueAt = req.DueAt;
        e.Status = req.Status; e.Priority = req.Priority;
        await _db.SaveChangesAsync(ct);
        return new WorkItemDto(e.Id, e.Kind, e.Title, e.Description, e.ProjectId, e.AssigneeUserId, e.ReporterUserId, e.DueAt, e.Status, e.Priority);
    }

    public async Task<object> OpenWorkloadAsync(Guid tenantId, CancellationToken ct = default)
    {
        var open = await _db.WorkItems.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Done" && x.Status != "Cancelled", ct);
        var overdue = await _db.WorkItems.CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.DueAt < DateTimeOffset.UtcNow && x.Status != "Done" && x.Status != "Cancelled", ct);
        return new { open, overdue };
    }
}

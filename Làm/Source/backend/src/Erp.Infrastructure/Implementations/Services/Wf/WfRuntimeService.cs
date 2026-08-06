using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Wf;
using Erp.Application.Common;
using Erp.Application.Interfaces.Realtime;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Wf;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Wf;

public sealed class WfRuntimeService : IWfRuntimeService
{
    private readonly AppDbContext _db;
    private readonly IWfRealtimeNotifier _realtime;
    private readonly IOutboxWriter _outbox;

    public WfRuntimeService(AppDbContext db, IWfRealtimeNotifier realtime, IOutboxWriter outbox)
    {
        _db = db;
        _realtime = realtime;
        _outbox = outbox;
    }

    public async Task<Guid> StartAsync(
        Guid tenantId,
        string definitionCode,
        string sourceModule,
        string sourceDocType,
        Guid sourceDocId,
        Guid requesterUserId,
        Guid? assigneeUserId,
        CancellationToken ct = default)
    {
        var def = await _db.WfDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == definitionCode && x.IsActive && !x.IsDeleted, ct)
            ?? throw new AppException($"WF definition `{definitionCode}` không tồn tại.");

        var ver = await _db.WfDefinitionVersions.AsNoTracking()
            .Where(x => x.DefinitionId == def.Id && x.IsPublished && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNo)
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("WF chưa có version published.");

        var node = await _db.WfNodes.AsNoTracking()
            .Where(x => x.DefinitionVersionId == ver.Id && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("WF chưa có node duyệt.");

        var assignee = assigneeUserId
            ?? await ResolveManagerUserIdAsync(tenantId, requesterUserId, ct)
            ?? throw new AppException("Không tìm thấy người duyệt (manager).");

        // Ủy quyền: chuyển sang người nhận ủy quyền nếu đang hiệu lực
        assignee = await ResolveDelegateeAsync(tenantId, assignee, sourceModule, ct) ?? assignee;

        var instance = new WfInstance
        {
            TenantId = tenantId,
            DefinitionVersionId = ver.Id,
            SourceModule = sourceModule,
            SourceDocType = sourceDocType,
            SourceDocId = sourceDocId,
            Status = "Running",
            CurrentNodeId = node.Id,
            CreatedBy = requesterUserId
        };
        _db.WfInstances.Add(instance);
        await _db.SaveChangesAsync(ct);

        var task = new WfTask
        {
            TenantId = tenantId,
            InstanceId = instance.Id,
            NodeId = node.Id,
            AssigneeUserId = assignee,
            Status = "Pending",
            DueAt = DateTimeOffset.UtcNow.AddDays(3),
            CreatedBy = requesterUserId
        };
        _db.WfTasks.Add(task);
        await _db.SaveChangesAsync(ct);

        await _realtime.NotifyInboxChangedAsync(assignee, "task_assigned", task.Id, ct);
        return instance.Id;
    }

    public async Task<IReadOnlyList<WfTaskDto>> MyPendingTasksAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var delegatedFrom = await _db.WfDelegations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive
                        && x.ToUserId == userId
                        && x.StartDate <= today && x.EndDate >= today)
            .Select(x => new { x.FromUserId, x.ModuleCode })
            .ToListAsync(ct);

        var fromIds = delegatedFrom.Select(x => x.FromUserId).Distinct().ToList();
        fromIds.Add(userId);

        var rows = await (
            from t in _db.WfTasks.AsNoTracking()
            join i in _db.WfInstances.AsNoTracking() on t.InstanceId equals i.Id
            join n in _db.WfNodes.AsNoTracking() on t.NodeId equals n.Id into nj
            from n in nj.DefaultIfEmpty()
            where t.TenantId == tenantId && t.Status == "Pending" && !t.IsDeleted
                  && t.AssigneeUserId != null && fromIds.Contains(t.AssigneeUserId.Value)
            orderby t.DueAt
            select new { t, i, NodeName = n != null ? n.Name : null }
        ).ToListAsync(ct);

        // Lọc theo module ủy quyền (nếu chỉ định)
        rows = rows.Where(r =>
        {
            if (r.t.AssigneeUserId == userId) return true;
            return delegatedFrom.Any(d =>
                d.FromUserId == r.t.AssigneeUserId
                && (d.ModuleCode == null || d.ModuleCode == r.i.SourceModule));
        }).ToList();

        var assigneeIds = rows.Where(x => x.t.AssigneeUserId is not null)
            .Select(x => x.t.AssigneeUserId!.Value).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(x => assigneeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var result = new List<WfTaskDto>();
        foreach (var r in rows)
        {
            var summary = await BuildDocSummaryAsync(r.i.SourceModule, r.i.SourceDocType, r.i.SourceDocId, ct);
            var aid = r.t.AssigneeUserId;
            result.Add(new WfTaskDto(
                r.t.Id, r.t.InstanceId, r.t.NodeId, r.NodeName, r.t.Status, r.t.DueAt,
                r.i.SourceModule, r.i.SourceDocType, r.i.SourceDocId, summary,
                aid, aid is Guid a ? users.GetValueOrDefault(a) : null,
                ViaDelegation: aid is Guid ag && ag != userId));
        }

        return result;
    }

    public async Task ActAsync(Guid tenantId, Guid taskId, Guid actorUserId, WfActRequest req, CancellationToken ct = default)
    {
        var action = (req.Action ?? "").Trim();
        if (action is not ("Approve" or "Reject"))
            throw new AppException("Action phải là Approve hoặc Reject.");

        var task = await _db.WfTasks.FirstOrDefaultAsync(
            x => x.Id == taskId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Task không tồn tại.", 404);

        if (task.Status != "Pending")
            throw new AppException("Task đã xử lý.");

        var instance = await _db.WfInstances.FirstAsync(x => x.Id == task.InstanceId, ct);
        var canAct = task.AssigneeUserId == actorUserId
                     || (task.AssigneeUserId is Guid assignee
                         && await IsActiveDelegateAsync(tenantId, assignee, actorUserId, instance.SourceModule, ct));
        if (!canAct)
            throw new AppException("Bạn không phải người được gán duyệt / ủy quyền.", 403);

        task.Status = action == "Approve" ? "Approved" : "Rejected";
        task.UpdatedBy = actorUserId;

        _db.WfTaskActions.Add(new WfTaskAction
        {
            TenantId = tenantId,
            TaskId = task.Id,
            ActorUserId = actorUserId,
            Action = action,
            Comment = req.Comment
        });

        instance.Status = action == "Approve" ? "Completed" : "Rejected";
        instance.UpdatedBy = actorUserId;

        if (instance.SourceModule == "HRM" && instance.SourceDocType == "leave_request")
        {
            await ApplyLeaveDecisionAsync(tenantId, instance.SourceDocId, action == "Approve", ct);
            await _outbox.EnqueueAsync(
                tenantId,
                action == "Approve" ? "hrm.leave.approved" : "hrm.leave.rejected",
                "HRM",
                new
                {
                    leaveRequestId = instance.SourceDocId,
                    taskId = task.Id,
                    actorUserId,
                    action,
                    correlationId = CorrelationContext.Current
                },
                CorrelationContext.Current,
                ct);
        }

        if (instance.SourceModule == "HRM" && instance.SourceDocType == "recruitment_request")
        {
            await ApplyRecruitDecisionAsync(tenantId, instance.SourceDocId, action == "Approve", ct);
            await _outbox.EnqueueAsync(
                tenantId,
                action == "Approve" ? "hrm.recruit.approved" : "hrm.recruit.rejected",
                "HRM",
                new
                {
                    recruitmentRequestId = instance.SourceDocId,
                    taskId = task.Id,
                    actorUserId,
                    action,
                    correlationId = CorrelationContext.Current
                },
                CorrelationContext.Current,
                ct);
        }

        await _db.SaveChangesAsync(ct);

        await _realtime.NotifyInboxChangedAsync(actorUserId, "task_acted", task.Id, ct);
        if (instance.CreatedBy is Guid requester && requester != actorUserId)
            await _realtime.NotifyInboxChangedAsync(requester, "task_acted", task.Id, ct);
    }

    private async Task ApplyLeaveDecisionAsync(Guid tenantId, Guid leaveId, bool approved, CancellationToken ct)
    {
        var leave = await _db.LeaveRequests.FirstOrDefaultAsync(
            x => x.Id == leaveId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (leave is null) return;

        leave.Status = approved ? "Approved" : "Rejected";
        if (!approved) return;

        var year = leave.FromDate.Year;
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.EmployeeId == leave.EmployeeId
                 && x.LeaveTypeId == leave.LeaveTypeId && x.Year == year && !x.IsDeleted, ct);
        if (bal is null) return;

        bal.Used += leave.Days;
        bal.Remaining = bal.Entitled - bal.Used;
    }

    private async Task ApplyRecruitDecisionAsync(Guid tenantId, Guid requestId, bool approved, CancellationToken ct)
    {
        var row = await _db.RecruitmentRequests.FirstOrDefaultAsync(
            x => x.Id == requestId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (row is null) return;
        row.Status = approved ? "Approved" : "Rejected";
    }

    private async Task<Guid?> ResolveManagerUserIdAsync(Guid tenantId, Guid requesterUserId, CancellationToken ct)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == requesterUserId && !x.IsDeleted, ct);
        if (emp?.ManagerEmployeeId is Guid mid)
        {
            var mgrUser = await _db.Employees.AsNoTracking()
                .Where(x => x.Id == mid && !x.IsDeleted)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync(ct);
            if (mgrUser is Guid uid) return uid;
        }

        if (emp?.DepartmentId is Guid did)
        {
            var deptMgr = await _db.Departments.AsNoTracking()
                .Where(x => x.Id == did)
                .Select(x => x.ManagerUserId)
                .FirstOrDefaultAsync(ct);
            if (deptMgr is Guid dmu) return dmu;
        }

        // fallback admin SUPER_ADMIN
        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.TenantId == tenantId && ur.IsActive && r.Code == "SUPER_ADMIN"
            select ur.UserId
        ).FirstOrDefaultAsync(ct);
    }

    private async Task<string?> BuildDocSummaryAsync(string module, string docType, Guid docId, CancellationToken ct)
    {
        if (module == "HRM" && docType == "leave_request")
        {
            var leave = await (
                from lr in _db.LeaveRequests.AsNoTracking()
                join e in _db.Employees.AsNoTracking() on lr.EmployeeId equals e.Id
                join lt in _db.LeaveTypes.AsNoTracking() on lr.LeaveTypeId equals lt.Id
                where lr.Id == docId
                select new { e.FullName, lt.Name, lr.FromDate, lr.ToDate, lr.Days }
            ).FirstOrDefaultAsync(ct);
            if (leave is null) return null;
            return $"{leave.FullName} · {leave.Name} · {leave.FromDate:dd/MM}–{leave.ToDate:dd/MM} ({leave.Days}d)";
        }

        if (module == "HRM" && docType == "recruitment_request")
        {
            var row = await (
                from rr in _db.RecruitmentRequests.AsNoTracking()
                join jt in _db.JobTitles.AsNoTracking() on rr.JobTitleId equals jt.Id
                where rr.Id == docId
                select new { rr.DocNo, jt.Name, rr.Headcount }
            ).FirstOrDefaultAsync(ct);
            if (row is null) return null;
            return $"Tuyển {row.DocNo} · {row.Name} ×{row.Headcount}";
        }

        return $"{module}/{docType}";
    }

    public async Task<IReadOnlyList<WfDelegationDto>> ListDelegationsAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var rows = await _db.WfDelegations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.FromUserId == userId || x.ToUserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return await MapDelegationsAsync(rows, ct);
    }

    public async Task<WfDelegationDto> UpsertDelegationAsync(
        Guid tenantId, Guid userId, WfDelegationUpsertRequest req, CancellationToken ct = default)
    {
        if (req.ToUserId == userId) throw new AppException("Không ủy quyền cho chính mình.");
        if (req.EndDate < req.StartDate) throw new AppException("Khoảng ngày không hợp lệ.");
        if (!await _db.Users.AnyAsync(x => x.Id == req.ToUserId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Người nhận ủy quyền không hợp lệ.", 404);

        WfDelegation e;
        if (req.Id is Guid id)
        {
            e = await _db.WfDelegations.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.FromUserId == userId && !x.IsDeleted, ct)
                ?? throw new AppException("Ủy quyền không tồn tại.", 404);
        }
        else
        {
            e = new WfDelegation
            {
                TenantId = tenantId,
                FromUserId = userId,
                CreatedBy = userId
            };
            _db.WfDelegations.Add(e);
        }

        e.ToUserId = req.ToUserId;
        e.StartDate = req.StartDate;
        e.EndDate = req.EndDate;
        e.ModuleCode = string.IsNullOrWhiteSpace(req.ModuleCode) ? null : req.ModuleCode.Trim().ToUpperInvariant();
        e.IsActive = req.IsActive;
        e.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapDelegationsAsync(new[] { e }, ct))[0];
    }

    public async Task DeactivateDelegationAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var e = await _db.WfDelegations.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.FromUserId == userId && !x.IsDeleted, ct)
            ?? throw new AppException("Ủy quyền không tồn tại.", 404);
        e.IsActive = false;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<WfDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Now);
        var startToday = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var tasks = await _db.WfTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var instances = await _db.WfInstances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).ToListAsync(ct);
        var actions = await _db.WfTaskActions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.CreatedAt >= startToday)
            .ToListAsync(ct);

        var pending = tasks.Count(x => x.Status == "Pending");
        var overdue = tasks.Count(x => x.Status == "Pending" && x.DueAt is DateTimeOffset d && d < now);
        var completedToday = actions.Count(x => x.Action == "Approve");
        var rejectedToday = actions.Count(x => x.Action == "Reject");

        var instById = instances.ToDictionary(x => x.Id);
        var byModule = tasks
            .Where(t => instById.ContainsKey(t.InstanceId))
            .GroupBy(t => instById[t.InstanceId].SourceModule)
            .Select(g => new WfModuleStatDto(
                g.Key,
                g.Count(x => x.Status == "Pending"),
                g.Count(x => x.Status == "Approved"),
                g.Count(x => x.Status == "Rejected")))
            .OrderByDescending(x => x.Pending)
            .ToList();

        var last7 = new List<WfDailyStatDto>();
        for (var i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var dayStart = new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);
            var dayActs = await _db.WfTaskActions.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd)
                .ToListAsync(ct);
            last7.Add(new WfDailyStatDto(
                d, dayActs.Count(x => x.Action == "Approve"), dayActs.Count(x => x.Action == "Reject")));
        }

        var topAssigneeIds = tasks
            .Where(x => x.Status == "Pending" && x.AssigneeUserId is not null)
            .GroupBy(x => x.AssigneeUserId!.Value)
            .OrderByDescending(g => g.Count())
            .Take(8)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToList();
        var userIds = topAssigneeIds.Select(x => x.UserId).ToList();
        var users = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        var top = topAssigneeIds
            .Select(x => new WfAssigneeLoadDto(x.UserId, users.GetValueOrDefault(x.UserId, "?"), x.Count))
            .ToList();

        return new WfDashboardDto(
            pending, overdue, completedToday, rejectedToday,
            instances.Count(x => x.Status == "Running"),
            instances.Count(x => x.Status == "Completed"),
            instances.Count(x => x.Status == "Rejected"),
            byModule, last7, top);
    }

    private async Task<Guid?> ResolveDelegateeAsync(
        Guid tenantId, Guid fromUserId, string moduleCode, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var d = await _db.WfDelegations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive
                        && x.FromUserId == fromUserId
                        && x.StartDate <= today && x.EndDate >= today
                        && (x.ModuleCode == null || x.ModuleCode == moduleCode))
            .OrderByDescending(x => x.ModuleCode != null)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return d?.ToUserId;
    }

    private async Task<bool> IsActiveDelegateAsync(
        Guid tenantId, Guid fromUserId, Guid toUserId, string moduleCode, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _db.WfDelegations.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive
                 && x.FromUserId == fromUserId && x.ToUserId == toUserId
                 && x.StartDate <= today && x.EndDate >= today
                 && (x.ModuleCode == null || x.ModuleCode == moduleCode), ct);
    }

    private async Task<IReadOnlyList<WfDelegationDto>> MapDelegationsAsync(
        IReadOnlyList<WfDelegation> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<WfDelegationDto>();
        var ids = rows.Select(x => x.FromUserId).Concat(rows.Select(x => x.ToUserId)).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        return rows.Select(d => new WfDelegationDto(
            d.Id, d.FromUserId, users.GetValueOrDefault(d.FromUserId, "?"),
            d.ToUserId, users.GetValueOrDefault(d.ToUserId, "?"),
            d.StartDate, d.EndDate, d.ModuleCode, d.IsActive, d.Note, d.CreatedAt)).ToList();
    }
}

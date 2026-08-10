using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Entities.Wf;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmRecruitService : IHrmRecruitService
{
    private readonly AppDbContext _db;
    private readonly IWfRuntimeService _wf;

    public HrmRecruitService(AppDbContext db, IWfRuntimeService wf)
    {
        _db = db;
        _wf = wf;
    }

    public async Task<IReadOnlyList<RecruitmentRequestDto>> ListAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var canManageAll = await HasPermAsync(tenantId, userId, "hrm.recruit.manage", ct);
        var q = _db.RecruitmentRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!canManageAll)
            q = q.Where(x => x.RequestedByUserId == userId);

        var rows = await q.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return await MapManyAsync(tenantId, rows, ct);
    }

    public async Task<RecruitmentRequestDto> CreateAsync(
        Guid tenantId, Guid userId, RecruitmentRequestCreateRequest req, CancellationToken ct = default)
    {
        if (req.Headcount < 1 || req.Headcount > 999)
            throw new AppException("Số lượng tuyển phải từ 1–999.");
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length == 0) throw new AppException("Nhập lý do tuyển dụng.");
        if (reason.Length < 5) throw new AppException("Lý do tuyển dụng quá ngắn (tối thiểu 5 ký tự).");
        if (reason.Length > 1000) throw new AppException("Lý do tối đa 1000 ký tự.");

        _ = await _db.JobTitles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.JobTitleId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Vị trí không hợp lệ.", 404);
        _ = await _db.OrgUnits.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.OrgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Đơn vị không hợp lệ.", 404);

        var docNo = await NextDocNoAsync(tenantId, ct);
        var entity = new RecruitmentRequest
        {
            TenantId = tenantId,
            DocNo = docNo,
            JobTitleId = req.JobTitleId,
            Headcount = req.Headcount,
            Reason = reason,
            OrgUnitId = req.OrgUnitId,
            Status = "Draft",
            RequestedByUserId = userId,
            CreatedBy = userId
        };
        _db.RecruitmentRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        if (req.Submit)
            await SubmitInternalAsync(tenantId, userId, entity, ct);

        return (await MapManyAsync(tenantId, new[] { entity }, ct))[0];
    }

    public async Task<RecruitmentRequestDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.RecruitmentRequests.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu không tồn tại.", 404);
        if (entity.RequestedByUserId != userId && !await HasPermAsync(tenantId, userId, "hrm.recruit.manage", ct))
            throw new ForbiddenException("Không có quyền gửi phiếu này.");
        if (entity.Status != "Draft")
            throw new AppException("Chỉ gửi duyệt phiếu ở trạng thái Draft.");

        await SubmitInternalAsync(tenantId, userId, entity, ct);
        return (await MapManyAsync(tenantId, new[] { entity }, ct))[0];
    }

    public async Task<RecruitmentRequestDto> CancelOrCloseAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.RecruitmentRequests.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu không tồn tại.", 404);
        if (entity.RequestedByUserId != userId && !await HasPermAsync(tenantId, userId, "hrm.recruit.manage", ct))
            throw new ForbiddenException("Không có quyền đóng/hủy phiếu này.");

        if (entity.Status == "Pending")
            throw new AppException("Phiếu đang chờ duyệt, vui lòng rút lại hoặc chờ xử lý trước khi đóng/hủy.", 400);

        if (entity.Status is "Closed" or "Cancelled")
            throw new AppException("Phiếu đề xuất này đã được đóng hoặc hủy từ trước.", 400);

        entity.Status = entity.Status switch
        {
            "Draft" => "Cancelled",
            "Rejected" => "Closed",
            "Approved" => "Closed",
            _ => throw new AppException("Chỉ đóng/hủy phiếu Draft, Rejected hoặc Approved.")
        };
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { entity }, ct))[0];
    }

    // ─── UC_HRM_051 — Duyệt / từ chối đề xuất ───

    public async Task<RecruitmentRequestDto> ApproveOrRejectAsync(
        Guid tenantId, Guid userId, Guid id, ApproveRecruitmentRequest req, CancellationToken ct = default)
    {
        var entity = await _db.RecruitmentRequests.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu đề xuất không tồn tại.", 404);

        if (entity.Status != "Pending")
            throw new AppException("Chỉ có thể duyệt hoặc từ chối phiếu đang ở trạng thái Chờ duyệt (Pending).", 400);

        var action = (req.Action ?? "").Trim().ToUpperInvariant();
        if (action != "APPROVE" && action != "REJECT")
            throw new AppException("Hành động phê duyệt không hợp lệ (chỉ chấp nhận Approve hoặc Reject).", 400);

        if (action == "REJECT" && string.IsNullOrWhiteSpace(req.Comment))
            throw new AppException("Vui lòng nhập lý do khi từ chối phiếu đề xuất.", 400);

        entity.Status = action == "APPROVE" ? "Approved" : "Rejected";
        entity.UpdatedBy = userId;

        if (entity.WfInstanceId is Guid wfId)
        {
            var task = await _db.WfTasks.FirstOrDefaultAsync(t => t.InstanceId == wfId && t.TenantId == tenantId && !t.IsDeleted, ct);
            if (task is null)
            {
                task = new WfTask
                {
                    TenantId = tenantId,
                    InstanceId = wfId,
                    NodeId = Guid.NewGuid(),
                    AssigneeUserId = userId,
                    Status = "Completed"
                };
                _db.WfTasks.Add(task);
                await _db.SaveChangesAsync(ct);
            }

            var act = new WfTaskAction
            {
                TenantId = tenantId,
                TaskId = task.Id,
                ActorUserId = userId,
                Action = action == "APPROVE" ? "Approved" : "Rejected",
                Comment = req.Comment?.Trim(),
                CreatedBy = userId
            };
            _db.WfTaskActions.Add(act);
        }

        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { entity }, ct))[0];
    }

    // ─── UC_HRM_052 — Xem lịch sử duyệt đề xuất ───

    public async Task<IReadOnlyList<RecruitmentApprovalStepDto>> GetApprovalHistoryAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.RecruitmentRequests.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Phiếu đề xuất không tồn tại.", 404);

        if (entity.WfInstanceId is not Guid instanceId)
            return Array.Empty<RecruitmentApprovalStepDto>();

        return await (
            from a in _db.WfTaskActions.AsNoTracking()
            join t in _db.WfTasks.AsNoTracking() on a.TaskId equals t.Id
            join u in _db.Users.AsNoTracking() on a.ActorUserId equals u.Id
            where a.TenantId == tenantId && t.InstanceId == instanceId && !a.IsDeleted
            orderby a.CreatedAt
            select new RecruitmentApprovalStepDto(
                a.Id, a.ActorUserId, u.DisplayName ?? u.Username, a.Action, a.Comment, a.CreatedAt)
        ).ToListAsync(ct);
    }


    private async Task SubmitInternalAsync(Guid tenantId, Guid userId, RecruitmentRequest entity, CancellationToken ct)
    {
        var instanceId = await _wf.StartAsync(
            tenantId, "RECRUIT_APPROVE", "HRM", "recruitment_request", entity.Id, userId, null, ct);
        entity.Status = "Pending";
        entity.WfInstanceId = instanceId;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<string> NextDocNoAsync(Guid tenantId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await _db.NumberSequences.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.DocType == "HRM.RECRUIT" && !x.IsDeleted, ct);
        if (seq is null)
        {
            seq = new NumberSequence
            {
                TenantId = tenantId,
                DocType = "HRM.RECRUIT",
                Pattern = "TD-{yyyy}-{seq:4}",
                NextValue = 1,
                ResetYear = year
            };
            _db.NumberSequences.Add(seq);
        }
        else if (seq.ResetYear != year)
        {
            seq.ResetYear = year;
            seq.NextValue = 1;
        }

        var n = seq.NextValue;
        seq.NextValue = n + 1;
        await _db.SaveChangesAsync(ct);
        return $"TD-{year}-{n:D4}";
    }

    private async Task<IReadOnlyList<RecruitmentRequestDto>> MapManyAsync(
        Guid tenantId, IReadOnlyList<RecruitmentRequest> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<RecruitmentRequestDto>();

        var jtIds = rows.Select(r => r.JobTitleId).Distinct().ToList();
        var ouIds = rows.Select(r => r.OrgUnitId).Distinct().ToList();
        var userIds = rows.Select(r => r.RequestedByUserId).Distinct().ToList();
        var instanceIds = rows.Where(r => r.WfInstanceId is not null).Select(r => r.WfInstanceId!.Value).Distinct().ToList();

        var titles = await _db.JobTitles.AsNoTracking()
            .Where(x => jtIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var orgs = await _db.OrgUnits.AsNoTracking()
            .Where(x => ouIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = await _db.Users.AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var historyByInstance = new Dictionary<Guid, List<RecruitmentApprovalStepDto>>();
        if (instanceIds.Count > 0)
        {
            var actions = await (
                from a in _db.WfTaskActions.AsNoTracking()
                join t in _db.WfTasks.AsNoTracking() on a.TaskId equals t.Id
                join u in _db.Users.AsNoTracking() on a.ActorUserId equals u.Id
                where a.TenantId == tenantId && instanceIds.Contains(t.InstanceId) && !a.IsDeleted
                orderby a.CreatedAt
                select new
                {
                    t.InstanceId,
                    Step = new RecruitmentApprovalStepDto(
                        a.Id, a.ActorUserId, u.DisplayName ?? u.Username, a.Action, a.Comment, a.CreatedAt)
                }
            ).ToListAsync(ct);

            foreach (var g in actions.GroupBy(x => x.InstanceId))
                historyByInstance[g.Key] = g.Select(x => x.Step).ToList();
        }

        return rows.Select(r => new RecruitmentRequestDto(
            r.Id, r.DocNo, r.JobTitleId, titles.GetValueOrDefault(r.JobTitleId, "?"),
            r.Headcount, r.Reason, r.OrgUnitId, orgs.GetValueOrDefault(r.OrgUnitId, "?"),
            r.Status, r.WfInstanceId, r.RequestedByUserId,
            users.GetValueOrDefault(r.RequestedByUserId, "?"), r.CreatedAt,
            r.WfInstanceId is Guid wid && historyByInstance.TryGetValue(wid, out var h)
                ? h
                : Array.Empty<RecruitmentApprovalStepDto>()
        )).ToList();
    }

    private async Task<bool> HasPermAsync(Guid tenantId, Guid userId, string code, CancellationToken ct)
    {
        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            join rp in _db.RolePermissions.AsNoTracking() on r.Id equals rp.RoleId
            join p in _db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id
            where ur.TenantId == tenantId && ur.UserId == userId && ur.IsActive
                  && !ur.IsDeleted && !r.IsDeleted && !rp.IsDeleted && !p.IsDeleted
                  && (r.BypassDataScope || p.Code == code)
            select p.Id
        ).AnyAsync(ct);
    }
}

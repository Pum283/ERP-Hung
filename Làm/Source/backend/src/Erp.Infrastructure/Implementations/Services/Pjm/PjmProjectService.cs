using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pjm;
using Erp.Application.Interfaces.Services.Pjm;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pjm;

public sealed class PjmProjectService : IPjmProjectService
{
    private static readonly (string Code, string Name, int Sort, bool Terminal)[] DefaultStatuses =
    [
        ("Draft", "Nháp", 10, false),
        ("Active", "Đang triển khai", 20, false),
        ("OnHold", "Tạm dừng", 30, false),
        ("Completed", "Hoàn thành", 40, true),
        ("Cancelled", "Hủy", 50, true),
    ];

    private readonly AppDbContext _db;
    public PjmProjectService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PjmProjectTypeDto>> ListTypesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PjmProjectTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new PjmProjectTypeDto(x.Id, x.Code, x.Name, x.Status, x.Note)).ToList();
    }

    public async Task<PjmProjectTypeDto> UpsertTypeAsync(
        Guid tenantId, Guid userId, PjmProjectTypeUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên loại DA");
        var status = ActiveInactive(req.Status);

        PjmProjectType entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmProjectTypes.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy loại DA.");
        }
        else
        {
            if (await _db.PjmProjectTypes.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã loại DA đã tồn tại.");
            entity = new PjmProjectType { TenantId = tenantId, CreatedBy = userId };
            _db.PjmProjectTypes.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Status = status;
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PjmProjectTypeDto(entity.Id, entity.Code, entity.Name, entity.Status, entity.Note);
    }

    public async Task EnsureDefaultStatusesAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var existing = await _db.PjmProjectStatuses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => x.Code).ToListAsync(ct);
        var have = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var s in DefaultStatuses)
        {
            if (have.Contains(s.Code)) continue;
            _db.PjmProjectStatuses.Add(new PjmProjectStatus
            {
                TenantId = tenantId, Code = s.Code, Name = s.Name,
                SortOrder = s.Sort, IsTerminal = s.Terminal, IsActive = true, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PjmProjectStatusDto>> ListStatusesAsync(Guid tenantId, CancellationToken ct = default)
    {
        await EnsureDefaultStatusesAsync(tenantId, Guid.Empty, ct);
        var list = await _db.PjmProjectStatuses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new PjmProjectStatusDto(
            x.Id, x.Code, x.Name, x.SortOrder, x.IsTerminal, x.IsActive)).ToList();
    }

    public async Task<PjmProjectStatusDto> UpsertStatusAsync(
        Guid tenantId, Guid userId, PjmProjectStatusUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 100, "Tên trạng thái");

        PjmProjectStatus entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmProjectStatuses.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy trạng thái.");
        }
        else
        {
            if (await _db.PjmProjectStatuses.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã trạng thái đã tồn tại.");
            entity = new PjmProjectStatus { TenantId = tenantId, CreatedBy = userId };
            _db.PjmProjectStatuses.Add(entity);
        }

        entity.Code = code; entity.Name = name;
        entity.SortOrder = req.SortOrder ?? entity.SortOrder;
        entity.IsTerminal = req.IsTerminal ?? entity.IsTerminal;
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PjmProjectStatusDto(entity.Id, entity.Code, entity.Name, entity.SortOrder, entity.IsTerminal, entity.IsActive);
    }

    public async Task<IReadOnlyList<PjmWbsTemplateDto>> ListTemplatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PjmWbsTemplates.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return await MapTemplatesAsync(tenantId, list, ct);
    }

    public async Task<PjmWbsTemplateDetailDto> GetTemplateDetailAsync(
        Guid tenantId, Guid templateId, CancellationToken ct = default)
    {
        var t = await _db.PjmWbsTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == templateId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy mẫu WBS.", 404);
        var dto = (await MapTemplatesAsync(tenantId, [t], ct))[0];
        var items = await _db.PjmWbsTemplateItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TemplateId == templateId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .Select(x => new PjmWbsTemplateItemDto(x.Id, x.TemplateId, x.Code, x.Name, x.ParentItemId, x.SortOrder))
            .ToListAsync(ct);
        return new PjmWbsTemplateDetailDto(dto, items);
    }

    public async Task<PjmWbsTemplateDto> UpsertTemplateAsync(
        Guid tenantId, Guid userId, PjmWbsTemplateUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên mẫu WBS");
        var status = ActiveInactive(req.Status);

        PjmWbsTemplate entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmWbsTemplates.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy mẫu WBS.");
        }
        else
        {
            if (await _db.PjmWbsTemplates.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã mẫu WBS đã tồn tại.");
            entity = new PjmWbsTemplate { TenantId = tenantId, CreatedBy = userId };
            _db.PjmWbsTemplates.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Status = status;
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTemplatesAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PjmWbsTemplateItemDto> UpsertTemplateItemAsync(
        Guid tenantId, Guid userId, Guid templateId, PjmWbsTemplateItemUpsertRequest req, CancellationToken ct = default)
    {
        var ok = await _db.PjmWbsTemplates.AnyAsync(
            x => x.Id == templateId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Không tìm thấy mẫu WBS.");

        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên hạng mục");

        PjmWbsTemplateItem item;
        if (req.Id is Guid id)
        {
            item = await _db.PjmWbsTemplateItems.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.TemplateId == templateId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy hạng mục mẫu.");
        }
        else
        {
            item = new PjmWbsTemplateItem { TenantId = tenantId, TemplateId = templateId, CreatedBy = userId };
            _db.PjmWbsTemplateItems.Add(item);
        }

        item.Code = code; item.Name = name;
        item.ParentItemId = req.ParentItemId;
        item.SortOrder = req.SortOrder ?? item.SortOrder;
        item.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PjmWbsTemplateItemDto(item.Id, item.TemplateId, item.Code, item.Name, item.ParentItemId, item.SortOrder);
    }

    public async Task<IReadOnlyList<PjmProjectDto>> ListProjectsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        await EnsureDefaultStatusesAsync(tenantId, Guid.Empty, ct);
        var query = _db.PjmProjects.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.Name.Contains(term)
                || (x.CustomerName != null && x.CustomerName.Contains(term))
                || (x.SourceOpportunityCode != null && x.SourceOpportunityCode.Contains(term)));
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapProjectsAsync(tenantId, list, ct);
    }

    public async Task<PjmProjectDetailDto> GetProjectDetailAsync(
        Guid tenantId, Guid projectId, CancellationToken ct = default)
    {
        var p = await _db.PjmProjects.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy dự án.", 404);
        var dto = (await MapProjectsAsync(tenantId, [p], ct))[0];

        var members = await _db.PjmProjectMembers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .ToListAsync(ct);
        var userIds = members.Select(x => x.UserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        var memberDtos = members.Select(m => new PjmProjectMemberDto(
            m.Id, m.ProjectId, m.UserId, users.GetValueOrDefault(m.UserId), m.Role, m.IsActive,
            m.AllocationPct, m.FromDate, m.ToDate)).ToList();

        var wbs = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var wbsDtos = wbs.Select(x => MapWbs(x, now)).ToList();

        var expenses = await _db.PjmExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.ExpenseDate).Take(100).ToListAsync(ct);
        var expenseDtos = expenses.Select(MapExpense).ToList();

        var issues = await _db.PjmMaterialIssues.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync(ct);
        var issueDtos = issues.Select(MapIssue).ToList();

        var acceptances = await _db.PjmAcceptances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        var acceptanceDtos = acceptances.Select(MapAcceptance).ToList();

        var summary = BuildCostSummary(p, expenses, issues, acceptances);
        return new PjmProjectDetailDto(dto, memberDtos, wbsDtos, expenseDtos, issueDtos, acceptanceDtos, summary);
    }

    public async Task<PjmProjectDto> UpsertProjectAsync(
        Guid tenantId, Guid userId, PjmProjectUpsertRequest req, CancellationToken ct = default)
    {
        await EnsureDefaultStatusesAsync(tenantId, userId, ct);
        var name = Req(req.Name, 200, "Tên dự án");
        var statusCode = string.IsNullOrWhiteSpace(req.StatusCode) ? "Draft" : req.StatusCode.Trim();
        var statusOk = await _db.PjmProjectStatuses.AnyAsync(
            x => x.TenantId == tenantId && x.Code == statusCode && x.IsActive && !x.IsDeleted, ct);
        if (!statusOk) throw new AppException("Trạng thái dự án không hợp lệ.");

        if (req.ProjectTypeId is Guid tid)
        {
            var ok = await _db.PjmProjectTypes.AnyAsync(
                x => x.Id == tid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Loại dự án không hợp lệ.");
        }

        if (req.StartDate is DateTimeOffset s && req.EndDate is DateTimeOffset e && e < s)
            throw new AppException("Ngày kết thúc phải ≥ ngày bắt đầu.");

        string? pmName = NullIfEmpty(req.PmName);
        if (req.PmUserId is Guid pmid)
        {
            var u = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == pmid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("PM không hợp lệ.");
            pmName ??= u.DisplayName ?? u.Username;
        }

        PjmProject entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmProjects.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy dự án.");
            if (!string.IsNullOrWhiteSpace(req.Code))
            {
                var code = NormCode(req.Code);
                if (await _db.PjmProjects.AnyAsync(
                        x => x.TenantId == tenantId && x.Code == code && x.Id != id && !x.IsDeleted, ct))
                    throw new AppException("Mã dự án đã tồn tại.");
                entity.Code = code;
            }
        }
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? await NextCodeAsync(tenantId, ct)
                : NormCode(req.Code);
            if (await _db.PjmProjects.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã dự án đã tồn tại.");
            entity = new PjmProject
            {
                TenantId = tenantId, Code = code, CreatedByUserId = userId, CreatedBy = userId
            };
            _db.PjmProjects.Add(entity);
        }

        entity.Name = name;
        entity.ProjectTypeId = req.ProjectTypeId;
        entity.StatusCode = statusCode;
        entity.CustomerName = NullIfEmpty(req.CustomerName);
        entity.ContractCode = NullIfEmpty(req.ContractCode)?.ToUpperInvariant();
        entity.SourceOpportunityCode = NullIfEmpty(req.SourceOpportunityCode)?.ToUpperInvariant();
        entity.PmUserId = req.PmUserId;
        entity.PmName = pmName;
        entity.Budget = req.Budget ?? entity.Budget;
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        // Gán PM vào members
        if (entity.PmUserId is Guid pmUserId)
        {
            var mem = await _db.PjmProjectMembers.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ProjectId == entity.Id && x.UserId == pmUserId && !x.IsDeleted, ct);
            if (mem is null)
            {
                _db.PjmProjectMembers.Add(new PjmProjectMember
                {
                    TenantId = tenantId, ProjectId = entity.Id, UserId = pmUserId,
                    Role = "PM", IsActive = true, CreatedBy = userId
                });
            }
            else
            {
                mem.Role = "PM"; mem.IsActive = true; mem.UpdatedBy = userId;
            }
            await _db.SaveChangesAsync(ct);
        }

        // Áp mẫu WBS khi tạo mới
        if (req.Id is null && req.ApplyTemplateId is Guid tmplId)
            await ApplyTemplateAsync(tenantId, userId, entity.Id, tmplId, ct);

        return (await MapProjectsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PjmProjectMemberDto> UpsertMemberAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmProjectMemberUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireProject(tenantId, projectId, ct);
        var role = string.IsNullOrWhiteSpace(req.Role) ? "Member" : req.Role.Trim();
        if (role is not ("PM" or "Member" or "Viewer"))
            throw new AppException("Vai trò: PM | Member | Viewer.");

        var u = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.UserId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Người dùng không hợp lệ.");

        PjmProjectMember entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PjmProjectMembers.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy thành viên.");
            entity.UserId = req.UserId;
        }
        else
        {
            var existing = await _db.PjmProjectMembers.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ProjectId == projectId && x.UserId == req.UserId && !x.IsDeleted, ct);
            if (existing is not null) entity = existing;
            else
            {
                entity = new PjmProjectMember
                {
                    TenantId = tenantId, ProjectId = projectId, UserId = req.UserId, CreatedBy = userId
                };
                _db.PjmProjectMembers.Add(entity);
            }
        }

        entity.Role = role;
        entity.IsActive = req.IsActive ?? true;
        if (req.AllocationPct is decimal pct)
            entity.AllocationPct = Math.Clamp(pct, 0, 100);
        if (req.FromDate.HasValue) entity.FromDate = req.FromDate;
        if (req.ToDate.HasValue) entity.ToDate = req.ToDate;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (role == "PM")
        {
            var project = await RequireProject(tenantId, projectId, ct);
            project.PmUserId = req.UserId;
            project.PmName = u.DisplayName ?? u.Username;
            project.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
        }

        return new PjmProjectMemberDto(
            entity.Id, entity.ProjectId, entity.UserId, u.DisplayName ?? u.Username, entity.Role, entity.IsActive,
            entity.AllocationPct, entity.FromDate, entity.ToDate);
    }

    public async Task<PjmWbsItemDto> UpsertWbsItemAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmWbsItemUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireProject(tenantId, projectId, ct);
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên hạng mục");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Open" : req.Status.Trim();
        if (status is not ("Open" or "InProgress" or "Done" or "Cancelled"))
            throw new AppException("Trạng thái WBS: Open | InProgress | Done | Cancelled.");

        string? assigneeName = NullIfEmpty(req.AssigneeName);
        if (req.AssigneeUserId is Guid aid)
        {
            var u = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == aid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Người thực hiện không hợp lệ.");
            assigneeName ??= u.DisplayName ?? u.Username;
        }

        PjmWbsItem item;
        if (req.Id is Guid id)
        {
            item = await _db.PjmWbsItems.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy hạng mục WBS.");
        }
        else
        {
            item = new PjmWbsItem { TenantId = tenantId, ProjectId = projectId, CreatedBy = userId };
            _db.PjmWbsItems.Add(item);
        }

        item.Code = code; item.Name = name;
        item.ParentItemId = req.ParentItemId;
        item.AssigneeUserId = req.AssigneeUserId;
        item.AssigneeName = assigneeName;
        item.Status = status;
        item.SortOrder = req.SortOrder ?? item.SortOrder;
        item.Note = NullIfEmpty(req.Note);
        if (req.PercentComplete is decimal pct)
        {
            if (pct is < 0 or > 100) throw new AppException("% hoàn thành 0–100.");
            item.PercentComplete = decimal.Round(pct, 2);
            if (pct >= 100 && status != "Cancelled") item.Status = "Done";
            else if (pct > 0 && status == "Open") item.Status = "InProgress";
        }
        if (req.IsMilestone is bool ms) item.IsMilestone = ms;
        if (req.DueDate.HasValue) item.DueDate = req.DueDate;
        item.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapWbs(item, DateTimeOffset.UtcNow);
    }

    private static PjmWbsItemDto MapWbs(PjmWbsItem x, DateTimeOffset now) =>
        new(x.Id, x.ProjectId, x.Code, x.Name, x.ParentItemId,
            x.AssigneeUserId, x.AssigneeName, x.Status, x.SortOrder, x.Note,
            x.PercentComplete, x.IsMilestone, x.DueDate,
            x.DueDate is DateTimeOffset d && d < now && x.Status is not ("Done" or "Cancelled")
                && x.PercentComplete < 100);

    private async Task ApplyTemplateAsync(
        Guid tenantId, Guid userId, Guid projectId, Guid templateId, CancellationToken ct)
    {
        var items = await _db.PjmWbsTemplateItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TemplateId == templateId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ToListAsync(ct);
        if (items.Count == 0) return;

        var map = new Dictionary<Guid, Guid>();
        foreach (var ti in items.Where(x => x.ParentItemId is null))
        {
            var n = new PjmWbsItem
            {
                TenantId = tenantId, ProjectId = projectId, Code = ti.Code, Name = ti.Name,
                SortOrder = ti.SortOrder, Status = "Open", CreatedBy = userId
            };
            _db.PjmWbsItems.Add(n);
            await _db.SaveChangesAsync(ct);
            map[ti.Id] = n.Id;
        }
        foreach (var ti in items.Where(x => x.ParentItemId is not null))
        {
            Guid? parentMapped = ti.ParentItemId is Guid pid && map.TryGetValue(pid, out var mapped)
                ? mapped : null;
            _db.PjmWbsItems.Add(new PjmWbsItem
            {
                TenantId = tenantId, ProjectId = projectId, Code = ti.Code, Name = ti.Name,
                ParentItemId = parentMapped,
                SortOrder = ti.SortOrder, Status = "Open", CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task<PjmProject> RequireProject(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PjmProjects.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy dự án.");

    private async Task<string> NextCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"DA-{today}-";
        var last = await _db.PjmProjects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private async Task<IReadOnlyList<PjmWbsTemplateDto>> MapTemplatesAsync(
        Guid tenantId, List<PjmWbsTemplate> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<PjmWbsTemplateDto>();
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.PjmWbsTemplateItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.TemplateId) && !x.IsDeleted)
            .GroupBy(x => x.TemplateId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(t => new PjmWbsTemplateDto(
            t.Id, t.Code, t.Name, t.Status, t.Note, counts.GetValueOrDefault(t.Id))).ToList();
    }

    private async Task<IReadOnlyList<PjmProjectDto>> MapProjectsAsync(
        Guid tenantId, List<PjmProject> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<PjmProjectDto>();
        var ids = list.Select(x => x.Id).ToList();
        var typeIds = list.Where(x => x.ProjectTypeId.HasValue).Select(x => x.ProjectTypeId!.Value).Distinct().ToList();
        var types = typeIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.PjmProjectTypes.AsNoTracking()
                .Where(x => x.TenantId == tenantId && typeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var statuses = await _db.PjmProjectStatuses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Code, x => x.Name, StringComparer.OrdinalIgnoreCase, ct);
        var memberCounts = await _db.PjmProjectMembers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted && x.IsActive)
            .GroupBy(x => x.ProjectId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var wbsCounts = await _db.PjmWbsItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted)
            .GroupBy(x => x.ProjectId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var expenseCosts = await _db.PjmExpenses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted && x.Status == "Posted")
            .GroupBy(x => x.ProjectId)
            .Select(g => new { g.Key, S = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.S, ct);
        var issueIds = await _db.PjmMaterialIssues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProjectId) && !x.IsDeleted && x.Status == "Posted")
            .Select(x => new { x.Id, x.ProjectId }).ToListAsync(ct);
        var postedIssueIds = issueIds.Select(x => x.Id).ToList();
        var lineCosts = postedIssueIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _db.PjmMaterialIssueLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && postedIssueIds.Contains(x.MaterialIssueId) && !x.IsDeleted)
                .GroupBy(x => x.MaterialIssueId)
                .Select(g => new { g.Key, S = g.Sum(x => x.Qty * x.UnitCost) })
                .ToDictionaryAsync(x => x.Key, x => x.S, ct);
        var materialByProject = issueIds
            .GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => g.Sum(i => lineCosts.GetValueOrDefault(i.Id)));

        return list.Select(p =>
        {
            var actual = expenseCosts.GetValueOrDefault(p.Id) + materialByProject.GetValueOrDefault(p.Id);
            return new PjmProjectDto(
                p.Id, p.Code, p.Name, p.ProjectTypeId,
                p.ProjectTypeId is Guid tid ? types.GetValueOrDefault(tid) : null,
                p.StatusCode, statuses.GetValueOrDefault(p.StatusCode),
                p.CustomerName, p.ContractCode, p.SourceOpportunityCode,
                p.PmUserId, p.PmName, p.Budget, p.StartDate, p.EndDate, p.Note,
                memberCounts.GetValueOrDefault(p.Id), wbsCounts.GetValueOrDefault(p.Id),
                decimal.Round(actual, 2), p.RecognizedRevenue,
                decimal.Round(p.RecognizedRevenue - actual, 2), p.ClosedAt);
        }).ToList();
    }

    private static PjmExpenseDto MapExpense(PjmExpense x) =>
        new(x.Id, x.ProjectId, x.Code, x.Category, x.Description, x.Amount, x.ExpenseDate,
            x.WbsItemId, x.Status, x.PostedAt, x.Note);

    private static PjmMaterialIssueDto MapIssue(PjmMaterialIssue x)
    {
        var lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new PjmMaterialIssueLineDto(
            l.Id, l.ProductCode, l.ProductName, l.Unit, l.Qty, l.UnitCost,
            decimal.Round(l.Qty * l.UnitCost, 2))).ToList();
        return new PjmMaterialIssueDto(
            x.Id, x.ProjectId, x.Code, x.Status, x.Note, x.PostedAt,
            decimal.Round(lines.Sum(l => l.Amount), 2), lines);
    }

    private static PjmAcceptanceDto MapAcceptance(PjmAcceptance x) =>
        new(x.Id, x.ProjectId, x.Code, x.Kind, x.Title, x.Status, x.SignerName, x.SignedAt, x.Note);

    private static PjmCostSummaryDto BuildCostSummary(
        PjmProject p, List<PjmExpense> expenses, List<PjmMaterialIssue> issues, List<PjmAcceptance> acceptances)
    {
        var exp = expenses.Where(x => x.Status == "Posted").Sum(x => x.Amount);
        var mat = issues.Where(x => x.Status == "Posted")
            .Sum(i => i.Lines.Where(l => !l.IsDeleted).Sum(l => l.Qty * l.UnitCost));
        var actual = decimal.Round(exp + mat, 2);
        return new PjmCostSummaryDto(
            p.Budget, decimal.Round(exp, 2), decimal.Round(mat, 2), actual,
            p.RecognizedRevenue, decimal.Round(p.RecognizedRevenue - actual, 2),
            decimal.Round(p.Budget - actual, 2),
            acceptances.Any(a => a.Kind == "Final" && a.Status == "Signed"));
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? s, int max, string label)
    {
        var v = (s ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string ActiveInactive(string? s)
    {
        var v = string.IsNullOrWhiteSpace(s) ? "Active" : s.Trim();
        if (v is not ("Active" or "Inactive")) throw new AppException("Trạng thái: Active | Inactive.");
        return v;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

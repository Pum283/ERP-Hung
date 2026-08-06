using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmRewardDisciplineService : IHrmRewardDisciplineService
{
    private readonly AppDbContext _db;

    public HrmRewardDisciplineService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RewardDisciplineDto>> ListAsync(
        Guid tenantId, string? kind, CancellationToken ct = default)
    {
        var q =
            from d in _db.RewardDisciplineDecisions.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on d.EmployeeId equals e.Id
            where d.TenantId == tenantId && !d.IsDeleted
            select new { d, e };
        if (!string.IsNullOrWhiteSpace(kind))
            q = q.Where(x => x.d.Kind == kind.Trim());
        return await q.OrderByDescending(x => x.d.DecisionDate).ThenByDescending(x => x.d.CreatedAt)
            .Select(x => Map(x.d, x.e.EmployeeCode, x.e.FullName))
            .ToListAsync(ct);
    }

    public async Task<RewardDisciplineDto> CreateAsync(
        Guid tenantId, Guid userId, RewardDisciplineCreateRequest req, CancellationToken ct = default)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        var kind = req.Kind.Trim();
        if (kind is not ("Reward" or "Discipline")) throw new AppException("Kind: Reward | Discipline.");
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Tiêu đề bắt buộc.");
        var impactKind = string.IsNullOrWhiteSpace(req.PayrollImpactKind) ? "None" : req.PayrollImpactKind.Trim();
        if (impactKind is not ("None" or "Bonus" or "Deduction" or "Allowance"))
            throw new AppException("PayrollImpactKind: None | Bonus | Deduction | Allowance.");

        var entity = new RewardDisciplineDecision
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            Kind = kind,
            Title = req.Title.Trim(),
            DecisionDate = req.DecisionDate,
            Reason = req.Reason,
            PayrollImpactAmount = Math.Abs(req.PayrollImpactAmount),
            PayrollImpactKind = impactKind,
            DecisionStorageKey = string.IsNullOrWhiteSpace(req.DecisionStorageKey) ? null : req.DecisionStorageKey.Trim(),
            Status = "Issued",
            Note = req.Note,
            CreatedBy = userId
        };
        _db.RewardDisciplineDecisions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity, emp.EmployeeCode, emp.FullName);
    }

    public async Task<RewardDisciplineDto> AttachAsync(
        Guid tenantId, Guid userId, Guid id, RewardDisciplineAttachRequest req, CancellationToken ct = default)
    {
        var entity = await _db.RewardDisciplineDecisions
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Quyết định không tồn tại.", 404);
        var key = (req.DecisionStorageKey ?? "").Trim();
        if (key.Length < 1) throw new AppException("StorageKey bắt buộc.");
        entity.DecisionStorageKey = key;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var emp = await _db.Employees.AsNoTracking().FirstAsync(x => x.Id == entity.EmployeeId, ct);
        return Map(entity, emp.EmployeeCode, emp.FullName);
    }

    public async Task<RewardDisciplineDto> ApplyToPayrollAsync(
        Guid tenantId, Guid userId, Guid id, Guid? periodId, CancellationToken ct = default)
    {
        var entity = await _db.RewardDisciplineDecisions
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Quyết định không tồn tại.", 404);
        if (entity.Status == "Applied") throw new AppException("Đã áp dụng vào lương.");
        if (entity.PayrollImpactKind is "None" || entity.PayrollImpactAmount <= 0)
            throw new AppException("Quyết định không có ảnh hưởng lương.");

        PayrollPeriod period;
        if (periodId is Guid pid)
        {
            period = await _db.PayrollPeriods.FirstOrDefaultAsync(x => x.Id == pid && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Kỳ lương không tồn tại.", 404);
        }
        else
        {
            period = await _db.PayrollPeriods
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Locked")
                .OrderByDescending(x => x.PeriodKey)
                .FirstOrDefaultAsync(ct)
                ?? throw new AppException("Không có kỳ lương mở để áp dụng.");
        }
        if (period.Status == "Locked") throw new AppException("Kỳ lương đã khóa.");

        var adjKind = entity.PayrollImpactKind is "Bonus" or "Allowance" ? entity.PayrollImpactKind : "Deduction";
        _db.PayrollAdjustments.Add(new PayrollAdjustment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PayrollPeriodId = period.Id,
            EmployeeId = entity.EmployeeId,
            Kind = adjKind,
            Title = $"{entity.Kind}: {entity.Title}",
            Amount = entity.PayrollImpactAmount,
            Note = $"Từ quyết định {entity.Id:N}",
            CreatedBy = userId
        });
        entity.Status = "Applied";
        entity.AppliedPayrollPeriodId = period.Id;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var emp = await _db.Employees.AsNoTracking().FirstAsync(x => x.Id == entity.EmployeeId, ct);
        return Map(entity, emp.EmployeeCode, emp.FullName);
    }

    public async Task<IReadOnlyList<RewardDisciplineReportRowDto>> ReportAsync(
        Guid tenantId, int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var from = new DateOnly(y, 1, 1);
        var to = new DateOnly(y, 12, 31);
        return await _db.RewardDisciplineDecisions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.DecisionDate >= from && x.DecisionDate <= to)
            .GroupBy(x => x.Kind)
            .Select(g => new RewardDisciplineReportRowDto(g.Key, g.Count(), g.Sum(x => x.PayrollImpactAmount)))
            .OrderBy(x => x.Kind)
            .ToListAsync(ct);
    }

    private static RewardDisciplineDto Map(RewardDisciplineDecision d, string code, string name) => new(
        d.Id, d.EmployeeId, code, name, d.Kind, d.Title, d.DecisionDate, d.Reason,
        d.PayrollImpactAmount, d.PayrollImpactKind, d.DecisionStorageKey, d.Status,
        d.AppliedPayrollPeriodId, d.Note, d.CreatedAt);
}

public sealed class HrmOffboardingService : IHrmOffboardingService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly OffboardingChecklistItemDto[] DefaultChecklist =
    [
        new("assets", "Thu hồi tài sản / thiết bị", false),
        new("docs", "Bàn giao hồ sơ / tài liệu", false),
        new("knowledge", "Bàn giao công việc", false),
        new("access", "Thu hồi quyền hệ thống", false),
        new("finance", "Quyết toán công nợ / tạm ứng", false),
    ];

    private readonly AppDbContext _db;

    public HrmOffboardingService(AppDbContext db) => _db = db;

    public async Task<OffboardingSettingDto> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
        => MapSettings(await EnsureSettingsAsync(tenantId, ct));

    public async Task<OffboardingSettingDto> UpsertSettingsAsync(
        Guid tenantId, Guid userId, OffboardingSettingUpsertRequest req, CancellationToken ct = default)
    {
        if (req.NoticeDays is < 0 or > 365) throw new AppException("Số ngày báo trước 0–365.");
        var s = await EnsureSettingsAsync(tenantId, ct);
        s.NoticeDays = req.NoticeDays;
        s.RequireChecklistComplete = req.RequireChecklistComplete;
        s.AutoRevokeAccessOnComplete = req.AutoRevokeAccessOnComplete;
        s.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapSettings(s);
    }

    public async Task<IReadOnlyList<OffboardingCaseDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cases = await _db.OffboardingCases.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return await MapManyAsync(tenantId, cases, ct);
    }

    public async Task<OffboardingCaseDto> CreateAsync(
        Guid tenantId, Guid userId, OffboardingCreateRequest req, CancellationToken ct = default)
    {
        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.EmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        if (await _db.OffboardingCases.AnyAsync(x =>
                x.TenantId == tenantId && !x.IsDeleted && x.EmployeeId == req.EmployeeId
                && x.Status != "Completed" && x.Status != "Cancelled" && x.Status != "Rejected", ct))
            throw new AppException("NV đã có hồ sơ offboarding đang mở.");

        var settings = await EnsureSettingsAsync(tenantId, ct);
        var noticeOk = req.LastWorkingDay.DayNumber - req.RequestDate.DayNumber >= settings.NoticeDays;
        var entity = new OffboardingCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = req.EmployeeId,
            RequestDate = req.RequestDate,
            LastWorkingDay = req.LastWorkingDay,
            ReasonCode = string.IsNullOrWhiteSpace(req.ReasonCode) ? "Personal" : req.ReasonCode.Trim(),
            ReasonDetail = req.ReasonDetail,
            Status = "Draft",
            RequiredNoticeDays = settings.NoticeDays,
            NoticeSatisfied = noticeOk,
            ChecklistJson = JsonSerializer.Serialize(DefaultChecklist, JsonOpts),
            CreatedBy = userId
        };
        _db.OffboardingCases.Add(entity);
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [entity], ct))[0];
    }

    public async Task<OffboardingCaseDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not "Draft") throw new AppException("Chỉ nộp đơn ở trạng thái Draft.");
        RefreshNotice(c, await EnsureSettingsAsync(tenantId, ct));
        c.Status = "Submitted";
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> ApproveAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not "Submitted") throw new AppException("Chỉ duyệt đơn đã nộp.");
        var settings = await EnsureSettingsAsync(tenantId, ct);
        RefreshNotice(c, settings);
        c.Status = "Approved";
        c.ApprovedAt = DateTimeOffset.UtcNow;
        c.ApprovedByUserId = userId;
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> RejectAsync(
        Guid tenantId, Guid userId, Guid id, OffboardingRejectRequest req, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not "Submitted") throw new AppException("Chỉ từ chối đơn đã nộp.");
        c.Status = "Rejected";
        c.RejectReason = req.Reason;
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> UpdateChecklistAsync(
        Guid tenantId, Guid userId, Guid id, OffboardingChecklistUpdateRequest req, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is "Rejected" or "Cancelled" or "Completed")
            throw new AppException("Không cập nhật checklist ở trạng thái hiện tại.");
        c.ChecklistJson = JsonSerializer.Serialize(req.Items ?? Array.Empty<OffboardingChecklistItemDto>(), JsonOpts);
        if (c.Status == "Approved") c.Status = "InProgress";
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> RevokeAccessAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not ("Approved" or "InProgress"))
            throw new AppException("Thu hồi quyền sau khi duyệt đơn.");
        var emp = await _db.Employees.AsNoTracking()
            .FirstAsync(x => x.Id == c.EmployeeId, ct);
        if (emp.UserId is Guid uid)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == uid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (user is not null)
            {
                user.Status = UserStatus.Disabled;
                user.UpdatedBy = userId;
            }
        }
        c.AccessRevoked = true;
        c.AccessRevokedAt = DateTimeOffset.UtcNow;
        c.UpdatedBy = userId;
        if (c.Status == "Approved") c.Status = "InProgress";
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> SettleAsync(
        Guid tenantId, Guid userId, Guid id, OffboardingSettleRequest req, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not ("Approved" or "InProgress"))
            throw new AppException("Quyết toán sau khi duyệt đơn.");

        var year = c.LastWorkingDay.Year;
        var leaveRemain = await _db.LeaveBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.EmployeeId == c.EmployeeId && x.Year == year)
            .SumAsync(x => (decimal?)x.Remaining, ct) ?? 0m;
        c.LeaveDaysRemaining = leaveRemain;
        c.LeaveSettlementAmount = req.LeaveSettlementAmount;
        c.FinalPayEstimate = req.FinalPayEstimate;
        c.SettlementNote = req.SettlementNote;
        if (c.Status == "Approved") c.Status = "InProgress";
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> SaveInterviewAsync(
        Guid tenantId, Guid userId, Guid id, OffboardingInterviewRequest req, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        c.InterviewNotes = req.InterviewNotes;
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<OffboardingCaseDto> CompleteAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var c = await GetCaseAsync(tenantId, id, ct);
        if (c.Status is not ("Approved" or "InProgress"))
            throw new AppException("Hoàn tất sau khi duyệt / đang xử lý.");
        var settings = await EnsureSettingsAsync(tenantId, ct);
        var items = ParseChecklist(c.ChecklistJson);
        if (settings.RequireChecklistComplete && items.Any(x => !x.Done))
            throw new AppException("Checklist bàn giao chưa hoàn tất.");

        if (settings.AutoRevokeAccessOnComplete && !c.AccessRevoked)
            await RevokeAccessAsync(tenantId, userId, id, ct);

        c = await GetCaseAsync(tenantId, id, ct);
        var emp = await _db.Employees.FirstOrDefaultAsync(x => x.Id == c.EmployeeId && x.TenantId == tenantId, ct)
            ?? throw new AppException("Nhân viên không tồn tại.", 404);
        var from = emp.Status;
        _db.EmploymentStatusChanges.Add(new EmploymentStatusChange
        {
            TenantId = tenantId,
            EmployeeId = emp.Id,
            FromStatus = from,
            ToStatus = "Resigned",
            EffectiveDate = c.LastWorkingDay,
            Reason = c.ReasonDetail ?? c.ReasonCode,
            OrgUnitId = emp.OrgUnitId,
            DepartmentId = emp.DepartmentId,
            JobTitleId = emp.JobTitleId,
            CreatedBy = userId
        });
        emp.Status = "Resigned";
        emp.TerminateDate = c.LastWorkingDay;
        emp.IsDeleted = true;
        emp.DeletedAt = DateTimeOffset.UtcNow;
        emp.UpdatedBy = userId;

        c.Status = "Completed";
        c.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, [c], ct))[0];
    }

    public async Task<IReadOnlyList<OffboardingReportRowDto>> ReportByReasonAsync(
        Guid tenantId, int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var from = new DateOnly(y, 1, 1);
        var to = new DateOnly(y, 12, 31);
        return await _db.OffboardingCases.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.RequestDate >= from && x.RequestDate <= to
                        && x.Status != "Cancelled")
            .GroupBy(x => x.ReasonCode)
            .Select(g => new OffboardingReportRowDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);
    }

    private async Task<OffboardingCase> GetCaseAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _db.OffboardingCases.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Hồ sơ offboarding không tồn tại.", 404);

    private static void RefreshNotice(OffboardingCase c, OffboardingSetting s)
    {
        c.RequiredNoticeDays = s.NoticeDays;
        c.NoticeSatisfied = c.LastWorkingDay.DayNumber - c.RequestDate.DayNumber >= s.NoticeDays;
    }

    private async Task<OffboardingSetting> EnsureSettingsAsync(Guid tenantId, CancellationToken ct)
    {
        var s = await _db.OffboardingSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        if (s is not null) return s;
        s = new OffboardingSetting { Id = Guid.NewGuid(), TenantId = tenantId };
        _db.OffboardingSettings.Add(s);
        await _db.SaveChangesAsync(ct);
        return s;
    }

    private async Task<IReadOnlyList<OffboardingCaseDto>> MapManyAsync(
        Guid tenantId, List<OffboardingCase> cases, CancellationToken ct)
    {
        var empIds = cases.Select(x => x.EmployeeId).Distinct().ToList();
        var emps = await (
            from e in _db.Employees.AsNoTracking()
            join o in _db.OrgUnits.AsNoTracking() on e.OrgUnitId equals o.Id into oj
            from o in oj.DefaultIfEmpty()
            where empIds.Contains(e.Id)
            select new { e.Id, e.EmployeeCode, e.FullName, Org = o != null ? o.Name : "" }
        ).ToListAsync(ct);
        var map = emps.ToDictionary(x => x.Id);
        return cases.Select(c =>
        {
            map.TryGetValue(c.EmployeeId, out var e);
            return new OffboardingCaseDto(
                c.Id, c.EmployeeId, e?.EmployeeCode ?? "", e?.FullName ?? "", e?.Org ?? "",
                c.RequestDate, c.LastWorkingDay, c.ReasonCode, c.ReasonDetail, c.Status,
                c.NoticeSatisfied, c.RequiredNoticeDays, ParseChecklist(c.ChecklistJson),
                c.AccessRevoked, c.LeaveDaysRemaining, c.LeaveSettlementAmount, c.FinalPayEstimate,
                c.SettlementNote, c.InterviewNotes, c.RejectReason, c.CreatedAt);
        }).ToList();
    }

    private static IReadOnlyList<OffboardingChecklistItemDto> ParseChecklist(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<OffboardingChecklistItemDto>>(json, JsonOpts)
                   ?? new List<OffboardingChecklistItemDto>();
        }
        catch { return new List<OffboardingChecklistItemDto>(); }
    }

    private static OffboardingSettingDto MapSettings(OffboardingSetting s)
        => new(s.NoticeDays, s.RequireChecklistComplete, s.AutoRevokeAccessOnComplete);
}

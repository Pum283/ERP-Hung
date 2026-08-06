using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmOnboardingService : IHrmOnboardingService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly OnboardingChecklistItemDto[] DefaultChecklist =
    [
        new("account", "Tạo tài khoản / email", false),
        new("equipment", "Cấp thiết bị", false),
        new("policy", "Ký nội quy / bảo mật", false),
        new("training", "Đào tạo hội nhập", false),
        new("intro", "Giới thiệu team / mentor", false),
    ];

    private readonly AppDbContext _db;
    private readonly ISysPlatformService _platform;

    public HrmOnboardingService(AppDbContext db, ISysPlatformService platform)
    {
        _db = db;
        _platform = platform;
    }

    public async Task<OnboardingSettingDto> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var s = await EnsureSettingsAsync(tenantId, ct);
        return new OnboardingSettingDto(s.OnboardingDays, s.TrialDays);
    }

    public async Task<OnboardingSettingDto> UpsertSettingsAsync(
        Guid tenantId, Guid userId, OnboardingSettingUpsertRequest req, CancellationToken ct = default)
    {
        if (req.OnboardingDays is < 1 or > 365) throw new AppException("Thời hạn onboarding 1–365 ngày.");
        if (req.TrialDays is < 1 or > 365) throw new AppException("Thời hạn thử việc 1–365 ngày.");
        var s = await EnsureSettingsAsync(tenantId, ct);
        s.OnboardingDays = req.OnboardingDays;
        s.TrialDays = req.TrialDays;
        s.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new OnboardingSettingDto(s.OnboardingDays, s.TrialDays);
    }

    public async Task<IReadOnlyList<OnboardingCaseDto>> ListCasesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cases = await _db.OnboardingCases.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return await MapManyAsync(tenantId, cases, ct);
    }

    public async Task<OnboardingCaseDto> HireFromCandidateAsync(
        Guid tenantId, Guid userId, HireFromCandidateRequest req, CancellationToken ct = default)
    {
        var cand = await _db.Candidates.FirstOrDefaultAsync(
            x => x.Id == req.CandidateId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Ứng viên không tồn tại.", 404);
        if (!string.Equals(cand.PipelineStatus, "Accepted", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Chỉ onboard ứng viên đã Accepted.");
        if (cand.ConvertedEmployeeId is not null)
            throw new AppException("Ứng viên đã được tạo hồ sơ NV.");

        var post = await _db.JobPostings.AsNoTracking()
            .FirstAsync(x => x.Id == cand.JobPostingId, ct);
        var rr = await _db.RecruitmentRequests.AsNoTracking()
            .FirstAsync(x => x.Id == post.RecruitmentRequestId, ct);

        var orgId = req.OrgUnitId ?? rr.OrgUnitId;
        var jobTitleId = req.JobTitleId ?? rr.JobTitleId;
        if (!await _db.OrgUnits.AnyAsync(x => x.Id == orgId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Đơn vị không hợp lệ.", 404);

        var settings = await EnsureSettingsAsync(tenantId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var code = await _platform.NextNumberAsync(tenantId, "HRM.EMP", ct);

        var emp = new Employee
        {
            TenantId = tenantId,
            EmployeeCode = code,
            FullName = cand.FullName,
            Email = cand.Email,
            Phone = cand.Phone,
            OrgUnitId = orgId,
            JobTitleId = jobTitleId,
            Status = "Probation",
            HireDate = today,
            CreatedBy = userId
        };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync(ct);

        cand.ConvertedEmployeeId = emp.Id;
        cand.PipelineStatus = "Accepted";

        var ob = new OnboardingCase
        {
            TenantId = tenantId,
            EmployeeId = emp.Id,
            CandidateId = cand.Id,
            StartDate = today,
            OnboardingDueDate = today.AddDays(settings.OnboardingDays),
            TrialEndDate = today.AddDays(settings.TrialDays),
            Status = "InProgress",
            ChecklistJson = JsonSerializer.Serialize(DefaultChecklist, JsonOpts),
            CreatedBy = userId
        };
        _db.OnboardingCases.Add(ob);
        await _db.SaveChangesAsync(ct);

        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<OnboardingCaseDto> AssignMentorAsync(
        Guid tenantId, Guid caseId, AssignMentorRequest req, CancellationToken ct = default)
    {
        var ob = await GetCaseAsync(tenantId, caseId, ct);
        if (!await _db.Employees.AnyAsync(
                x => x.Id == req.MentorEmployeeId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Mentor không tồn tại.", 404);
        ob.MentorEmployeeId = req.MentorEmployeeId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<OnboardingCaseDto> UpdateChecklistAsync(
        Guid tenantId, Guid caseId, OnboardingChecklistUpdateRequest req, CancellationToken ct = default)
    {
        var ob = await GetCaseAsync(tenantId, caseId, ct);
        var items = (req.Items ?? Array.Empty<OnboardingChecklistItemDto>())
            .Select(i => new OnboardingChecklistItemDto(i.Key, i.Label, i.Done))
            .ToList();
        ob.ChecklistJson = JsonSerializer.Serialize(items, JsonOpts);
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<OnboardingCaseDto> AddDocumentAsync(
        Guid tenantId, Guid caseId, OnboardingDocUploadRequest req, CancellationToken ct = default)
    {
        var ob = await GetCaseAsync(tenantId, caseId, ct);
        var title = (req.Title ?? "").Trim();
        var key = (req.StorageKey ?? "").Trim();
        if (title.Length == 0 || key.Length == 0) throw new AppException("Thiếu tiêu đề hoặc file.");
        _db.OnboardingDocuments.Add(new OnboardingDocument
        {
            TenantId = tenantId,
            OnboardingCaseId = ob.Id,
            Title = title,
            StorageKey = key,
            CreatedBy = ob.CreatedBy
        });
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<OnboardingCaseDto> EvaluateTrialAsync(
        Guid tenantId, Guid caseId, TrialEvalRequest req, CancellationToken ct = default)
    {
        var ob = await GetCaseAsync(tenantId, caseId, ct);
        if (req.Score is < 0 or > 100) throw new AppException("Điểm 0–100.");
        ob.TrialScore = req.Score;
        ob.TrialComment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim();
        ob.Status = req.Score >= 50 ? "TrialPassed" : "InProgress";
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<OnboardingCaseDto> ConvertToOfficialAsync(Guid tenantId, Guid caseId, CancellationToken ct = default)
    {
        var ob = await GetCaseAsync(tenantId, caseId, ct);
        if (ob.Status is not ("TrialPassed" or "InProgress"))
            throw new AppException("Không thể chuyển chính thức từ trạng thái hiện tại.");
        if (ob.TrialScore is null)
            throw new AppException("Cần đánh giá thử việc trước khi chuyển chính thức.");

        var emp = await _db.Employees.FirstAsync(x => x.Id == ob.EmployeeId && !x.IsDeleted, ct);
        emp.Status = "Active";
        ob.Status = "Converted";
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(tenantId, new[] { ob }, ct))[0];
    }

    public async Task<IReadOnlyList<TrialExpiringDto>> ListTrialExpiringAsync(
        Guid tenantId, int withinDays, CancellationToken ct = default)
    {
        withinDays = Math.Clamp(withinDays, 1, 90);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(withinDays);

        return await (
            from ob in _db.OnboardingCases.AsNoTracking()
            join e in _db.Employees.AsNoTracking() on ob.EmployeeId equals e.Id
            where ob.TenantId == tenantId && !ob.IsDeleted && !e.IsDeleted
                  && ob.Status != "Converted" && ob.Status != "Cancelled"
                  && ob.TrialEndDate >= today && ob.TrialEndDate <= until
            orderby ob.TrialEndDate
            select new TrialExpiringDto(
                ob.Id, e.Id, e.EmployeeCode, e.FullName, ob.TrialEndDate,
                ob.TrialEndDate.DayNumber - today.DayNumber)
        ).ToListAsync(ct);
    }

    private async Task<OnboardingSetting> EnsureSettingsAsync(Guid tenantId, CancellationToken ct)
    {
        var s = await _db.OnboardingSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted, ct);
        if (s is not null) return s;
        s = new OnboardingSetting { TenantId = tenantId, OnboardingDays = 30, TrialDays = 60 };
        _db.OnboardingSettings.Add(s);
        await _db.SaveChangesAsync(ct);
        return s;
    }

    private async Task<OnboardingCase> GetCaseAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _db.OnboardingCases.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException("Hồ sơ onboarding không tồn tại.", 404);

    private async Task<IReadOnlyList<OnboardingCaseDto>> MapManyAsync(
        Guid tenantId, IReadOnlyList<OnboardingCase> cases, CancellationToken ct)
    {
        if (cases.Count == 0) return Array.Empty<OnboardingCaseDto>();
        var empIds = cases.Select(c => c.EmployeeId).Concat(cases.Where(c => c.MentorEmployeeId is not null).Select(c => c.MentorEmployeeId!.Value)).Distinct().ToList();
        var candIds = cases.Where(c => c.CandidateId is not null).Select(c => c.CandidateId!.Value).Distinct().ToList();
        var caseIds = cases.Select(c => c.Id).ToList();

        var emps = await _db.Employees.AsNoTracking().Where(x => empIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var cands = candIds.Count == 0
            ? new Dictionary<Guid, Candidate>()
            : await _db.Candidates.AsNoTracking().Where(x => candIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var docs = await _db.OnboardingDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && caseIds.Contains(x.OnboardingCaseId) && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
        var docsByCase = docs.GroupBy(d => d.OnboardingCaseId)
            .ToDictionary(g => g.Key, g => g.Select(d => new OnboardingDocumentDto(d.Id, d.Title, d.StorageKey, d.CreatedAt)).ToList());

        return cases.Select(c =>
        {
            emps.TryGetValue(c.EmployeeId, out var emp);
            Candidate? cand = c.CandidateId is Guid cid && cands.TryGetValue(cid, out var cv) ? cv : null;
            string? mentorName = null;
            if (c.MentorEmployeeId is Guid mid && emps.TryGetValue(mid, out var mentor))
                mentorName = mentor.FullName;
            var checklist = ParseChecklist(c.ChecklistJson);
            return new OnboardingCaseDto(
                c.Id, c.EmployeeId, emp?.EmployeeCode ?? "?", emp?.FullName ?? "?", emp?.Status ?? "?",
                c.CandidateId, cand?.FullName, c.MentorEmployeeId, mentorName,
                c.StartDate, c.OnboardingDueDate, c.TrialEndDate, c.Status,
                c.TrialScore, c.TrialComment, checklist,
                docsByCase.GetValueOrDefault(c.Id) ?? new List<OnboardingDocumentDto>());
        }).ToList();
    }

    private static IReadOnlyList<OnboardingChecklistItemDto> ParseChecklist(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<OnboardingChecklistItemDto>>(json, JsonOpts)
                   ?? DefaultChecklist.ToList();
        }
        catch
        {
            return DefaultChecklist.ToList();
        }
    }
}

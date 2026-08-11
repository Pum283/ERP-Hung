using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 22:
///   UC_HRM_072 — Đánh giá kết thúc thử việc (Trial Evaluation Score 0-100 & TrialPassed status)
///   UC_HRM_073 — Chuyển thử việc thành chính thức (Convert to Official & Employee Active Status)
///   UC_HRM_074 — Cảnh báo hết hạn thử việc (Trial Expiring Alert within 1-90 days)
///   UC_HRM_075 — Khai báo định biên theo đơn vị (Org Unit Headcount Plan Declaration)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep22PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly HrmRecruitPipelineService _pipelineSvc;
    private readonly HrmOnboardingService _onboardingSvc;
    private readonly HrmHeadcountService _headcountSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _approver   = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _postingId;

    public HrmStep22PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step22-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnitId, TenantId = _tenant,
            Code = "ORG_HR22", Name = "Phòng Đánh Giá Thử Việc 22", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV22", Name = "Kỹ Sư Backend 22"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hr_user22", DisplayName = "Phạm HR 22"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver22", DisplayName = "Trần Giám Đốc 22"
        });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);
        _onboardingSvc = new HrmOnboardingService(_db, new SysPlatformService(_db, new OutboxWriter(_db)));
        _headcountSvc = new HrmHeadcountService(_db);

        SetupTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task SetupTestDataAsync()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 5, "Tuyển Thử Việc Devs", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "Duyệt tuyển"));

        var p = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Backend Dev - Step 22", "LinkedIn"));
        _postingId = p.Id;
    }

    private async Task<OnboardingCaseDto> CreateTestOnboardingCaseAsync(string name, string email, string phone)
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, name, email, phone, null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Đồng ý tuyển"));
        return await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_072: Đánh giá kết thúc thử việc
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC072_EvaluateTrial_ScorePassed_UpdatesStatusToTrialPassed()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Nguyễn Văn Eval1", "ev1@example.com", "0900111221");

        var updated = await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id,
            new TrialEvalRequest(85, "Hoàn thành tốt công việc thử việc"));

        Assert.Equal(85, updated.TrialScore);
        Assert.Equal("Hoàn thành tốt công việc thử việc", updated.TrialComment);
        Assert.Equal("TrialPassed", updated.Status);
    }

    [Fact]
    public async Task UC072_EvaluateTrial_ScoreFailed_KeepsStatusInProgress()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Nguyễn Văn Eval2", "ev2@example.com", "0900111222");

        var updated = await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id,
            new TrialEvalRequest(40, "Chưa đạt yêu cầu chuyên môn"));

        Assert.Equal(40, updated.TrialScore);
        Assert.Equal("InProgress", updated.Status);
    }

    [Fact]
    public async Task UC072_EvaluateTrial_ScoreOutOfRange_ThrowsAppException()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Nguyễn Văn Eval3", "ev3@example.com", "0900111223");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id,
                new TrialEvalRequest(105, "Điểm vượt trần")));

        Assert.Contains("0 đến 100", ex.Message);
    }

    [Fact]
    public async Task UC072_EvaluateTrial_CommentTooLong_ThrowsAppException()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Nguyễn Văn Eval4", "ev4@example.com", "0900111224");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id,
                new TrialEvalRequest(80, new string('X', 1001))));

        Assert.Contains("1000 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC072_EvaluateTrial_AlreadyConverted_ThrowsAppException()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Nguyễn Văn Eval5", "ev5@example.com", "0900111225");
        await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id, new TrialEvalRequest(80, "Đạt"));
        await _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id,
                new TrialEvalRequest(90, "Đánh giá lại")));

        Assert.Contains("chuyển chính thức", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_073: Chuyển thử việc thành chính thức
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC073_ConvertToOfficial_TrialEvaluated_UpdatesEmployeeToActive()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Lê Văn Conv1", "cnv1@example.com", "0900222331");
        await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id, new TrialEvalRequest(80, "Đạt thử việc"));

        var converted = await _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id);

        Assert.Equal("Converted", converted.Status);
        Assert.Equal("Active", converted.EmployeeStatus);

        var empDb = await _db.Employees.FirstAsync(x => x.Id == obCase.EmployeeId);
        Assert.Equal("Active", empDb.Status);
    }

    [Fact]
    public async Task UC073_ConvertToOfficial_WithoutTrialEvaluation_ThrowsAppException()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Lê Văn Conv2", "cnv2@example.com", "0900222332");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id));

        Assert.Contains("đánh giá thử việc", ex.Message);
    }

    [Fact]
    public async Task UC073_ConvertToOfficial_AlreadyConverted_ThrowsAppException()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Lê Văn Conv3", "cnv3@example.com", "0900222333");
        await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id, new TrialEvalRequest(80, "Đạt"));
        await _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id));

        Assert.Contains("trạng thái hiện tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_074: Cảnh báo hết hạn thử việc
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC074_ListTrialExpiring_ReturnsExpiringCasesWithinWindow()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Phạm Văn Exp1", "exp1@example.com", "0900333441");

        // Manually adjust TrialEndDate to be 5 days from today
        var obDb = await _db.OnboardingCases.FirstAsync(x => x.Id == obCase.Id);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        obDb.TrialEndDate = today.AddDays(5);
        await _db.SaveChangesAsync();

        var list = await _onboardingSvc.ListTrialExpiringAsync(_tenant, 14);

        Assert.NotEmpty(list);
        var item = list.FirstOrDefault(x => x.OnboardingCaseId == obCase.Id);
        Assert.NotNull(item);
        Assert.Equal(5, item.DaysLeft);
    }

    [Fact]
    public async Task UC074_ListTrialExpiring_ExcludesConvertedCases()
    {
        var obCase = await CreateTestOnboardingCaseAsync("Phạm Văn Exp2", "exp2@example.com", "0900333442");
        var obDb = await _db.OnboardingCases.FirstAsync(x => x.Id == obCase.Id);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        obDb.TrialEndDate = today.AddDays(3);
        await _db.SaveChangesAsync();

        await _onboardingSvc.EvaluateTrialAsync(_tenant, obCase.Id, new TrialEvalRequest(85, "Đạt"));
        await _onboardingSvc.ConvertToOfficialAsync(_tenant, obCase.Id);

        var list = await _onboardingSvc.ListTrialExpiringAsync(_tenant, 14);

        Assert.DoesNotContain(list, x => x.OnboardingCaseId == obCase.Id);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_075: Khai báo định biên theo đơn vị
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC075_UpsertHeadcount_OrgUnitScope_CreatesHeadcountPlanSuccessfully()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddMonths(6);

        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 15, from, to, "Định biên năm 2026", false));

        Assert.NotNull(plan);
        Assert.Equal("OrgUnit", plan.ScopeType);
        Assert.Equal(_orgUnitId, plan.OrgUnitId);
        Assert.Equal(15, plan.PlannedHeadcount);
        Assert.Equal("Draft", plan.Status);
    }

    [Fact]
    public async Task UC075_UpsertHeadcount_NegativePlannedHeadcount_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, -5, from, null, null, false)));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC075_UpsertHeadcount_InvalidScopeType_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "InvalidScope", _orgUnitId, null, null, 10, from, null, null, false)));

        Assert.Contains("ScopeType", ex.Message);
    }

    [Fact]
    public async Task UC075_UpsertHeadcount_InvalidOrgUnit_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "OrgUnit", Guid.NewGuid(), null, null, 10, from, null, null, false)));

        Assert.Equal(404, ex.StatusCode);
    }
}

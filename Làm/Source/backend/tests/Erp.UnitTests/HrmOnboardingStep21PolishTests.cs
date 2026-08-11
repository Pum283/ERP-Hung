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
/// Unit tests cho Bước 21:
///   UC_HRM_068 — Tạo hồ sơ nhân viên mới từ ứng viên trúng tuyển (Hire Employee from Accepted Candidate)
///   UC_HRM_069 — Gán người hướng dẫn (Assign Onboarding Mentor)
///   UC_HRM_070 — Checklist tiếp nhận nhân viên mới (Onboarding Checklist)
///   UC_HRM_071 — Upload chứng chỉ / giấy tờ tiếp nhận (Onboarding Document Upload)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmOnboardingStep21PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly HrmRecruitPipelineService _pipelineSvc;
    private readonly HrmOnboardingService _onboardingSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _approver   = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _postingId;
    private Guid _mentorEmpId;

    public HrmOnboardingStep21PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-onboarding-step21-" + Guid.NewGuid())
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
            Code = "ORG_HR21", Name = "Phòng Onboarding 21", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV21", Name = "Kỹ Sư Fullstack"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hr_user21", DisplayName = "Phạm HR 21"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver21", DisplayName = "Trần Giám Đốc 21"
        });

        var mentorEmp = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP-MENTOR01", FullName = "Nguyễn Văn Mentor",
            OrgUnitId = _orgUnitId, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(mentorEmp);
        _db.SaveChanges();
        _mentorEmpId = mentorEmp.Id;

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);
        _onboardingSvc = new HrmOnboardingService(_db, new SysPlatformService(_db, new OutboxWriter(_db)));

        SetupTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task SetupTestDataAsync()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 5, "Tuyển Onboarding Devs", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "Duyệt tuyển"));

        var p = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Fullstack Dev - Onboarding", "LinkedIn"));
        _postingId = p.Id;
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_068: Tạo hồ sơ nhân viên mới từ ứng viên trúng tuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC068_HireFromCandidate_AcceptedCandidate_CreatesEmployeeAndOnboardingCase()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Trần Văn Onboard1", "ob1@example.com", "0988000111", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Thư mời làm việc"));

        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        Assert.NotNull(obCase);
        Assert.Equal(c.Id, obCase.CandidateId);
        Assert.Equal("Trần Văn Onboard1", obCase.EmployeeName);
        Assert.Equal("InProgress", obCase.Status);
        Assert.NotEmpty(obCase.Checklist);

        var candDb = await _db.Candidates.FirstAsync(x => x.Id == c.Id);
        Assert.NotNull(candDb.ConvertedEmployeeId);
        Assert.Equal(obCase.EmployeeId, candDb.ConvertedEmployeeId);
    }

    [Fact]
    public async Task UC068_HireFromCandidate_NonAcceptedCandidate_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Lê Văn Onboard2", "ob2@example.com", "0988000112", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
                new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId)));

        Assert.Contains("Accepted", ex.Message);
    }

    [Fact]
    public async Task UC068_HireFromCandidate_AlreadyConverted_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Phạm Văn Onboard3", "ob3@example.com", "0988000113", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Đồng ý nhận việc"));

        await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
                new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId)));

        Assert.Contains("được tạo hồ sơ", ex.Message);
    }

    [Fact]
    public async Task UC068_HireFromCandidate_NonExistentCandidate_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
                new HireFromCandidateRequest(Guid.NewGuid(), _orgUnitId, _jobTitleId)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC068_HireFromCandidate_InvalidOrgUnit_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Đỗ Văn Onboard4", "ob4@example.com", "0988000114", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Đồng ý"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
                new HireFromCandidateRequest(c.Id, Guid.NewGuid(), _jobTitleId)));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_069: Gán người hướng dẫn (Assign Onboarding Mentor)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC069_AssignMentor_ValidMentor_UpdatesCaseSuccessfully()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Hoàng Văn Mentor1", "m1@example.com", "0977000111", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var updated = await _onboardingSvc.AssignMentorAsync(_tenant, obCase.Id,
            new AssignMentorRequest(_mentorEmpId));

        Assert.Equal("Nguyễn Văn Mentor", updated.MentorName);
    }

    [Fact]
    public async Task UC069_AssignMentor_SelfAsMentor_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Hoàng Văn Mentor2", "m2@example.com", "0977000112", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        // Gán chính nhân viên mới làm mentor cho chính mình -> throw
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AssignMentorAsync(_tenant, obCase.Id,
                new AssignMentorRequest(obCase.EmployeeId)));

        Assert.Contains("chính nhân viên mới", ex.Message);
    }

    [Fact]
    public async Task UC069_AssignMentor_NonExistentMentor_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Hoàng Văn Mentor3", "m3@example.com", "0977000113", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AssignMentorAsync(_tenant, obCase.Id,
                new AssignMentorRequest(Guid.NewGuid())));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC069_AssignMentor_NonExistentCase_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AssignMentorAsync(_tenant, Guid.NewGuid(),
                new AssignMentorRequest(_mentorEmpId)));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_070: Checklist tiếp nhận nhân viên mới (Onboarding Checklist)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC070_UpdateChecklist_ValidItems_UpdatesChecklistSuccessfully()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Bùi Văn Check1", "chk1@example.com", "0966000111", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var newItems = new[]
        {
            new OnboardingChecklistItemDto("account", "Tạo tài khoản Email & ERP", true),
            new OnboardingChecklistItemDto("equipment", "Cấp phát laptop", true),
            new OnboardingChecklistItemDto("policy", "Học nội quy công ty", false)
        };

        var updated = await _onboardingSvc.UpdateChecklistAsync(_tenant, obCase.Id,
            new OnboardingChecklistUpdateRequest(newItems));

        Assert.Equal(3, updated.Checklist.Count);
        Assert.True(updated.Checklist[0].Done);
        Assert.True(updated.Checklist[1].Done);
        Assert.False(updated.Checklist[2].Done);
    }

    [Fact]
    public async Task UC070_UpdateChecklist_NonExistentCase_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.UpdateChecklistAsync(_tenant, Guid.NewGuid(),
                new OnboardingChecklistUpdateRequest(Array.Empty<OnboardingChecklistItemDto>())));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_071: Upload chứng chỉ / giấy tờ tiếp nhận (Onboarding Document)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC071_AddDocument_ValidDoc_AddsDocumentSuccessfully()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Vũ Văn Doc1", "doc1@example.com", "0955000111", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var updated = await _onboardingSvc.AddDocumentAsync(_tenant, obCase.Id,
            new OnboardingDocUploadRequest("Bằng đại học CNTT", "docs/degree_01.pdf"));

        Assert.Single(updated.Documents);
        Assert.Equal("Bằng đại học CNTT", updated.Documents[0].Title);
        Assert.Equal("docs/degree_01.pdf", updated.Documents[0].StorageKey);
    }

    [Fact]
    public async Task UC071_AddDocument_EmptyTitle_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Vũ Văn Doc2", "doc2@example.com", "0955000112", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AddDocumentAsync(_tenant, obCase.Id,
                new OnboardingDocUploadRequest("   ", "docs/degree_02.pdf")));

        Assert.Contains("Thiếu tiêu đề", ex.Message);
    }

    [Fact]
    public async Task UC071_AddDocument_TitleTooLong_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId, "Vũ Văn Doc3", "doc3@example.com", "0955000113", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Duyệt"));
        var obCase = await _onboardingSvc.HireFromCandidateAsync(_tenant, _user,
            new HireFromCandidateRequest(c.Id, _orgUnitId, _jobTitleId));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AddDocumentAsync(_tenant, obCase.Id,
                new OnboardingDocUploadRequest(new string('A', 201), "docs/degree_03.pdf")));

        Assert.Contains("200 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC071_AddDocument_NonExistentCase_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.AddDocumentAsync(_tenant, Guid.NewGuid(),
                new OnboardingDocUploadRequest("Bằng cấp", "docs/degree.pdf")));

        Assert.Equal(404, ex.StatusCode);
    }
}

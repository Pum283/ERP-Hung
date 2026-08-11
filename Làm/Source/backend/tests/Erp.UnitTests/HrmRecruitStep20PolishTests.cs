using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Erp.Infrastructure.Implementations.Services.Sys;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 20:
///   UC_HRM_064 — Lịch sử chăm sóc ứng viên (Structured Care Notes History)
///   UC_HRM_065 — Báo cáo hiệu quả kênh tuyển (Recruit Channel Funnel Report)
///   UC_HRM_066 — Cấu hình thời hạn onboarding (Onboarding Duration Configuration 1-365 days)
///   UC_HRM_067 — Cấu hình thời hạn thử việc (Probation Duration Configuration 1-365 days)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmRecruitStep20PolishTests : IDisposable
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

    private Guid _postingId1;
    private Guid _postingId2;

    public HrmRecruitStep20PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-recruit-step20-" + Guid.NewGuid())
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
            Code = "ORG_HR20", Name = "Phòng Tuyển Dụng 20", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV20", Name = "Kỹ Sư Backend"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hr_user20", DisplayName = "Phạm HR 20"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver20", DisplayName = "Trần Giám Đốc"
        });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);
        _onboardingSvc = new HrmOnboardingService(_db, new SysPlatformService(_db, new OutboxWriter(_db)));

        SetupTestDataAsync().GetAwaiter().GetResult();
    }

    private async Task SetupTestDataAsync()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 5, "Tuyển Backend Devs", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "Duyệt tuyển"));

        var p1 = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Backend Dev - LinkedIn", "LinkedIn"));
        var p2 = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Backend Dev - Website", "Website"));

        _postingId1 = p1.Id;
        _postingId2 = p2.Id;
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_064: Lịch sử chăm sóc ứng viên (Care Notes History)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC064_GetCareNotes_EmptyNotes_ReturnsEmptyList()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId1, "Trần Văn Care1", "care1@example.com", "0911000111", null));

        var history = await _pipelineSvc.GetCareNotesAsync(_tenant, c.Id);

        Assert.Empty(history);
    }

    [Fact]
    public async Task UC064_AddAndGetCareNotes_MultipleEntries_ReturnsStructuredHistory()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId1, "Lê Văn Care2", "care2@example.com", "0911000112", null));

        await _pipelineSvc.AddCareNoteAsync(_tenant, c.Id, new CandidateCareNoteRequest("Ghi chú 1: Đã gọi điện lần 1"));
        await _pipelineSvc.AddCareNoteAsync(_tenant, c.Id, new CandidateCareNoteRequest("Ghi chú 2: Hẹn phỏng vấn tuần sau"));

        var history = await _pipelineSvc.GetCareNotesAsync(_tenant, c.Id);

        Assert.Equal(2, history.Count);
        Assert.Contains("Đã gọi điện lần 1", history[0].Note);
        Assert.Contains("Hẹn phỏng vấn tuần sau", history[1].Note);
    }

    [Fact]
    public async Task UC064_GetCareNotes_NonExistentCandidate_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.GetCareNotesAsync(_tenant, Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC064_AddCareNote_EmptyNote_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId1, "Phạm Văn Care3", "care3@example.com", "0911000113", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.AddCareNoteAsync(_tenant, c.Id, new CandidateCareNoteRequest("   ")));
        Assert.Contains("trống", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_065: Báo cáo hiệu quả kênh tuyển (Recruit Channel Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC065_GetChannelReport_CalculatesFunnelAndConversionRatesCorrectly()
    {
        // Kênh LinkedIn: 2 candidates (1 Accepted, 1 Rejected)
        var c1 = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId1, "UV LinkedIn 1", "li1@example.com", "0922111001", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c1.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c1.Id, new CandidateDecideRequest("Accept", "Nhận việc"));

        var c2 = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId1, "UV LinkedIn 2", "li2@example.com", "0922111002", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c2.Id, new CandidateScreenRequest("ScreenReject", "Chưa đạt"));

        // Kênh Website: 1 candidate (Screening)
        var c3 = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_postingId2, "UV Web 1", "web1@example.com", "0922111003", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c3.Id, new CandidateScreenRequest("Screen", "Sơ loại OK"));

        var report = await _pipelineSvc.GetChannelReportAsync(_tenant);

        Assert.NotEmpty(report);
        var li = report.FirstOrDefault(x => x.Channel == "LinkedIn");
        Assert.NotNull(li);
        Assert.Equal(1, li.PostingCount);
        Assert.Equal(2, li.CandidateCount);
        Assert.Equal(1, li.AcceptedCount);
        Assert.Equal(1, li.RejectedCount);
        Assert.Equal(50.0, li.ConversionRatePct); // 1 / 2 * 100%

        var web = report.FirstOrDefault(x => x.Channel == "Website");
        Assert.NotNull(web);
        Assert.Equal(1, web.CandidateCount);
        Assert.Equal(1, web.ScreeningCount);
        Assert.Equal(0, web.AcceptedCount);
        Assert.Equal(0.0, web.ConversionRatePct);
    }

    [Fact]
    public async Task UC065_GetChannelReport_NoCandidates_ReturnsZeroConversionRate()
    {
        var report = await _pipelineSvc.GetChannelReportAsync(_tenant);

        Assert.NotEmpty(report);
        foreach (var row in report)
        {
            if (row.CandidateCount == 0)
                Assert.Equal(0.0, row.ConversionRatePct);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_066: Cấu hình thời hạn onboarding (Onboarding Duration Setting)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC066_GetSettings_DefaultOnboardingDays_ReturnsDefault()
    {
        var settings = await _onboardingSvc.GetSettingsAsync(_tenant);
        Assert.True(settings.OnboardingDays >= 1 && settings.OnboardingDays <= 365);
    }

    [Fact]
    public async Task UC066_UpsertSettings_ValidOnboardingDays_UpdatesSuccessfully()
    {
        var updated = await _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
            new OnboardingSettingUpsertRequest(45, 60));

        Assert.Equal(45, updated.OnboardingDays);
        Assert.Equal(60, updated.TrialDays);

        var current = await _onboardingSvc.GetSettingsAsync(_tenant);
        Assert.Equal(45, current.OnboardingDays);
    }

    [Fact]
    public async Task UC066_UpsertSettings_OnboardingDaysZero_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
                new OnboardingSettingUpsertRequest(0, 60)));
        Assert.Contains("1–365", ex.Message);
    }

    [Fact]
    public async Task UC066_UpsertSettings_OnboardingDaysOver365_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
                new OnboardingSettingUpsertRequest(400, 60)));
        Assert.Contains("1–365", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_067: Cấu hình thời hạn thử việc (Probation / Trial Duration Setting)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC067_GetSettings_DefaultTrialDays_ReturnsDefault()
    {
        var settings = await _onboardingSvc.GetSettingsAsync(_tenant);
        Assert.True(settings.TrialDays >= 1 && settings.TrialDays <= 365);
    }

    [Fact]
    public async Task UC067_UpsertSettings_ValidTrialDays_UpdatesSuccessfully()
    {
        var updated = await _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
            new OnboardingSettingUpsertRequest(30, 90));

        Assert.Equal(30, updated.OnboardingDays);
        Assert.Equal(90, updated.TrialDays);

        var current = await _onboardingSvc.GetSettingsAsync(_tenant);
        Assert.Equal(90, current.TrialDays);
    }

    [Fact]
    public async Task UC067_UpsertSettings_TrialDaysZero_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
                new OnboardingSettingUpsertRequest(30, 0)));
        Assert.Contains("1–365", ex.Message);
    }

    [Fact]
    public async Task UC067_UpsertSettings_TrialDaysOver365_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _onboardingSvc.UpsertSettingsAsync(_tenant, _user,
                new OnboardingSettingUpsertRequest(30, 500)));
        Assert.Contains("1–365", ex.Message);
    }
}

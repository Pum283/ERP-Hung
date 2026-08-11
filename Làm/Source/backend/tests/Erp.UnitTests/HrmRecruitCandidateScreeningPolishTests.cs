using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 18:
///   UC_HRM_055 — Ghi nhận kênh đăng tuyển (channel tracking &amp; stats)
///   UC_HRM_056 — Nhập hồ sơ ứng viên (validation chặt, chống trùng email/phone)
///   UC_HRM_057 — Upload file CV (gắn CvStorageKey vào candidate)
///   UC_HRM_059 — Sơ loại ứng viên (Screen / ScreenReject)
/// 15 test cases bao phủ luồng thành công và luồng lỗi đầy đủ.
/// </summary>
public sealed class HrmRecruitCandidateScreeningPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly HrmRecruitPipelineService _pipelineSvc;

    private readonly Guid _tenant   = Guid.NewGuid();
    private readonly Guid _user     = Guid.NewGuid();
    private readonly Guid _approver = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    // Mỗi test có 1 posting riêng để tránh side-effect
    private Guid _openPostingId;

    public HrmRecruitCandidateScreeningPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-recruit-step18-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant,
            PlanCode = "ENTERPRISE",
            Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100,
            MaxOrgUnits = 500
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnitId, TenantId = _tenant,
            Code = "ORG_HR18", Name = "Phòng Nhân Sự", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV", Name = "Lập Trình Viên"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hr_user", DisplayName = "Nguyễn HR"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "director", DisplayName = "Trần Giám Đốc"
        });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);

        // Tạo phiếu đề xuất đã duyệt và tin tuyển mở sẵn
        SetupApprovedPostingAsync().GetAwaiter().GetResult();
    }

    private async Task SetupApprovedPostingAsync()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 3, "Tuyển dev khẩn", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "Phê duyệt!"));
        var posting = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Tuyển Lập Trình Viên Hà Nội", "LinkedIn"));
        _openPostingId = posting.Id;
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_055: Kênh đăng tuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC055_ChannelStats_AfterCreatePosting_ReturnsLinkedInChannel()
    {
        var stats = await _pipelineSvc.ChannelStatsAsync(_tenant);

        var linkedIn = stats.FirstOrDefault(s => s.Channel == "LinkedIn");
        Assert.NotNull(linkedIn);
        Assert.Equal(1, linkedIn!.PostingCount);
    }

    [Fact]
    public async Task UC055_CreatePosting_InvalidChannel_ThrowsAppException()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 1, "Tuyển kênh sai", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "OK"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreatePostingAsync(_tenant, _user,
                new JobPostingCreateRequest(rr.Id, "Test kênh sai", "TikTok")));
        Assert.Contains("Kênh không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC055_ChannelStats_AfterAddCandidate_IncreasesCandidateCount()
    {
        await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Nguyễn Văn A", "a@example.com", "0901234567", null));

        var stats = await _pipelineSvc.ChannelStatsAsync(_tenant);
        var linkedIn = stats.FirstOrDefault(s => s.Channel == "LinkedIn");
        Assert.NotNull(linkedIn);
        Assert.Equal(1, linkedIn!.CandidateCount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_056: Nhập hồ sơ ứng viên — validation chặt
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC056_CreateCandidate_ValidData_ReturnsNewPipeline()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Trần Thị B", "b@example.com", "0912345678", null));

        Assert.Equal("New", c.PipelineStatus);
        Assert.Equal("Trần Thị B", c.FullName);
        Assert.Equal("b@example.com", c.Email);
    }

    [Fact]
    public async Task UC056_CreateCandidate_EmptyName_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreateCandidateAsync(_tenant, _user,
                new CandidateCreateRequest(_openPostingId, "   ", null, null, null)));
        Assert.Contains("Họ tên", ex.Message);
    }

    [Fact]
    public async Task UC056_CreateCandidate_InvalidEmailFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreateCandidateAsync(_tenant, _user,
                new CandidateCreateRequest(_openPostingId, "Lê Văn C", "not-an-email", null, null)));
        Assert.Contains("email", ex.Message);
    }

    [Fact]
    public async Task UC056_CreateCandidate_InvalidPhoneFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreateCandidateAsync(_tenant, _user,
                new CandidateCreateRequest(_openPostingId, "Lê Văn D", null, "12", null)));
        Assert.Contains("điện thoại", ex.Message);
    }

    [Fact]
    public async Task UC056_CreateCandidate_DuplicateEmail_SamePosting_ThrowsAppException()
    {
        // Tạo lần 1
        await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Phạm Văn E", "dup@example.com", null, null));

        // Tạo lần 2 cùng email — phải reject
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreateCandidateAsync(_tenant, _user,
                new CandidateCreateRequest(_openPostingId, "Phạm Văn E2", "dup@example.com", null, null)));
        Assert.Contains("Email", ex.Message);
        Assert.Contains("đã được đăng ký", ex.Message);
    }

    [Fact]
    public async Task UC056_CreateCandidate_DuplicatePhone_SamePosting_ThrowsAppException()
    {
        await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Hoàng Văn F", null, "0933333333", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.CreateCandidateAsync(_tenant, _user,
                new CandidateCreateRequest(_openPostingId, "Hoàng Văn F2", null, "0933333333", null)));
        Assert.Contains("điện thoại", ex.Message);
        Assert.Contains("đã được đăng ký", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_057: Upload file CV (gắn CvStorageKey)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC057_CreateCandidate_WithCvStorageKey_StoresKey()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Đặng Văn G", "g@example.com", null,
                "uploads/2026/cv_dang_van_g.pdf"));

        Assert.Equal("uploads/2026/cv_dang_van_g.pdf", c.CvStorageKey);
    }

    [Fact]
    public async Task UC057_CreateCandidate_NoCv_CvStorageKeyIsNull()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Vũ Thị H", "h@example.com", null, null));

        Assert.Null(c.CvStorageKey);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_059: Sơ loại ứng viên
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC059_Screen_NewCandidate_ChangesStatusToScreening()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Ngô Văn I", "i@example.com", null, null));

        var result = await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id,
            new CandidateScreenRequest("Screen", "CV phù hợp, mời vòng phỏng vấn sơ bộ."));

        Assert.Equal("Screening", result.PipelineStatus);
        Assert.Equal("CV phù hợp, mời vòng phỏng vấn sơ bộ.", result.ScreeningNote);
    }

    [Fact]
    public async Task UC059_ScreenReject_NewCandidate_ChangesStatusToRejected()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Bùi Thị K", "k@example.com", null, null));

        var result = await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id,
            new CandidateScreenRequest("ScreenReject", "Kinh nghiệm chưa đủ yêu cầu."));

        Assert.Equal("Rejected", result.PipelineStatus);
        Assert.Equal("Kinh nghiệm chưa đủ yêu cầu.", result.ScreeningNote);
    }

    [Fact]
    public async Task UC059_Screen_AcceptedCandidate_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Đinh Văn L", "l@example.com", null, null));
        // Chuyển thẳng sang Accepted
        await _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Accepted"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id,
                new CandidateScreenRequest("Screen", "Sơ loại sau accepted?")));
        Assert.Contains("Accepted", ex.Message);
    }

    [Fact]
    public async Task UC059_Screen_EmptyNote_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Lý Thị M", "m@example.com", null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id,
                new CandidateScreenRequest("Screen", "   ")));
        Assert.Contains("ghi chú sơ loại", ex.Message);
    }

    [Fact]
    public async Task UC059_Screen_InvalidAction_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Mạc Văn N", "n@example.com", null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id,
                new CandidateScreenRequest("INVALID_ACTION", "Ghi chú gì đó")));
        Assert.Contains("không hợp lệ", ex.Message);
    }
}

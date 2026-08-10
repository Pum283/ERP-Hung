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
/// Unit tests cho Bước 17: UC_HRM_051 (Duyệt / từ chối đề xuất), UC_HRM_052 (Xem lịch sử duyệt đề xuất),
/// UC_HRM_053 (Đóng / hủy phiếu đề xuất), UC_HRM_054 (Tạo tin tuyển từ phiếu đã duyệt).
/// 13+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmRecruitApprovalPostingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly HrmRecruitPipelineService _pipelineSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user   = Guid.NewGuid();
    private readonly Guid _approver = Guid.NewGuid();
    private readonly Guid _orgUnitId = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    public HrmRecruitApprovalPostingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-recruit-step17-" + Guid.NewGuid())
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

        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnitId, TenantId = _tenant, Code = "ORG_REC2", Name = "Phòng Tuyển dụng", UnitType = "Department", Path = "/1" });
        _db.JobTitles.Add(new JobTitle { Id = _jobTitleId, TenantId = _tenant, Code = "JT_QA", Name = "Kỹ sư QC/QA" });
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "requester", DisplayName = "Lê Đề Xuất" });
        _db.Users.Add(new AppUser { Id = _approver, TenantId = _tenant, Username = "approver", DisplayName = "Trần Trưởng Phòng" });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_051: Duyệt / từ chối đề xuất ───

    [Fact]
    public async Task UC051_ApproveRequest_PendingStatus_TransitionsToApproved()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 1", _orgUnitId, true));

        var approved = await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Approve", "Đồng ý tuyển dụng"));

        Assert.Equal("Approved", approved.Status);
    }

    [Fact]
    public async Task UC051_RejectRequest_PendingStatus_TransitionsToRejected()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 2", _orgUnitId, true));

        var rejected = await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Reject", "Tạm hoãn ngân sách"));

        Assert.Equal("Rejected", rejected.Status);
    }

    [Fact]
    public async Task UC051_ApproveRequest_NotPending_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 3", _orgUnitId, false));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Approve", null)));
        Assert.Contains("Chờ duyệt", ex.Message);
    }

    [Fact]
    public async Task UC051_RejectRequest_WithoutComment_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 4", _orgUnitId, true));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Reject", "   ")));
        Assert.Contains("lý do khi từ chối", ex.Message);
    }

    [Fact]
    public async Task UC051_ApproveRequest_InvalidAction_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 5", _orgUnitId, true));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("INVALID", "OK")));
        Assert.Contains("không hợp lệ", ex.Message);
    }

    // ─── UC_HRM_052: Xem lịch sử duyệt đề xuất ───

    [Fact]
    public async Task UC052_GetApprovalHistory_PendingApprove_ReturnsStepHistory()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA cho team 6", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Approve", "Duyệt nhanh"));

        var history = await _recruitSvc.GetApprovalHistoryAsync(_tenant, req.Id);

        Assert.Single(history);
        Assert.Equal("Approved", history[0].Action);
        Assert.Equal("Trần Trưởng Phòng", history[0].ActorName);
        Assert.Equal("Duyệt nhanh", history[0].Comment);
    }

    [Fact]
    public async Task UC052_GetApprovalHistory_DraftRequest_ReturnsEmptyList()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA nháp", _orgUnitId, false));

        var history = await _recruitSvc.GetApprovalHistoryAsync(_tenant, req.Id);

        Assert.Empty(history);
    }

    // ─── UC_HRM_053: Đóng / hủy phiếu đề xuất ───

    [Fact]
    public async Task UC053_CancelOrClose_DraftRequest_SetsCancelled()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA nháp hủy", _orgUnitId, false));

        var res = await _recruitSvc.CancelOrCloseAsync(_tenant, _user, req.Id);

        Assert.Equal("Cancelled", res.Status);
    }

    [Fact]
    public async Task UC053_CancelOrClose_ApprovedRequest_SetsClosed()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA đóng", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Approve", "OK"));

        var res = await _recruitSvc.CancelOrCloseAsync(_tenant, _user, req.Id);

        Assert.Equal("Closed", res.Status);
    }

    [Fact]
    public async Task UC053_CancelOrClose_PendingRequest_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA chờ duyệt", _orgUnitId, true));

        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CancelOrCloseAsync(_tenant, _user, req.Id));
        Assert.Contains("Phiếu đang chờ duyệt", ex.Message);
    }

    [Fact]
    public async Task UC053_CancelOrClose_AlreadyCancelled_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển QA đã hủy", _orgUnitId, false));
        await _recruitSvc.CancelOrCloseAsync(_tenant, _user, req.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CancelOrCloseAsync(_tenant, _user, req.Id));
        Assert.Contains("đã được đóng hoặc hủy", ex.Message);
    }

    // ─── UC_HRM_054: Tạo tin tuyển từ phiếu đã duyệt ───

    [Fact]
    public async Task UC054_CreatePosting_ApprovedRequest_CreatesOpenPosting()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 3, "Tuyển Senior QA gấp", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, req.Id, new ApproveRecruitmentRequest("Approve", "Duyệt ngay"));

        var postReq = new JobPostingCreateRequest(req.Id, "Tuyển Dụng Senior QA - Hà Nội", "LinkedIn");
        var post = await _pipelineSvc.CreatePostingAsync(_tenant, _user, postReq);

        Assert.Equal("Open", post.Status);
        Assert.Equal("Tuyển Dụng Senior QA - Hà Nội", post.Title);
        Assert.Equal("LinkedIn", post.Channel);
        Assert.Equal(req.DocNo, post.RequestDocNo);
        Assert.Equal("Kỹ sư QC/QA", post.JobTitleName);
    }

    [Fact]
    public async Task UC054_CreatePosting_DraftRequest_ThrowsAppException()
    {
        var req = await _recruitSvc.CreateAsync(_tenant, _user, new RecruitmentRequestCreateRequest(_jobTitleId, 3, "Tuyển QA chưa duyệt", _orgUnitId, false));

        var postReq = new JobPostingCreateRequest(req.Id, "Tuyển Dụng QA Chưa Duyệt", "Website");
        var ex = await Assert.ThrowsAsync<AppException>(() => _pipelineSvc.CreatePostingAsync(_tenant, _user, postReq));
        Assert.Contains("phiếu đã duyệt", ex.Message);
    }
}

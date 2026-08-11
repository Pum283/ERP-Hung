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
/// Unit tests cho Bước 19:
///   UC_HRM_060 — Chuyển ứng viên cho đơn vị đánh giá (Assign Eval Org Unit)
///   UC_HRM_061 — Form đánh giá ứng viên chi tiết (Score 0-100, EvalResult Pass|Fail|Hold)
///   UC_HRM_062 — Từ chối / chấp nhận ứng viên (Accept / Reject with DecisionNote)
///   UC_HRM_063 — Pipeline trạng thái ứng viên (Strict State Machine Transitions)
/// 18 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmRecruitEvaluationDecidePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly HrmRecruitPipelineService _pipelineSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _approver   = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _openPostingId;

    public HrmRecruitEvaluationDecidePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-recruit-step19-" + Guid.NewGuid())
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
            Code = "ORG_HR19", Name = "Phòng Công Nghệ Thông Tin", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_ARCH", Name = "Kỹ Sư Kiến Trúc Phầm Mềm"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hr_user19", DisplayName = "Phạm HR 19"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver19", DisplayName = "Nguyễn Giám Đốc"
        });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
        _pipelineSvc = new HrmRecruitPipelineService(_db);

        SetupApprovedPostingAsync().GetAwaiter().GetResult();
    }

    private async Task SetupApprovedPostingAsync()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển Solution Architect", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id,
            new ApproveRecruitmentRequest("Approve", "Duyệt tuyển dụng Architect"));
        var posting = await _pipelineSvc.CreatePostingAsync(_tenant, _user,
            new JobPostingCreateRequest(rr.Id, "Tuyển Solution Architect - Hồ Chí Minh", "LinkedIn"));
        _openPostingId = posting.Id;
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_060: Chuyển ứng viên cho đơn vị đánh giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC060_AssignEvalOrgUnit_ScreeningCandidate_TransitionsToEvaluating()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Trần Văn Arch", "arch@example.com", "0988111222", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "CV đáp ứng yêu cầu"));

        var updated = await _pipelineSvc.AssignEvalOrgUnitAsync(_tenant, c.Id,
            new CandidateAssignEvalOrgRequest(_orgUnitId));

        Assert.Equal("Evaluating", updated.PipelineStatus);
        Assert.Equal(_orgUnitId, updated.EvalOrgUnitId);
        Assert.Equal("Phòng Công Nghệ Thông Tin", updated.EvalOrgUnitName);
    }

    [Fact]
    public async Task UC060_AssignEvalOrgUnit_InvalidOrgUnit_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Lê Văn Arch2", "arch2@example.com", "0988111223", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.AssignEvalOrgUnitAsync(_tenant, c.Id,
                new CandidateAssignEvalOrgRequest(Guid.NewGuid())));
        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Đơn vị đánh giá", ex.Message);
    }

    [Fact]
    public async Task UC060_AssignEvalOrgUnit_AcceptedCandidate_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Nguyễn Văn Arch3", "arch3@example.com", "0988111224", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.AssignEvalOrgUnitAsync(_tenant, c.Id, new CandidateAssignEvalOrgRequest(_orgUnitId));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Đồng ý nhận việc"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.AssignEvalOrgUnitAsync(_tenant, c.Id, new CandidateAssignEvalOrgRequest(_orgUnitId)));
        Assert.Contains("Accepted", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_061: Form đánh giá ứng viên chi tiết
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC061_SubmitEvaluation_ValidScoreAndResult_ReturnsEvaluatingStatus()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Hoàng Văn Eval", "eval@example.com", "0988222333", null));

        var updated = await _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
            new CandidateSubmitEvalRequest(_orgUnitId, 85, "Pass", "Ứng viên nắm vững kiến thức kiến trúc hệ thống."));

        Assert.Equal("Evaluating", updated.PipelineStatus);
        Assert.Equal(85, updated.EvalScore);
        Assert.Equal("Pass", updated.EvalResult);
        Assert.Equal("Ứng viên nắm vững kiến thức kiến trúc hệ thống.", updated.EvalComment);
    }

    [Fact]
    public async Task UC061_SubmitEvaluation_InvalidScore_Negative_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Vũ Văn ScoreErr", "score_err@example.com", "0988222334", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
                new CandidateSubmitEvalRequest(_orgUnitId, -5, "Pass", "Điểm âm")));
        Assert.Contains("0–100", ex.Message);
    }

    [Fact]
    public async Task UC061_SubmitEvaluation_InvalidScore_Over100_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Vũ Văn ScoreErr2", "score_err2@example.com", "0988222335", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
                new CandidateSubmitEvalRequest(_orgUnitId, 105, "Pass", "Điểm quá 100")));
        Assert.Contains("0–100", ex.Message);
    }

    [Fact]
    public async Task UC061_SubmitEvaluation_InvalidResult_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Phạm Văn ResultErr", "res_err@example.com", "0988222336", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
                new CandidateSubmitEvalRequest(_orgUnitId, 70, "INVALID_RESULT", "Kết quả sai")));
        Assert.Contains("Pass|Fail|Hold", ex.Message);
    }

    [Fact]
    public async Task UC061_SubmitEvaluation_CommentTooLong_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Bùi Văn CommentErr", "com_err@example.com", "0988222337", null));
        var longComment = new string('A', 1001);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
                new CandidateSubmitEvalRequest(_orgUnitId, 70, "Pass", longComment)));
        Assert.Contains("1000 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC061_SubmitEvaluation_AcceptedCandidate_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Đặng Văn Accepted", "accepted_eval@example.com", "0988222338", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Nhận ngay"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
                new CandidateSubmitEvalRequest(_orgUnitId, 90, "Pass", "Đánh giá lại ứng viên đã nhận")));
        Assert.Contains("Accepted", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_062: Ra quyết định tuyển dụng (Accept / Reject)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC062_DecideCandidate_Accept_EvaluatingCandidate_TransitionsToAccepted()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Nghiêm Văn Pass", "pass@example.com", "0988333444", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "CV ok"));
        await _pipelineSvc.SubmitEvaluationAsync(_tenant, c.Id,
            new CandidateSubmitEvalRequest(_orgUnitId, 92, "Pass", "Xuất sắc"));

        var decided = await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
            new CandidateDecideRequest("Accept", "Gửi thư mời làm việc mức lương Gross 45M"));

        Assert.Equal("Accepted", decided.PipelineStatus);
        Assert.Equal("Gửi thư mời làm việc mức lương Gross 45M", decided.DecisionNote);
    }

    [Fact]
    public async Task UC062_DecideCandidate_Accept_ClosedPosting_ThrowsAppException()
    {
        // Tạo tin tuyển riêng và đóng lại
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 1, "Tuyển QA đóng", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id, new ApproveRecruitmentRequest("Approve", "OK"));
        var closedPost = await _pipelineSvc.CreatePostingAsync(_tenant, _user, new JobPostingCreateRequest(rr.Id, "Tin sắp đóng", "Website"));
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(closedPost.Id, "Phan Văn Closed", "closed@example.com", "0988333445", null));
        await _pipelineSvc.ClosePostingAsync(_tenant, closedPost.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
                new CandidateDecideRequest("Accept", "Chấp nhận khi tin đã đóng")));
        Assert.Contains("Tin tuyển đã đóng", ex.Message);
    }

    [Fact]
    public async Task UC062_DecideCandidate_Reject_EvaluatingCandidate_TransitionsToRejected()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Đỗ Văn Fail", "fail@example.com", "0988333446", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "Vào vòng sau"));

        var decided = await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
            new CandidateDecideRequest("Reject", "Mức lương kỳ vọng vượt quá ngân sách bộ phận."));

        Assert.Equal("Rejected", decided.PipelineStatus);
        Assert.Equal("Mức lương kỳ vọng vượt quá ngân sách bộ phận.", decided.DecisionNote);
    }

    [Fact]
    public async Task UC062_DecideCandidate_WithoutNote_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Dương Văn NoNote", "nonote@example.com", "0988333447", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
                new CandidateDecideRequest("Accept", "   ")));
        Assert.Contains("ghi chú", ex.Message);
    }

    [Fact]
    public async Task UC062_DecideCandidate_InvalidAction_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Cao Văn ActionErr", "action_err@example.com", "0988333448", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
                new CandidateDecideRequest("INVALID_ACTION", "Ghi chú hợp lệ")));
        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC062_DecideCandidate_Reject_AlreadyAcceptedCandidate_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Trịnh Văn DoubleDecide", "double@example.com", "0988333449", null));
        await _pipelineSvc.ScreenCandidateAsync(_tenant, c.Id, new CandidateScreenRequest("Screen", "OK"));
        await _pipelineSvc.DecideCandidateAsync(_tenant, c.Id, new CandidateDecideRequest("Accept", "Đã chấp nhận"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.DecideCandidateAsync(_tenant, c.Id,
                new CandidateDecideRequest("Reject", "Từ chối sau khi đã accept")));
        Assert.Contains("Accepted", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_063: Pipeline trạng thái ứng viên (Strict State Machine)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC063_UpdatePipeline_ValidTransitions_Succeeds()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Chu Văn Pipeline", "pipe@example.com", "0988444555", null));

        // New -> Screening
        var s1 = await _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Screening"));
        Assert.Equal("Screening", s1.PipelineStatus);

        // Screening -> Evaluating
        var s2 = await _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Evaluating"));
        Assert.Equal("Evaluating", s2.PipelineStatus);

        // Evaluating -> Accepted
        var s3 = await _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Accepted"));
        Assert.Equal("Accepted", s3.PipelineStatus);
    }

    [Fact]
    public async Task UC063_UpdatePipeline_InvalidDirectTransition_NewToAccepted_ThrowsAppException()
    {
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(_openPostingId, "Tạ Văn Jump", "jump@example.com", "0988444556", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Accepted")));
        Assert.Contains("Không thể chuyển trạng thái trực tiếp", ex.Message);
    }

    [Fact]
    public async Task UC063_UpdatePipeline_ClosedPosting_ThrowsAppException()
    {
        var rr = await _recruitSvc.CreateAsync(_tenant, _user,
            new RecruitmentRequestCreateRequest(_jobTitleId, 1, "Tuyển QA cho tin closed", _orgUnitId, true));
        await _recruitSvc.ApproveOrRejectAsync(_tenant, _approver, rr.Id, new ApproveRecruitmentRequest("Approve", "OK"));
        var post = await _pipelineSvc.CreatePostingAsync(_tenant, _user, new JobPostingCreateRequest(rr.Id, "Tin tuyển closed pipeline", "Website"));
        var c = await _pipelineSvc.CreateCandidateAsync(_tenant, _user,
            new CandidateCreateRequest(post.Id, "Đồng Văn ClosedPipe", "cpipe@example.com", "0988444557", null));

        await _pipelineSvc.ClosePostingAsync(_tenant, post.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _pipelineSvc.UpdatePipelineAsync(_tenant, c.Id, new CandidatePipelineUpdateRequest("Screening")));
        Assert.Contains("Tin tuyển đã đóng", ex.Message);
    }
}

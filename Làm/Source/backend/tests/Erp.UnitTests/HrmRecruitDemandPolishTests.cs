using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Wf;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Lightweight fake implementation of IWfRuntimeService for testing.
/// </summary>
public sealed class FakeWfRuntimeService : IWfRuntimeService
{
    public int StartCallCount { get; private set; }
    public Guid LastStartedDocId { get; private set; }
    public string? LastDefinitionCode { get; private set; }

    public Task<Guid> StartAsync(Guid tenantId, string definitionCode, string sourceModule, string sourceDocType, Guid sourceDocId, Guid requesterUserId, Guid? assigneeUserId, CancellationToken ct = default)
    {
        StartCallCount++;
        LastStartedDocId = sourceDocId;
        LastDefinitionCode = definitionCode;
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<IReadOnlyList<WfTaskDto>> MyPendingTasksAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<WfTaskDto>>(Array.Empty<WfTaskDto>());

    public Task ActAsync(Guid tenantId, Guid taskId, Guid actorUserId, WfActRequest req, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<WfDelegationDto>> ListDelegationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<WfDelegationDto>>(Array.Empty<WfDelegationDto>());

    public Task<WfDelegationDto> UpsertDelegationAsync(Guid tenantId, Guid userId, WfDelegationUpsertRequest req, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task DeactivateDelegationAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<WfDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default)
        => throw new NotImplementedException();
}

/// <summary>
/// Unit tests cho Bước 16: UC_HRM_047 (Tạo phiếu đề xuất tuyển dụng), UC_HRM_048 (Chọn vị trí & số lượng cần tuyển),
/// UC_HRM_049 (Nhập lý do tuyển dụng), UC_HRM_050 (Gửi phiếu đề xuất đi duyệt).
/// 13+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmRecruitDemandPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FakeWfRuntimeService _wfFake;
    private readonly HrmRecruitService _recruitSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user   = Guid.NewGuid();
    private readonly Guid _orgUnitId = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    public HrmRecruitDemandPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-recruit-step16-" + Guid.NewGuid())
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

        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnitId, TenantId = _tenant, Code = "ORG_REC", Name = "Phòng Nhân sự", UnitType = "Department", Path = "/1" });
        _db.JobTitles.Add(new JobTitle { Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV", Name = "Lập trình viên Senior" });
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "recruiter", DisplayName = "Nguyễn Văn Đề Xuất" });
        _db.SaveChanges();

        _wfFake = new FakeWfRuntimeService();
        _recruitSvc = new HrmRecruitService(_db, _wfFake);
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_047: Tạo phiếu đề xuất tuyển dụng & Mã tự động ───

    [Fact]
    public async Task UC047_CreateRequest_ValidData_CreatesDraftWithDocNo()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 3, "Tuyển bổ sung dự án mới ERP", _orgUnitId, false);
        var res = await _recruitSvc.CreateAsync(_tenant, _user, req);

        Assert.NotNull(res.DocNo);
        Assert.StartsWith("TD-", res.DocNo);
        Assert.Equal("Draft", res.Status);
        Assert.Equal(3, res.Headcount);
        Assert.Equal("Phòng Nhân sự", res.OrgUnitName);
        Assert.Equal("Lập trình viên Senior", res.JobTitleName);
    }

    [Fact]
    public async Task UC047_CreateRequest_InvalidOrgUnitId_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển dụng phòng ban", Guid.NewGuid(), false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("Đơn vị", ex.Message);
    }

    [Fact]
    public async Task UC047_CreateRequest_ConsecutiveRequests_IncrementsDocNo()
    {
        var req1 = new RecruitmentRequestCreateRequest(_jobTitleId, 1, "Tuyển đợt 1 mở rộng", _orgUnitId, false);
        var req2 = new RecruitmentRequestCreateRequest(_jobTitleId, 1, "Tuyển đợt 2 mở rộng", _orgUnitId, false);

        var res1 = await _recruitSvc.CreateAsync(_tenant, _user, req1);
        var res2 = await _recruitSvc.CreateAsync(_tenant, _user, req2);

        Assert.NotEqual(res1.DocNo, res2.DocNo);
        Assert.EndsWith("0001", res1.DocNo);
        Assert.EndsWith("0002", res2.DocNo);
    }

    // ─── UC_HRM_048: Chọn vị trí & số lượng cần tuyển ───

    [Fact]
    public async Task UC048_CreateRequest_InvalidJobTitle_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(Guid.NewGuid(), 2, "Tuyển vị trí mới", _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("Vị trí", ex.Message);
    }

    [Fact]
    public async Task UC048_CreateRequest_ZeroHeadcount_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 0, "Tuyển 0 người", _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("1–999", ex.Message);
    }

    [Fact]
    public async Task UC048_CreateRequest_ExceedMaxHeadcount_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 1000, "Tuyển 1000 người", _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("1–999", ex.Message);
    }

    // ─── UC_HRM_049: Nhập lý do tuyển dụng ───

    [Fact]
    public async Task UC049_CreateRequest_EmptyReason_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "   ", _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("lý do", ex.Message);
    }

    [Fact]
    public async Task UC049_CreateRequest_ReasonTooShort_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "ABC", _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("tối thiểu 5 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC049_CreateRequest_ReasonTooLong_ThrowsAppException()
    {
        var longReason = new string('A', 1001);
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, longReason, _orgUnitId, false);
        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.CreateAsync(_tenant, _user, req));
        Assert.Contains("1000 ký tự", ex.Message);
    }

    // ─── UC_HRM_050: Gửi phiếu đề xuất đi duyệt ───

    [Fact]
    public async Task UC050_SubmitRequest_Draft_TransitionsToPendingAndCallsWf()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển nhân sự mảng AI", _orgUnitId, false);
        var created = await _recruitSvc.CreateAsync(_tenant, _user, req);

        var submitted = await _recruitSvc.SubmitAsync(_tenant, _user, created.Id);

        Assert.Equal("Pending", submitted.Status);
        Assert.NotNull(submitted.WfInstanceId);
        Assert.Equal(1, _wfFake.StartCallCount);
        Assert.Equal(created.Id, _wfFake.LastStartedDocId);
        Assert.Equal("RECRUIT_APPROVE", _wfFake.LastDefinitionCode);
    }

    [Fact]
    public async Task UC050_SubmitRequest_AlreadySubmitted_ThrowsAppException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển nhân sự mảng Data", _orgUnitId, false);
        var created = await _recruitSvc.CreateAsync(_tenant, _user, req);
        await _recruitSvc.SubmitAsync(_tenant, _user, created.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() => _recruitSvc.SubmitAsync(_tenant, _user, created.Id));
        Assert.Contains("trạng thái Draft", ex.Message);
    }

    [Fact]
    public async Task UC050_SubmitRequest_UnauthorizedUser_ThrowsForbiddenException()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 2, "Tuyển nhân sự DevOps", _orgUnitId, false);
        var created = await _recruitSvc.CreateAsync(_tenant, _user, req);

        var otherUser = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _recruitSvc.SubmitAsync(_tenant, otherUser, created.Id));
        Assert.Contains("Không có quyền", ex.Message);
    }

    [Fact]
    public async Task UC050_CreateRequest_WithSubmitTrue_AutomaticallySubmits()
    {
        var req = new RecruitmentRequestCreateRequest(_jobTitleId, 4, "Tuyển gấp cho dự án trọng điểm", _orgUnitId, true);
        var res = await _recruitSvc.CreateAsync(_tenant, _user, req);

        Assert.Equal("Pending", res.Status);
        Assert.NotNull(res.WfInstanceId);
        Assert.Equal(1, _wfFake.StartCallCount);
    }
}

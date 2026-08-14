using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvProjectTransferSerialTrackingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvProjectTransferSerialTrackingService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();

    public InvProjectTransferSerialTrackingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-prj-trf-serial-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvProjectTransferSerialTrackingService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateProjectDispatch_GeneratesDispatchNumberAndAllocatesValue()
    {
        var req = new InvCreateProjectDispatchRequest(_projectId, "Dự Án Xây Dựng Nhà Xưởng Bến Cát", Guid.NewGuid(), 85000000m, "Phase 2");
        var res = await _svc.CreateProjectDispatchAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("PRJ-OUT-", res.DispatchNumber);
        Assert.Equal(85000000m, res.TotalAllocatedValueVnd);
    }

    [Fact]
    public async Task CreateAndDecideTransferApproval_UpdatesApprovalStatus()
    {
        var createReq = new InvCreateTransferApprovalRequest("TRF-REQ-2026-001", Guid.NewGuid(), Guid.NewGuid());
        var created = await _svc.CreateTransferApprovalAsync(_tenant, createReq);

        Assert.NotNull(created);
        Assert.Equal("PendingApproval", created.ApprovalStatus);

        var decideReq = new InvDecideTransferApprovalRequest(created.Id, true, "Trần Giám Đốc", "Phê duyệt điều chuyển ngay trong ngày");
        var decided = await _svc.DecideTransferApprovalAsync(_tenant, decideReq);

        Assert.Equal("Approved", decided.ApprovalStatus);
        Assert.Equal("Trần Giám Đốc", decided.ApproverName);
    }

    [Fact]
    public async Task ExecuteOneStepTransfer_GeneratesDirectTransferNumber()
    {
        var req = new InvExecuteOneStepTransferRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50, "Cân bằng tồn kho giữa 2 trạm");
        var res = await _svc.ExecuteOneStepTransferAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("TRF-DIRECT-", res.TransferNumber);
        Assert.Equal(50, res.Quantity);
    }

    [Fact]
    public async Task RecordAndGetSerialHistory_LogsSerialLifecycleEvents()
    {
        var recordReq = new InvRecordSerialEventRequest(Guid.NewGuid(), "SKU-ROUTER-CISCO", "SN-CISCO-8899", "GoodsReceipt", "Kho Tân Bình", "GRN-9988");
        var recorded = await _svc.RecordSerialEventAsync(_tenant, recordReq);

        Assert.NotNull(recorded);
        Assert.Equal("SN-CISCO-8899", recorded.SerialNumber);

        var history = await _svc.GetSerialHistoryAsync(_tenant, "SN-CISCO-8899");
        Assert.NotEmpty(history);
        Assert.Equal("SN-CISCO-8899", history[0].SerialNumber);
    }
}

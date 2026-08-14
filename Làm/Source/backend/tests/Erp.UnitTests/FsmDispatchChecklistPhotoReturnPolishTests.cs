using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmDispatchChecklistPhotoReturnPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmDispatchChecklistPhotoReturnService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmDispatchChecklistPhotoReturnPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-dispatch-photo-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmDispatchChecklistPhotoReturnService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateAutoDispatchRule_SavesRuleCriteria()
    {
        var req = new FsmCreateAutoDispatchRuleRequest("Phân Công Robot Miền Trung", "REGION-CENTRAL", "SKILL-ROBOTIC", 4, true);
        var res = await _svc.CreateAutoDispatchRuleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("REGION-CENTRAL", res.TerritoryCode);
        Assert.True(res.AutoAssignOnTicketCreation);
    }

    [Fact]
    public async Task AddChecklistStep_SavesStepForTicket()
    {
        var req = new FsmAddChecklistStepRequest(Guid.NewGuid(), "TCK-990", "Kiểm tra mức dầu bôi trơn bánh răng", true);
        var res = await _svc.AddChecklistStepAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Kiểm tra mức dầu bôi trơn bánh răng", res.StepDescription);
        Assert.False(res.IsCompleted);
    }

    [Fact]
    public async Task UploadJobPhoto_SavesBeforeAfterAttachment()
    {
        var req = new FsmUploadJobPhotoRequest(Guid.NewGuid(), "TCK-990", "Before", "/photos/before.jpg", "Ảnh trước khi bảo trì");
        var res = await _svc.UploadJobPhotoAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Before", res.PhotoType);
        Assert.Equal("/photos/before.jpg", res.PhotoUrl);
    }

    [Fact]
    public async Task CreateSparePartReturn_GeneratesReturnSlipNumber()
    {
        var req = new FsmCreateSparePartReturnRequest(Guid.NewGuid(), "TCK-990", "PART-FUSE-10A", "Cầu Chì 10A", 2, "Thừa linh kiện", "KHO-FSM-01");
        var res = await _svc.CreateSparePartReturnAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("RET-PART-", res.ReturnSlipNumber);
        Assert.Equal(2, res.ReturnedQuantity);
    }
}

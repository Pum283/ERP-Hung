using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvMaterialRequisitionApprovalSlowMovingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMaterialRequisitionApprovalSlowMovingService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public InvMaterialRequisitionApprovalSlowMovingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-req-slowmoving-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvMaterialRequisitionApprovalSlowMovingService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateRequisition_GeneratesRequisitionNumber()
    {
        var req = new InvCreateMaterialRequisitionRequest("Lê Văn Kỹ Sư", "Xưởng Cơ Khí", _warehouseId, _productId, 100);
        var res = await _svc.CreateRequisitionAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("REQ-MAT-", res.RequisitionNumber);
        Assert.Equal("Submitted", res.Status);
    }

    [Fact]
    public async Task DecideAndConvertRequisition_ConvertsApprovedRequisitionToIssue()
    {
        var createReq = new InvCreateMaterialRequisitionRequest("Lê Văn Kỹ Sư", "Xưởng Cơ Khí", _warehouseId, _productId, 50);
        var created = await _svc.CreateRequisitionAsync(_tenant, createReq);

        var decideReq = new InvDecideMaterialRequisitionRequest(created.Id, true, "Trưởng Kho Minh");
        var approved = await _svc.DecideRequisitionAsync(_tenant, decideReq);

        Assert.Equal("Approved", approved.Status);

        var convertReq = new InvConvertRequisitionToIssueRequest(created.Id);
        var converted = await _svc.ConvertToStockIssueAsync(_tenant, convertReq);

        Assert.Equal("ConvertedToIssue", converted.Status);
        Assert.StartsWith("ISSUE-MAT-", converted.ConvertedIssueNumber);
    }

    [Fact]
    public async Task GetSlowMovingAnalysis_CalculatesCapitalTiedUp()
    {
        var res = await _svc.GetSlowMovingAnalysisAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalSlowMovingSkus > 0);
        Assert.True(res.TotalTiedUpCapitalVnd > 0);
        Assert.NotEmpty(res.Items);
    }
}

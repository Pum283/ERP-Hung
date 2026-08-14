using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmRepairFeedbackReopenFinancePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmRepairFeedbackReopenFinanceService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmRepairFeedbackReopenFinancePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-repair-finance-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmRepairFeedbackReopenFinanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task RecordRepairCost_CalculatesTotalBillable_WhenOutOfWarranty()
    {
        var req = new FsmRecordRepairCostRequest(Guid.NewGuid(), "TCK-550", 400000, 300000, 100000, false);
        var res = await _svc.RecordRepairCostAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(800000, res.TotalBillableAmountVnd);
        Assert.False(res.IsCoveredByWarranty);
    }

    [Fact]
    public async Task RecordRepairCost_SetsZeroBillable_WhenCoveredByWarranty()
    {
        var req = new FsmRecordRepairCostRequest(Guid.NewGuid(), "TCK-551", 400000, 300000, 100000, true);
        var res = await _svc.RecordRepairCostAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(0, res.TotalBillableAmountVnd);
        Assert.True(res.IsCoveredByWarranty);
    }

    [Fact]
    public async Task SubmitFeedback_SavesCsatRating()
    {
        var req = new FsmSubmitFeedbackRequest(Guid.NewGuid(), "TCK-550", 5, "Dịch vụ xuất sắc", "Anh Hoàng FPT");
        var res = await _svc.SubmitFeedbackAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(5, res.StarRating);
        Assert.Equal("Anh Hoàng FPT", res.CustomerSignerName);
    }

    [Fact]
    public async Task TransferCostToFinance_GeneratesPostedJournalVoucher()
    {
        var req = new FsmTransferCostToFinanceRequest(Guid.NewGuid(), "TCK-550", 800000, "627", "154");
        var res = await _svc.TransferCostToFinanceAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("FIN-FSM-", res.TransferVoucherNumber);
        Assert.Equal("Posted", res.JournalEntryStatus);
    }
}

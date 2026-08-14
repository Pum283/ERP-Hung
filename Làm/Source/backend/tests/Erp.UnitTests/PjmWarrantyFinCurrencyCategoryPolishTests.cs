using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PjmWarrantyFinCurrencyCategoryPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PjmWarrantyProductivityService _pjmSvc;
    private readonly FinCurrencyCashFlowCategoryService _finSvc;
    private readonly Guid _tenant = Guid.NewGuid();

    public PjmWarrantyFinCurrencyCategoryPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pjm-fin-step213-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _pjmSvc = new PjmWarrantyProductivityService(_db);
        _finSvc = new FinCurrencyCashFlowCategoryService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateWarrantyCoverage_SavesPeriodMonths()
    {
        var req = new PjmCreateWarrantyCoverageRequest(Guid.NewGuid(), "PRJ-088", "Khách Hàng Viettel", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(24), 24, "1900-8888");
        var res = await _pjmSvc.CreateWarrantyCoverageAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(24, res.WarrantyPeriodMonths);
        Assert.Equal("1900-8888", res.SupportHotline);
    }

    [Fact]
    public async Task GetResourceProductivityReport_ReturnsUtilizationRate()
    {
        var res = await _pjmSvc.GetResourceProductivityReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalEngineersCount > 0);
        Assert.True(res.ResourceUtilizationRatePct > 80);
    }

    [Fact]
    public async Task CreateExchangeRate_SavesVndRate()
    {
        var req = new FinCreateExchangeRateRequest("SGD", "Đô La Singapore", 19200m, "Vietcombank", false, DateTimeOffset.UtcNow);
        var res = await _finSvc.CreateExchangeRateAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SGD", res.CurrencyCode);
        Assert.Equal(19200m, res.ExchangeRateToVnd);
    }

    [Fact]
    public async Task CreateCashFlowCategory_SavesInflowCategory()
    {
        var req = new FinCreateCashFlowCategoryRequest("CASH-IN-SVC", "Thu phí dịch vụ kỹ thuật FSM", "Inflow", "Operating", true);
        var res = await _finSvc.CreateCashFlowCategoryAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("CASH-IN-SVC", res.CategoryCode);
        Assert.Equal("Inflow", res.CashFlowType);
    }
}

using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FinStatementOffsetDunningBadDebtPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FinStatementOffsetDunningBadDebtService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FinStatementOffsetDunningBadDebtPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fin-stmt-offset-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FinStatementOffsetDunningBadDebtService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task ImportBankStatement_SavesTransactionTotals()
    {
        var req = new FinImportBankStatementRequest("190388889999", "Techcombank", "statement.xlsx", 50, 500000000m, 300000000m);
        var res = await _svc.ImportBankStatementAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(50, res.TotalTransactionsCount);
        Assert.Equal("Success", res.ImportStatus);
    }

    [Fact]
    public async Task CreateArApOffset_GeneratesSettlementNumber()
    {
        var req = new FinCreateArApOffsetRequest("Đối tác ABC", 50000000m, 50000000m, 0m, "PKT-BT-01");
        var res = await _svc.CreateArApOffsetAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("BT-", res.SettlementNumber);
        Assert.Equal("Approved", res.Status);
    }

    [Fact]
    public async Task SendDunningNotification_SavesDunningDetails()
    {
        var req = new FinSendDunningNotificationRequest("INV-001", "Khách hàng XYZ", 20000000m, 10, "Level1_Reminder", "Email", "test@domain.vn");
        var res = await _svc.SendDunningNotificationAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Level1_Reminder", res.DunningLevel);
        Assert.Equal(10, res.OverdueDays);
    }

    [Fact]
    public async Task ProcessBadDebt_SavesProvisionRateAndDoc()
    {
        var req = new FinProcessBadDebtRequest("Cơ Khí Hoàng Gia", 30000000m, 30000000m, 100.0, "WriteOff", "Nghị quyết HĐQT");
        var res = await _svc.ProcessBadDebtAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("NX-", res.DebtRecordNumber);
        Assert.Equal(100.0, res.ProvisionRatePct);
        Assert.Equal("WriteOff", res.ActionType);
    }
}

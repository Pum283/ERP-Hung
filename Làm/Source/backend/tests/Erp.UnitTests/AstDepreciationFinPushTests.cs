using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Ast;
using Erp.Domain.Entities.Ast;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Implementations.Services.Ast;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_AST_012 — đẩy bút toán khấu hao AST → FIN JE thật (Posted, cân Nợ/Có).</summary>
public sealed class AstDepreciationFinPushTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AstAssetService _svc;
    private readonly Guid _tenant = Guid.Parse("dddddddd-eeee-ffff-0000-111111111111");
    private readonly Guid _user = Guid.Parse("44444444-5555-6666-7777-888888888888");

    public AstDepreciationFinPushTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ast-fin-push-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new AstAssetService(_db);
    }

    public void Dispose() => _db.Dispose();

    private (AstDepreciationRun Run, FinAccount Expense, FinAccount Accum) Seed(
        decimal total = 1_000_000m, int lines = 2, bool withPeriod = true,
        string expenseCode = "6424", string accumCode = "2141")
    {
        var run = new AstDepreciationRun
        {
            TenantId = _tenant, Code = "KH-2026-07", Year = 2026, Month = 7,
            PeriodStart = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            PeriodEnd = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero),
            Status = "Posted", TotalAmount = total, LineCount = lines,
            CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.AstDepreciationRuns.Add(run);

        var expense = new FinAccount { TenantId = _tenant, Code = expenseCode, Name = "CP KH TSCĐ", AccountType = "Expense", CreatedBy = _user };
        var accum = new FinAccount { TenantId = _tenant, Code = accumCode, Name = "Hao mòn TSCĐ", AccountType = "Asset", CreatedBy = _user };
        _db.FinAccounts.AddRange(expense, accum);

        if (withPeriod)
        {
            _db.FinPeriods.Add(new FinPeriod
            {
                TenantId = _tenant, FiscalYearId = Guid.NewGuid(), Code = "2026-07", Name = "Tháng 7/2026",
                StartDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero),
                Status = "Open", CreatedBy = _user,
            });
        }
        _db.SaveChanges();
        return (run, expense, accum);
    }

    [Fact]
    public async Task Push_CreatesPostedBalancedJournal()
    {
        var (run, expense, accum) = Seed();

        var dto = await _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null));

        Assert.Equal("Pushed", dto.Status);
        Assert.NotNull(dto.FinJournalId);
        var je = await _db.FinJournals.SingleAsync(x => x.Id == dto.FinJournalId);
        Assert.Equal("JE-AST-KH-2026-07", je.Code);
        Assert.Equal("Posted", je.Status);
        Assert.Equal("Auto", je.Source);
        var jl = await _db.FinJournalLines.Where(x => x.JournalId == je.Id).ToListAsync();
        Assert.Equal(2, jl.Count);
        Assert.Equal(jl.Sum(x => x.Debit), jl.Sum(x => x.Credit));
        Assert.Equal(1_000_000m, jl.Single(x => x.AccountId == expense.Id).Debit);
        Assert.Equal(1_000_000m, jl.Single(x => x.AccountId == accum.Id).Credit);
    }

    [Fact]
    public async Task Push_AutoResolvesAccountsByCoaPrefix()
    {
        var (run, expense, accum) = Seed();

        var dto = await _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(null, null, null));

        var jl = await _db.FinJournalLines.Where(x => x.JournalId == dto.FinJournalId).ToListAsync();
        Assert.Equal(expense.Id, jl.Single(x => x.Debit > 0).AccountId);
        Assert.Equal(accum.Id, jl.Single(x => x.Credit > 0).AccountId);
    }

    [Fact]
    public async Task Push_ThrowsWhenNoMatchingCoaAccount()
    {
        var (run, _, _) = Seed(expenseCode: "9991", accumCode: "9992");

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(null, null, null)));
        Assert.Contains("TK chi phí KH", ex.Message);
    }

    [Fact]
    public async Task Push_ThrowsWhenNoOpenPeriodMatchesMonth()
    {
        var (run, expense, accum) = Seed(withPeriod: false);

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null)));
        Assert.Contains("kỳ FIN", ex.Message);
    }

    [Fact]
    public async Task Push_RejectsEmptyRun()
    {
        var (run, expense, accum) = Seed(total: 0m, lines: 0);

        await Assert.ThrowsAsync<AppException>(
            () => _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null)));
    }

    [Fact]
    public async Task Push_RejectsSecondPushOfSameRun()
    {
        var (run, expense, accum) = Seed();
        await _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null));

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null)));
        Assert.Contains("Đã đẩy FIN", ex.Message);
        Assert.Equal(1, await _db.FinJournals.CountAsync());
    }

    [Fact]
    public async Task Push_LegacyStubPushedRunWithoutJe_CanPushAgain()
    {
        var (run, expense, accum) = Seed();
        run.Status = "Pushed"; // trạng thái stub cũ: Pushed nhưng chưa có JE
        await _db.SaveChangesAsync();

        var dto = await _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, null));

        Assert.NotNull(dto.FinJournalId);
    }

    [Fact]
    public async Task Push_RejectsLockedPeriod()
    {
        var (run, expense, accum) = Seed(withPeriod: false);
        var locked = new FinPeriod
        {
            TenantId = _tenant, FiscalYearId = Guid.NewGuid(), Code = "2026-07L", Name = "Tháng 7/2026",
            StartDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero),
            Status = "Locked", CreatedBy = _user,
        };
        _db.FinPeriods.Add(locked);
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _svc.PushToFinAsync(_tenant, _user, run.Id, new AstPushFinRequest(expense.Id, accum.Id, locked.Id)));
        Assert.Contains("khóa", ex.Message);
    }
}

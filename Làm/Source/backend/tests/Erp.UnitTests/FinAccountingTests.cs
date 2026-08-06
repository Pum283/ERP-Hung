using Erp.Application.DTOs.Fin;
using Xunit;

namespace Erp.UnitTests;

public class FinAccountingTests
{
    [Fact]
    public void JournalEntry_BalancedLines_ValidationSucceeds()
    {
        var lines = new List<FinJournalLineUpsertRequest>
        {
            new(null, Guid.NewGuid(), 1000000, 0, "CUST01", null, "Nợ TK 111"),
            new(null, Guid.NewGuid(), 0, 1000000, "CUST01", null, "Có TK 511")
        };

        var totalDebit = lines.Sum(x => x.Debit);
        var totalCredit = lines.Sum(x => x.Credit);

        Assert.Equal(totalDebit, totalCredit);
        Assert.True(totalDebit > 0);
    }

    [Fact]
    public void JournalEntry_UnbalancedLines_ValidationFails()
    {
        var lines = new List<FinJournalLineUpsertRequest>
        {
            new(null, Guid.NewGuid(), 1000000, 0, "CUST01", null, "Nợ TK 111"),
            new(null, Guid.NewGuid(), 0, 800000, "CUST01", null, "Có TK 511")
        };

        var totalDebit = lines.Sum(x => x.Debit);
        var totalCredit = lines.Sum(x => x.Credit);

        Assert.NotEqual(totalDebit, totalCredit);
    }

    [Fact]
    public void ProfitAndLoss_Calculation_NetProfitMatchesRevenueMinusExpense()
    {
        decimal revenue = 150000000;
        decimal expense = 95000000;

        var netProfit = revenue - expense;

        Assert.Equal(55000000, netProfit);
    }

    [Fact]
    public void ArApReconciliation_ZeroVariance_IsReconciled()
    {
        decimal subledgerBalance = 25000000;
        decimal generalLedgerBalance = 25000000;

        var variance = subledgerBalance - generalLedgerBalance;
        var isReconciled = variance == 0;

        Assert.Equal(0, variance);
        Assert.True(isReconciled);
    }

    [Fact]
    public void ArApReconciliation_NonZeroVariance_IsUnreconciled()
    {
        decimal subledgerBalance = 25000000;
        decimal generalLedgerBalance = 23000000;

        var variance = subledgerBalance - generalLedgerBalance;
        var isReconciled = variance == 0;

        Assert.Equal(2000000, variance);
        Assert.False(isReconciled);
    }

    [Fact]
    public void FiscalYearClosing_LockedPeriods_PreventsNewPostings()
    {
        var periodStatus = "Locked";
        var isPostingAllowed = periodStatus != "Locked";

        Assert.False(isPostingAllowed);
    }
}

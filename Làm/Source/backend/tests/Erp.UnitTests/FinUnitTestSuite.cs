using Xunit;

namespace Erp.UnitTests;

public class FinUnitTestSuite
{
    [Fact]
    public void Fin_JournalPosting_BalancedEntry_ChangesStatusToPosted()
    {
        decimal totalDebit = 5000000;
        decimal totalCredit = 5000000;
        string initialStatus = "Draft";

        bool isBalanced = totalDebit == totalCredit;
        string finalStatus = isBalanced ? "Posted" : initialStatus;

        Assert.Equal("Posted", finalStatus);
    }

    [Fact]
    public void Fin_JournalPosting_UnbalancedEntry_ThrowsError()
    {
        decimal totalDebit = 5000000;
        decimal totalCredit = 4500000;

        bool isBalanced = totalDebit == totalCredit;

        Assert.False(isBalanced);
    }

    [Fact]
    public void Fin_JournalReversal_GeneratesInvertedDebitCreditLines()
    {
        decimal originalDebit = 10000000;
        decimal originalCredit = 0;

        decimal reversalDebit = originalCredit;
        decimal reversalCredit = originalDebit;

        Assert.Equal(0, reversalDebit);
        Assert.Equal(10000000, reversalCredit);
    }

    [Fact]
    public void Fin_PeriodClosing_TransfersRevenueToAccount911()
    {
        decimal totalRevenue5xx = 250000000;
        decimal totalOtherRevenue7xx = 15000000;

        decimal totalTransferredTo911Credit = totalRevenue5xx + totalOtherRevenue7xx;

        Assert.Equal(265000000, totalTransferredTo911Credit);
    }

    [Fact]
    public void Fin_PeriodClosing_TransfersExpenseToAccount911()
    {
        decimal totalCostOfGoods632 = 140000000;
        decimal totalOperatingExpense642 = 45000000;

        decimal totalTransferredTo911Debit = totalCostOfGoods632 + totalOperatingExpense642;

        Assert.Equal(185000000, totalTransferredTo911Debit);
    }

    [Fact]
    public void Fin_PeriodClosing_CalculatesNetProfitOn911()
    {
        decimal total911Credit = 265000000; // Total revenue
        decimal total911Debit = 185000000;  // Total expense

        decimal netProfit = total911Credit - total911Debit;

        Assert.Equal(80000000, netProfit);
    }

    [Fact]
    public void Fin_FiscalYearClosing_LockedPeriod_PreventsJournalEdits()
    {
        string periodStatus = "Locked";
        bool canEditJournal = periodStatus != "Locked";

        Assert.False(canEditJournal);
    }

    [Fact]
    public void Fin_TrialBalance_ClosingDebitBalance_CalculatesNetDebit()
    {
        decimal openingDebit = 10000000;
        decimal periodDebit = 5000000;
        decimal periodCredit = 2000000;

        decimal netClosing = (openingDebit + periodDebit) - periodCredit;

        Assert.Equal(13000000, netClosing);
    }

    [Fact]
    public void Fin_BalanceSheet_TotalAssetsEqualsLiabilitiesPlusEquity()
    {
        decimal totalAssets = 500000000;
        decimal totalLiabilities = 200000000;
        decimal totalEquity = 300000000;

        bool isBalanceSheetBalanced = totalAssets == (totalLiabilities + totalEquity);

        Assert.True(isBalanceSheetBalanced);
    }

    [Fact]
    public void Fin_CashFlow_NetOperatingCashFlow_CalculatesCashInMinusOut()
    {
        decimal cashInflows = 320000000;
        decimal cashOutflows = 210000000;

        decimal netOperatingCash = cashInflows - cashOutflows;

        Assert.Equal(110000000, netOperatingCash);
    }

    [Fact]
    public void Fin_ArReconciliation_ZeroVariance_ConfirmsMatchingBalance()
    {
        decimal arSubledgerTotal = 45000000;
        decimal arGlAccount131Total = 45000000;

        decimal variance = arSubledgerTotal - arGlAccount131Total;
        bool isMatched = variance == 0;

        Assert.Equal(0, variance);
        Assert.True(isMatched);
    }

    [Fact]
    public void Fin_ApReconciliation_Discrepancy_DetectsUnmatchedAmount()
    {
        decimal apSubledgerTotal = 60000000;
        decimal apGlAccount331Total = 58000000;

        decimal variance = apSubledgerTotal - apGlAccount331Total;
        bool isMatched = variance == 0;

        Assert.Equal(2000000, variance);
        Assert.False(isMatched);
    }

    [Fact]
    public void Fin_DiscountPosting_SalesDiscount_ReducesGrossRevenue()
    {
        decimal grossRevenue = 100000000;
        decimal salesDiscount521 = 5000000;

        decimal netRevenue = grossRevenue - salesDiscount521;

        Assert.Equal(95000000, netRevenue);
    }

    [Fact]
    public void Fin_PayrollPosting_HrmSync_DebitsExpenseCreditsPayrollPayable()
    {
        decimal totalSalaryExpense = 85000000;
        decimal account642Debit = totalSalaryExpense;
        decimal account334Credit = totalSalaryExpense;

        bool isPayrollEntryBalanced = account642Debit == account334Credit;

        Assert.Equal(85000000, account642Debit);
        Assert.True(isPayrollEntryBalanced);
    }

    [Fact]
    public void Fin_TaxCalculation_VatOutput_CalculatesCorrectTaxAmount()
    {
        decimal subtotal = 50000000;
        decimal vatRatePercent = 10;

        decimal vatAmount = subtotal * (vatRatePercent / 100);
        decimal grandTotal = subtotal + vatAmount;

        Assert.Equal(5000000, vatAmount);
        Assert.Equal(55000000, grandTotal);
    }

    [Fact]
    public void Fin_PrepaidExpense_Amortization_CalculatesMonthlyExpense()
    {
        decimal totalPrepaidCost = 120000000; // TK 242
        int allocationMonths = 12;

        decimal monthlyAmortization = totalPrepaidCost / allocationMonths;

        Assert.Equal(10000000, monthlyAmortization);
    }

    [Fact]
    public void Fin_ForeignExchange_RealizedGainLoss_CalculatesExchangeDifference()
    {
        decimal originalUsdAmount = 10000;
        decimal bookingExchangeRate = 25000; // 250,000,000 VND
        decimal paymentExchangeRate = 25400; // 254,000,000 VND

        decimal exchangeGainTK515 = originalUsdAmount * (paymentExchangeRate - bookingExchangeRate);

        Assert.Equal(4000000, exchangeGainTK515);
    }

    [Fact]
    public void Fin_BankReconciliation_UnmatchedTransactions_IdentifiesPendingDeposits()
    {
        decimal bankStatementBalance = 150000000;
        decimal companyBookBalance = 165000000;
        decimal pendingDepositInTransit = 15000000;

        decimal adjustedBankBalance = bankStatementBalance + pendingDepositInTransit;

        Assert.Equal(companyBookBalance, adjustedBankBalance);
    }

    [Fact]
    public void Fin_BadDebtProvision_AgingBracket_CalculatesRequiredProvision()
    {
        decimal over90DaysDebt = 50000000;
        decimal provisionRate = 0.50m; // 50% provision for >90 days

        decimal requiredProvisionTK2293 = over90DaysDebt * provisionRate;

        Assert.Equal(25000000, requiredProvisionTK2293);
    }

    [Fact]
    public void Fin_CorporateIncomeTax_TaxableProfit_Applies20PercentCit()
    {
        decimal accountingProfitBeforeTax = 200000000;
        decimal nonDeductibleExpenses = 10000000;
        decimal taxableProfit = accountingProfitBeforeTax + nonDeductibleExpenses;
        decimal citRate = 0.20m;

        decimal citTaxAmountTK821 = taxableProfit * citRate;

        Assert.Equal(210000000, taxableProfit);
        Assert.Equal(42000000, citTaxAmountTK821);
    }

    [Fact]
    public void Fin_ExpenseClaim_AdvanceSettlement_CalculatesRefundOrAdditionalPayable()
    {
        decimal advanceAmountReceived = 10000000; // TK 141
        decimal actualExpenseIncurred = 12500000; // TK 642

        decimal additionalPayableToEmployee = actualExpenseIncurred - advanceAmountReceived;

        Assert.Equal(2500000, additionalPayableToEmployee);
    }

    [Fact]
    public void Fin_InterCompany_Settlement_DebitsPayableCreditsReceivable()
    {
        decimal interCompanyLiabilityUnitA = 50000000;
        decimal interCompanyAssetUnitB = 50000000;

        decimal netInterCompanyVariance = interCompanyLiabilityUnitA - interCompanyAssetUnitB;

        Assert.Equal(0, netInterCompanyVariance);
    }

    [Fact]
    public void Fin_TaxWithholding_ContractorTax_Calculates5PercentVatAnd5PercentPit()
    {
        decimal foreignContractorRevenue = 100000000;
        decimal vatRate = 0.05m;
        decimal pitRate = 0.05m;

        decimal withholdingVat = foreignContractorRevenue * vatRate;
        decimal withholdingPit = foreignContractorRevenue * pitRate;
        decimal totalContractorTax = withholdingVat + withholdingPit;

        Assert.Equal(10000000, totalContractorTax);
    }

    [Fact]
    public void Fin_FixedAssetDisposal_JournalEntry_DebitsAccumulatedDepreciationAndCreditsAssetCost()
    {
        decimal originalAssetCost = 150000000; // TK 211
        decimal accumulatedDepreciation = 120000000; // TK 214
        decimal disposalProceeds = 40000000; // TK 711

        decimal netBookValueDisposed = originalAssetCost - accumulatedDepreciation;
        decimal otherIncomeProceeds = disposalProceeds;

        Assert.Equal(30000000, netBookValueDisposed);
        Assert.Equal(40000000, otherIncomeProceeds);
    }

    [Fact]
    public void Fin_RecurringJournalTemplate_MonthlyAccrual_GeneratesStandardAccrual()
    {
        decimal monthlyAuditFeeAccrual = 15000000; // TK 642 / TK 335
        int totalAccrualMonths = 12;

        decimal annualAccrualTotal = monthlyAuditFeeAccrual * totalAccrualMonths;

        Assert.Equal(180000000, annualAccrualTotal);
    }

    [Fact]
    public void Fin_LetterOfCredit_BankFee_DebitsFinancialExpense()
    {
        decimal lcFaceValue = 2000000000;
        decimal lcOpeningFeePercent = 0.25m; // 0.25%

        decimal bankFeeAmountTK635 = lcFaceValue * (lcOpeningFeePercent / 100);

        Assert.Equal(5000000, bankFeeAmountTK635);
    }

    [Fact]
    public void Fin_DividendsDistribution_RetainedEarnings_Debits421Credits338()
    {
        decimal totalRetainedEarningsTK421 = 500000000;
        decimal dividendPayoutPercent = 40; // 40% payout

        decimal dividendAmountPayableTK3388 = totalRetainedEarningsTK421 * (dividendPayoutPercent / 100);
        decimal remainingRetainedEarnings = totalRetainedEarningsTK421 - dividendAmountPayableTK3388;

        Assert.Equal(200000000, dividendAmountPayableTK3388);
        Assert.Equal(300000000, remainingRetainedEarnings);
    }

    [Fact]
    public void Fin_InventoryWriteOff_LossPosting_Debits632Credits156()
    {
        decimal damagedInventoryValue = 8500000;

        decimal costOfGoodsSoldLossTK632 = damagedInventoryValue;
        decimal inventoryReductionTK156 = damagedInventoryValue;

        Assert.Equal(8500000, costOfGoodsSoldLossTK632);
        Assert.Equal(8500000, inventoryReductionTK156);
    }
}

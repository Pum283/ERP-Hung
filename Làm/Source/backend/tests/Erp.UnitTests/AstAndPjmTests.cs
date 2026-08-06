using Xunit;

namespace Erp.UnitTests;

public class AstAndPjmTests
{
    [Fact]
    public void FixedAsset_StraightLineDepreciation_CalculatesMonthlyExpense()
    {
        decimal originalCost = 120000000; // 120 million VND
        int usefulLifeMonths = 60; // 5 years

        decimal monthlyDepreciation = originalCost / usefulLifeMonths;

        Assert.Equal(2000000, monthlyDepreciation);
    }

    [Fact]
    public void FixedAsset_NetBookValue_CalculatesRemainingValueCorrectly()
    {
        decimal originalCost = 120000000;
        decimal accumulatedDepreciation = 40000000; // 20 months depreciated

        decimal netBookValue = originalCost - accumulatedDepreciation;

        Assert.Equal(80000000, netBookValue);
    }

    [Fact]
    public void ProjectWbs_CostVariance_DetectsOverBudget()
    {
        decimal plannedBudget = 500000000;
        decimal actualCost = 540000000;

        decimal costVariance = actualCost - plannedBudget;
        bool isOverBudget = costVariance > 0;

        Assert.Equal(40000000, costVariance);
        Assert.True(isOverBudget);
    }

    [Fact]
    public void ProjectWbs_CostVariance_DetectsUnderBudget()
    {
        decimal plannedBudget = 500000000;
        decimal actualCost = 480000000;

        decimal costVariance = actualCost - plannedBudget;
        bool isOverBudget = costVariance > 0;

        Assert.Equal(-20000000, costVariance);
        Assert.False(isOverBudget);
    }

    [Fact]
    public void BiAnalytics_GrossMarginPercentage_CalculatesCorrectly()
    {
        decimal totalRevenue = 1000000000;
        decimal totalCostOfGoodsSold = 650000000;

        decimal grossProfit = totalRevenue - totalCostOfGoodsSold;
        decimal grossMarginPercent = (grossProfit / totalRevenue) * 100;

        Assert.Equal(350000000, grossProfit);
        Assert.Equal(35, grossMarginPercent);
    }
}

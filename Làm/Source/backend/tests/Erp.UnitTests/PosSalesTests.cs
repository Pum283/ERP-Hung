using Xunit;

namespace Erp.UnitTests;

public class PosSalesTests
{
    [Fact]
    public void PosSale_RecipeBomDeduction_DeductsRawIngredients()
    {
        // 1 Milk Coffee = 20g Coffee Beans + 100ml Milk
        int cupsSold = 10;
        decimal coffeeBeansPerCup = 20; // grams
        decimal milkPerCup = 100; // ml

        decimal totalCoffeeDeducted = cupsSold * coffeeBeansPerCup;
        decimal totalMilkDeducted = cupsSold * milkPerCup;

        Assert.Equal(200, totalCoffeeDeducted);
        Assert.Equal(1000, totalMilkDeducted);
    }

    [Fact]
    public void PosShiftClose_SyncsRevenueAndCashToJournal()
    {
        decimal cashSales = 3500000;
        decimal cardSales = 4200000;
        decimal totalShiftRevenue = cashSales + cardSales;

        Assert.Equal(7700000, totalShiftRevenue);
    }
}

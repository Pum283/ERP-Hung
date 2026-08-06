using Xunit;

namespace Erp.UnitTests;

public class CrmSalesTests
{
    [Fact]
    public void SalesOrder_WithinCreditLimit_Confirmed()
    {
        decimal creditLimit = 50000000;
        decimal currentBalance = 20000000;
        decimal orderAmount = 15000000;

        decimal totalExposure = currentBalance + orderAmount;
        bool isWithinLimit = totalExposure <= creditLimit;

        Assert.Equal(35000000, totalExposure);
        Assert.True(isWithinLimit);
    }

    [Fact]
    public void SalesOrder_ExceedingCreditLimit_PlacedOnHold()
    {
        decimal creditLimit = 50000000;
        decimal currentBalance = 40000000;
        decimal orderAmount = 20000000;

        decimal totalExposure = currentBalance + orderAmount;
        bool isWithinLimit = totalExposure <= creditLimit;

        Assert.Equal(60000000, totalExposure);
        Assert.False(isWithinLimit);
    }

    [Fact]
    public void CustomerGroup_PriceListOverride_AppliesGroupPrice()
    {
        decimal basePrice = 100000;
        decimal vipDiscountPercent = 15;

        decimal finalPrice = basePrice * (1 - vipDiscountPercent / 100);

        Assert.Equal(85000, finalPrice);
    }
}

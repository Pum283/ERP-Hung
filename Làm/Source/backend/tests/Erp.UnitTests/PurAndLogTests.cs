using Xunit;

namespace Erp.UnitTests;

public class PurAndLogTests
{
    [Fact]
    public void PurchaseOrder_TotalCalculation_IncludesTaxAndShipping()
    {
        decimal itemsSubTotal = 50000000;
        decimal taxRatePercent = 10;
        decimal shippingFee = 2000000;

        decimal taxAmount = itemsSubTotal * (taxRatePercent / 100);
        decimal grandTotal = itemsSubTotal + taxAmount + shippingFee;

        Assert.Equal(5000000, taxAmount);
        Assert.Equal(57000000, grandTotal);
    }

    [Fact]
    public void SupplierRating_OnTimeDeliveryRate_CalculatesPercentage()
    {
        int totalOrders = 20;
        int onTimeOrders = 18;

        decimal onTimeRatePercent = ((decimal)onTimeOrders / totalOrders) * 100;
        bool isPreferredVendor = onTimeRatePercent >= 90;

        Assert.Equal(90, onTimeRatePercent);
        Assert.True(isPreferredVendor);
    }

    [Fact]
    public void DeliveryOrder_CodReconciliation_MatchesCollectedCash()
    {
        decimal expectedCodAmount = 1500000;
        decimal collectedCodAmount = 1500000;

        decimal variance = expectedCodAmount - collectedCodAmount;
        bool isReconciled = variance == 0;

        Assert.Equal(0, variance);
        Assert.True(isReconciled);
    }

    [Fact]
    public void DeliveryOrder_CodReconciliation_DetectsShortfall()
    {
        decimal expectedCodAmount = 1500000;
        decimal collectedCodAmount = 1400000;

        decimal variance = expectedCodAmount - collectedCodAmount;
        bool isReconciled = variance == 0;

        Assert.Equal(100000, variance);
        Assert.False(isReconciled);
    }

    [Fact]
    public void CarrierSelection_LowestRate_SelectsOptimalCarrier()
    {
        var rates = new Dictionary<string, decimal>
        {
            { "CarrierA", 50000 },
            { "CarrierB", 35000 },
            { "CarrierC", 42000 }
        };

        var lowestCarrier = rates.OrderBy(x => x.Value).First();

        Assert.Equal("CarrierB", lowestCarrier.Key);
        Assert.Equal(35000, lowestCarrier.Value);
    }
}

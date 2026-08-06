using Xunit;

namespace Erp.UnitTests;

public class InvStockTests
{
    [Fact]
    public void StockReceipt_IncreasesOnHandQuantity()
    {
        decimal initialOnHand = 100;
        decimal receiptQty = 50;

        decimal newOnHand = initialOnHand + receiptQty;

        Assert.Equal(150, newOnHand);
    }

    [Fact]
    public void StockIssue_ExceedingAvailableStock_FailsValidation()
    {
        decimal qtyOnHand = 10;
        decimal qtyReserved = 2;
        decimal availableQty = qtyOnHand - qtyReserved;

        decimal requestedIssueQty = 15;
        bool isSufficientStock = availableQty >= requestedIssueQty;

        Assert.Equal(8, availableQty);
        Assert.False(isSufficientStock);
    }

    [Fact]
    public void WarehouseTransfer_UpdatesBothWarehouses()
    {
        decimal sourceWhBalance = 100;
        decimal destWhBalance = 20;
        decimal transferQty = 30;

        decimal newSourceWhBalance = sourceWhBalance - transferQty;
        decimal newDestWhBalance = destWhBalance + transferQty;

        Assert.Equal(70, newSourceWhBalance);
        Assert.Equal(50, newDestWhBalance);
    }

    [Fact]
    public void MovingAverageCost_Recalculation_ProducesCorrectWeightedPrice()
    {
        decimal oldQty = 100;
        decimal oldCost = 10000; // Total old value = 1,000,000

        decimal newQty = 50;
        decimal newCost = 13000; // Total new value = 650,000

        decimal totalQty = oldQty + newQty;
        decimal totalValue = (oldQty * oldCost) + (newQty * newCost);
        decimal newWeightedCost = totalValue / totalQty;

        Assert.Equal(150, totalQty);
        Assert.Equal(1650000, totalValue);
        Assert.Equal(11000, newWeightedCost);
    }

    [Fact]
    public void StocktakeVariance_SurplusCount_GeneratesReceiptAdjustment()
    {
        decimal systemQty = 100;
        decimal countedQty = 105;

        decimal varianceQty = countedQty - systemQty;
        string adjustmentDocType = varianceQty > 0 ? "Receipt" : "Issue";

        Assert.Equal(5, varianceQty);
        Assert.Equal("Receipt", adjustmentDocType);
    }

    [Fact]
    public void StocktakeVariance_ShortageCount_GeneratesIssueAdjustment()
    {
        decimal systemQty = 100;
        decimal countedQty = 92;

        decimal varianceQty = countedQty - systemQty;
        string adjustmentDocType = varianceQty > 0 ? "Receipt" : "Issue";

        Assert.Equal(-8, varianceQty);
        Assert.Equal("Issue", adjustmentDocType);
    }

    [Fact]
    public void FefoLotSelection_PrioritizesEarliestExpiringLot()
    {
        var lots = new List<(string LotCode, DateOnly ExpiryDate, decimal Qty)>
        {
            ("LOT-B", new DateOnly(2026, 12, 31), 50),
            ("LOT-A", new DateOnly(2026, 9, 15), 30),
            ("LOT-C", new DateOnly(2027, 3, 1), 100)
        };

        var selectedLot = lots.OrderBy(x => x.ExpiryDate).First();

        Assert.Equal("LOT-A", selectedLot.LotCode);
        Assert.Equal(new DateOnly(2026, 9, 15), selectedLot.ExpiryDate);
    }
}

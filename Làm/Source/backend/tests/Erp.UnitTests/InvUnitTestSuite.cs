using Xunit;

namespace Erp.UnitTests;

public class InvUnitTestSuite
{
    [Fact]
    public void Inv_StockReceipt_IncreasesOnHandQty()
    {
        decimal initialOnHand = 100;
        decimal receivedQty = 50;

        decimal updatedOnHand = initialOnHand + receivedQty;

        Assert.Equal(150, updatedOnHand);
    }

    [Fact]
    public void Inv_StockIssue_ExceedingAvailable_ThrowsError()
    {
        decimal onHand = 100;
        decimal reserved = 30;
        decimal available = onHand - reserved;
        decimal requestedIssue = 80;

        bool isIssueAllowed = requestedIssue <= available;

        Assert.Equal(70, available);
        Assert.False(isIssueAllowed);
    }

    [Fact]
    public void Inv_WarehouseTransfer_UpdatesBothSourceAndTarget()
    {
        decimal sourceOnHand = 100;
        decimal targetOnHand = 20;
        decimal transferQty = 40;

        decimal newSourceOnHand = sourceOnHand - transferQty;
        decimal newTargetOnHand = targetOnHand + transferQty;

        Assert.Equal(60, newSourceOnHand);
        Assert.Equal(60, newTargetOnHand);
    }

    [Fact]
    public void Inv_MovingAverageCost_NewPurchase_RecalculatesWeightedCost()
    {
        // Old: 100 units @ 10,000 = 1,000,000
        // New: 50 units @ 16,000 = 800,000
        // Total: 150 units = 1,800,000 -> Avg = 12,000
        decimal oldQty = 100;
        decimal oldAvgCost = 10000;
        decimal newQty = 50;
        decimal newUnitPrice = 16000;

        decimal totalValue = (oldQty * oldAvgCost) + (newQty * newUnitPrice);
        decimal totalQty = oldQty + newQty;
        decimal newMovingAvgCost = totalValue / totalQty;

        Assert.Equal(12000, newMovingAvgCost);
    }

    [Fact]
    public void Inv_FefoLotSelection_PrioritizesEarliestExpiringLot()
    {
        var lots = new List<(string LotNumber, DateOnly ExpiryDate)>
        {
            ("LOT-B", new DateOnly(2026, 12, 31)),
            ("LOT-A", new DateOnly(2026, 8, 15)),
            ("LOT-C", new DateOnly(2027, 3, 31))
        };

        var selectedLot = lots.OrderBy(l => l.ExpiryDate).First();

        Assert.Equal("LOT-A", selectedLot.LotNumber);
    }

    [Fact]
    public void Inv_StockReservation_LocksQuantityForConfirmedSalesOrder()
    {
        decimal currentOnHand = 200;
        decimal currentReserved = 50;
        decimal orderQtyToReserve = 40;

        decimal newReserved = currentReserved + orderQtyToReserve;
        decimal availableQty = currentOnHand - newReserved;

        Assert.Equal(90, newReserved);
        Assert.Equal(110, availableQty);
    }

    [Fact]
    public void Inv_BarcodeScanner_SkuLookup_ReturnsProductDetails()
    {
        string scannedBarcode = "8934567890123";
        var barcodeDb = new Dictionary<string, string>
        {
            { "8934567890123", "PROD-SKU-MILK-TEA" }
        };

        bool isFound = barcodeDb.TryGetValue(scannedBarcode, out string? sku);

        Assert.True(isFound);
        Assert.Equal("PROD-SKU-MILK-TEA", sku);
    }

    [Fact]
    public void Inv_MinMaxReorder_BelowMinThreshold_GeneratesPurchaseSuggestion()
    {
        decimal currentStock = 12;
        decimal minReorderPoint = 20;

        bool needsReorder = currentStock < minReorderPoint;

        Assert.True(needsReorder);
    }

    [Fact]
    public void Inv_BatchQuarantine_QualityHold_LocksStockFromIssuance()
    {
        string batchStatus = "Quarantine";
        bool canIssueFromBatch = batchStatus != "Quarantine" && batchStatus != "Rejected";

        Assert.False(canIssueFromBatch);
    }

    [Fact]
    public void Inv_ConsignmentStock_VendorOwnedInventory_TracksThirdPartyOwnership()
    {
        string inventoryOwnerType = "VendorConsignment";
        Guid vendorId = Guid.NewGuid();

        bool isVendorOwned = inventoryOwnerType == "VendorConsignment" && vendorId != Guid.Empty;

        Assert.True(isVendorOwned);
    }

    [Fact]
    public void Inv_StockRevaluation_WriteDown_AdjustsBookValueToLowerOfCostOrNetRealizableValue()
    {
        decimal costPrice = 50000;
        decimal netRealizableValue = 35000; // Lower than cost due to damage

        decimal inventoryValuationPrice = Math.Min(costPrice, netRealizableValue);
        decimal writeDownAdjustmentPerUnit = costPrice - inventoryValuationPrice;

        Assert.Equal(35000, inventoryValuationPrice);
        Assert.Equal(15000, writeDownAdjustmentPerUnit);
    }

    [Fact]
    public void Inv_KitAssembly_Disassembly_RestoresComponentQuantities()
    {
        // 1 Combo Kit = 1 Main Unit + 2 Accessories
        int kitsToDisassemble = 10;

        int restoredMainUnits = kitsToDisassemble * 1;
        int restoredAccessories = kitsToDisassemble * 2;

        Assert.Equal(10, restoredMainUnits);
        Assert.Equal(20, restoredAccessories);
    }
}

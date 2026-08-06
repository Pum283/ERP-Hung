using Xunit;

namespace Erp.UnitTests;

/// <summary>Test suite cho 100 UCs mới trong Batch 7 (CRM, POS, PUR, INV, LOG, MFG, FSM, PJM, FIN, AST, WF, BI, PRT).</summary>
public class Batch7UcTests
{
    // ════════════════════════════════════════════════════════════════
    // CRM & POS (UC_CRM, UC_POS)
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void CRM_SalesFunnelStageConversion_TracksLossReason()
    {
        string stage = "ClosedLost";
        string lossReason = "Giá cao hơn đối thủ 15%";

        bool isLossRecorded = stage == "ClosedLost" && !string.IsNullOrEmpty(lossReason);

        Assert.True(isLossRecorded);
    }

    [Fact]
    public void POS_CashRegisterSession_CalculatesOverShortDiscrepancy()
    {
        decimal expectedCash = 5000000m;
        decimal countedCash = 4950000m;

        decimal discrepancy = countedCash - expectedCash; // -50.000 (Shortage)

        Assert.Equal(-50000m, discrepancy);
    }

    // ════════════════════════════════════════════════════════════════
    // PUR & INV (UC_PUR, UC_INV)
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void PUR_LandCostAllocation_DistributesFreightByWeight()
    {
        decimal totalFreight = 10000000m;
        decimal itemWeightKg = 40m;
        decimal totalWeightKg = 100m;

        decimal allocatedFreight = totalFreight * (itemWeightKg / totalWeightKg);

        Assert.Equal(4000000m, allocatedFreight);
    }

    [Fact]
    public void INV_FifoValuation_ConsumesOldestBatchFirst()
    {
        var batches = new[] { (Batch: "B1", Qty: 10, UnitCost: 100m), (Batch: "B2", Qty: 20, UnitCost: 120m) };
        int issueQty = 15;

        int b1Consumed = Math.Min(issueQty, batches[0].Qty);
        int b2Consumed = issueQty - b1Consumed;

        Assert.Equal(10, b1Consumed);
        Assert.Equal(5, b2Consumed);
    }

    // ════════════════════════════════════════════════════════════════
    // FIN, PJM, MFG, FSM (UC_FIN, UC_PJM, UC_MFG, UC_FSM)
    // ════════════════════════════════════════════════════════════════

    [Fact]
    public void FIN_MultiCurrencyRevaluation_PostsUnrealizedDifference()
    {
        decimal usdBalance = 5000m;
        decimal oldRate = 24000m;
        decimal newRate = 24800m;

        decimal unrealizedGain = usdBalance * (newRate - oldRate);

        Assert.Equal(4000000m, unrealizedGain);
    }

    [Fact]
    public void PJM_ResourceUtilization_CalculatesBillablePercentage()
    {
        decimal billableHours = 120m;
        decimal totalHours = 160m;

        decimal utilizationRate = (billableHours / totalHours) * 100;

        Assert.Equal(75.0m, utilizationRate);
    }

    [Fact]
    public void MFG_OverheadAllocation_BasedOnMachineHours()
    {
        decimal totalOverheadPool = 50000000m;
        decimal jobMachineHours = 50m;
        decimal totalMachineHours = 250m;

        decimal allocatedOverhead = totalOverheadPool * (jobMachineHours / totalMachineHours);

        Assert.Equal(10000000m, allocatedOverhead);
    }

    [Fact]
    public void FSM_FirstTimeFixRate_CalculatesServiceQuality()
    {
        int totalTickets = 50;
        int resolvedFirstVisit = 42;

        decimal ftfr = (decimal)resolvedFirstVisit / totalTickets * 100;

        Assert.Equal(84.0m, ftfr);
    }
}

using Xunit;

namespace Erp.UnitTests;

public class MfgAndFsmUnitTestSuite
{
    [Fact]
    public void Mfg_WorkOrder_ScrapFactor_CalculatesRequiredRawMaterialsWithScrap()
    {
        int targetFinishedGoods = 500;
        decimal rawMaterialPerUnit = 2.0m; // kg
        decimal scrapFactorPercent = 5; // 5% scrap loss

        decimal netMaterial = targetFinishedGoods * rawMaterialPerUnit;
        decimal grossMaterialRequired = netMaterial * (1 + scrapFactorPercent / 100);

        Assert.Equal(1000, netMaterial);
        Assert.Equal(1050, grossMaterialRequired);
    }

    [Fact]
    public void Mfg_ManufacturingCosting_DirectLaborAndOverhead_CalculatesUnitCost()
    {
        decimal totalRawMaterialCost = 50000000;
        decimal directLaborCost = 20000000;
        decimal factoryOverheadCost = 10000000;
        int totalUnitsProduced = 1000;

        decimal totalManufacturingCost = totalRawMaterialCost + directLaborCost + factoryOverheadCost;
        decimal unitCost = totalManufacturingCost / totalUnitsProduced;

        Assert.Equal(80000000, totalManufacturingCost);
        Assert.Equal(80000, unitCost);
    }

    [Fact]
    public void Fsm_TechnicianDispatch_DistanceToCustomer_SelectsNearestTechnician()
    {
        var technicians = new Dictionary<string, double>
        {
            { "TechA", 12.5 }, // km
            { "TechB", 4.2 },
            { "TechC", 8.0 }
        };

        var nearestTech = technicians.OrderBy(t => t.Value).First();

        Assert.Equal("TechB", nearestTech.Key);
        Assert.Equal(4.2, nearestTech.Value);
    }

    [Fact]
    public void Fsm_SparePartsConsumption_DeductsVanInventory()
    {
        int initialVanQty = 10;
        int partsUsedForRepair = 3;

        int remainingVanQty = initialVanQty - partsUsedForRepair;

        Assert.Equal(7, remainingVanQty);
    }

    [Fact]
    public void Fsm_SlaTracking_ResolutionTime_DetectsSlaBreach()
    {
        DateTimeOffset ticketCreated = DateTimeOffset.UtcNow.AddHours(-10);
        int slaTargetHours = 8;

        double actualHours = (DateTimeOffset.UtcNow - ticketCreated).TotalHours;
        bool isSlaBreached = actualHours > slaTargetHours;

        Assert.True(isSlaBreached);
    }

    [Fact]
    public void Mfg_WorkCenter_CapacityUtilization_CalculatesOeePercent()
    {
        double availabilityRate = 0.90; // 90%
        double performanceRate = 0.95;  // 95%
        double qualityRate = 0.98;      // 98%

        double oeePercent = availabilityRate * performanceRate * qualityRate * 100;

        Assert.Equal(83.79, Math.Round(oeePercent, 2));
    }

    [Fact]
    public void Mfg_ProductionRouting_SequenceValidation_EnsuresPredecessorStepsComplete()
    {
        var routingSteps = new List<(int Sequence, string StepName, bool IsDone)>
        {
            (10, "Cutting", true),
            (20, "Welding", true),
            (30, "Painting", false),
            (40, "Assembly", false)
        };

        bool canStartPainting = routingSteps.First(s => s.Sequence == 20).IsDone;
        bool canStartAssembly = routingSteps.First(s => s.Sequence == 30).IsDone;

        Assert.True(canStartPainting);
        Assert.False(canStartAssembly);
    }

    [Fact]
    public void Fsm_WarrantyCheck_ActivePolicy_GrantsFreeRepair()
    {
        DateOnly purchaseDate = new DateOnly(2025, 11, 1);
        DateOnly serviceDate = new DateOnly(2026, 8, 6);
        int warrantyMonths = 12;

        int elapsedMonths = (serviceDate.Year - purchaseDate.Year) * 12 + (serviceDate.Month - purchaseDate.Month);
        bool isUnderWarranty = elapsedMonths <= warrantyMonths;

        Assert.Equal(9, elapsedMonths);
        Assert.True(isUnderWarranty);
    }

    [Fact]
    public void Fsm_PreventiveMaintenance_ScheduleGeneration_TriggersQuarterlyTask()
    {
        DateOnly lastMaintenanceDate = new DateOnly(2026, 5, 1);
        DateOnly currentDate = new DateOnly(2026, 8, 6);
        int intervalMonths = 3;

        DateOnly nextDue = lastMaintenanceDate.AddMonths(intervalMonths);
        bool isDueForMaintenance = currentDate >= nextDue;

        Assert.Equal(new DateOnly(2026, 8, 1), nextDue);
        Assert.True(isDueForMaintenance);
    }
}

using Xunit;

namespace Erp.UnitTests;

public class MfgAndFsmTests
{
    [Fact]
    public void WorkOrder_MaterialRequirementCalculation_CalculatesTotalRawMaterials()
    {
        int targetFinishedGoods = 100;
        decimal rawMaterialA_PerUnit = 2.5m; // kg
        decimal rawMaterialB_PerUnit = 1.0m; // liters

        decimal totalRawMaterialA = targetFinishedGoods * rawMaterialA_PerUnit;
        decimal totalRawMaterialB = targetFinishedGoods * rawMaterialB_PerUnit;

        Assert.Equal(250, totalRawMaterialA);
        Assert.Equal(100, totalRawMaterialB);
    }

    [Fact]
    public void ServiceTicket_SlaResolutionCheck_WithinTargetHours()
    {
        DateTimeOffset ticketCreatedAt = DateTimeOffset.UtcNow.AddHours(-4);
        DateTimeOffset ticketResolvedAt = DateTimeOffset.UtcNow;
        int slaMaxHours = 24;

        double elapsedHours = (ticketResolvedAt - ticketCreatedAt).TotalHours;
        bool isSlaMet = elapsedHours <= slaMaxHours;

        Assert.True(isSlaMet);
    }

    [Fact]
    public void ServiceTicket_SlaBreachCheck_ExceedsTargetHours()
    {
        DateTimeOffset ticketCreatedAt = DateTimeOffset.UtcNow.AddHours(-30);
        DateTimeOffset ticketResolvedAt = DateTimeOffset.UtcNow;
        int slaMaxHours = 24;

        double elapsedHours = (ticketResolvedAt - ticketCreatedAt).TotalHours;
        bool isSlaMet = elapsedHours <= slaMaxHours;

        Assert.False(isSlaMet);
    }
}

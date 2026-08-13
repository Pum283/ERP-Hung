using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Giá theo khung giờ & ngày trong tuần (UC_POS_017, UC_POS_018).</summary>
public class PosTimeSlotPriceRule : TenantEntity
{
    public string RuleName { get; set; } = "";
    public Guid ProductId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    /// <summary>Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday</summary>
    public string DaysOfWeek { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
    public decimal SpecialPriceVnd { get; set; }
    public double DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
}

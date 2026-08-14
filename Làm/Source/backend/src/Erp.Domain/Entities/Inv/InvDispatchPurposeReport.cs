using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Báo cáo xuất kho theo mục đích (UC_INV_068).</summary>
public class InvDispatchPurposeReport : TenantEntity
{
    public string PurposeCategory { get; set; } = "Bán Hàng"; // Bán Hàng | Sản Xuất | Dự Án | Kỹ Thuật Bảo Trì | Xuất Nội Bộ
    public int DispatchCount { get; set; }
    public decimal TotalDispatchedValueVnd { get; set; }
    public double ValuePercentage { get; set; }
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }
}

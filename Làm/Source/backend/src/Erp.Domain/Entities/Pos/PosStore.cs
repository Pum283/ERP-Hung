using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Điểm bán POS (UC_POS_001).</summary>
public class PosStore : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    /// <summary>Kho INV gắn điểm bán — trừ tồn BOM khi bán (UC_POS_054).</summary>
    public Guid? WarehouseId { get; set; }
    /// <summary>Target doanh thu tháng (UC_POS_072) — 0 = chưa đặt.</summary>
    public decimal MonthlyRevenueTarget { get; set; }
}

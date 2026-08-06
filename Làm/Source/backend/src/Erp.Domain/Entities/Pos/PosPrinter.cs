using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Máy in hóa đơn (UC_POS_003).</summary>
public class PosPrinter : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Receipt · Kitchen</summary>
    public string PrinterType { get; set; } = "Receipt";
    public string? ConnectionInfo { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}

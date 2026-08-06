using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Đơn vị vận chuyển (UC_LOG_001).</summary>
public class LogCarrier : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? ContactName { get; set; }
    public string? Note { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}

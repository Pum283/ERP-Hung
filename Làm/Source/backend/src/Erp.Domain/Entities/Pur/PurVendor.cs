using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Nhà cung cấp (UC_PUR_001, 003).</summary>
public class PurVendor : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? TaxCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PaymentTerms { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}

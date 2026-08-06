using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Người liên hệ NCC (UC_PUR_003).</summary>
public class PurVendorContact : TenantEntity
{
    public Guid VendorId { get; set; }
    public string FullName { get; set; } = "";
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

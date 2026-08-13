using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Hồ sơ pháp lý nhà cung cấp (UC_PUR_008).</summary>
public class PurSupplierLegalDocument : TenantEntity
{
    public Guid SupplierId { get; set; }
    public string DocumentType { get; set; } = "BusinessLicense"; // BusinessLicense | TaxRegistration | FoodSafetyCert | ISO
    public string DocumentNumber { get; set; } = "";
    public DateTimeOffset IssuedDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public string FileUrl { get; set; } = "";
    public string Status { get; set; } = "Valid"; // Valid | ExpiringSoon | Expired
}

using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Yêu cầu báo giá gửi nhiều nhà cung cấp (UC_PUR_021).</summary>
public class PurRfqMultiSupplier : TenantEntity
{
    public string RfqNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string SupplierIdsJson { get; set; } = "[]";
    public string ItemsJson { get; set; } = "[]";
    public DateTimeOffset DeadlineDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft | Sent | QuotationsReceived | Closed
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lịch sử bàn giao phụ trách (UC_CRM_009).</summary>
public class CrmCustomerHandover : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Guid? FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset HandedAt { get; set; }
}

using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Người liên hệ của khách (UC_CRM_011).</summary>
public class CrmContact : TenantEntity
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = "";
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

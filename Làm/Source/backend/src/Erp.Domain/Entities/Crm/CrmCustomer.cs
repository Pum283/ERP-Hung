using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Master khách hàng CN/DN (UC_CRM_001–006, 008–010).</summary>
public class CrmCustomer : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Person · Organization</summary>
    public string CustomerType { get; set; } = "Person";
    public string DisplayName { get; set; } = "";
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxCode { get; set; }
    /// <summary>Lead · Prospect · Customer · Partner</summary>
    public string Segment { get; set; } = "Prospect";
    public Guid? OwnerUserId { get; set; }
    /// <summary>Active · Inactive · Merged</summary>
    public string Status { get; set; } = "Active";
    public Guid? MergedIntoId { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    /// <summary>1–5 tiềm năng (UC_007 light)</summary>
    public int? PotentialScore { get; set; }
}

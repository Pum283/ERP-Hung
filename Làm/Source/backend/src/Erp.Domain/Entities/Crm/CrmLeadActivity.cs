using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Nhật ký chăm sóc lead (UC_CRM_056).</summary>
public class CrmLeadActivity : TenantEntity
{
    public Guid LeadId { get; set; }
    /// <summary>Call · Email · Meeting · Note · Other</summary>
    public string ActivityType { get; set; } = "Note";
    public string Content { get; set; } = "";
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset ActivityAt { get; set; } = DateTimeOffset.UtcNow;
}

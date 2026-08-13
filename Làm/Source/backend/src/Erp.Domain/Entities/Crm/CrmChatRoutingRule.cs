using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Quy tắc phân phối hội thoại tự động (UC_CRM_041).</summary>
public class CrmChatRoutingRule : TenantEntity
{
    public string RuleName { get; set; } = "";
    /// <summary>RoundRobin | LoadBalance | SkillBased</summary>
    public string Strategy { get; set; } = "RoundRobin";
    public string TargetSkillGroup { get; set; } = "Sales_Support";
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
}

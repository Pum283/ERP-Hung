using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Hệ thống tài khoản COA (UC_FIN_001).</summary>
public class FinAccount : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? GroupId { get; set; }
    /// <summary>Asset · Liability · Equity · Revenue · Expense</summary>
    public string AccountType { get; set; } = "Asset";
    public bool IsPostable { get; set; } = true;
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

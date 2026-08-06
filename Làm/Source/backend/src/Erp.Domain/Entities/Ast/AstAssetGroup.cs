using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Nhóm TSCĐ (UC_AST_001).</summary>
public class AstAssetGroup : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int DefaultUsefulLifeMonths { get; set; } = 36;
    public decimal DefaultDepreciationRate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

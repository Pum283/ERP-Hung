using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Phương pháp khấu hao (UC_AST_008–009).</summary>
public class AstDepreciationMethod : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>StraightLine · DecliningBalance</summary>
    public string MethodType { get; set; } = "StraightLine";
    public int DefaultUsefulLifeMonths { get; set; } = 36;
    public decimal DefaultRatePercent { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

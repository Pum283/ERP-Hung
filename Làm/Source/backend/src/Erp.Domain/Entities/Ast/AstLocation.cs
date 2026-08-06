using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Vị trí / chi nhánh tài sản (UC_AST_004).</summary>
public class AstLocation : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? BranchName { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

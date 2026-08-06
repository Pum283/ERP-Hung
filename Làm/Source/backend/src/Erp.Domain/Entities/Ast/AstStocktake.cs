using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Đợt kiểm kê TSCĐ (UC_AST_021–022).</summary>
public class AstStocktake : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid? LocationId { get; set; }
    /// <summary>Counting · Reviewed · Closed</summary>
    public string Status { get; set; } = "Counting";
    public DateTimeOffset? CountedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

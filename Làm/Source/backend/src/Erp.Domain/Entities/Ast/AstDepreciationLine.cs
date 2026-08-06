using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

public class AstDepreciationLine : TenantEntity
{
    public Guid RunId { get; set; }
    public Guid AssetId { get; set; }
    public decimal Amount { get; set; }
    public decimal BookValueBefore { get; set; }
    public decimal BookValueAfter { get; set; }
    public int LineNo { get; set; }
}

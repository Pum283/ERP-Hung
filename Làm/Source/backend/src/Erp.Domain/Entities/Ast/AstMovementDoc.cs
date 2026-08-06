using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Chứng từ điều chuyển / bàn giao / thanh lý TSCĐ (UC_AST_016–018).</summary>
public class AstMovementDoc : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Transfer · Handover · Disposal</summary>
    public string DocType { get; set; } = "Transfer";
    public DateTimeOffset DocDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid AssetId { get; set; }

    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public Guid? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public Guid? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }

    /// <summary>Scrap · Sale — chỉ Disposal</summary>
    public string? DisposalKind { get; set; }
    public decimal? DisposalAmount { get; set; }
    public decimal? BookValueSnapshot { get; set; }

    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}

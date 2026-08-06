using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Bút toán (UC_FIN_010, 012, 015).</summary>
public class FinJournal : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid PeriodId { get; set; }
    public DateTimeOffset EntryDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = "";
    /// <summary>Draft · Posted · Reversed</summary>
    public string Status { get; set; } = "Draft";
    /// <summary>Manual · Auto</summary>
    public string Source { get; set; } = "Manual";
    public Guid? ReversedFromId { get; set; }
    public Guid? ReversalId { get; set; }
    public string? PartnerCode { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
}

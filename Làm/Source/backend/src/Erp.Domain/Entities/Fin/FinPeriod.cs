using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Kỳ kế toán · khóa sổ (UC_FIN_003–004).</summary>
public class FinPeriod : TenantEntity
{
    public Guid FiscalYearId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    /// <summary>Open · Locked</summary>
    public string Status { get; set; } = "Open";
    public DateTimeOffset? LockedAt { get; set; }
    public Guid? LockedBy { get; set; }
}

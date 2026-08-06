using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

public class FinJournalLine : TenantEntity
{
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? PartnerCode { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Note { get; set; }
    public int LineNo { get; set; }
}

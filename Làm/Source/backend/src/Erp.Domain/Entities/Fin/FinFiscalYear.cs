using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Năm tài chính (UC_FIN_003).</summary>
public class FinFiscalYear : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Year { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

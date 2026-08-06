using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class Contract : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string ContractType { get; set; } = "Indefinite";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public Guid? ParentContractId { get; set; }
    public decimal? BaseSalary { get; set; }
    public Guid? ScanFileId { get; set; }
}

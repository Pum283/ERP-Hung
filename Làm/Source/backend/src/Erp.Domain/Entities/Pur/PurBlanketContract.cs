using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Hợp đồng mua khung, theo dõi sản lượng & cảnh báo hết hạn (UC_PUR_045, UC_PUR_046, UC_PUR_047).</summary>
public class PurBlanketContract : TenantEntity
{
    public string ContractNumber { get; set; } = "";
    public string ContractTitle { get; set; } = "";
    public Guid SupplierId { get; set; }
    public decimal TotalContractValueVnd { get; set; }
    public decimal ConsumedValueVnd { get; set; }
    public int TotalContractQty { get; set; }
    public int ConsumedQty { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public string Status { get; set; } = "Active"; // Active | ExpiringSoon | Expired | Closed
}

using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Hình thức thanh toán (UC_FIN_008).</summary>
public class FinPaymentMethod : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
}

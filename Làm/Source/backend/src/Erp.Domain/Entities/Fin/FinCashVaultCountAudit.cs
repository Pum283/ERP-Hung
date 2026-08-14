using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Kiểm kê quỹ tiền mặt thực tế và đối chiếu chênh lệch sổ cái (UC_FIN_022).</summary>
public class FinCashVaultCountAudit : TenantEntity
{
    public string FundCode { get; set; } = "QUY-MAT-VND";
    public string FundName { get; set; } = "Quỹ Tiền Mặt Trụ Sở Chính (VND)";
    public decimal BookBalanceVnd { get; set; } = 85200000;
    public decimal PhysicalCountVnd { get; set; } = 85200000;
    public decimal VarianceVnd { get; set; } = 0;
    public string AuditorName { get; set; } = "Kế Toán Trưởng & Thủ Quỹ";
    public string AuditConclusion { get; set; } = "Khớp đúng 100% giữa sổ quỹ tiền mặt và tiền mặt thực tế tại két sắt";
    public DateTimeOffset AuditDate { get; set; } = DateTimeOffset.UtcNow;
}

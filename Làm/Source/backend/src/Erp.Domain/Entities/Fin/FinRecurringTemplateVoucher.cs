using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Bút toán định kỳ và mẫu chứng từ hạch toán lặp lại (UC_FIN_011).</summary>
public class FinRecurringTemplateVoucher : TenantEntity
{
    public string TemplateCode { get; set; } = "TMPL-DEPR-OFFICE";
    public string TemplateName { get; set; } = "Trích khấu hao tài sản cố định văn phòng định kỳ hàng tháng";
    public string Frequency { get; set; } = "Monthly"; // Monthly | Quarterly | Annual
    public decimal DefaultAmountVnd { get; set; } = 35000000;
    public string DebitAccountCode { get; set; } = "6424";
    public string CreditAccountCode { get; set; } = "2141";
    public bool IsActive { get; set; } = true;
}

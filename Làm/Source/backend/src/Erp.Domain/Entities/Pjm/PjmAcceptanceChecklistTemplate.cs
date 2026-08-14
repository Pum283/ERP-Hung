using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Mẫu checklist nghiệm thu bàn giao dự án (UC_PJM_003).</summary>
public class PjmAcceptanceChecklistTemplate : TenantEntity
{
    public string TemplateCode { get; set; } = "TMPL-ACCEPT-MECH";
    public string TemplateName { get; set; } = "Nghiệm Thu Hệ Thống Cơ Điện (M&E)";
    public string ProjectCategory { get; set; } = "Thi Công Lắp Đặt";
    public string ChecklistItemContent { get; set; } = "1. Kiểm tra đấu nối dây tiếp địa và điện trở đất < 4 Ohm";
    public int SequenceOrder { get; set; } = 1;
    public bool IsMandatory { get; set; } = true;
}

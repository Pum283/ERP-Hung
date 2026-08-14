using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Checklist bảo trì chuẩn theo nhóm thiết bị (UC_FSM_035).</summary>
public class FsmStandardMaintenanceChecklist : TenantEntity
{
    public string EquipmentCategory { get; set; } = "Chiller & HVAC";
    public string ChecklistItemName { get; set; } = "1. Vệ sinh dàn trao đổi nhiệt và màng lọc gió";
    public string StandardOperatingProcedure { get; set; } = "Dùng vòi xịt áp lực thấp và dung dịch tẩy cặn chuyên dụng";
    public int SequenceOrder { get; set; } = 1;
    public bool IsMandatory { get; set; } = true;
}

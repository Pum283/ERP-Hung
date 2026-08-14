using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Tiêu chí kiểm soát chất lượng nguyên vật liệu đầu vào IQC (UC_MFG_032).</summary>
public class MfgIncomingQcCriterion : TenantEntity
{
    public string CriterionCode { get; set; } = "";
    public string CriterionName { get; set; } = "";
    public string MaterialGroup { get; set; } = "";
    public string StandardSpecification { get; set; } = "";
    public string InspectionMethod { get; set; } = "Thước kẹp / Máy đo quang học";
    public decimal MinAcceptableValue { get; set; }
    public decimal MaxAcceptableValue { get; set; }
    public bool IsMandatory { get; set; } = true;
}

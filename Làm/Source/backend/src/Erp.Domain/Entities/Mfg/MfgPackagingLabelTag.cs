using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Quy cách đóng gói & in tem mã vạch sản phẩm (UC_MFG_039).</summary>
public class MfgPackagingLabelTag : TenantEntity
{
    public string ProductCode { get; set; } = "";
    public string PackagingType { get; set; } = "Thùng Carton 5 Lớp";
    public decimal UnitsPerPackage { get; set; } = 24;
    public string BarcodeLabelFormat { get; set; } = "GS1-128 / QR Code";
    public string LabelTemplatePath { get; set; } = "/templates/labels/mfg-standard-100x150.prn";
    public bool IsActive { get; set; } = true;
}

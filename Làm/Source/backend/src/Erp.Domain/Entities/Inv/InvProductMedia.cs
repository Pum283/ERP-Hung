using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Ảnh & mô tả sản phẩm kỹ thuật chi tiết (UC_INV_006).</summary>
public class InvProductMedia : TenantEntity
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string PrimaryImageUrl { get; set; } = "";
    public string GalleryImageUrlsJson { get; set; } = "[]";
    public string RichTechnicalDescription { get; set; } = "";
    public string MaterialSpecification { get; set; } = "";
}

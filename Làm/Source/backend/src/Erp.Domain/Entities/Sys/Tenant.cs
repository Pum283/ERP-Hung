using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class Tenant : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string DefaultLocale { get; set; } = "vi-VN";
    public string DefaultCurrency { get; set; } = "VND";
    /// <summary>URL công khai logo (Cloudinary secure_url).</summary>
    public string? LogoUrl { get; set; }
    /// <summary>Storage key (vd cloudinary:public_id) để thay/xóa.</summary>
    public string? LogoStorageKey { get; set; }
}

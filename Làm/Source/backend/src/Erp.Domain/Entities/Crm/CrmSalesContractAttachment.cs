using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Đính kèm file hợp đồng (UC_CRM_107).</summary>
public class CrmSalesContractAttachment : TenantEntity
{
    public Guid ContractId { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
    public string FileType { get; set; } = "application/pdf";
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}

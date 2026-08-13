using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmSalesContractAdminService
{
    // UC_CRM_105: Báo cáo năng suất Sales Admin
    Task<IReadOnlyList<CrmSalesAdminProductivityDto>> GetProductivityReportsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_106: Quản lý hợp đồng bán
    Task<CrmSalesContractDto> CreateContractAsync(Guid tenantId, CrmCreateSalesContractRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmSalesContractDto>> GetContractsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default);

    // UC_CRM_107: Đính kèm file hợp đồng
    Task<CrmContractAttachmentDto> AttachFileAsync(Guid tenantId, CrmAttachContractFileRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmContractAttachmentDto>> GetAttachmentsAsync(Guid tenantId, Guid contractId, CancellationToken ct = default);

    // UC_CRM_108: Theo dõi hiệu lực / tái tục
    Task<CrmContractRenewalStatusDto> RenewContractAsync(Guid tenantId, CrmRenewContractRequest req, CancellationToken ct = default);
}

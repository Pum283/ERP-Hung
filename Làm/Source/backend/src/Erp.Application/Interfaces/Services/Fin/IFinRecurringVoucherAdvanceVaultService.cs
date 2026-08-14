using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFinRecurringVoucherAdvanceVaultService
{
    // UC_FIN_011: Bút toán định kỳ / mẫu
    Task<FinRecurringTemplateVoucherDto> CreateRecurringTemplateAsync(Guid tenantId, FinCreateRecurringTemplateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinRecurringTemplateVoucherDto>> GetRecurringTemplatesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_017: Đính kèm chứng từ gốc
    Task<FinOriginalVoucherAttachmentDto> UploadVoucherAttachmentAsync(Guid tenantId, FinUploadVoucherAttachmentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinOriginalVoucherAttachmentDto>> GetVoucherAttachmentsAsync(Guid tenantId, Guid journalEntryId, CancellationToken ct = default);

    // UC_FIN_021: Đề nghị tạm ứng / hoàn ứng
    Task<FinAdvanceSettlementRequestDto> CreateAdvanceSettlementAsync(Guid tenantId, FinCreateAdvanceSettlementRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinAdvanceSettlementRequestDto>> GetAdvanceSettlementsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_022: Kiểm kê quỹ
    Task<FinCashVaultCountAuditDto> CreateVaultCountAuditAsync(Guid tenantId, FinCreateVaultCountAuditRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinCashVaultCountAuditDto>> GetVaultCountAuditsAsync(Guid tenantId, CancellationToken ct = default);
}

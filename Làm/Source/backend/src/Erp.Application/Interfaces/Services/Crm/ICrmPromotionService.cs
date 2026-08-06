using Erp.Application.DTOs.Crm;

namespace Erp.Application.Interfaces.Services.Crm;

/// <summary>Khuyến mại, voucher & chat history (UC_CRM_032–038, 047).</summary>
public interface ICrmPromotionService
{
    // ── Promotion CRUD (UC_CRM_032, 033) ──
    Task<IReadOnlyList<CrmPromotionDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<CrmPromotionDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CrmPromotionDto> UpsertAsync(Guid tenantId, Guid userId, CrmPromotionUpsertRequest req, CancellationToken ct = default);

    // ── Voucher (UC_CRM_034, 035) ──
    Task<IReadOnlyList<CrmVoucherDto>> GenerateVouchersAsync(Guid tenantId, Guid userId, CrmVoucherGenerateRequest req, CancellationToken ct = default);
    Task<CrmVoucherRedeemResult> RedeemVoucherAsync(Guid tenantId, Guid userId, CrmVoucherRedeemRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmVoucherDto>> ListVouchersAsync(Guid tenantId, Guid promotionId, CancellationToken ct = default);

    // ── Apply promotion on quote (UC_CRM_037) ──
    Task<CrmApplyPromotionResult> ApplyOnQuoteAsync(Guid tenantId, Guid userId, CrmApplyPromotionRequest req, CancellationToken ct = default);

    // ── Sync CRM → POS (UC_CRM_036) ──
    Task<CrmSyncPromoToPosResult> SyncToPosAsync(Guid tenantId, Guid userId, Guid promotionId, CancellationToken ct = default);

    // ── Voucher usage report (UC_CRM_038) ──
    Task<IReadOnlyList<CrmVoucherUsageReportRowDto>> GetVoucherUsageReportAsync(
        Guid tenantId, Guid? promotionId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default);

    // ── Chat history (UC_CRM_047) ──
    Task<CrmChatHistoryDto> SaveChatAsync(Guid tenantId, Guid userId, CrmChatHistoryRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmChatHistoryDto>> ListChatAsync(Guid tenantId, Guid? customerId, string? channel, CancellationToken ct = default);
}

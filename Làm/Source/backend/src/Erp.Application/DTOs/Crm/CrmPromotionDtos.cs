namespace Erp.Application.DTOs.Crm;

// ── Promotion (UC_CRM_032, 033, 037) ──
public sealed record CrmPromotionDto(
    Guid Id, string Code, string Name, string? Description,
    string DiscountType, decimal DiscountValue,
    decimal? MaxDiscountAmount, decimal? MinOrderValue,
    string Status, DateTimeOffset? StartDate, DateTimeOffset? EndDate,
    int? MaxUsageTotal, int? MaxUsagePerCustomer, int CurrentUsageCount,
    Guid? CampaignId,
    IReadOnlyList<CrmPromotionConditionDto> Conditions);

public sealed record CrmPromotionUpsertRequest(
    Guid? Id, string Code, string Name, string? Description,
    string DiscountType, decimal DiscountValue,
    decimal? MaxDiscountAmount, decimal? MinOrderValue,
    DateTimeOffset? StartDate, DateTimeOffset? EndDate,
    int? MaxUsageTotal, int? MaxUsagePerCustomer, Guid? CampaignId,
    IReadOnlyList<CrmPromotionConditionRequest>? Conditions);

// ── Promotion Condition (UC_CRM_033) ──
public sealed record CrmPromotionConditionDto(
    Guid Id, Guid PromotionId, string ConditionType,
    string ConditionValue, string Operator);

public sealed record CrmPromotionConditionRequest(
    string ConditionType, string ConditionValue, string Operator);

// ── Voucher (UC_CRM_034, 035) ──
public sealed record CrmVoucherDto(
    Guid Id, Guid PromotionId, string VoucherCode,
    string Status, DateTimeOffset? ExpiresAt,
    int UsageCount, int MaxUsage, Guid? AssignedCustomerId);

public sealed record CrmVoucherGenerateRequest(
    Guid PromotionId, int Quantity, string? Prefix,
    int MaxUsagePerVoucher, DateTimeOffset? ExpiresAt);

public sealed record CrmVoucherRedeemRequest(
    string VoucherCode, Guid? CustomerId, Guid? QuoteId, Guid? SalesOrderId);

public sealed record CrmVoucherRedeemResult(
    bool Success, string? ErrorMessage, decimal DiscountApplied,
    CrmVoucherDto? Voucher);

// ── Quote Promotion Apply (UC_CRM_037) ──
public sealed record CrmApplyPromotionRequest(
    Guid QuoteId, Guid? PromotionId, string? VoucherCode);

public sealed record CrmApplyPromotionResult(
    bool Applied, decimal DiscountAmount, string? Message);

// ── Sync CRM → POS (UC_CRM_036) ──
public sealed record CrmSyncPromoToPosResult(
    Guid CrmPromotionId, Guid PosPromotionId, string PosPromotionCode,
    bool Created, int VouchersSynced, int VouchersSkipped, string Message);

// ── Voucher usage report (UC_CRM_038) ──
public sealed record CrmVoucherUsageReportRowDto(
    Guid VoucherId, string VoucherCode, Guid PromotionId,
    string PromotionCode, string PromotionName,
    int RedeemCount, decimal TotalDiscount, DateTimeOffset? LastUsedAt);

// ── Chat History (UC_CRM_047) ──
public sealed record CrmChatHistoryDto(
    Guid Id, string Channel, string? ExternalConversationId,
    Guid? CustomerId, Guid? AgentUserId,
    string Direction, string MessageText,
    string? AttachmentUrl, DateTimeOffset SentAt);

public sealed record CrmChatHistoryRequest(
    string Channel, string? ExternalConversationId,
    Guid? CustomerId, string Direction,
    string MessageText, string? AttachmentUrl);

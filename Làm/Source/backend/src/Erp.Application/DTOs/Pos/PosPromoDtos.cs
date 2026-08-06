namespace Erp.Application.DTOs.Pos;

public sealed record PosPromotionDto(
    Guid Id, string Code, string Name, string DiscountType, decimal DiscountValue,
    decimal MinOrderAmount, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt,
    string Status, string? Note, int VoucherCount);

public sealed record PosPromotionUpsertRequest(
    Guid? Id, string Code, string Name, string DiscountType, decimal DiscountValue,
    decimal? MinOrderAmount, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt,
    string? Status, string? Note);

public sealed record PosVoucherDto(
    Guid Id, string Code, Guid PromotionId, string? PromotionCode, string? PromotionName,
    int MaxUses, int UsedCount, string Status, string? Note);

public sealed record PosVoucherUpsertRequest(
    Guid? Id, string Code, Guid PromotionId, int MaxUses, string? Status, string? Note);

public sealed record PosApplyPromotionRequest(Guid PromotionId);
public sealed record PosApplyVoucherRequest(string VoucherCode);
public sealed record PosManualDiscountRequest(string DiscountType, decimal Value, string? Note);
public sealed record PosDecideDiscountRequest(bool Approved, string? Note);

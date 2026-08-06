namespace Erp.Application.DTOs.Pos;

public sealed record PosShiftDto(
    Guid Id, string Code, Guid StoreId, string? StoreName, Guid? TerminalId, string? TerminalName,
    Guid CashierUserId, string? CashierName, DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt,
    decimal OpeningCash, decimal? ClosingCashCounted, decimal? ExpectedCash, decimal? Variance,
    string Status, DateTimeOffset? ReportPrintedAt, string? Note,
    decimal SalesTotal, decimal CashSalesTotal, int SaleCount, int OpenSaleCount);
public sealed record PosShiftOpenRequest(Guid StoreId, Guid? TerminalId, decimal OpeningCash, string? Note);
public sealed record PosShiftCloseRequest(decimal ClosingCashCounted, string? Note);
public sealed record PosShiftDetailDto(
    PosShiftDto Shift, IReadOnlyList<PosSaleDto> Sales);

/// <summary>UC_POS_059 — đồng bộ doanh thu ca → FIN khi đóng ca / sync lại.</summary>
public sealed record PosShiftFinSyncResult(
    Guid ShiftId, int PaidSaleCount, int SyncedCount, int AlreadyHadCount, int FailedCount, string Message);

public sealed record PosSaleDto(
    Guid Id, string Code, Guid ShiftId, Guid StoreId, string? StoreName, Guid? TerminalId,
    string Status, string? AreaName, decimal SubTotal, decimal TaxAmount, decimal DiscountAmount,
    decimal TotalAmount, decimal PaidAmount, decimal ReturnedAmount,
    DateTimeOffset? PaidAt, DateTimeOffset? ReceiptPrintedAt, string? Note, int LineCount,
    string DiscountSource, Guid? PromotionId, string? PromotionCode, Guid? VoucherId,
    string? AppliedVoucherCode, string? ManualDiscountType, decimal ManualDiscountValue,
    string DiscountApprovalStatus, string? DiscountNote);
public sealed record PosSaleLineDto(
    Guid Id, Guid SaleId, Guid? ProductId, string ProductCode, string ProductName,
    decimal Quantity, decimal UnitPrice, decimal TaxRatePct, decimal LineAmount, string Status, int LineNo);
public sealed record PosSalePaymentDto(
    Guid Id, Guid SaleId, string Code, DateTimeOffset PaidAt, decimal Amount, string Method, string? Note);
public sealed record PosSaleDetailDto(
    PosSaleDto Sale, IReadOnlyList<PosSaleLineDto> Lines, IReadOnlyList<PosSalePaymentDto> Payments);
public sealed record PosSaleOpenRequest(Guid ShiftId, string? AreaName, string? Note);
public sealed record PosSaleLineUpsertRequest(
    Guid? Id, Guid? ProductId, string? ProductCode, string? ProductName,
    decimal Quantity, decimal? UnitPrice, decimal? TaxRatePct);
public sealed record PosSalePayRequest(string Method, decimal Amount, string? Note);

/// <summary>Cảnh báo tồn cho POS (UC_POS_055).</summary>
public sealed record PosStockAlertDto(
    Guid WarehouseId, string? WarehouseName,
    Guid SkuId, string SkuCode, string SkuName,
    decimal QtyOnHand, decimal? MinQty, decimal? ReorderQty, string AlertType);
public sealed record PosSaleHoldRequest(string? Note);

public sealed record PosReturnDto(
    Guid Id, string Code, Guid SaleId, string? SaleCode, Guid? ShiftId, string Status,
    decimal RefundAmount, string RefundMethod, string? Reason, DateTimeOffset? CompletedAt, int LineCount);
public sealed record PosReturnLineDto(
    Guid Id, Guid ReturnId, Guid? SaleLineId, string ProductCode, string ProductName,
    decimal Quantity, decimal LineAmount);
public sealed record PosReturnDetailDto(PosReturnDto Return, IReadOnlyList<PosReturnLineDto> Lines);
public sealed record PosReturnCreateRequest(Guid SaleId, string? Reason);
public sealed record PosReturnLineRequest(Guid SaleLineId, decimal Quantity);
public sealed record PosReturnCompleteRequest(string RefundMethod, string? Reason);

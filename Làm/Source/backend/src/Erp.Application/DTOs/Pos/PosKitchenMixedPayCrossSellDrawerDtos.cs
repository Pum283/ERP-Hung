namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_031: Gửi lệnh khu vực chế biến (KOT Ticket)
// ────────────────────────────────────────────────────────────────────────────

public record PosDispatchKitchenTicketRequest(
    Guid OrderId,
    string StationCode, // KITCHEN | BAR
    IReadOnlyList<string> ItemSummaries
);

public record PosKitchenOrderTicketDto(
    Guid Id,
    Guid OrderId,
    string TicketNumber,
    string StationCode,
    IReadOnlyList<string> ItemSummaries,
    string Status,
    DateTimeOffset SentAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_036: Thanh toán hỗn hợp
// ────────────────────────────────────────────────────────────────────────────

public record PosPaymentSplitMethodDto(
    string PaymentMethod, // Cash | CreditCard | BankTransfer | EWallet
    decimal AmountVnd
);

public record PosProcessMixedPaymentRequest(
    Guid OrderId,
    decimal OrderTotalVnd,
    IReadOnlyList<PosPaymentSplitMethodDto> Payments
);

public record PosMixedPaymentResultDto(
    Guid OrderId,
    decimal OrderTotalVnd,
    decimal TotalPaidVnd,
    decimal BalanceRemainingVnd,
    bool IsFullyPaid,
    DateTimeOffset PaidAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_041: Gợi ý bán kèm (Cross-sell / Upsell)
// ────────────────────────────────────────────────────────────────────────────

public record PosCrossSellRecommendationDto(
    Guid RecommendedProductId,
    string ProductCode,
    string ProductName,
    decimal PriceVnd,
    string Reason // Combo phù hợp | Bánh ngọt kèm Cà phê | Topping ưu đãi
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_044: Nộp tiền / rút tiền ca (Cash In / Cash Out)
// ────────────────────────────────────────────────────────────────────────────

public record PosCashInDrawerRequest(
    Guid ShiftId,
    decimal AmountVnd,
    string Reason
);

public record PosCashOutDrawerRequest(
    Guid ShiftId,
    decimal AmountVnd,
    string Reason
);

public record PosShiftCashTransactionDto(
    Guid Id,
    Guid ShiftId,
    string TransactionType, // CashIn | CashOut
    decimal AmountVnd,
    string Reason,
    DateTimeOffset TransactionTime
);

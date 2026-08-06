namespace Erp.Application.DTOs.Crm;

public sealed record CrmPriceListDto(
    Guid Id, string Code, string Name, string Status, string? Note, int ItemCount);
public sealed record CrmPriceListUpsertRequest(
    Guid? Id, string Code, string Name, string? Status, string? Note);
public sealed record CrmPriceListItemDto(
    Guid Id, Guid PriceListId, string ItemCode, string ItemName, decimal UnitPrice);
public sealed record CrmPriceListItemUpsertRequest(
    Guid? Id, string ItemCode, string ItemName, decimal UnitPrice);
public sealed record CrmPriceListDetailDto(
    CrmPriceListDto PriceList, IReadOnlyList<CrmPriceListItemDto> Items);

public sealed record CrmQuoteDto(
    Guid Id, string Code, Guid? OpportunityId, string? OpportunityCode,
    Guid? CustomerId, string? CustomerName, Guid? PriceListId, string? PriceListName,
    DateTimeOffset QuoteDate, DateTimeOffset? ValidUntil,
    decimal SubTotal, decimal DiscountPercent, decimal DiscountAmount, decimal TotalAmount,
    string Status, string DiscountApprovalStatus, int Version,
    DateTimeOffset? SentAt, string SentChannel, Guid? OrderId, string? OrderCode, string? Note,
    int LineCount);
public sealed record CrmQuoteLineDto(
    Guid Id, Guid QuoteId, string ItemCode, string ItemName,
    decimal Quantity, decimal UnitPrice, decimal LineAmount, int LineNo);
public sealed record CrmQuoteLineUpsertRequest(
    Guid? Id, string ItemCode, string ItemName, decimal Quantity, decimal UnitPrice);
public sealed record CrmQuoteDetailDto(
    CrmQuoteDto Quote, IReadOnlyList<CrmQuoteLineDto> Lines);
public sealed record CrmQuoteUpsertRequest(
    Guid? Id, Guid? OpportunityId, Guid? CustomerId, Guid? PriceListId,
    DateTimeOffset? ValidUntil, decimal? DiscountPercent, string? Note);
public sealed record CrmQuoteDiscountRequest(decimal DiscountPercent, string? Note);
public sealed record CrmQuoteDiscountDecisionRequest(bool Approved, string? Note);
public sealed record CrmQuoteSendRequest(string Channel);

public sealed record CrmSalesOrderDto(
    Guid Id, string Code, Guid? QuoteId, string? QuoteCode,
    Guid? CustomerId, string? CustomerName, Guid? OpportunityId, Guid? OwnerUserId, string? OwnerName,
    DateTimeOffset OrderDate, string Status,
    decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, decimal PaidAmount,
    string StockHoldStatus, string WarehousePushStatus, string? CancelReason, string? Note,
    int LineCount, int PaymentCount);
public sealed record CrmSalesOrderLineDto(
    Guid Id, Guid OrderId, string ItemCode, string ItemName,
    decimal Quantity, decimal UnitPrice, decimal LineAmount, int LineNo);
public sealed record CrmOrderPaymentDto(
    Guid Id, Guid OrderId, string Code, DateTimeOffset PaidAt, decimal Amount, string Method, string? Note);
public sealed record CrmOrderPaymentRequest(decimal Amount, string Method, string? Note);
public sealed record CrmSalesOrderDetailDto(
    CrmSalesOrderDto Order,
    IReadOnlyList<CrmSalesOrderLineDto> Lines,
    IReadOnlyList<CrmOrderPaymentDto> Payments);
public sealed record CrmOrderStatusRequest(string Status);
public sealed record CrmOrderCancelRequest(string Reason);

namespace Erp.Application.DTOs.Prt;

public sealed record PrtAccountDto(
    Guid Id, string Code, string Email, string DisplayName, string? CustomerCode, string? CustomerName,
    string Status, DateTimeOffset? LastLoginAt, int OrderCount, decimal OpenAr);
public sealed record PrtRegisterRequest(string Email, string DisplayName, string Password, string? CustomerCode);
public sealed record PrtLoginRequest(string Email, string Password);
public sealed record PrtForgotPasswordRequest(string Email);
public sealed record PrtLinkCustomerRequest(Guid AccountId, string CustomerCode, string? CustomerName);
public sealed record PrtAccountUpsertRequest(
    Guid? Id, string? Code, string Email, string DisplayName, string? Password,
    string? CustomerCode, string? CustomerName, string? Status);

public sealed record PrtOrderLineDto(
    Guid Id, Guid OrderId, string ItemCode, string ItemName, decimal Quantity, decimal UnitPrice, decimal LineAmount, int LineNo);
public sealed record PrtOrderDto(
    Guid Id, Guid AccountId, string? AccountEmail, string Code, DateTimeOffset OrderDate,
    string Status, decimal TotalAmount, string? ShippingAddress, string? Note, int LineCount);
public sealed record PrtOrderDetailDto(PrtOrderDto Order, IReadOnlyList<PrtOrderLineDto> Lines);
public sealed record PrtOrderUpsertRequest(
    Guid? Id, Guid AccountId, string? Code, DateTimeOffset? OrderDate, string? Status,
    string? ShippingAddress, string? Note,
    IReadOnlyList<PrtOrderLineUpsertRequest>? Lines);
public sealed record PrtOrderLineUpsertRequest(string ItemCode, string ItemName, decimal Quantity, decimal UnitPrice);

public sealed record PrtArSummaryDto(Guid AccountId, decimal OpenAmount, int OpenInvoiceCount, decimal PaidYtd);
public sealed record PrtInvoiceDto(
    Guid Id, Guid AccountId, string Code, DateTimeOffset InvoiceDate, DateTimeOffset? DueDate,
    decimal Amount, decimal PaidAmount, decimal OpenAmount, string Status);
public sealed record PrtInvoiceUpsertRequest(
    Guid? Id, Guid AccountId, string? Code, DateTimeOffset? InvoiceDate, DateTimeOffset? DueDate,
    decimal Amount, decimal? PaidAmount, string? Status);

public sealed record PrtPaymentDto(
    Guid Id, Guid AccountId, Guid? InvoiceId, string? InvoiceCode, string Code,
    DateTimeOffset PaidAt, decimal Amount, string Method, string? Note);
public sealed record PrtPaymentUpsertRequest(
    Guid? Id, Guid AccountId, Guid? InvoiceId, string? Code, DateTimeOffset? PaidAt,
    decimal Amount, string? Method, string? Note);

public sealed record PrtTicketDto(
    Guid Id, Guid AccountId, string? AccountEmail, string Code, string Subject,
    string? Description, string Status, DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt);
public sealed record PrtTicketUpsertRequest(
    Guid? Id, Guid AccountId, string Subject, string? Description, string? Status);
public sealed record PrtLoginResultDto(PrtAccountDto Account, string Message);

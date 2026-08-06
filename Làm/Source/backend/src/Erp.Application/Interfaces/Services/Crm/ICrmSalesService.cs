using Erp.Application.DTOs.Crm;

namespace Erp.Application.Interfaces.Services.Crm;

public interface ICrmSalesService
{
    Task<IReadOnlyList<CrmPriceListDto>> ListPriceListsAsync(Guid tenantId, CancellationToken ct = default);
    Task<CrmPriceListDetailDto> GetPriceListDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CrmPriceListDto> UpsertPriceListAsync(Guid tenantId, Guid userId, CrmPriceListUpsertRequest req, CancellationToken ct = default);
    Task<CrmPriceListItemDto> UpsertPriceListItemAsync(Guid tenantId, Guid userId, Guid priceListId, CrmPriceListItemUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<CrmQuoteDto>> ListQuotesAsync(Guid tenantId, string? status = null, CancellationToken ct = default);
    Task<CrmQuoteDetailDto> GetQuoteDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CrmQuoteDto> UpsertQuoteAsync(Guid tenantId, Guid userId, CrmQuoteUpsertRequest req, CancellationToken ct = default);
    Task<CrmQuoteDto> CreateQuoteFromOpportunityAsync(Guid tenantId, Guid userId, Guid opportunityId, CancellationToken ct = default);
    Task<CrmQuoteLineDto> UpsertQuoteLineAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteLineUpsertRequest req, CancellationToken ct = default);
    Task<CrmQuoteDto> ApplyPriceListAsync(Guid tenantId, Guid userId, Guid quoteId, Guid priceListId, CancellationToken ct = default);
    Task<CrmQuoteDto> RequestDiscountAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountRequest req, CancellationToken ct = default);
    Task<CrmQuoteDto> DecideDiscountAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountDecisionRequest req, CancellationToken ct = default);
    Task<CrmQuoteDto> SendQuoteAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteSendRequest req, CancellationToken ct = default);
    Task<CrmSalesOrderDto> ConvertQuoteToOrderAsync(Guid tenantId, Guid userId, Guid quoteId, CancellationToken ct = default);

    Task<IReadOnlyList<CrmSalesOrderDto>> ListOrdersAsync(Guid tenantId, string? status = null, CancellationToken ct = default);
    Task<CrmSalesOrderDetailDto> GetOrderDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<CrmSalesOrderDto> SetOrderStatusAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderStatusRequest req, CancellationToken ct = default);
    Task<CrmSalesOrderDto> HoldStockAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
    Task<CrmSalesOrderDto> CancelOrderAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderCancelRequest req, CancellationToken ct = default);
    Task<CrmOrderPaymentDto> AddPaymentAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderPaymentRequest req, CancellationToken ct = default);
    Task<CrmSalesOrderDto> PushToWarehouseAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
}

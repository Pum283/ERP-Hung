using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinRevenueService
{
    Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
        Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null,
        CancellationToken ct = default);

    Task<FinRevenueSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, CancellationToken ct = default);

    Task<FinRevenueDocumentDto> RecognizeFromPosAsync(
        Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default);

    Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default);

    Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default);

    Task<FinRevenueDocumentDto> RecognizeCogsAsync(
        Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default);

    Task<FinRevenueDocumentDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);
}

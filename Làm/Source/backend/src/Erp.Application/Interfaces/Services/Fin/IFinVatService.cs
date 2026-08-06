using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinVatService
{
    Task<FinVatCalcResult> CalculateAsync(Guid tenantId, FinVatCalcRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinVatDocumentDto>> ListDocumentsAsync(
        Guid tenantId, string? direction = null, Guid? periodId = null, string? status = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);

    Task<FinVatDocumentDto> UpsertDocumentAsync(Guid tenantId, Guid userId, FinVatDocumentUpsertRequest req, CancellationToken ct = default);
    Task<FinVatDocumentDto> PostDocumentAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinVatDocumentDto> VoidDocumentAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);

    Task<FinVatSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);

    Task<FinVatDocumentDto> RegisterFromArAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, Guid? taxId = null, CancellationToken ct = default);
    Task<FinVatDocumentDto> RegisterFromApAsync(
        Guid tenantId, Guid userId, Guid apInvoiceId, Guid? taxId = null, CancellationToken ct = default);
}
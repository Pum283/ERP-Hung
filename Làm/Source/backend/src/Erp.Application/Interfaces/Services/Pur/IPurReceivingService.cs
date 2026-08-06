using Erp.Application.DTOs.Pur;

namespace Erp.Application.Interfaces.Services.Pur;

public interface IPurReceivingService
{
    Task<IReadOnlyList<PurGrnDto>> ListGrnsAsync(Guid tenantId, Guid? poId = null, CancellationToken ct = default);
    Task<PurGrnDetailDto> GetGrnDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PurGrnDto> CreateGrnFromPoAsync(Guid tenantId, Guid userId, PurGrnCreateRequest req, CancellationToken ct = default);
    Task<PurGrnLineDto> UpdateGrnLineAsync(Guid tenantId, Guid userId, Guid grnId, PurGrnLineUpdateRequest req, CancellationToken ct = default);
    Task<PurGrnDto> PostGrnAsync(Guid tenantId, Guid userId, Guid grnId, CancellationToken ct = default);
    Task<PurGrnDto> PushGrnToInventoryAsync(Guid tenantId, Guid userId, Guid grnId, CancellationToken ct = default);

    Task<IReadOnlyList<PurInvoiceDto>> ListInvoicesAsync(Guid tenantId, Guid? vendorId = null, CancellationToken ct = default);
    Task<PurInvoiceDetailDto> GetInvoiceDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PurInvoiceDto> CreateInvoiceAsync(Guid tenantId, Guid userId, PurInvoiceCreateRequest req, CancellationToken ct = default);
    Task<PurInvoiceLineDto> UpsertInvoiceLineAsync(Guid tenantId, Guid userId, Guid invoiceId, PurInvoiceLineUpsertRequest req, CancellationToken ct = default);
    Task<PurInvoiceDto> MatchThreeWayAsync(Guid tenantId, Guid userId, Guid invoiceId, CancellationToken ct = default);
    Task<PurInvoiceDto> PushInvoiceToApAsync(Guid tenantId, Guid userId, Guid invoiceId, CancellationToken ct = default);
}
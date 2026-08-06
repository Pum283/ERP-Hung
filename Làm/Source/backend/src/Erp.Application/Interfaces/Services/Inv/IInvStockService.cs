using Erp.Application.DTOs.Inv;

namespace Erp.Application.Interfaces.Services.Inv;

public interface IInvStockService
{
    Task<IReadOnlyList<InvBalanceDto>> ListBalancesAsync(Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvStockDocDto>> ListDocsAsync(Guid tenantId, string? docType = null, CancellationToken ct = default);
    Task<InvStockDocDetailDto> GetDocDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<InvStockDocDto> CreateDocAsync(Guid tenantId, Guid userId, InvStockDocCreateRequest req, CancellationToken ct = default);
    Task<InvStockDocLineDto> UpsertDocLineAsync(Guid tenantId, Guid userId, Guid docId, InvStockDocLineRequest req, CancellationToken ct = default);
    Task<InvStockDocDto> PostDocAsync(Guid tenantId, Guid userId, Guid docId, CancellationToken ct = default);
    Task<IReadOnlyList<InvLotPickDto>> SuggestLotsAsync(
        Guid tenantId, InvSuggestLotsRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<InvReservationDto>> ListReservationsAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default);
    Task<InvReservationDetailDto> GetReservationDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default);
    Task<InvReservationDetailDto> CreateReservationAsync(
        Guid tenantId, Guid userId, InvReservationCreateRequest req, CancellationToken ct = default);
    Task<InvReservationDetailDto> ActivateReservationAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<InvReservationDetailDto> ReleaseReservationAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<InvAtpAlertRowDto>> AtpAlertsAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default);
    Task<InvStockDocDto> PostPurchaseReceiptFromGrnAsync(Guid tenantId, Guid userId, Guid grnId, Guid? warehouseId = null, CancellationToken ct = default);
    Task<InvStockDocDto> PostReceiptFromLogReturnAsync(Guid tenantId, Guid userId, Guid returnNoteId, Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvTransferDto>> ListTransfersAsync(Guid tenantId, string? status = null, CancellationToken ct = default);
    Task<InvTransferDetailDto> GetTransferDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<InvTransferDto> CreateTransferAsync(Guid tenantId, Guid userId, InvTransferCreateRequest req, CancellationToken ct = default);
    Task<InvTransferLineDto> UpsertTransferLineAsync(Guid tenantId, Guid userId, Guid transferId, InvTransferLineRequest req, CancellationToken ct = default);
    Task<InvTransferDto> ShipTransferAsync(Guid tenantId, Guid userId, Guid transferId, CancellationToken ct = default);
    Task<InvTransferDto> ReceiveTransferAsync(Guid tenantId, Guid userId, Guid transferId, CancellationToken ct = default);

    Task<IReadOnlyList<InvStocktakeDto>> ListStocktakesAsync(Guid tenantId, CancellationToken ct = default);
    Task<InvStocktakeDetailDto> GetStocktakeDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<InvStocktakeDto> CreateStocktakeAsync(Guid tenantId, Guid userId, InvStocktakeCreateRequest req, CancellationToken ct = default);
    Task<InvStocktakeLineDto> CountStocktakeLineAsync(Guid tenantId, Guid userId, Guid stocktakeId, InvStocktakeCountRequest req, CancellationToken ct = default);
    Task<InvStocktakeDto> ReviewStocktakeAsync(Guid tenantId, Guid userId, Guid stocktakeId, CancellationToken ct = default);
    Task<InvStocktakeDto> PostStocktakeAsync(Guid tenantId, Guid userId, Guid stocktakeId, CancellationToken ct = default);
}

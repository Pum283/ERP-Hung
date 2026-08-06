using Erp.Application.DTOs.Pur;

namespace Erp.Application.Interfaces.Services.Pur;

public interface IPurPurchasingService
{
    Task<IReadOnlyList<PurVendorDto>> ListVendorsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<PurVendorDto> UpsertVendorAsync(Guid tenantId, Guid userId, PurVendorUpsertRequest req, CancellationToken ct = default);
    Task<PurVendorDetailDto> GetVendorDetailAsync(Guid tenantId, Guid vendorId, CancellationToken ct = default);
    Task<PurVendorContactDto> UpsertContactAsync(Guid tenantId, Guid userId, Guid vendorId, PurVendorContactUpsertRequest req, CancellationToken ct = default);
    Task<PurVendorProductDto> UpsertVendorProductAsync(Guid tenantId, Guid userId, Guid vendorId, PurVendorProductUpsertRequest req, CancellationToken ct = default);
    Task<PurVendorPriceDto> UpsertVendorPriceAsync(Guid tenantId, Guid userId, Guid vendorId, PurVendorPriceUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PurPurchaseRequestDto>> ListPrsAsync(Guid tenantId, CancellationToken ct = default);
    Task<PurPurchaseRequestDto> UpsertPrAsync(Guid tenantId, Guid userId, PurPurchaseRequestUpsertRequest req, CancellationToken ct = default);
    Task<PurPrDetailDto> GetPrDetailAsync(Guid tenantId, Guid prId, CancellationToken ct = default);
    Task<PurPrLineDto> UpsertPrLineAsync(Guid tenantId, Guid userId, Guid prId, PurPrLineUpsertRequest req, CancellationToken ct = default);
    Task<PurPurchaseRequestDto> SubmitPrAsync(Guid tenantId, Guid userId, Guid prId, CancellationToken ct = default);
    Task<PurPurchaseRequestDto> ApprovePrAsync(Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default);
    Task<PurPurchaseRequestDto> RejectPrAsync(Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default);
    Task<PurPurchaseRequestDto> ReturnPrAsync(Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PurPurchaseOrderDto>> ListPosAsync(Guid tenantId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> UpsertPoAsync(Guid tenantId, Guid userId, PurPurchaseOrderCreateRequest req, CancellationToken ct = default);
    Task<PurPoDetailDto> GetPoDetailAsync(Guid tenantId, Guid poId, CancellationToken ct = default);
    Task<PurPoLineDto> UpsertPoLineAsync(Guid tenantId, Guid userId, Guid poId, PurPoLineUpsertRequest req, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> CreatePoFromPrAsync(Guid tenantId, Guid userId, Guid prId, PurCreatePoFromPrRequest req, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> SubmitPoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> ApprovePoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> SendPoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> RevisePoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> ClosePoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> CancelPoAsync(Guid tenantId, Guid userId, Guid poId, PurPoCancelRequest req, CancellationToken ct = default);
    Task<PurPurchaseOrderDto> PrintPoAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);

    /// <summary>UC_PUR_033 — xuất PO CSV thật (header NCC + dòng + tổng), đồng thời đóng dấu PrintedAt.</summary>
    Task<(string FileName, string Csv)> ExportPoCsvAsync(Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default);
}

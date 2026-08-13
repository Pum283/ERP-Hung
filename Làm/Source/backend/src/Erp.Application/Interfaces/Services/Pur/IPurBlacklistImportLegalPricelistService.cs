using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurBlacklistImportLegalPricelistService
{
    // UC_PUR_006: Blacklist / ngưng dùng
    Task<PurSupplierBlacklistStatusDto> BlacklistSupplierAsync(Guid tenantId, PurBlacklistSupplierRequest req, CancellationToken ct = default);

    // UC_PUR_007: Import danh sách nhà cung cấp
    Task<PurBatchImportSuppliersResultDto> ImportSuppliersBatchAsync(Guid tenantId, PurBatchImportSuppliersRequest req, CancellationToken ct = default);

    // UC_PUR_008: Hồ sơ pháp lý
    Task<PurSupplierLegalDocumentDto> SaveSupplierLegalDocumentAsync(Guid tenantId, PurSaveSupplierLegalDocumentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurSupplierLegalDocumentDto>> GetSupplierLegalDocumentsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);

    // UC_PUR_011: Hiệu lực bảng giá mua
    Task<PurPurchasePricelistValidityDto> SavePurchasePricelistValidityAsync(Guid tenantId, PurSavePurchasePricelistValidityRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurPurchasePricelistValidityDto>> GetPurchasePricelistsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);
}

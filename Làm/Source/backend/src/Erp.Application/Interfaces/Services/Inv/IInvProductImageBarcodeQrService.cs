using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvProductImageBarcodeQrService
{
    // UC_INV_006: Ảnh & mô tả sản phẩm
    Task<InvProductMediaDto> UpdateProductMediaAsync(Guid tenantId, InvUpdateProductMediaRequest req, CancellationToken ct = default);
    Task<InvProductMediaDto?> GetProductMediaAsync(Guid tenantId, Guid productId, CancellationToken ct = default);

    // UC_INV_009: Barcode / QR theo sản phẩm
    Task<InvProductBarcodeQrDto> GenerateProductBarcodeQrAsync(Guid tenantId, InvGenerateBarcodeQrRequest req, CancellationToken ct = default);
    Task<InvProductBarcodeQrDto?> GetProductBarcodeQrAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
}

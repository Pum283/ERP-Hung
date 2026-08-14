namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_006: Ảnh & mô tả sản phẩm
// ────────────────────────────────────────────────────────────────────────────

public record InvUpdateProductMediaRequest(
    Guid ProductId,
    string ProductCode,
    string PrimaryImageUrl,
    IReadOnlyList<string> GalleryImageUrls,
    string RichTechnicalDescription,
    string MaterialSpecification
);

public record InvProductMediaDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string PrimaryImageUrl,
    IReadOnlyList<string> GalleryImageUrls,
    string RichTechnicalDescription,
    string MaterialSpecification
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_009: Barcode / QR theo sản phẩm
// ────────────────────────────────────────────────────────────────────────────

public record InvGenerateBarcodeQrRequest(
    Guid ProductId,
    string ProductCode,
    string CustomBarcode,
    string LabelTemplate
);

public record InvProductBarcodeQrDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string BarcodeEan13,
    string QrCodePayload,
    string PrintableLabelTemplate,
    DateTimeOffset GeneratedAt
);

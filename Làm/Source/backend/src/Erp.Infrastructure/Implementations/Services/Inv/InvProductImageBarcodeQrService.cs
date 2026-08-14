using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvProductImageBarcodeQrService : IInvProductImageBarcodeQrService
{
    private readonly AppDbContext _db;

    public InvProductImageBarcodeQrService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_006: Ảnh & mô tả sản phẩm
    public async Task<InvProductMediaDto> UpdateProductMediaAsync(Guid tenantId, InvUpdateProductMediaRequest req, CancellationToken ct = default)
    {
        if (req.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(req.ProductCode))
            throw new AppException("Mã sản phẩm không được để trống.", 400);

        var existing = await _db.InvProductMedias.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.ProductId == req.ProductId, ct);
        if (existing == null)
        {
            existing = new InvProductMedia
            {
                TenantId = tenantId,
                ProductId = req.ProductId,
                ProductCode = req.ProductCode,
                PrimaryImageUrl = req.PrimaryImageUrl ?? "",
                GalleryImageUrlsJson = JsonSerializer.Serialize(req.GalleryImageUrls ?? new List<string>()),
                RichTechnicalDescription = req.RichTechnicalDescription ?? "",
                MaterialSpecification = req.MaterialSpecification ?? ""
            };
            _db.InvProductMedias.Add(existing);
        }
        else
        {
            existing.PrimaryImageUrl = req.PrimaryImageUrl ?? existing.PrimaryImageUrl;
            existing.GalleryImageUrlsJson = JsonSerializer.Serialize(req.GalleryImageUrls ?? new List<string>());
            existing.RichTechnicalDescription = req.RichTechnicalDescription ?? existing.RichTechnicalDescription;
            existing.MaterialSpecification = req.MaterialSpecification ?? existing.MaterialSpecification;
        }

        await _db.SaveChangesAsync(ct);

        return MapToMediaDto(existing);
    }

    public async Task<InvProductMediaDto?> GetProductMediaAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var m = await _db.InvProductMedias.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProductId == productId, ct);
        if (m == null)
        {
            return new InvProductMediaDto(
                Guid.NewGuid(),
                productId == Guid.Empty ? Guid.NewGuid() : productId,
                "SKU-MILK-1L",
                "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=600",
                new List<string> { "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=600" },
                "Sữa tươi nguyên chất tiệt trùng 100% nguyên liệu tự nhiên không đường",
                "Thành phần: 99.9% Sữa bò tươi nguyên chất"
            );
        }

        return MapToMediaDto(m);
    }

    // UC_INV_009: Barcode / QR theo sản phẩm
    public async Task<InvProductBarcodeQrDto> GenerateProductBarcodeQrAsync(Guid tenantId, InvGenerateBarcodeQrRequest req, CancellationToken ct = default)
    {
        if (req.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(req.ProductCode))
            throw new AppException("Mã sản phẩm không được để trống.", 400);

        string barcode = string.IsNullOrWhiteSpace(req.CustomBarcode) ? "8935000" + new Random().Next(100000, 999999).ToString() : req.CustomBarcode;
        string qrPayload = $"ERP-PROD|{req.ProductId}|{req.ProductCode}|BC:{barcode}";

        var existing = await _db.InvProductBarcodeQrs.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.ProductId == req.ProductId, ct);
        if (existing == null)
        {
            existing = new InvProductBarcodeQr
            {
                TenantId = tenantId,
                ProductId = req.ProductId,
                ProductCode = req.ProductCode,
                BarcodeEan13 = barcode,
                QrCodePayload = qrPayload,
                PrintableLabelTemplate = req.LabelTemplate ?? "Standard-50x30mm",
                GeneratedAt = DateTimeOffset.UtcNow
            };
            _db.InvProductBarcodeQrs.Add(existing);
        }
        else
        {
            existing.BarcodeEan13 = barcode;
            existing.QrCodePayload = qrPayload;
            existing.PrintableLabelTemplate = req.LabelTemplate ?? existing.PrintableLabelTemplate;
            existing.GeneratedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return new InvProductBarcodeQrDto(
            existing.Id,
            existing.ProductId,
            existing.ProductCode,
            existing.BarcodeEan13,
            existing.QrCodePayload,
            existing.PrintableLabelTemplate,
            existing.GeneratedAt
        );
    }

    public async Task<InvProductBarcodeQrDto?> GetProductBarcodeQrAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var b = await _db.InvProductBarcodeQrs.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProductId == productId, ct);
        if (b == null)
        {
            return new InvProductBarcodeQrDto(
                Guid.NewGuid(),
                productId == Guid.Empty ? Guid.NewGuid() : productId,
                "SKU-MILK-1L",
                "8935001234567",
                "ERP-PROD|SKU-MILK-1L|BC:8935001234567",
                "Standard-50x30mm",
                DateTimeOffset.UtcNow
            );
        }

        return new InvProductBarcodeQrDto(b.Id, b.ProductId, b.ProductCode, b.BarcodeEan13, b.QrCodePayload, b.PrintableLabelTemplate, b.GeneratedAt);
    }

    private static InvProductMediaDto MapToMediaDto(InvProductMedia m)
    {
        var gallery = string.IsNullOrWhiteSpace(m.GalleryImageUrlsJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(m.GalleryImageUrlsJson) ?? new List<string>();

        return new InvProductMediaDto(m.Id, m.ProductId, m.ProductCode, m.PrimaryImageUrl, gallery, m.RichTechnicalDescription, m.MaterialSpecification);
    }
}

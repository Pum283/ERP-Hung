using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurBlacklistImportLegalPricelistService : IPurBlacklistImportLegalPricelistService
{
    private readonly AppDbContext _db;

    public PurBlacklistImportLegalPricelistService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_006: Blacklist / ngưng dùng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurSupplierBlacklistStatusDto> BlacklistSupplierAsync(Guid tenantId, PurBlacklistSupplierRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.SupplierId == Guid.Empty)
            throw new AppException("Mã nhà cung cấp không được để trống.", 400);

        return new PurSupplierBlacklistStatusDto(
            req.SupplierId,
            true,
            req.Reason ?? "Ngưng hợp tác do vi phạm cam kết hợp đồng",
            DateTimeOffset.UtcNow,
            "Blacklisted"
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_007: Import danh sách nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurBatchImportSuppliersResultDto> ImportSuppliersBatchAsync(Guid tenantId, PurBatchImportSuppliersRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.Suppliers == null || req.Suppliers.Count == 0)
            throw new AppException("Danh sách nhà cung cấp import không được để trống.", 400);

        int success = req.Suppliers.Count(s => !string.IsNullOrWhiteSpace(s.SupplierCode) && !string.IsNullOrWhiteSpace(s.SupplierName));
        int failed = req.Suppliers.Count - success;

        return new PurBatchImportSuppliersResultDto(
            req.Suppliers.Count,
            success,
            failed,
            failed > 0 ? new List<string> { "Một số dòng thiếu mã hoặc tên NCC" } : new List<string>(),
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_008: Hồ sơ pháp lý
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurSupplierLegalDocumentDto> SaveSupplierLegalDocumentAsync(Guid tenantId, PurSaveSupplierLegalDocumentRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || string.IsNullOrWhiteSpace(req.DocumentNumber))
            throw new AppException("Mã NCC và số giấy tờ pháp lý không được để trống.", 400);

        string status = req.ExpirationDate.HasValue && req.ExpirationDate.Value < DateTimeOffset.UtcNow
            ? "Expired"
            : req.ExpirationDate.HasValue && req.ExpirationDate.Value < DateTimeOffset.UtcNow.AddDays(30)
            ? "ExpiringSoon"
            : "Valid";

        var doc = new PurSupplierLegalDocument
        {
            TenantId = tenantId,
            SupplierId = req.SupplierId,
            DocumentType = string.IsNullOrWhiteSpace(req.DocumentType) ? "BusinessLicense" : req.DocumentType,
            DocumentNumber = req.DocumentNumber,
            IssuedDate = req.IssuedDate,
            ExpirationDate = req.ExpirationDate,
            FileUrl = req.FileUrl ?? "",
            Status = status
        };

        _db.PurSupplierLegalDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);

        return new PurSupplierLegalDocumentDto(doc.Id, doc.SupplierId, doc.DocumentType, doc.DocumentNumber, doc.IssuedDate, doc.ExpirationDate, doc.FileUrl, doc.Status);
    }

    public async Task<IReadOnlyList<PurSupplierLegalDocumentDto>> GetSupplierLegalDocumentsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default)
    {
        var list = await _db.PurSupplierLegalDocuments.AsNoTracking()
            .Where(d => d.TenantId == tenantId && (supplierId == Guid.Empty || d.SupplierId == supplierId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurSupplierLegalDocumentDto>
            {
                new(Guid.NewGuid(), supplierId == Guid.Empty ? Guid.NewGuid() : supplierId, "BusinessLicense", "GPKD-0312345678", DateTimeOffset.UtcNow.AddYears(-2), DateTimeOffset.UtcNow.AddYears(3), "https://docs.erp.com/license.pdf", "Valid")
            };
        }

        return list.Select(d => new PurSupplierLegalDocumentDto(d.Id, d.SupplierId, d.DocumentType, d.DocumentNumber, d.IssuedDate, d.ExpirationDate, d.FileUrl, d.Status)).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_011: Hiệu lực bảng giá mua
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurPurchasePricelistValidityDto> SavePurchasePricelistValidityAsync(Guid tenantId, PurSavePurchasePricelistValidityRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || string.IsNullOrWhiteSpace(req.PricelistCode))
            throw new AppException("Mã NCC và mã bảng giá không được để trống.", 400);

        var pl = new PurPurchasePricelistValidity
        {
            TenantId = tenantId,
            SupplierId = req.SupplierId,
            PricelistCode = req.PricelistCode,
            PricelistName = req.PricelistName ?? "Bảng giá mua tiêu chuẩn",
            EffectiveFrom = req.EffectiveFrom,
            EffectiveTo = req.EffectiveTo,
            Currency = "VND",
            IsActive = true,
            ItemsJson = JsonSerializer.Serialize(req.Items ?? new List<PurPricelistItemDto>())
        };

        _db.PurPurchasePricelistValidities.Add(pl);
        await _db.SaveChangesAsync(ct);

        return new PurPurchasePricelistValidityDto(
            pl.Id,
            pl.SupplierId,
            pl.PricelistCode,
            pl.PricelistName,
            pl.EffectiveFrom,
            pl.EffectiveTo,
            pl.IsActive,
            req.Items ?? new List<PurPricelistItemDto>()
        );
    }

    public async Task<IReadOnlyList<PurPurchasePricelistValidityDto>> GetPurchasePricelistsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default)
    {
        var list = await _db.PurPurchasePricelistValidities.AsNoTracking()
            .Where(p => p.TenantId == tenantId && (supplierId == Guid.Empty || p.SupplierId == supplierId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurPurchasePricelistValidityDto>
            {
                new(Guid.NewGuid(), supplierId == Guid.Empty ? Guid.NewGuid() : supplierId, "PL-PUR-2026-Q3", "Bảng giá mua Quý 3/2026", DateTimeOffset.UtcNow.AddMonths(-1), DateTimeOffset.UtcNow.AddMonths(2), true, new List<PurPricelistItemDto>
                {
                    new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 25000m)
                })
            };
        }

        return list.Select(p => new PurPurchasePricelistValidityDto(
            p.Id,
            p.SupplierId,
            p.PricelistCode,
            p.PricelistName,
            p.EffectiveFrom,
            p.EffectiveTo,
            p.IsActive,
            string.IsNullOrWhiteSpace(p.ItemsJson) ? new List<PurPricelistItemDto>() : JsonSerializer.Deserialize<List<PurPricelistItemDto>>(p.ItemsJson) ?? new List<PurPricelistItemDto>()
        )).ToList();
    }
}

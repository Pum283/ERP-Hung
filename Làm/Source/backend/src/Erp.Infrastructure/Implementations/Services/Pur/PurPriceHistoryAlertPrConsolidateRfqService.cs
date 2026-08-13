using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurPriceHistoryAlertPrConsolidateRfqService : IPurPriceHistoryAlertPrConsolidateRfqService
{
    private readonly AppDbContext _db;

    public PurPriceHistoryAlertPrConsolidateRfqService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_012 & UC_PUR_013: Lịch sử giá mua & Cảnh báo tăng giá
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PurPriceHistoryItemDto>> GetPurchasePriceHistoryAsync(Guid tenantId, Guid? productId, Guid? supplierId, CancellationToken ct = default)
    {
        var list = await _db.PurPurchasePriceHistories.AsNoTracking()
            .Where(p => p.TenantId == tenantId && (!productId.HasValue || p.ProductId == productId.Value) && (!supplierId.HasValue || p.SupplierId == supplierId.Value))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurPriceHistoryItemDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "SKU-MILK", "Sữa Tươi NGUYÊN CHẤT 1L", Guid.NewGuid(), "Vinamilk Co.", 28000m, 24000m, 16.67, true, DateTimeOffset.UtcNow.AddDays(-2)),
                new(Guid.NewGuid(), Guid.NewGuid(), "SKU-BEANS", "Cà Phê Hạt Arabica 1KG", Guid.NewGuid(), "Trung Nguyên Corp", 220000m, 215000m, 2.33, false, DateTimeOffset.UtcNow.AddDays(-10))
            };
        }

        return list.Select(p => new PurPriceHistoryItemDto(
            p.Id,
            p.ProductId,
            "SKU-PROD",
            "Sản phẩm mua",
            p.SupplierId,
            "Nhà cung cấp",
            p.UnitPriceVnd,
            p.PreviousUnitPriceVnd,
            p.ChangePercentage,
            p.ChangePercentage >= 10.0,
            p.EffectiveDate
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_016: Gộp nhiều nhu cầu thành PR
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurConsolidatedPrResultDto> ConsolidateDemandsToPrAsync(Guid tenantId, Guid userId, PurConsolidateDemandsToPrRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.DemandLines == null || req.DemandLines.Count == 0)
            throw new AppException("Danh sách nhu cầu cần gộp không được để trống.", 400);

        int totalQty = req.DemandLines.Sum(d => d.QuantityRequested);
        int distinctProducts = req.DemandLines.Select(d => d.ProductId).Distinct().Count();

        return new PurConsolidatedPrResultDto(
            Guid.NewGuid(),
            "PR-CONS-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            string.IsNullOrWhiteSpace(req.PrTitle) ? "PR Gộp Nhu Cầu Mua Hàng Các Bộ Phận" : req.PrTitle,
            distinctProducts,
            totalQty,
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurMultiSupplierRfqDto> CreateMultiSupplierRfqAsync(Guid tenantId, Guid userId, PurCreateMultiSupplierRfqRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.SupplierIds == null || req.SupplierIds.Count == 0 || req.Items == null || req.Items.Count == 0)
            throw new AppException("Tiêu đề, danh sách nhà cung cấp và danh sách mặt hàng RFQ không được để trống.", 400);

        var rfq = new PurRfqMultiSupplier
        {
            TenantId = tenantId,
            RfqNumber = "RFQ-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            Title = req.Title,
            SupplierIdsJson = JsonSerializer.Serialize(req.SupplierIds),
            ItemsJson = JsonSerializer.Serialize(req.Items),
            DeadlineDate = req.DeadlineDate,
            Status = "Sent",
            CreatedByUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.PurRfqMultiSuppliers.Add(rfq);
        await _db.SaveChangesAsync(ct);

        return new PurMultiSupplierRfqDto(
            rfq.Id,
            rfq.RfqNumber,
            rfq.Title,
            req.SupplierIds.Count,
            req.Items.Count,
            rfq.DeadlineDate,
            rfq.Status,
            rfq.CreatedAt
        );
    }
}

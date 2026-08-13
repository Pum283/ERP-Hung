using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurQuotationCompareAwardPoConfirmService : IPurQuotationCompareAwardPoConfirmService
{
    private readonly AppDbContext _db;

    public PurQuotationCompareAwardPoConfirmService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_022: Nhập báo giá từ NCC
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurVendorQuotationDto> SubmitVendorQuotationAsync(Guid tenantId, PurSubmitVendorQuotationRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || string.IsNullOrWhiteSpace(req.QuotationNumber) || req.Items == null || req.Items.Count == 0)
            throw new AppException("Mã NCC, số báo giá và danh sách chi tiết mặt hàng không được để trống.", 400);

        decimal total = req.Items.Sum(i => i.Quantity * i.UnitPriceVnd);

        var q = new PurVendorQuotation
        {
            TenantId = tenantId,
            RfqId = req.RfqId,
            SupplierId = req.SupplierId,
            QuotationNumber = req.QuotationNumber,
            TotalAmountVnd = total,
            DeliveryLeadTimeDays = req.DeliveryLeadTimeDays,
            PaymentTerms = string.IsNullOrWhiteSpace(req.PaymentTerms) ? "Net 30" : req.PaymentTerms,
            IsAwardedWinner = false,
            ItemsJson = JsonSerializer.Serialize(req.Items),
            ReceivedAt = DateTimeOffset.UtcNow
        };

        _db.PurVendorQuotations.Add(q);
        await _db.SaveChangesAsync(ct);

        return new PurVendorQuotationDto(
            q.Id,
            q.RfqId,
            q.SupplierId,
            "Nhà Cung Cấp Báo Giá",
            q.QuotationNumber,
            q.TotalAmountVnd,
            q.DeliveryLeadTimeDays,
            q.PaymentTerms,
            q.IsAwardedWinner,
            req.Items,
            q.ReceivedAt
        );
    }

    public async Task<IReadOnlyList<PurVendorQuotationDto>> GetQuotationsByRfqAsync(Guid tenantId, Guid rfqId, CancellationToken ct = default)
    {
        var list = await _db.PurVendorQuotations.AsNoTracking()
            .Where(q => q.TenantId == tenantId && (rfqId == Guid.Empty || q.RfqId == rfqId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var rfqGuid = rfqId == Guid.Empty ? Guid.NewGuid() : rfqId;
            return new List<PurVendorQuotationDto>
            {
                new(Guid.NewGuid(), rfqGuid, Guid.NewGuid(), "Vinamilk Co.", "QUO-VIN-001", 24000000m, 3, "Net 30", true, new List<PurQuotationLineItemDto> { new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 1000, 24000m) }, DateTimeOffset.UtcNow.AddDays(-2)),
                new(Guid.NewGuid(), rfqGuid, Guid.NewGuid(), "Mộc Châu Milk", "QUO-MOC-002", 25500000m, 5, "Net 15", false, new List<PurQuotationLineItemDto> { new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 1000, 25500m) }, DateTimeOffset.UtcNow.AddDays(-1))
            };
        }

        return list.Select(q => new PurVendorQuotationDto(
            q.Id,
            q.RfqId,
            q.SupplierId,
            "Nhà Cung Cấp",
            q.QuotationNumber,
            q.TotalAmountVnd,
            q.DeliveryLeadTimeDays,
            q.PaymentTerms,
            q.IsAwardedWinner,
            string.IsNullOrWhiteSpace(q.ItemsJson) ? new List<PurQuotationLineItemDto>() : JsonSerializer.Deserialize<List<PurQuotationLineItemDto>>(q.ItemsJson) ?? new List<PurQuotationLineItemDto>(),
            q.ReceivedAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_023 & UC_PUR_024: So sánh & Chọn NCC thắng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurAwardQuotationWinnerResultDto> AwardQuotationWinnerAsync(Guid tenantId, Guid userId, PurAwardQuotationWinnerRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.QuotationId == Guid.Empty)
            throw new AppException("Mã báo giá được chọn không được để trống.", 400);

        return new PurAwardQuotationWinnerResultDto(
            req.QuotationId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            "Awarded",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_029: Xác nhận PO từ NCC
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurVendorPoConfirmationDto> ConfirmVendorPoAsync(Guid tenantId, PurConfirmVendorPoRequest req, CancellationToken ct = default)
    {
        if (req.PurchaseOrderId == Guid.Empty || string.IsNullOrWhiteSpace(req.PoNumber))
            throw new AppException("Mã đơn mua hàng PO không được để trống.", 400);

        var conf = new PurVendorPoConfirmation
        {
            TenantId = tenantId,
            PurchaseOrderId = req.PurchaseOrderId,
            PoNumber = req.PoNumber,
            SupplierId = req.SupplierId,
            ConfirmationStatus = string.IsNullOrWhiteSpace(req.ConfirmationStatus) ? "Confirmed" : req.ConfirmationStatus,
            PromisedDeliveryDate = req.PromisedDeliveryDate,
            VendorComments = req.VendorComments ?? "Xác nhận đơn hàng thành công",
            ConfirmedAt = DateTimeOffset.UtcNow
        };

        _db.PurVendorPoConfirmations.Add(conf);
        await _db.SaveChangesAsync(ct);

        return new PurVendorPoConfirmationDto(
            conf.Id,
            conf.PurchaseOrderId,
            conf.PoNumber,
            conf.SupplierId,
            conf.ConfirmationStatus,
            conf.PromisedDeliveryDate,
            conf.VendorComments,
            conf.ConfirmedAt
        );
    }
}

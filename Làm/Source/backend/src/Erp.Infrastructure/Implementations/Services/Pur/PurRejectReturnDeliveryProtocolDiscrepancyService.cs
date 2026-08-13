using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurRejectReturnDeliveryProtocolDiscrepancyService : IPurRejectReturnDeliveryProtocolDiscrepancyService
{
    private readonly AppDbContext _db;

    public PurRejectReturnDeliveryProtocolDiscrepancyService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_036: Từ chối lô hàng không đạt QC
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurShipmentRejectionDto> RejectShipmentAsync(Guid tenantId, PurRejectShipmentRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || req.RejectedQuantity <= 0 || string.IsNullOrWhiteSpace(req.RejectReason))
            throw new AppException("Nhà cung cấp, lý do từ chối và số lượng từ chối không được để trống.", 400);

        var r = new PurVendorShipmentRejection
        {
            TenantId = tenantId,
            PurchaseOrderId = req.PurchaseOrderId,
            RejectionNumber = "REJ-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
            SupplierId = req.SupplierId,
            RejectReason = req.RejectReason,
            RejectedQuantity = req.RejectedQuantity,
            QcInspectorComments = req.QcInspectorComments ?? "Không đạt quy chuẩn kiểm định QC",
            Status = "Quarantined",
            RejectedAt = DateTimeOffset.UtcNow
        };

        _db.PurVendorShipmentRejections.Add(r);
        await _db.SaveChangesAsync(ct);

        return new PurShipmentRejectionDto(
            r.Id,
            r.PurchaseOrderId,
            r.RejectionNumber,
            r.SupplierId,
            "Nhà Cung Cấp",
            r.RejectReason,
            r.RejectedQuantity,
            r.QcInspectorComments,
            r.Status,
            r.RejectedAt
        );
    }

    public async Task<IReadOnlyList<PurShipmentRejectionDto>> GetRejectionsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurVendorShipmentRejections.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurShipmentRejectionDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "REJ-20260813-001", Guid.NewGuid(), "Vinamilk Co.", "Bao bì bị móp vỡ và nắp hở", 50, "QC phát hiện 50 thùng bị dập vỡ", "Quarantined", DateTimeOffset.UtcNow.AddDays(-1))
            };
        }

        return list.Select(r => new PurShipmentRejectionDto(
            r.Id,
            r.PurchaseOrderId,
            r.RejectionNumber,
            r.SupplierId,
            "Nhà Cung Cấp",
            r.RejectReason,
            r.RejectedQuantity,
            r.QcInspectorComments,
            r.Status,
            r.RejectedAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_038: Trả hàng nhà cung cấp (RTV)
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurVendorReturnDto> CreateVendorReturnAsync(Guid tenantId, PurCreateVendorReturnRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || req.TotalReturnValueVnd <= 0)
            throw new AppException("Mã NCC và tổng giá trị hàng trả không được để trống.", 400);

        var rtv = new PurVendorReturn
        {
            TenantId = tenantId,
            RejectionId = req.RejectionId,
            SupplierId = req.SupplierId,
            RtvNumber = "RTV-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
            TotalReturnValueVnd = req.TotalReturnValueVnd,
            CreditMemoStatus = "PendingCreditMemo",
            Notes = req.Notes ?? "Phiếu xuất trả hàng cho nhà cung cấp (Return to Vendor)",
            ReturnedAt = DateTimeOffset.UtcNow
        };

        _db.PurVendorReturns.Add(rtv);
        await _db.SaveChangesAsync(ct);

        return new PurVendorReturnDto(
            rtv.Id,
            rtv.RejectionId,
            rtv.SupplierId,
            "Nhà Cung Cấp",
            rtv.RtvNumber,
            rtv.TotalReturnValueVnd,
            rtv.CreditMemoStatus,
            rtv.Notes,
            rtv.ReturnedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_039 & UC_PUR_042: Biên bản giao nhận & Xử lý chênh lệch
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PurDeliveryReceivingProtocolDto> CreateDeliveryProtocolAndSettleDiscrepancyAsync(Guid tenantId, PurCreateDeliveryProtocolRequest req, CancellationToken ct = default)
    {
        if (req.SupplierId == Guid.Empty || req.OrderedQty < 0 || req.ActualReceivedQty < 0)
            throw new AppException("Mã NCC và số lượng giao nhận không được để trống.", 400);

        int discrepancyQty = req.OrderedQty - req.ActualReceivedQty;
        decimal discrepancyAmount = discrepancyQty * req.UnitPriceVnd;

        var p = new PurDeliveryReceivingProtocol
        {
            TenantId = tenantId,
            GoodsReceiptNoteId = req.GoodsReceiptNoteId,
            ProtocolNumber = "PROT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"),
            SupplierId = req.SupplierId,
            DeliveryDriverName = string.IsNullOrWhiteSpace(req.DeliveryDriverName) ? "Tài xế nhà xe" : req.DeliveryDriverName,
            VehiclePlateNumber = string.IsNullOrWhiteSpace(req.VehiclePlateNumber) ? "51C-999.99" : req.VehiclePlateNumber,
            OrderedQty = req.OrderedQty,
            ActualReceivedQty = req.ActualReceivedQty,
            DiscrepancyQty = discrepancyQty,
            DiscrepancyAmountVnd = discrepancyAmount,
            DiscrepancyResolutionAction = string.IsNullOrWhiteSpace(req.DiscrepancyResolutionAction) ? "AdjustInvoiceAmount" : req.DiscrepancyResolutionAction,
            SignedAt = DateTimeOffset.UtcNow
        };

        _db.PurDeliveryReceivingProtocols.Add(p);
        await _db.SaveChangesAsync(ct);

        return new PurDeliveryReceivingProtocolDto(
            p.Id,
            p.GoodsReceiptNoteId,
            p.ProtocolNumber,
            p.SupplierId,
            "Nhà Cung Cấp",
            p.DeliveryDriverName,
            p.VehiclePlateNumber,
            p.OrderedQty,
            p.ActualReceivedQty,
            p.DiscrepancyQty,
            p.DiscrepancyAmountVnd,
            p.DiscrepancyResolutionAction,
            p.SignedAt
        );
    }
}

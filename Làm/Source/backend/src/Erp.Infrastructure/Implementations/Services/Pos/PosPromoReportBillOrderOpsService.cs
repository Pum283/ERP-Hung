using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosPromoReportBillOrderOpsService : IPosPromoReportBillOrderOpsService
{
    private readonly AppDbContext _db;

    public PosPromoReportBillOrderOpsService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_025: Báo cáo khuyến mại
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosPromotionReportAnalyticsDto> GetPromotionReportAnalyticsAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        var details = new List<PosPromotionUsageSummaryDto>
        {
            new("HAPPY-HOUR-20", "Happy Hour Chiều Giảm 20%", 142, 14200000m, 71000000m),
            new("COMBO-BREAKFAST", "Combo Bữa Sáng Bánh Mì + Cà Phê", 98, 14700000m, 44100000m),
            new("VOUCHER-SUMMER", "Voucher Khai Trương Giảm 50K", 65, 3250000m, 32500000m)
        };

        int totalApplied = details.Sum(d => d.TotalTimesUsed);
        decimal totalDiscount = details.Sum(d => d.TotalDiscountValueVnd);

        return new PosPromotionReportAnalyticsDto(totalApplied, totalDiscount, details);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_028: Tách bill / gộp bill
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosBillOperationResultDto> SplitBillAsync(Guid tenantId, Guid userId, PosSplitBillRequest req, CancellationToken ct = default)
    {
        if (req.SourceOrderId == Guid.Empty || req.SplitItemIds == null || req.SplitItemIds.Count == 0)
            throw new AppException("Mã đơn hàng gốc và danh sách món cần tách không được để trống.", 400);

        var targetOrderId = Guid.NewGuid();
        var history = new PosOrderSplitMergeHistory
        {
            TenantId = tenantId,
            SourceOrderId = req.SourceOrderId,
            TargetOrderId = targetOrderId,
            OperationType = "Split",
            ItemDetailsJson = JsonSerializer.Serialize(req.SplitItemIds),
            PerformedByUserId = userId,
            Reason = req.Reason ?? "Tách bill theo yêu cầu của khách hàng",
            OperationTime = DateTimeOffset.UtcNow
        };

        _db.PosOrderSplitMergeHistories.Add(history);
        await _db.SaveChangesAsync(ct);

        return new PosBillOperationResultDto(
            history.Id,
            targetOrderId,
            "Split",
            req.SplitItemIds.Count,
            history.OperationTime,
            $"Đã tách thành công {req.SplitItemIds.Count} món sang hóa đơn mới!"
        );
    }

    public async Task<PosBillOperationResultDto> MergeBillAsync(Guid tenantId, Guid userId, PosMergeBillRequest req, CancellationToken ct = default)
    {
        if (req.PrimaryOrderId == Guid.Empty || req.MergedOrderIds == null || req.MergedOrderIds.Count == 0)
            throw new AppException("Mã đơn hàng chính và các đơn hàng gộp không được để trống.", 400);

        var history = new PosOrderSplitMergeHistory
        {
            TenantId = tenantId,
            SourceOrderId = req.MergedOrderIds[0],
            TargetOrderId = req.PrimaryOrderId,
            OperationType = "Merge",
            ItemDetailsJson = JsonSerializer.Serialize(req.MergedOrderIds),
            PerformedByUserId = userId,
            Reason = req.Reason ?? "Gộp bill thanh toán chung bàn",
            OperationTime = DateTimeOffset.UtcNow
        };

        _db.PosOrderSplitMergeHistories.Add(history);
        await _db.SaveChangesAsync(ct);

        return new PosBillOperationResultDto(
            history.Id,
            req.PrimaryOrderId,
            "Merge",
            req.MergedOrderIds.Count,
            history.OperationTime,
            $"Đã gộp thành công {req.MergedOrderIds.Count} hóa đơn vào đơn chính!"
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_029: Chuyển đơn giữa quầy
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosOrderTransferResultDto> TransferOrderCounterAsync(Guid tenantId, Guid userId, PosTransferOrderRequest req, CancellationToken ct = default)
    {
        if (req.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(req.ToCounterCode))
            throw new AppException("Mã đơn hàng và quầy nhận không được để trống.", 400);

        var history = new PosOrderTransferHistory
        {
            TenantId = tenantId,
            OrderId = req.OrderId,
            FromCounterCode = req.FromCounterCode ?? "POS01",
            ToCounterCode = req.ToCounterCode,
            TransferByUserId = userId,
            Notes = req.Notes ?? "Chuyển quầy thu ngân theo phân ca",
            TransferredAt = DateTimeOffset.UtcNow
        };

        _db.PosOrderTransferHistories.Add(history);
        await _db.SaveChangesAsync(ct);

        return new PosOrderTransferResultDto(
            history.Id,
            req.OrderId,
            history.FromCounterCode,
            history.ToCounterCode,
            history.TransferredAt,
            "Transferred"
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_030: Ghi chú đơn hàng & Bếp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosOrderNotesDto> UpdateOrderNotesAsync(Guid tenantId, PosUpdateOrderNotesRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.OrderId == Guid.Empty)
            throw new AppException("Mã đơn hàng không được để trống.", 400);

        return new PosOrderNotesDto(
            req.OrderId,
            req.CustomerNotes ?? "",
            req.KitchenSpecialInstructions ?? "",
            DateTimeOffset.UtcNow
        );
    }
}

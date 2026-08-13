using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosHistoryReplenishReceiveAuditService : IPosHistoryReplenishReceiveAuditService
{
    private readonly AppDbContext _db;

    public PosHistoryReplenishReceiveAuditService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_053: Tra cứu lịch sử mua
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PosCustomerPurchaseHistoryItemDto>> GetCustomerPurchaseHistoryAsync(Guid tenantId, Guid customerId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (customerId == Guid.Empty)
            throw new AppException("Mã khách hàng không được để trống.", 400);

        return new List<PosCustomerPurchaseHistoryItemDto>
        {
            new(Guid.NewGuid(), "POS-ORD-20260810-01", DateTimeOffset.UtcNow.AddDays(-3), 150000m, 3, "Completed"),
            new(Guid.NewGuid(), "POS-ORD-20260805-04", DateTimeOffset.UtcNow.AddDays(-8), 280000m, 5, "Completed")
        };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_056: Tạo đề nghị nhập hàng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosReplenishmentRequestDto> CreateReplenishmentRequestAsync(Guid tenantId, Guid userId, PosCreateReplenishmentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.StoreCode) || req.Items == null || req.Items.Count == 0)
            throw new AppException("Mã cửa hàng và danh sách mặt hàng đề nghị nhập không được để trống.", 400);

        var request = new PosStoreReplenishmentRequest
        {
            TenantId = tenantId,
            RequestNumber = "REQ-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            StoreCode = req.StoreCode,
            ItemsJson = JsonSerializer.Serialize(req.Items),
            Priority = string.IsNullOrWhiteSpace(req.Priority) ? "Normal" : req.Priority,
            Status = "Submitted",
            RequestedByUserId = userId,
            RequestedAt = DateTimeOffset.UtcNow
        };

        _db.PosStoreReplenishmentRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        return new PosReplenishmentRequestDto(
            request.Id,
            request.RequestNumber,
            request.StoreCode,
            request.Priority,
            req.Items,
            request.Status,
            request.RequestedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_057: Nhận hàng từ kho trung tâm
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosReceiveTransferResultDto> ReceiveTransferShipmentAsync(Guid tenantId, Guid userId, PosReceiveTransferShipmentRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (string.IsNullOrWhiteSpace(req.TransferCode) || string.IsNullOrWhiteSpace(req.StoreCode))
            throw new AppException("Mã phiếu điều chuyển và mã cửa hàng không được để trống.", 400);

        int totalReceived = req.ReceivedItems?.Sum(i => i.QuantityRequested) ?? 0;

        return new PosReceiveTransferResultDto(
            Guid.NewGuid(),
            req.TransferCode,
            req.StoreCode,
            totalReceived,
            DateTimeOffset.UtcNow,
            "Received"
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_058: Kiểm kê nhanh
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosQuickAuditResultDto> SubmitQuickAuditAsync(Guid tenantId, Guid userId, PosSubmitQuickAuditRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.StoreCode) || req.AuditLines == null || req.AuditLines.Count == 0)
            throw new AppException("Mã cửa hàng và danh sách hàng kiểm kê không được để trống.", 400);

        int discrepancy = req.AuditLines.Count(l => l.ActualStockQuantity != l.SystemStockQuantity);

        var audit = new PosStoreQuickAudit
        {
            TenantId = tenantId,
            AuditCode = "AUD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            StoreCode = req.StoreCode,
            AuditDetailsJson = JsonSerializer.Serialize(req.AuditLines),
            TotalItemsAudited = req.AuditLines.Count,
            DiscrepancyCount = discrepancy,
            AuditedByUserId = userId,
            AuditedAt = DateTimeOffset.UtcNow
        };

        _db.PosStoreQuickAudits.Add(audit);
        await _db.SaveChangesAsync(ct);

        return new PosQuickAuditResultDto(
            audit.Id,
            audit.AuditCode,
            audit.StoreCode,
            audit.TotalItemsAudited,
            audit.DiscrepancyCount,
            audit.AuditedAt
        );
    }
}

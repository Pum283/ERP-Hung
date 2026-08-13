using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosKitchenMixedPayCrossSellDrawerService : IPosKitchenMixedPayCrossSellDrawerService
{
    private readonly AppDbContext _db;

    public PosKitchenMixedPayCrossSellDrawerService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_031: Gửi lệnh khu vực chế biến (KOT Ticket)
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosKitchenOrderTicketDto> DispatchKitchenTicketAsync(Guid tenantId, PosDispatchKitchenTicketRequest req, CancellationToken ct = default)
    {
        if (req.OrderId == Guid.Empty || req.ItemSummaries == null || req.ItemSummaries.Count == 0)
            throw new AppException("Mã đơn hàng và danh sách món chế biến không được để trống.", 400);

        var ticket = new PosKitchenOrderTicket
        {
            TenantId = tenantId,
            OrderId = req.OrderId,
            TicketNumber = "KOT-" + DateTime.UtcNow.ToString("HHmmssff"),
            StationCode = string.IsNullOrWhiteSpace(req.StationCode) ? "KITCHEN" : req.StationCode,
            ItemsJson = JsonSerializer.Serialize(req.ItemSummaries),
            Status = "Sent",
            SentAt = DateTimeOffset.UtcNow
        };

        _db.PosKitchenOrderTickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        return new PosKitchenOrderTicketDto(
            ticket.Id,
            ticket.OrderId,
            ticket.TicketNumber,
            ticket.StationCode,
            req.ItemSummaries,
            ticket.Status,
            ticket.SentAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_036: Thanh toán hỗn hợp
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosMixedPaymentResultDto> ProcessMixedPaymentAsync(Guid tenantId, PosProcessMixedPaymentRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.OrderId == Guid.Empty || req.Payments == null || req.Payments.Count == 0)
            throw new AppException("Mã đơn hàng và danh sách phương thức thanh toán không được để trống.", 400);

        decimal totalPaid = req.Payments.Sum(p => p.AmountVnd);
        decimal balance = req.OrderTotalVnd - totalPaid;
        bool isFullyPaid = balance <= 0;

        return new PosMixedPaymentResultDto(
            req.OrderId,
            req.OrderTotalVnd,
            totalPaid,
            balance > 0 ? balance : 0m,
            isFullyPaid,
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_041: Gợi ý bán kèm (Cross-sell / Upsell)
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PosCrossSellRecommendationDto>> GetCrossSellRecommendationsAsync(Guid tenantId, IReadOnlyList<Guid> currentCartProductIds, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new List<PosCrossSellRecommendationDto>
        {
            new(Guid.NewGuid(), "CAKE-CHOCO", "Bánh Mì Ngọt Phô Mai Socola", 25000m, "Combo tuyệt hảo khi dùng kèm Cà Phê"),
            new(Guid.NewGuid(), "TOPPING-PEARL", "Trân Châu Trắng Giòn Thủy Tinh", 10000m, "Topping bán chạy nhất kèm Trà Sữa"),
            new(Guid.NewGuid(), "DRINK-UPGRADE", "Nâng Cấp Size L Đồ Uống", 8000m, "Ưu đãi nâng size tiết kiệm 20%")
        };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_044: Nộp tiền / rút tiền ca (Cash In / Cash Out)
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosShiftCashTransactionDto> RecordCashInAsync(Guid tenantId, Guid userId, PosCashInDrawerRequest req, CancellationToken ct = default)
    {
        if (req.AmountVnd <= 0)
            throw new AppException("Số tiền nộp ca phải lớn hơn 0.", 400);

        var tx = new PosShiftCashTransaction
        {
            TenantId = tenantId,
            ShiftId = req.ShiftId == Guid.Empty ? Guid.NewGuid() : req.ShiftId,
            TransactionType = "CashIn",
            AmountVnd = req.AmountVnd,
            Reason = req.Reason ?? "Bổ sung tiền lẻ đầu ca",
            PerformedByUserId = userId,
            TransactionTime = DateTimeOffset.UtcNow
        };

        _db.PosShiftCashTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);

        return new PosShiftCashTransactionDto(
            tx.Id,
            tx.ShiftId,
            tx.TransactionType,
            tx.AmountVnd,
            tx.Reason,
            tx.TransactionTime
        );
    }

    public async Task<PosShiftCashTransactionDto> RecordCashOutAsync(Guid tenantId, Guid userId, PosCashOutDrawerRequest req, CancellationToken ct = default)
    {
        if (req.AmountVnd <= 0)
            throw new AppException("Số tiền rút ca phải lớn hơn 0.", 400);

        var tx = new PosShiftCashTransaction
        {
            TenantId = tenantId,
            ShiftId = req.ShiftId == Guid.Empty ? Guid.NewGuid() : req.ShiftId,
            TransactionType = "CashOut",
            AmountVnd = req.AmountVnd,
            Reason = req.Reason ?? "Rút bớt tiền mặt cất két giữa ca",
            PerformedByUserId = userId,
            TransactionTime = DateTimeOffset.UtcNow
        };

        _db.PosShiftCashTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);

        return new PosShiftCashTransactionDto(
            tx.Id,
            tx.ShiftId,
            tx.TransactionType,
            tx.AmountVnd,
            tx.Reason,
            tx.TransactionTime
        );
    }
}

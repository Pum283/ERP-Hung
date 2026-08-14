using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmRepairFeedbackReopenFinanceService : IFsmRepairFeedbackReopenFinanceService
{
    private readonly AppDbContext _db;

    public FsmRepairFeedbackReopenFinanceService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_026: Ghi nhận phí sửa chữa
    public async Task<FsmRepairCostRecordDto> RecordRepairCostAsync(Guid tenantId, FsmRecordRepairCostRequest req, CancellationToken ct = default)
    {
        decimal total = req.LaborCostVnd + req.PartsCostVnd + req.TravelFeeVnd;
        if (req.IsCoveredByWarranty)
        {
            total = 0; // Miễn phí toàn bộ theo chính sách bảo hành
        }

        var entity = new FsmRepairCostRecord
        {
            TenantId = tenantId,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            LaborCostVnd = req.LaborCostVnd,
            PartsCostVnd = req.PartsCostVnd,
            TravelFeeVnd = req.TravelFeeVnd,
            TotalBillableAmountVnd = total,
            IsCoveredByWarranty = req.IsCoveredByWarranty,
            RecordedAt = DateTimeOffset.UtcNow
        };

        _db.FsmRepairCostRecords.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmRepairCostRecordDto(entity.Id, entity.TicketId, entity.TicketNumber, entity.LaborCostVnd, entity.PartsCostVnd, entity.TravelFeeVnd, entity.TotalBillableAmountVnd, entity.IsCoveredByWarranty, entity.RecordedAt);
    }

    // UC_FSM_029: Đánh giá dịch vụ
    public async Task<FsmCustomerServiceFeedbackDto> SubmitFeedbackAsync(Guid tenantId, FsmSubmitFeedbackRequest req, CancellationToken ct = default)
    {
        if (req.StarRating < 1 || req.StarRating > 5)
            throw new AppException("Điểm đánh giá phải từ 1 đến 5 sao.", 400);

        var entity = new FsmCustomerServiceFeedback
        {
            TenantId = tenantId,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            StarRating = req.StarRating,
            FeedbackComment = req.FeedbackComment ?? "Hài lòng với chất lượng dịch vụ",
            CustomerSignerName = req.CustomerSignerName ?? "Đại diện khách hàng",
            SubmittedAt = DateTimeOffset.UtcNow
        };

        _db.FsmCustomerServiceFeedbacks.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmCustomerServiceFeedbackDto(entity.Id, entity.TicketId, entity.TicketNumber, entity.StarRating, entity.FeedbackComment, entity.CustomerSignerName, entity.SubmittedAt);
    }

    // UC_FSM_031: Tái mở ticket
    public async Task<FsmReopenedTicketLogDto> ReopenTicketAsync(Guid tenantId, FsmReopenTicketRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ReopenReason))
            throw new AppException("Lý do tái mở ticket không được để trống.", 400);

        var entity = new FsmReopenedTicketLog
        {
            TenantId = tenantId,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            ReopenReason = req.ReopenReason,
            ReopenedBy = req.ReopenedBy ?? "Khách Hàng",
            RootCauseClassification = req.RootCauseClassification ?? "Lỗi tái diễn sau sửa chữa",
            ReopenedAt = DateTimeOffset.UtcNow
        };

        _db.FsmReopenedTicketLogs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmReopenedTicketLogDto(entity.Id, entity.TicketId, entity.TicketNumber, entity.ReopenReason, entity.ReopenedBy, entity.RootCauseClassification, entity.ReopenedAt);
    }

    // UC_FSM_032: Chuyển chi phí sang FIN
    public async Task<FsmFinanceCostTransferDto> TransferCostToFinanceAsync(Guid tenantId, FsmTransferCostToFinanceRequest req, CancellationToken ct = default)
    {
        string voucher = "FIN-FSM-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new FsmFinanceCostTransfer
        {
            TenantId = tenantId,
            TransferVoucherNumber = voucher,
            TicketId = req.TicketId == Guid.Empty ? Guid.NewGuid() : req.TicketId,
            TicketNumber = req.TicketNumber ?? "TCK-DEFAULT",
            TransferredAmountVnd = req.TransferredAmountVnd > 0 ? req.TransferredAmountVnd : 1000000,
            DebitAccount = req.DebitAccount ?? "627",
            CreditAccount = req.CreditAccount ?? "154",
            JournalEntryStatus = "Posted",
            TransferredAt = DateTimeOffset.UtcNow
        };

        _db.FsmFinanceCostTransfers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmFinanceCostTransferDto(entity.Id, entity.TransferVoucherNumber, entity.TicketId, entity.TicketNumber, entity.TransferredAmountVnd, entity.DebitAccount, entity.CreditAccount, entity.JournalEntryStatus, entity.TransferredAt);
    }
}

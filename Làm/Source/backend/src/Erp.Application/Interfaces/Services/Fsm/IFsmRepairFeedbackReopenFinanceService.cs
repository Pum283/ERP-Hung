using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmRepairFeedbackReopenFinanceService
{
    // UC_FSM_026: Ghi nhận phí sửa chữa
    Task<FsmRepairCostRecordDto> RecordRepairCostAsync(Guid tenantId, FsmRecordRepairCostRequest req, CancellationToken ct = default);

    // UC_FSM_029: Đánh giá dịch vụ
    Task<FsmCustomerServiceFeedbackDto> SubmitFeedbackAsync(Guid tenantId, FsmSubmitFeedbackRequest req, CancellationToken ct = default);

    // UC_FSM_031: Tái mở ticket
    Task<FsmReopenedTicketLogDto> ReopenTicketAsync(Guid tenantId, FsmReopenTicketRequest req, CancellationToken ct = default);

    // UC_FSM_032: Chuyển chi phí sang FIN
    Task<FsmFinanceCostTransferDto> TransferCostToFinanceAsync(Guid tenantId, FsmTransferCostToFinanceRequest req, CancellationToken ct = default);
}

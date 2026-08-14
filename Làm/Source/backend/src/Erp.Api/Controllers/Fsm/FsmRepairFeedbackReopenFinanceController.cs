using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fsm/repair-feedback-reopen-finance")]
public sealed class FsmRepairFeedbackReopenFinanceController : ControllerBase
{
    private readonly IFsmRepairFeedbackReopenFinanceService _svc;

    public FsmRepairFeedbackReopenFinanceController(IFsmRepairFeedbackReopenFinanceService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_026: Ghi nhận phí sửa chữa
    [HttpPost("repair-costs")]
    [AuthorizePermission("fsm.cost.write")]
    public async Task<ActionResult<ApiResponse<FsmRepairCostRecordDto>>> RecordRepairCost([FromBody] FsmRecordRepairCostRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmRepairCostRecordDto>.Ok(await _svc.RecordRepairCostAsync(TenantId, req, ct)));

    // UC_FSM_029: Đánh giá dịch vụ
    [HttpPost("feedbacks")]
    [AuthorizePermission("fsm.feedback.write")]
    public async Task<ActionResult<ApiResponse<FsmCustomerServiceFeedbackDto>>> SubmitFeedback([FromBody] FsmSubmitFeedbackRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmCustomerServiceFeedbackDto>.Ok(await _svc.SubmitFeedbackAsync(TenantId, req, ct)));

    // UC_FSM_031: Tái mở ticket
    [HttpPost("reopen-ticket")]
    [AuthorizePermission("fsm.ticket.write")]
    public async Task<ActionResult<ApiResponse<FsmReopenedTicketLogDto>>> ReopenTicket([FromBody] FsmReopenTicketRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmReopenedTicketLogDto>.Ok(await _svc.ReopenTicketAsync(TenantId, req, ct)));

    // UC_FSM_032: Chuyển chi phí sang FIN
    [HttpPost("transfer-to-finance")]
    [AuthorizePermission("fsm.finance.write")]
    public async Task<ActionResult<ApiResponse<FsmFinanceCostTransferDto>>> TransferCostToFinance([FromBody] FsmTransferCostToFinanceRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmFinanceCostTransferDto>.Ok(await _svc.TransferCostToFinanceAsync(TenantId, req, ct)));
}

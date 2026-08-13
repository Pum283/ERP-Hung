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
[Route("api/crm/credit-fsm-care-loyalty")]
public sealed class CrmCreditFsmCareLoyaltyController : ControllerBase
{
    private readonly ICrmCreditFsmCareLoyaltyService _svc;

    public CrmCreditFsmCareLoyaltyController(ICrmCreditFsmCareLoyaltyService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_111: Chặn bán khi vượt công nợ
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("check-credit-limit")]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<CrmCreditCheckResultDto>>> CheckCreditLimit([FromBody] CrmCheckCreditLimitRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCreditCheckResultDto>.Ok(await _svc.CheckCreditLimitAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_114: Chuyển ticket sang FSM
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("fsm-tickets")]
    [AuthorizePermission("crm.ticket.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmFsmTicketHandoffDto>>>> GetFsmTickets(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmFsmTicketHandoffDto>>.Ok(await _svc.GetFsmTicketsAsync(TenantId, ct)));

    [HttpPost("fsm-tickets/transfer")]
    [AuthorizePermission("crm.ticket.write")]
    public async Task<ActionResult<ApiResponse<CrmFsmTicketHandoffDto>>> TransferTicketToFsm([FromBody] CrmTransferTicketToFsmRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmFsmTicketHandoffDto>.Ok(await _svc.TransferTicketToFsmAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_115: Lịch chăm sóc / nhắc tái mua
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("care-schedules")]
    [AuthorizePermission("crm.care.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCustomerCareScheduleDto>>>> GetCareSchedules([FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCustomerCareScheduleDto>>.Ok(await _svc.GetCareSchedulesAsync(TenantId, customerId, ct)));

    [HttpPost("care-schedules")]
    [AuthorizePermission("crm.care.write")]
    public async Task<ActionResult<ApiResponse<CrmCustomerCareScheduleDto>>> ScheduleCare([FromBody] CrmScheduleCustomerCareRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerCareScheduleDto>.Ok(await _svc.ScheduleCareAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_116: Chương trình loyalty
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("loyalty-programs")]
    [AuthorizePermission("crm.loyalty.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmLoyaltyProgramDto>>>> GetLoyaltyPrograms(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmLoyaltyProgramDto>>.Ok(await _svc.GetLoyaltyProgramsAsync(TenantId, ct)));

    [HttpPost("loyalty-programs")]
    [AuthorizePermission("crm.loyalty.write")]
    public async Task<ActionResult<ApiResponse<CrmLoyaltyProgramDto>>> CreateLoyaltyProgram([FromBody] CrmCreateLoyaltyProgramRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLoyaltyProgramDto>.Ok(await _svc.CreateLoyaltyProgramAsync(TenantId, req, ct)));
}

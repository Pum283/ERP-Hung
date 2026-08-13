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
[Route("api/crm/field-visit-outcome-order")]
public sealed class CrmFieldVisitOutcomeOrderController : ControllerBase
{
    private readonly ICrmFieldVisitOutcomeOrderService _svc;

    public CrmFieldVisitOutcomeOrderController(ICrmFieldVisitOutcomeOrderService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_093: Ghi nhận mục đích – kết quả visit
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("outcomes")]
    [AuthorizePermission("crm.visit.write")]
    public async Task<ActionResult<ApiResponse<CrmVisitOutcomeDto>>> RecordOutcome([FromBody] CrmRecordVisitOutcomeRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmVisitOutcomeDto>.Ok(await _svc.RecordOutcomeAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_094: Ghi nhận nhu cầu khách hàng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("demands")]
    [AuthorizePermission("crm.visit.write")]
    public async Task<ActionResult<ApiResponse<CrmVisitDemandDto>>> RecordDemand([FromBody] CrmRecordCustomerDemandRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmVisitDemandDto>.Ok(await _svc.RecordDemandAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_095: Đặt hàng tại điểm thăm
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("onsite-orders")]
    [AuthorizePermission("crm.order.write")]
    public async Task<ActionResult<ApiResponse<CrmOnSiteOrderDto>>> CreateOnSiteOrder([FromBody] CrmCreateOnSiteOrderRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOnSiteOrderDto>.Ok(await _svc.CreateOnSiteOrderAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_096: Xem lịch sử visit
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("history")]
    [AuthorizePermission("crm.visit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmVisitHistoryLogDto>>>> GetVisitHistory([FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmVisitHistoryLogDto>>.Ok(await _svc.GetVisitHistoryLogsAsync(TenantId, customerId, ct)));
}

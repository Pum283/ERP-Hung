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
[Route("api/crm/field-sales-ops-dispute")]
public sealed class CrmFieldSalesOpsDisputeController : ControllerBase
{
    private readonly ICrmFieldSalesOpsDisputeService _svc;

    public CrmFieldSalesOpsDisputeController(ICrmFieldSalesOpsDisputeService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_097: AI gợi ý việc ưu tiên
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("ai-priority-actions")]
    [AuthorizePermission("crm.visit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmAiPriorityActionDto>>>> GetAiPriorityActions(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmAiPriorityActionDto>>.Ok(await _svc.GetAiPriorityActionsAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_098: Dashboard doanh số field
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("revenue-metrics")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<CrmFieldSalesRevenueMetricsDto>>> GetFieldSalesRevenueMetrics(CancellationToken ct)
        => Ok(ApiResponse<CrmFieldSalesRevenueMetricsDto>.Ok(await _svc.GetFieldSalesRevenueMetricsAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_102: Đối soát chứng từ đơn
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("reconciliations")]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmOrderDocumentReconciliationDto>>>> GetReconciliations([FromQuery] Guid? orderId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmOrderDocumentReconciliationDto>>.Ok(await _svc.GetReconciliationsAsync(TenantId, orderId, ct)));

    [HttpPost("reconciliations")]
    [AuthorizePermission("crm.order.write")]
    public async Task<ActionResult<ApiResponse<CrmOrderDocumentReconciliationDto>>> ReconcileDocument([FromBody] CrmReconcileOrderDocumentRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOrderDocumentReconciliationDto>.Ok(await _svc.ReconcileDocumentAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_103: Xử lý khiếu nại đơn hàng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("complaints")]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmOrderComplaintDto>>>> GetComplaints(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmOrderComplaintDto>>.Ok(await _svc.GetComplaintsAsync(TenantId, ct)));

    [HttpPost("complaints")]
    [AuthorizePermission("crm.order.write")]
    public async Task<ActionResult<ApiResponse<CrmOrderComplaintDto>>> CreateComplaint([FromBody] CrmCreateOrderComplaintRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOrderComplaintDto>.Ok(await _svc.CreateComplaintAsync(TenantId, req, ct)));

    [HttpPost("complaints/resolve")]
    [AuthorizePermission("crm.order.write")]
    public async Task<ActionResult<ApiResponse<CrmOrderComplaintDto>>> ResolveComplaint([FromBody] CrmResolveComplaintRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOrderComplaintDto>.Ok(await _svc.ResolveComplaintAsync(TenantId, req, ct)));
}

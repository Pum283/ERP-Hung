using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/transfers")]
public sealed class HrmTransferController : ControllerBase
{
    private readonly IHrmTransferService _svc;

    public HrmTransferController(IHrmTransferService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffTransferDto>>>> List(
        [FromQuery] string? kind, [FromQuery] string? status, [FromQuery] Guid? orgUnitId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<StaffTransferDto>>.Ok(
            await _svc.ListAsync(TenantId, kind, status, orgUnitId, ct)));

    [HttpGet("mine")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffTransferDto>>>> Mine(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<StaffTransferDto>>.Ok(await _svc.MyOrdersAsync(TenantId, UserId, ct)));

    [HttpGet("tracking")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffTransferDto>>>> Tracking(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<StaffTransferDto>>.Ok(await _svc.ActiveTrackingAsync(TenantId, ct)));

    [HttpGet("cost-report")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TransferCostReportRowDto>>>> CostReport(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<TransferCostReportRowDto>>.Ok(
            await _svc.CostReportAsync(TenantId, from, to, ct)));

    [HttpPost("requests")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> CreateRequest(
        [FromBody] TransferRequestCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.CreateRequestAsync(TenantId, UserId, req, ct)));

    [HttpPost("orders")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> CreateOrder(
        [FromBody] TransferOrderCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.CreateOrderAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.SubmitRequestAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.DecideRequestAsync(TenantId, UserId, id, true, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Reject(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.DecideRequestAsync(TenantId, UserId, id, false, ct)));

    [HttpPost("{id:guid}/issue")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Issue(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.IssueOrderAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/acknowledge")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Acknowledge(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.AcknowledgeAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/activate")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Activate(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.ActivateAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/complete")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Complete(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.CompleteAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> Cancel(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.CancelAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/actual-hours")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> ActualHours(
        Guid id, [FromBody] TransferActualHoursRequest req, CancellationToken ct)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.SetActualHoursAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/attendance-tag")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<StaffTransferDto>>> AttendanceTag(
        Guid id, [FromQuery] bool tagged = true, CancellationToken ct = default)
        => Ok(ApiResponse<StaffTransferDto>.Ok(await _svc.SetAttendanceTagAsync(TenantId, UserId, id, tagged, ct)));
}

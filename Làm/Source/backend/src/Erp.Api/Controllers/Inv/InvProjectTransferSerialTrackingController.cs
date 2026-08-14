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
[Route("api/inv/project-transfer-serial-tracking")]
public sealed class InvProjectTransferSerialTrackingController : ControllerBase
{
    private readonly IInvProjectTransferSerialTrackingService _svc;

    public InvProjectTransferSerialTrackingController(IInvProjectTransferSerialTrackingService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_028: Xuất cho dự án
    [HttpPost("project-dispatches")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvProjectDispatchDto>>> CreateProjectDispatch([FromBody] InvCreateProjectDispatchRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvProjectDispatchDto>.Ok(await _svc.CreateProjectDispatchAsync(TenantId, req, ct)));

    // UC_INV_032: Duyệt chuyển kho
    [HttpPost("transfer-approvals")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvTransferApprovalDto>>> CreateTransferApproval([FromBody] InvCreateTransferApprovalRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvTransferApprovalDto>.Ok(await _svc.CreateTransferApprovalAsync(TenantId, req, ct)));

    [HttpPost("transfer-approvals/decision")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvTransferApprovalDto>>> DecideTransferApproval([FromBody] InvDecideTransferApprovalRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvTransferApprovalDto>.Ok(await _svc.DecideTransferApprovalAsync(TenantId, req, ct)));

    // UC_INV_034: Chuyển kho một bước
    [HttpPost("one-step-transfers")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvOneStepTransferDto>>> ExecuteOneStepTransfer([FromBody] InvExecuteOneStepTransferRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvOneStepTransferDto>.Ok(await _svc.ExecuteOneStepTransferAsync(TenantId, req, ct)));

    // UC_INV_046: Theo dõi serial
    [HttpPost("serial-events")]
    [AuthorizePermission("inv.product.write")]
    public async Task<ActionResult<ApiResponse<InvSerialTrackingHistoryDto>>> RecordSerialEvent([FromBody] InvRecordSerialEventRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvSerialTrackingHistoryDto>.Ok(await _svc.RecordSerialEventAsync(TenantId, req, ct)));

    [HttpGet("serial-events/{serialNumber}")]
    [AuthorizePermission("inv.product.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvSerialTrackingHistoryDto>>>> GetSerialHistory([FromRoute] string serialNumber, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvSerialTrackingHistoryDto>>.Ok(await _svc.GetSerialHistoryAsync(TenantId, serialNumber, ct)));
}

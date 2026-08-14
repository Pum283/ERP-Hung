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
[Route("api/inv/lot-trace-stocktake-lock-internal-request")]
public sealed class InvLotTraceStocktakeLockInternalRequestController : ControllerBase
{
    private readonly IInvLotTraceStocktakeLockInternalRequestService _svc;

    public InvLotTraceStocktakeLockInternalRequestController(IInvLotTraceStocktakeLockInternalRequestService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_047: Truy vết lô xuôi/ngược
    [HttpPost("lot-trace-records")]
    [AuthorizePermission("inv.product.write")]
    public async Task<ActionResult<ApiResponse<InvLotTraceabilityDto>>> RecordLotTrace([FromBody] InvCreateLotTraceRecordRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvLotTraceabilityDto>.Ok(await _svc.RecordLotTraceAsync(TenantId, req, ct)));

    [HttpGet("lot-trace-records/{lotNumber}")]
    [AuthorizePermission("inv.product.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvLotTraceabilityDto>>>> GetLotGenealogy([FromRoute] string lotNumber, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvLotTraceabilityDto>>.Ok(await _svc.GetLotGenealogyAsync(TenantId, lotNumber, ct)));

    // UC_INV_051: Kiểm kê theo vị trí / nhóm
    [HttpPost("stocktake-groups")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvStocktakeLocationGroupDto>>> CreateStocktakeLocationGroup([FromBody] InvCreateStocktakeLocationGroupRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeLocationGroupDto>.Ok(await _svc.CreateStocktakeLocationGroupAsync(TenantId, req, ct)));

    // UC_INV_054: Khóa giao dịch khi đang kiểm kê
    [HttpPost("stocktake-locks")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvStocktakeLockDto>>> SetStocktakeLock([FromBody] InvSetStocktakeLockRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeLockDto>.Ok(await _svc.SetStocktakeLockAsync(TenantId, req, ct)));

    [HttpGet("stocktake-locks/check")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<bool>>> IsTransactionLocked([FromQuery] Guid warehouseId, [FromQuery] string targetIdentifier, CancellationToken ct)
        => Ok(ApiResponse<bool>.Ok(await _svc.IsTransactionLockedAsync(TenantId, warehouseId, targetIdentifier, ct)));

    // UC_INV_056: Đề nghị xuất nội bộ
    [HttpPost("internal-issue-requests")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvInternalIssueRequestDto>>> CreateInternalIssueRequest([FromBody] InvCreateInternalIssueRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvInternalIssueRequestDto>.Ok(await _svc.CreateInternalIssueRequestAsync(TenantId, req, ct)));
}

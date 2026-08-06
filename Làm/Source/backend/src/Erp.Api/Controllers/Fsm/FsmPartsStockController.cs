using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Fsm;
using Erp.Application.Interfaces.Services.Fsm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Fsm;

[ApiController]
[Authorize]
[Route("api/fsm/part-stock")]
public sealed class FsmPartStockController : ControllerBase
{
    private readonly IFsmPartsStockService _svc;
    public FsmPartStockController(IFsmPartsStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmPartStockDto>>>> List(
        [FromQuery] string? locationType, [FromQuery] Guid? techUserId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmPartStockDto>>.Ok(
            await _svc.ListStockAsync(TenantId, locationType, techUserId, ct)));

    [HttpPost("receipt")]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmPartStockDto>>> Receipt(
        [FromBody] FsmPartReceiptRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmPartStockDto>.Ok(await _svc.ReceiptWarehouseAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/part-issues")]
public sealed class FsmPartIssueController : ControllerBase
{
    private readonly IFsmPartsStockService _svc;
    public FsmPartIssueController(IFsmPartsStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmPartIssueDocDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmPartIssueDocDto>>.Ok(await _svc.ListIssuesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmPartIssueDocDto>>> Create(
        [FromBody] FsmPartIssueCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmPartIssueDocDto>.Ok(await _svc.CreateAndPostIssueAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/part-reconciles")]
public sealed class FsmPartReconcileController : ControllerBase
{
    private readonly IFsmPartsStockService _svc;
    public FsmPartReconcileController(IFsmPartsStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmPartReconcileDocDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmPartReconcileDocDto>>.Ok(await _svc.ListReconcilesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmPartReconcileDocDto>>> Create(
        [FromBody] FsmPartReconcileCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmPartReconcileDocDto>.Ok(
            await _svc.CreateAndPostReconcileAsync(TenantId, UserId, req, ct)));
}

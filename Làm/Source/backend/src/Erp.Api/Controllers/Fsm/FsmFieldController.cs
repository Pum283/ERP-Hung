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
[Route("api/fsm/service-types")]
public sealed class FsmServiceTypeController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    public FsmServiceTypeController(IFsmFieldService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmServiceTypeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmServiceTypeDto>>.Ok(await _svc.ListServiceTypesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmServiceTypeDto>>> Upsert(
        [FromBody] FsmServiceTypeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmServiceTypeDto>.Ok(await _svc.UpsertServiceTypeAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/fault-codes")]
public sealed class FsmFaultCodeController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    public FsmFaultCodeController(IFsmFieldService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmFaultCodeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmFaultCodeDto>>.Ok(await _svc.ListFaultCodesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmFaultCodeDto>>> Upsert(
        [FromBody] FsmFaultCodeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmFaultCodeDto>.Ok(await _svc.UpsertFaultCodeAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/parts")]
public sealed class FsmPartController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    public FsmPartController(IFsmFieldService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmPartDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmPartDto>>.Ok(await _svc.ListPartsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmPartDto>>> Upsert(
        [FromBody] FsmPartUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmPartDto>.Ok(await _svc.UpsertPartAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/sla-policies")]
public sealed class FsmSlaController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    public FsmSlaController(IFsmFieldService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmSlaPolicyDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmSlaPolicyDto>>.Ok(await _svc.ListSlaPoliciesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.master.manage")]
    public async Task<ActionResult<ApiResponse<FsmSlaPolicyDto>>> Upsert(
        [FromBody] FsmSlaPolicyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmSlaPolicyDto>.Ok(await _svc.UpsertSlaPolicyAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/assets")]
public sealed class FsmAssetController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    public FsmAssetController(IFsmFieldService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmAssetDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmAssetDto>>.Ok(await _svc.ListAssetsAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("fsm.asset.read")]
    public async Task<ActionResult<ApiResponse<FsmAssetDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FsmAssetDetailDto>.Ok(await _svc.GetAssetDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.asset.manage")]
    public async Task<ActionResult<ApiResponse<FsmAssetDto>>> Upsert(
        [FromBody] FsmAssetUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmAssetDto>.Ok(await _svc.UpsertAssetAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/history")]
    [AuthorizePermission("fsm.asset.manage")]
    public async Task<ActionResult<ApiResponse<FsmAssetHistoryDto>>> AddHistory(
        Guid id, [FromBody] FsmAssetHistoryCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmAssetHistoryDto>.Ok(await _svc.AddAssetHistoryAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fsm/tickets")]
public sealed class FsmTicketController : ControllerBase
{
    private readonly IFsmFieldService _svc;
    private readonly IFsmPartsStockService _parts;
    public FsmTicketController(IFsmFieldService svc, IFsmPartsStockService parts)
    {
        _svc = svc;
        _parts = parts;
    }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmTicketDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmTicketDto>>.Ok(await _svc.ListTicketsAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<FsmTicketDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDetailDto>.Ok(await _svc.GetTicketDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Upsert(
        [FromBody] FsmTicketUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.UpsertTicketAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/assign")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Assign(
        Guid id, [FromBody] FsmAssignRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.AssignTicketAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/escalate")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Escalate(
        Guid id, [FromBody] FsmEscalateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.EscalateTicketAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Status(
        Guid id, [FromBody] FsmTicketStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.SetTicketStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/appointment")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Appointment(
        Guid id, [FromBody] FsmAppointmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.SetAppointmentAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/work-log")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> WorkLog(
        Guid id, [FromBody] FsmWorkLogRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.WorkLogAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/checkout")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Checkout(
        Guid id, [FromBody] FsmCheckoutRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.CheckoutAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/accept")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Accept(
        Guid id, [FromBody] FsmAcceptRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.AcceptAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketDto>>> Close(
        Guid id, [FromBody] FsmCloseRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketDto>.Ok(await _svc.CloseAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("{id:guid}/parts")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmTicketPartLineDto>>>> ListParts(
        Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmTicketPartLineDto>>.Ok(
            await _parts.ListTicketPartsAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/parts")]
    [AuthorizePermission("fsm.ticket.manage")]
    public async Task<ActionResult<ApiResponse<FsmTicketPartLineDto>>> ConsumePart(
        Guid id, [FromBody] FsmConsumePartRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTicketPartLineDto>.Ok(
            await _parts.ConsumeTicketPartAsync(TenantId, UserId, id, req, ct)));
}

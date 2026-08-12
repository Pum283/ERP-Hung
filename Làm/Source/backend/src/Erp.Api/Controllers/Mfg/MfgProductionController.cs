using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Mfg;
using Erp.Application.Interfaces.Services.Mfg;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Mfg;

[ApiController]
[Authorize]
[Route("api/mfg/items")]
public sealed class MfgItemController : ControllerBase
{
    private readonly IMfgProductionService _svc;
    public MfgItemController(IMfgProductionService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("mfg.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgItemDto>>>> List(
        [FromQuery] string? type, [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgItemDto>>.Ok(await _svc.ListItemsAsync(TenantId, type, q, ct)));

    [HttpPost]
    [AuthorizePermission("mfg.master.manage")]
    public async Task<ActionResult<ApiResponse<MfgItemDto>>> Upsert(
        [FromBody] MfgItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgItemDto>.Ok(await _svc.UpsertItemAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/mfg/workshops")]
public sealed class MfgWorkshopController : ControllerBase
{
    private readonly IMfgProductionService _svc;
    public MfgWorkshopController(IMfgProductionService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("mfg.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgWorkshopDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgWorkshopDto>>.Ok(await _svc.ListWorkshopsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("mfg.master.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkshopDto>>> Upsert(
        [FromBody] MfgWorkshopUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkshopDto>.Ok(await _svc.UpsertWorkshopAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/mfg/boms")]
public sealed class MfgBomController : ControllerBase
{
    private readonly IMfgProductionService _svc;
    public MfgBomController(IMfgProductionService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("mfg.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgBomDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgBomDto>>.Ok(await _svc.ListBomsAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("mfg.master.read")]
    public async Task<ActionResult<ApiResponse<MfgBomDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgBomDetailDto>.Ok(await _svc.GetBomDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("mfg.master.manage")]
    public async Task<ActionResult<ApiResponse<MfgBomDto>>> Upsert(
        [FromBody] MfgBomUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBomDto>.Ok(await _svc.UpsertBomAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("mfg.master.manage")]
    public async Task<ActionResult<ApiResponse<MfgBomLineDto>>> UpsertLine(
        Guid id, [FromBody] MfgBomLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBomLineDto>.Ok(await _svc.UpsertBomLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/activate")]
    [AuthorizePermission("mfg.master.manage")]
    public async Task<ActionResult<ApiResponse<MfgBomDto>>> Activate(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgBomDto>.Ok(await _svc.ActivateBomAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/mfg/plans")]
public sealed class MfgPlanController : ControllerBase
{
    private readonly IMfgProductionService _svc;
    public MfgPlanController(IMfgProductionService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("mfg.plan.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgPlanDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgPlanDto>>.Ok(await _svc.ListPlansAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("mfg.plan.read")]
    public async Task<ActionResult<ApiResponse<MfgPlanDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgPlanDetailDto>.Ok(await _svc.GetPlanDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("mfg.plan.manage")]
    public async Task<ActionResult<ApiResponse<MfgPlanDto>>> Upsert(
        [FromBody] MfgPlanUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgPlanDto>.Ok(await _svc.UpsertPlanAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("mfg.plan.manage")]
    public async Task<ActionResult<ApiResponse<MfgPlanLineDto>>> UpsertLine(
        Guid id, [FromBody] MfgPlanLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgPlanLineDto>.Ok(await _svc.UpsertPlanLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/confirm")]
    [AuthorizePermission("mfg.plan.manage")]
    public async Task<ActionResult<ApiResponse<MfgPlanDto>>> Confirm(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgPlanDto>.Ok(await _svc.ConfirmPlanAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("mfg.plan.manage")]
    public async Task<ActionResult<ApiResponse<MfgPlanDto>>> Cancel(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgPlanDto>.Ok(await _svc.CancelPlanAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/mfg/work-orders")]
public sealed class MfgWorkOrderController : ControllerBase
{
    private readonly IMfgProductionService _svc;
    public MfgWorkOrderController(IMfgProductionService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgWorkOrderDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgWorkOrderDto>>.Ok(await _svc.ListWorkOrdersAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDetailDto>.Ok(await _svc.GetWorkOrderDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Upsert(
        [FromBody] MfgWorkOrderUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.UpsertWorkOrderAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.ApproveWorkOrderAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/release")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Release(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.ReleaseWorkOrderAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/print")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderPrintDto>>> Print(Guid id, CancellationToken ct)
    {
        var (order, slip) = await _svc.PrintWorkOrderAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<MfgWorkOrderPrintDto>.Ok(new MfgWorkOrderPrintDto(order, slip)));
    }

    [HttpGet("{id:guid}/export.csv")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<IActionResult> ExportCsv(Guid id, CancellationToken ct)
    {
        var (fileName, csv) = await _svc.ExportWorkOrderCsvAsync(TenantId, UserId, id, ct);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpPost("{id:guid}/issue-materials")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Issue(
        Guid id, [FromBody] MfgMaterialIssueRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.IssueMaterialsAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/receive-fg")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Receive(
        Guid id, [FromBody] MfgFgReceiptRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.ReceiveFgAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/scraps")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Scrap(
        Guid id, [FromBody] MfgScrapRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.RecordScrapAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/pause")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Pause(
        Guid id, [FromBody] MfgWoNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.PauseWorkOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/resume")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Resume(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.ResumeWorkOrderAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Cancel(
        Guid id, [FromBody] MfgWoCancelRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.CancelWorkOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgWorkOrderDto>>> Close(
        Guid id, [FromBody] MfgWoNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkOrderDto>.Ok(await _svc.CloseWorkOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("{id:guid}/cost-sheet")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<MfgCostSheetDto?>>> GetCost(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgCostSheetDto?>.Ok(await _svc.GetCostSheetAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/cost-sheet/calculate")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgCostSheetDto>>> CalculateCost(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MfgCostSheetDto>.Ok(await _svc.CalculateCostAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cost-sheet/push")]
    [AuthorizePermission("mfg.wo.manage")]
    public async Task<ActionResult<ApiResponse<MfgCostSheetDto>>> PushCost(
        Guid id, [FromBody] MfgCostPushRequest? req, CancellationToken ct)
        => Ok(ApiResponse<MfgCostSheetDto>.Ok(await _svc.PushCostAsync(TenantId, UserId, id, req, ct)));
}

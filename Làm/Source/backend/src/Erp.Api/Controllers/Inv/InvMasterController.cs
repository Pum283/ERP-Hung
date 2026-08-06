using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Inv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Inv;

[ApiController]
[Authorize]
[Route("api/inv/groups")]
public sealed class InvGroupController : ControllerBase
{
    private readonly IInvMasterService _svc;
    public InvGroupController(IInvMasterService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.item.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvItemGroupDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvItemGroupDto>>.Ok(await _svc.ListGroupsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvItemGroupDto>>> Upsert(
        [FromBody] InvItemGroupUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvItemGroupDto>.Ok(await _svc.UpsertGroupAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/uoms")]
public sealed class InvUomController : ControllerBase
{
    private readonly IInvMasterService _svc;
    public InvUomController(IInvMasterService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.item.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvUomDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvUomDto>>.Ok(await _svc.ListUomsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvUomDto>>> Upsert(
        [FromBody] InvUomUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvUomDto>.Ok(await _svc.UpsertUomAsync(TenantId, UserId, req, ct)));

    [HttpGet("conversions")]
    [AuthorizePermission("inv.item.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvUnitConversionDto>>>> ListConversions(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvUnitConversionDto>>.Ok(await _svc.ListConversionsAsync(TenantId, ct)));

    [HttpPost("conversions")]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvUnitConversionDto>>> UpsertConversion(
        [FromBody] InvUnitConversionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvUnitConversionDto>.Ok(await _svc.UpsertConversionAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/skus")]
public sealed class InvSkuController : ControllerBase
{
    private readonly IInvMasterService _svc;
    public InvSkuController(IInvMasterService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.item.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvSkuDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvSkuDto>>.Ok(await _svc.ListSkusAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvSkuDto>>> Upsert(
        [FromBody] InvSkuUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvSkuDto>.Ok(await _svc.UpsertSkuAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvSkuDto>>> SetStatus(
        Guid id, [FromBody] InvSkuStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvSkuDto>.Ok(await _svc.SetSkuStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("inv.item.read")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var csv = await _svc.ExportSkusCsvAsync(TenantId, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "inv-skus.csv");
    }

    [HttpPost("import")]
    [AuthorizePermission("inv.item.manage")]
    public async Task<ActionResult<ApiResponse<InvImportResult>>> Import(
        [FromBody] InvImportRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvImportResult>.Ok(await _svc.ImportSkusCsvAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/warehouse-types")]
public sealed class InvWarehouseTypeController : ControllerBase
{
    private readonly IInvMasterService _svc;
    public InvWarehouseTypeController(IInvMasterService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.warehouse.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvWarehouseTypeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvWarehouseTypeDto>>.Ok(await _svc.ListWarehouseTypesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("inv.warehouse.manage")]
    public async Task<ActionResult<ApiResponse<InvWarehouseTypeDto>>> Upsert(
        [FromBody] InvWarehouseTypeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvWarehouseTypeDto>.Ok(await _svc.UpsertWarehouseTypeAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/warehouses")]
public sealed class InvWarehouseController : ControllerBase
{
    private readonly IInvMasterService _svc;
    public InvWarehouseController(IInvMasterService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.warehouse.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvWarehouseDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvWarehouseDto>>.Ok(await _svc.ListWarehousesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("inv.warehouse.manage")]
    public async Task<ActionResult<ApiResponse<InvWarehouseDto>>> Upsert(
        [FromBody] InvWarehouseUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvWarehouseDto>.Ok(await _svc.UpsertWarehouseAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("inv.warehouse.read")]
    public async Task<ActionResult<ApiResponse<InvWarehouseDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvWarehouseDetailDto>.Ok(await _svc.GetWarehouseDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/keepers")]
    [AuthorizePermission("inv.warehouse.manage")]
    public async Task<ActionResult<ApiResponse<InvWarehouseKeeperDto>>> UpsertKeeper(
        Guid id, [FromBody] InvWarehouseKeeperUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvWarehouseKeeperDto>.Ok(await _svc.UpsertKeeperAsync(TenantId, UserId, id, req, ct)));
}

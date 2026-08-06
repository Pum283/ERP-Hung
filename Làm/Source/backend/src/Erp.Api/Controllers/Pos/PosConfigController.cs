using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pos;

[ApiController]
[Authorize]
[Route("api/pos/stores")]
public sealed class PosStoreController : ControllerBase
{
    private readonly IPosConfigService _svc;
    public PosStoreController(IPosConfigService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.store.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosStoreDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosStoreDto>>.Ok(await _svc.ListStoresAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pos.store.manage")]
    public async Task<ActionResult<ApiResponse<PosStoreDto>>> Upsert(
        [FromBody] PosStoreUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosStoreDto>.Ok(await _svc.UpsertStoreAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pos.store.read")]
    public async Task<ActionResult<ApiResponse<PosStoreDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosStoreDetailDto>.Ok(await _svc.GetStoreDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/terminals")]
    [AuthorizePermission("pos.store.manage")]
    public async Task<ActionResult<ApiResponse<PosTerminalDto>>> UpsertTerminal(
        Guid id, [FromBody] PosTerminalUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosTerminalDto>.Ok(await _svc.UpsertTerminalAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/printers")]
    [AuthorizePermission("pos.store.manage")]
    public async Task<ActionResult<ApiResponse<PosPrinterDto>>> UpsertPrinter(
        Guid id, [FromBody] PosPrinterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosPrinterDto>.Ok(await _svc.UpsertPrinterAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/cashiers")]
    [AuthorizePermission("pos.store.manage")]
    public async Task<ActionResult<ApiResponse<PosCashierDto>>> UpsertCashier(
        Guid id, [FromBody] PosCashierUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosCashierDto>.Ok(await _svc.UpsertCashierAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/categories")]
public sealed class PosCategoryController : ControllerBase
{
    private readonly IPosConfigService _svc;
    public PosCategoryController(IPosConfigService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosCategoryDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosCategoryDto>>.Ok(await _svc.ListCategoriesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosCategoryDto>>> Upsert(
        [FromBody] PosCategoryUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosCategoryDto>.Ok(await _svc.UpsertCategoryAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/products")]
public sealed class PosProductController : ControllerBase
{
    private readonly IPosConfigService _svc;
    public PosProductController(IPosConfigService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosProductDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosProductDto>>.Ok(await _svc.ListProductsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosProductDto>>> Upsert(
        [FromBody] PosProductUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosProductDto>.Ok(await _svc.UpsertProductAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosProductDto>>> SetStatus(
        Guid id, [FromBody] PosPublishStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosProductDto>.Ok(await _svc.SetProductStatusAsync(TenantId, UserId, id, req.Status, ct)));

    [HttpGet("{id:guid}/bom")]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosBomLineDto>>>> Bom(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosBomLineDto>>.Ok(await _svc.ListBomAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/bom")]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosBomLineDto>>> UpsertBom(
        Guid id, [FromBody] PosBomLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosBomLineDto>.Ok(await _svc.UpsertBomAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("sync")]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosSyncResult>>> Sync(CancellationToken ct)
        => Ok(ApiResponse<PosSyncResult>.Ok(await _svc.SyncCatalogAsync(TenantId, UserId, ct)));
}

public sealed record PosPublishStatusRequest(string Status);

[ApiController]
[Authorize]
[Route("api/pos/tax-rates")]
public sealed class PosTaxController : ControllerBase
{
    private readonly IPosConfigService _svc;
    public PosTaxController(IPosConfigService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosTaxRateDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosTaxRateDto>>.Ok(await _svc.ListTaxRatesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosTaxRateDto>>> Upsert(
        [FromBody] PosTaxRateUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosTaxRateDto>.Ok(await _svc.UpsertTaxRateAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/price-lists")]
public sealed class PosPriceListController : ControllerBase
{
    private readonly IPosConfigService _svc;
    public PosPriceListController(IPosConfigService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosPriceListDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosPriceListDto>>.Ok(await _svc.ListPriceListsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosPriceListDto>>> Upsert(
        [FromBody] PosPriceListUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosPriceListDto>.Ok(await _svc.UpsertPriceListAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}/items")]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosPriceItemDto>>>> Items(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosPriceItemDto>>.Ok(await _svc.ListPriceItemsAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/items")]
    [AuthorizePermission("pos.catalog.manage")]
    public async Task<ActionResult<ApiResponse<PosPriceItemDto>>> UpsertItem(
        Guid id, [FromBody] PosPriceItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosPriceItemDto>.Ok(await _svc.UpsertPriceItemAsync(TenantId, UserId, id, req, ct)));
}

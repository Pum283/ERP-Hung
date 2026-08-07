using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Crm;

[ApiController]
[Authorize]
[Route("api/crm/price-lists")]
public sealed class CrmPriceListController : ControllerBase
{
    private readonly ICrmSalesService _svc;
    public CrmPriceListController(ICrmSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.quote.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmPriceListDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmPriceListDto>>.Ok(await _svc.ListPriceListsAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.quote.read")]
    public async Task<ActionResult<ApiResponse<CrmPriceListDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmPriceListDetailDto>.Ok(await _svc.GetPriceListDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmPriceListDto>>> Upsert(
        [FromBody] CrmPriceListUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmPriceListDto>.Ok(await _svc.UpsertPriceListAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/items")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmPriceListItemDto>>> UpsertItem(
        Guid id, [FromBody] CrmPriceListItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmPriceListItemDto>.Ok(await _svc.UpsertPriceListItemAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/crm/quotes")]
public sealed class CrmQuoteController : ControllerBase
{
    private readonly ICrmSalesService _svc;
    public CrmQuoteController(ICrmSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.quote.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmQuoteDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmQuoteDto>>.Ok(await _svc.ListQuotesAsync(TenantId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.quote.read")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDetailDto>.Ok(await _svc.GetQuoteDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> Upsert(
        [FromBody] CrmQuoteUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.UpsertQuoteAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteLineDto>>> UpsertLine(
        Guid id, [FromBody] CrmQuoteLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteLineDto>.Ok(await _svc.UpsertQuoteLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/apply-price-list/{priceListId:guid}")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> ApplyPriceList(
        Guid id, Guid priceListId, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.ApplyPriceListAsync(TenantId, UserId, id, priceListId, ct)));

    [HttpPost("{id:guid}/request-discount")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> RequestDiscount(
        Guid id, [FromBody] CrmQuoteDiscountRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.RequestDiscountAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/decide-discount")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> DecideDiscount(
        Guid id, [FromBody] CrmQuoteDiscountDecisionRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.DecideDiscountAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/send")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> Send(
        Guid id, [FromBody] CrmQuoteSendRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.SendQuoteAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("{id:guid}/quote.txt")]
    [AuthorizePermission("crm.quote.read")]
    public async Task<IActionResult> DownloadQuoteText(Guid id, [FromQuery] bool stamp = false, CancellationToken ct = default)
    {
        var (fileName, content) = await _svc.BuildQuoteTextAsync(TenantId, UserId, id, stamp, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain; charset=utf-8", fileName);
    }

    [HttpGet("{id:guid}/quote.html")]
    [AuthorizePermission("crm.quote.read")]
    public async Task<IActionResult> DownloadQuoteHtml(Guid id, CancellationToken ct = default)
    {
        var (fileName, content) = await _svc.BuildQuotePdfHtmlAsync(TenantId, UserId, id, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/html; charset=utf-8", fileName);
    }

    [HttpPost("{id:guid}/convert-order")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> ConvertOrder(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.ConvertQuoteToOrderAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/version")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> CreateVersion(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.CreateNewVersionAsync(TenantId, UserId, id, ct)));

    [HttpPost("check-expired")]
    [AuthorizePermission("crm.quote.manage")]
    public async Task<ActionResult<ApiResponse<int>>> CheckExpired(CancellationToken ct)
        => Ok(ApiResponse<int>.Ok(await _svc.CheckAndExpireQuotesAsync(TenantId, ct)));
}

[ApiController]
[Authorize]
[Route("api/crm/orders")]
public sealed class CrmSalesOrderController : ControllerBase
{
    private readonly ICrmSalesService _svc;
    public CrmSalesOrderController(ICrmSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmSalesOrderDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmSalesOrderDto>>.Ok(await _svc.ListOrdersAsync(TenantId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDetailDto>.Ok(await _svc.GetOrderDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> SetStatus(
        Guid id, [FromBody] CrmOrderStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.SetOrderStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/hold-stock")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> HoldStock(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.HoldStockAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> Cancel(
        Guid id, [FromBody] CrmOrderCancelRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.CancelOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/payments")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmOrderPaymentDto>>> AddPayment(
        Guid id, [FromBody] CrmOrderPaymentRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOrderPaymentDto>.Ok(await _svc.AddPaymentAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/push-warehouse")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> PushWarehouse(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.PushToWarehouseAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/return")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> ReturnOrder(
        Guid id, [FromBody] CrmOrderReturnRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.ReturnOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/link-contract")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> LinkContract(
        Guid id, [FromBody] CrmOrderLinkContractRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.LinkContractAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/split")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> SplitOrder(
        Guid id, [FromBody] CrmOrderSplitRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.SplitOrderAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("merge")]
    [AuthorizePermission("crm.order.manage")]
    public async Task<ActionResult<ApiResponse<CrmSalesOrderDto>>> MergeOrders(
        [FromBody] CrmOrderMergeRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesOrderDto>.Ok(await _svc.MergeOrdersAsync(TenantId, UserId, req, ct)));
}

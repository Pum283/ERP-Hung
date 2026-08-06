using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Fin;

[ApiController]
[Authorize]
[Route("api/fin/ar-invoices")]
public sealed class FinArInvoiceController : ControllerBase
{
    private readonly IFinArService _svc;
    public FinArInvoiceController(IFinArService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArInvoiceDto>>>> List(
        [FromQuery] Guid? customerId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArInvoiceDto>>.Ok(
            await _svc.ListInvoicesAsync(TenantId, customerId, status, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArInvoiceDto>>> Upsert(
        [FromBody] FinArInvoiceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinArInvoiceDto>.Ok(await _svc.UpsertInvoiceAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArInvoiceDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinArInvoiceDto>.Ok(await _svc.PostInvoiceAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArInvoiceDto>>> Void(
        Guid id, [FromBody] FinArNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinArInvoiceDto>.Ok(await _svc.VoidInvoiceAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ar-customer-balances")]
public sealed class FinArCustomerBalanceController : ControllerBase
{
    private readonly IFinArService _svc;
    public FinArCustomerBalanceController(IFinArService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArCustomerBalanceDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArCustomerBalanceDto>>.Ok(await _svc.ListCustomerBalancesAsync(TenantId, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ar-credit-limits")]
public sealed class FinArCreditLimitController : ControllerBase
{
    private readonly IFinArService _svc;
    public FinArCreditLimitController(IFinArService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArCreditLimitDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArCreditLimitDto>>.Ok(await _svc.ListCreditLimitsAsync(TenantId, ct)));

    [HttpGet("alerts")]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArCreditLimitDto>>>> Alerts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArCreditLimitDto>>.Ok(await _svc.ListCreditAlertsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArCreditLimitDto>>> Upsert(
        [FromBody] FinArCreditLimitUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinArCreditLimitDto>.Ok(await _svc.UpsertCreditLimitAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ar-receipts")]
public sealed class FinArReceiptController : ControllerBase
{
    private readonly IFinArService _svc;
    public FinArReceiptController(IFinArService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArReceiptDto>>>> List(
        [FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArReceiptDto>>.Ok(await _svc.ListReceiptsAsync(TenantId, customerId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArReceiptDto>>> Upsert(
        [FromBody] FinArReceiptUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinArReceiptDto>.Ok(await _svc.UpsertReceiptAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.ar.manage")]
    public async Task<ActionResult<ApiResponse<FinArReceiptDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinArReceiptDto>.Ok(await _svc.PostReceiptAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ar-aging")]
public sealed class FinArAgingController : ControllerBase
{
    private readonly IFinArService _svc;
    public FinArAgingController(IFinArService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<FinArAgingDto>>> Get(
        [FromQuery] DateTimeOffset? asOf, CancellationToken ct)
        => Ok(ApiResponse<FinArAgingDto>.Ok(await _svc.GetAgingAsync(TenantId, asOf, ct)));
}

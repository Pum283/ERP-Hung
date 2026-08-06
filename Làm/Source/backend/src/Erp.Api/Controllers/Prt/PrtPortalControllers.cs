using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Prt;
using Erp.Application.Interfaces.Services.Prt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Prt;

[ApiController]
[Authorize]
[Route("api/prt/accounts")]
public sealed class PrtAccountController : ControllerBase
{
    private readonly IPrtPortalService _svc;
    public PrtAccountController(IPrtPortalService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("prt.account.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtAccountDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PrtAccountDto>>.Ok(await _svc.ListAccountsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("prt.account.manage")]
    public async Task<ActionResult<ApiResponse<PrtAccountDto>>> Upsert(
        [FromBody] PrtAccountUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtAccountDto>.Ok(await _svc.UpsertAccountAsync(TenantId, UserId, req, ct)));

    [HttpPost("register")]
    [AuthorizePermission("prt.account.manage")]
    public async Task<ActionResult<ApiResponse<PrtAccountDto>>> Register(
        [FromBody] PrtRegisterRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtAccountDto>.Ok(await _svc.RegisterAsync(TenantId, UserId, req, ct)));

    [HttpPost("login-stub")]
    [AuthorizePermission("prt.account.manage")]
    public async Task<ActionResult<ApiResponse<PrtLoginResultDto>>> Login(
        [FromBody] PrtLoginRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtLoginResultDto>.Ok(await _svc.LoginStubAsync(TenantId, req, ct)));

    [HttpPost("forgot-password-stub")]
    [AuthorizePermission("prt.account.manage")]
    public async Task<ActionResult<ApiResponse<PrtAccountDto>>> Forgot(
        [FromBody] PrtForgotPasswordRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtAccountDto>.Ok(await _svc.ForgotPasswordStubAsync(TenantId, req, ct)));

    [HttpPost("link-customer")]
    [AuthorizePermission("prt.account.manage")]
    public async Task<ActionResult<ApiResponse<PrtAccountDto>>> Link(
        [FromBody] PrtLinkCustomerRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtAccountDto>.Ok(await _svc.LinkCustomerAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/prt/orders")]
public sealed class PrtOrderController : ControllerBase
{
    private readonly IPrtPortalService _svc;
    public PrtOrderController(IPrtPortalService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtOrderDto>>>> List(
        [FromQuery] Guid? accountId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PrtOrderDto>>.Ok(await _svc.ListOrdersAsync(TenantId, accountId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<PrtOrderDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PrtOrderDetailDto>.Ok(await _svc.GetOrderDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("prt.portal.manage")]
    public async Task<ActionResult<ApiResponse<PrtOrderDto>>> Upsert(
        [FromBody] PrtOrderUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtOrderDto>.Ok(await _svc.UpsertOrderAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/prt/ar")]
public sealed class PrtArController : ControllerBase
{
    private readonly IPrtPortalService _svc;
    public PrtArController(IPrtPortalService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("{accountId:guid}/summary")]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<PrtArSummaryDto>>> Summary(Guid accountId, CancellationToken ct)
        => Ok(ApiResponse<PrtArSummaryDto>.Ok(await _svc.GetArSummaryAsync(TenantId, accountId, ct)));

    [HttpGet("{accountId:guid}/invoices")]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtInvoiceDto>>>> Invoices(
        Guid accountId, [FromQuery] bool openOnly = false, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PrtInvoiceDto>>.Ok(
            await _svc.ListInvoicesAsync(TenantId, accountId, openOnly, ct)));

    [HttpPost("invoices")]
    [AuthorizePermission("prt.portal.manage")]
    public async Task<ActionResult<ApiResponse<PrtInvoiceDto>>> UpsertInvoice(
        [FromBody] PrtInvoiceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtInvoiceDto>.Ok(await _svc.UpsertInvoiceAsync(TenantId, UserId, req, ct)));

    [HttpGet("{accountId:guid}/payments")]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtPaymentDto>>>> Payments(
        Guid accountId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PrtPaymentDto>>.Ok(await _svc.ListPaymentsAsync(TenantId, accountId, ct)));

    [HttpPost("payments")]
    [AuthorizePermission("prt.portal.manage")]
    public async Task<ActionResult<ApiResponse<PrtPaymentDto>>> UpsertPayment(
        [FromBody] PrtPaymentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtPaymentDto>.Ok(await _svc.UpsertPaymentAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/prt/tickets")]
public sealed class PrtTicketController : ControllerBase
{
    private readonly IPrtPortalService _svc;
    public PrtTicketController(IPrtPortalService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtTicketDto>>>> List(
        [FromQuery] Guid? accountId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PrtTicketDto>>.Ok(await _svc.ListTicketsAsync(TenantId, accountId, ct)));

    [HttpPost]
    [AuthorizePermission("prt.portal.manage")]
    public async Task<ActionResult<ApiResponse<PrtTicketDto>>> Upsert(
        [FromBody] PrtTicketUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtTicketDto>.Ok(await _svc.UpsertTicketAsync(TenantId, UserId, req, ct)));
}

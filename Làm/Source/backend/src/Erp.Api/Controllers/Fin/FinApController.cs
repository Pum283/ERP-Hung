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
[Route("api/fin/ap-invoices")]
public sealed class FinApInvoiceController : ControllerBase
{
    private readonly IFinApService _svc;
    public FinApInvoiceController(IFinApService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ap.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinApInvoiceDto>>>> List(
        [FromQuery] Guid? vendorId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinApInvoiceDto>>.Ok(
            await _svc.ListInvoicesAsync(TenantId, vendorId, status, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApInvoiceDto>>> Upsert(
        [FromBody] FinApInvoiceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinApInvoiceDto>.Ok(await _svc.UpsertInvoiceAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApInvoiceDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinApInvoiceDto>.Ok(await _svc.PostInvoiceAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApInvoiceDto>>> Void(
        Guid id, [FromBody] FinApNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinApInvoiceDto>.Ok(await _svc.VoidInvoiceAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ap-vendor-balances")]
public sealed class FinApVendorBalanceController : ControllerBase
{
    private readonly IFinApService _svc;
    public FinApVendorBalanceController(IFinApService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ap.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinApVendorBalanceDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinApVendorBalanceDto>>.Ok(await _svc.ListVendorBalancesAsync(TenantId, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ap-payment-requests")]
public sealed class FinApPaymentRequestController : ControllerBase
{
    private readonly IFinApService _svc;
    public FinApPaymentRequestController(IFinApService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ap.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinApPaymentRequestDto>>>> List(
        [FromQuery] Guid? vendorId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinApPaymentRequestDto>>.Ok(
            await _svc.ListPaymentRequestsAsync(TenantId, vendorId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentRequestDto>>> Upsert(
        [FromBody] FinApPaymentRequestUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentRequestDto>.Ok(await _svc.UpsertPaymentRequestAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentRequestDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentRequestDto>.Ok(await _svc.SubmitPaymentRequestAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentRequestDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentRequestDto>.Ok(await _svc.ApprovePaymentRequestAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentRequestDto>>> Reject(
        Guid id, [FromBody] FinApNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentRequestDto>.Ok(
            await _svc.RejectPaymentRequestAsync(TenantId, UserId, id, req?.Note, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentRequestDto>>> Void(
        Guid id, [FromBody] FinApNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentRequestDto>.Ok(
            await _svc.VoidPaymentRequestAsync(TenantId, UserId, id, req?.Note, ct)));

    [HttpPost("{id:guid}/pay")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentDto>>> Pay(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentDto>.Ok(await _svc.PayFromRequestAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ap-payments")]
public sealed class FinApPaymentController : ControllerBase
{
    private readonly IFinApService _svc;
    public FinApPaymentController(IFinApService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ap.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinApPaymentDto>>>> List(
        [FromQuery] Guid? vendorId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinApPaymentDto>>.Ok(await _svc.ListPaymentsAsync(TenantId, vendorId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentDto>>> Upsert(
        [FromBody] FinApPaymentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentDto>.Ok(await _svc.UpsertPaymentAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.ap.manage")]
    public async Task<ActionResult<ApiResponse<FinApPaymentDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinApPaymentDto>.Ok(await _svc.PostPaymentAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ap-aging")]
public sealed class FinApAgingController : ControllerBase
{
    private readonly IFinApService _svc;
    public FinApAgingController(IFinApService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.ap.read")]
    public async Task<ActionResult<ApiResponse<FinApAgingDto>>> Get(
        [FromQuery] DateTimeOffset? asOf, CancellationToken ct)
        => Ok(ApiResponse<FinApAgingDto>.Ok(await _svc.GetAgingAsync(TenantId, asOf, ct)));
}

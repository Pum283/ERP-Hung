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
[Route("api/fin/cash-funds")]
public sealed class FinCashFundController : ControllerBase
{
    private readonly IFinCashService _svc;
    public FinCashFundController(IFinCashService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.cash.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCashFundDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCashFundDto>>.Ok(await _svc.ListFundsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.cash.manage")]
    public async Task<ActionResult<ApiResponse<FinCashFundDto>>> Upsert(
        [FromBody] FinCashFundUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCashFundDto>.Ok(await _svc.UpsertFundAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/cash-vouchers")]
public sealed class FinCashVoucherController : ControllerBase
{
    private readonly IFinCashService _svc;
    public FinCashVoucherController(IFinCashService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.cash.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCashVoucherDto>>>> List(
        [FromQuery] Guid? fundId, [FromQuery] string? type, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCashVoucherDto>>.Ok(
            await _svc.ListVouchersAsync(TenantId, fundId, type, ct)));

    [HttpPost]
    [AuthorizePermission("fin.cash.manage")]
    public async Task<ActionResult<ApiResponse<FinCashVoucherDto>>> Upsert(
        [FromBody] FinCashVoucherUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCashVoucherDto>.Ok(await _svc.UpsertVoucherAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.cash.manage")]
    public async Task<ActionResult<ApiResponse<FinCashVoucherDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinCashVoucherDto>.Ok(await _svc.PostVoucherAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.cash.manage")]
    public async Task<ActionResult<ApiResponse<FinCashVoucherDto>>> Void(
        Guid id, [FromBody] FinCashVoidRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinCashVoucherDto>.Ok(await _svc.VoidVoucherAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/cash-book")]
public sealed class FinCashBookController : ControllerBase
{
    private readonly IFinCashService _svc;
    public FinCashBookController(IFinCashService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.cash.read")]
    public async Task<ActionResult<ApiResponse<FinCashBookDto>>> Get(
        [FromQuery] Guid fundId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(ApiResponse<FinCashBookDto>.Ok(await _svc.GetCashBookAsync(TenantId, fundId, from, to, ct)));
}

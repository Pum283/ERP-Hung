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
[Route("api/fin/bank-accounts")]
public sealed class FinBankAccountController : ControllerBase
{
    private readonly IFinBankService _svc;
    public FinBankAccountController(IFinBankService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBankAccountDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBankAccountDto>>.Ok(await _svc.ListAccountsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankAccountDto>>> Upsert(
        [FromBody] FinBankAccountUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankAccountDto>.Ok(await _svc.UpsertAccountAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/bank-vouchers")]
public sealed class FinBankVoucherController : ControllerBase
{
    private readonly IFinBankService _svc;
    public FinBankVoucherController(IFinBankService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBankVoucherDto>>>> List(
        [FromQuery] Guid? bankAccountId, [FromQuery] string? type, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBankVoucherDto>>.Ok(
            await _svc.ListVouchersAsync(TenantId, bankAccountId, type, ct)));

    [HttpPost]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankVoucherDto>>> Upsert(
        [FromBody] FinBankVoucherUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankVoucherDto>.Ok(await _svc.UpsertVoucherAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankVoucherDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankVoucherDto>.Ok(await _svc.PostVoucherAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankVoucherDto>>> Void(
        Guid id, [FromBody] FinBankVoidRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinBankVoucherDto>.Ok(await _svc.VoidVoucherAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/bank-transfers")]
public sealed class FinBankTransferController : ControllerBase
{
    private readonly IFinBankService _svc;
    public FinBankTransferController(IFinBankService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBankTransferRequestDto>>>> List(
        [FromQuery] Guid? bankAccountId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBankTransferRequestDto>>.Ok(
            await _svc.ListTransfersAsync(TenantId, bankAccountId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Upsert(
        [FromBody] FinBankTransferUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.UpsertTransferAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.SubmitTransferAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.ApproveTransferAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Reject(
        Guid id, [FromBody] FinBankVoidRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.RejectTransferAsync(TenantId, UserId, id, req?.Note, ct)));

    [HttpPost("{id:guid}/execute")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Execute(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.ExecuteTransferAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankTransferRequestDto>>> Void(
        Guid id, [FromBody] FinBankVoidRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinBankTransferRequestDto>.Ok(await _svc.VoidTransferAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/bank-statements")]
public sealed class FinBankStatementController : ControllerBase
{
    private readonly IFinBankService _svc;
    public FinBankStatementController(IFinBankService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBankStatementLineDto>>>> List(
        [FromQuery] Guid? bankAccountId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBankStatementLineDto>>.Ok(
            await _svc.ListStatementsAsync(TenantId, bankAccountId, status, ct)));

    [HttpPost]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankStatementLineDto>>> Upsert(
        [FromBody] FinBankStatementUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankStatementLineDto>.Ok(await _svc.UpsertStatementAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/match")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankStatementLineDto>>> Match(
        Guid id, [FromBody] FinBankMatchRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankStatementLineDto>.Ok(
            await _svc.MatchStatementAsync(TenantId, UserId, id, req.VoucherId, ct)));

    [HttpPost("{id:guid}/unmatch")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankStatementLineDto>>> Unmatch(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankStatementLineDto>.Ok(await _svc.UnmatchStatementAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/ignore")]
    [AuthorizePermission("fin.bank.manage")]
    public async Task<ActionResult<ApiResponse<FinBankStatementLineDto>>> Ignore(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinBankStatementLineDto>.Ok(await _svc.IgnoreStatementAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/bank-book")]
public sealed class FinBankBookController : ControllerBase
{
    private readonly IFinBankService _svc;
    public FinBankBookController(IFinBankService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<FinBankBookDto>>> Get(
        [FromQuery] Guid bankAccountId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(ApiResponse<FinBankBookDto>.Ok(await _svc.GetBankBookAsync(TenantId, bankAccountId, from, to, ct)));
}

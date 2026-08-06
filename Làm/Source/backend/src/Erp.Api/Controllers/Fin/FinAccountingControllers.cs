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
[Route("api/fin/account-groups")]
public sealed class FinAccountGroupController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinAccountGroupController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinAccountGroupDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinAccountGroupDto>>.Ok(await _svc.ListGroupsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinAccountGroupDto>>> Upsert(
        [FromBody] FinAccountGroupUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinAccountGroupDto>.Ok(await _svc.UpsertGroupAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/accounts")]
public sealed class FinAccountController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinAccountController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinAccountDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinAccountDto>>.Ok(await _svc.ListAccountsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinAccountDto>>> Upsert(
        [FromBody] FinAccountUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinAccountDto>.Ok(await _svc.UpsertAccountAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/fiscal-years")]
public sealed class FinFiscalYearController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinFiscalYearController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinFiscalYearDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinFiscalYearDto>>.Ok(await _svc.ListFiscalYearsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinFiscalYearDto>>> Upsert(
        [FromBody] FinFiscalYearUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinFiscalYearDto>.Ok(await _svc.UpsertFiscalYearAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/periods")]
public sealed class FinPeriodController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinPeriodController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinPeriodDto>>>> List(
        [FromQuery] Guid? fiscalYearId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinPeriodDto>>.Ok(await _svc.ListPeriodsAsync(TenantId, fiscalYearId, ct)));

    [HttpPost("{id:guid}/lock")]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinPeriodDto>>> SetLock(
        Guid id, [FromBody] FinPeriodLockRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinPeriodDto>.Ok(await _svc.SetPeriodLockAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/cost-centers")]
public sealed class FinCostCenterController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinCostCenterController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCostCenterDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCostCenterDto>>.Ok(await _svc.ListCostCentersAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinCostCenterDto>>> Upsert(
        [FromBody] FinCostCenterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCostCenterDto>.Ok(await _svc.UpsertCostCenterAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/payment-methods")]
public sealed class FinPaymentMethodController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinPaymentMethodController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinPaymentMethodDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinPaymentMethodDto>>.Ok(await _svc.ListPaymentMethodsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinPaymentMethodDto>>> Upsert(
        [FromBody] FinPaymentMethodUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinPaymentMethodDto>.Ok(await _svc.UpsertPaymentMethodAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/taxes")]
public sealed class FinTaxController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinTaxController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinTaxDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinTaxDto>>.Ok(await _svc.ListTaxesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<FinTaxDto>>> Upsert(
        [FromBody] FinTaxUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinTaxDto>.Ok(await _svc.UpsertTaxAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/journals")]
public sealed class FinJournalController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinJournalController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinJournalDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinJournalDto>>.Ok(await _svc.ListJournalsAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<FinJournalDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDetailDto>.Ok(await _svc.GetJournalDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("fin.journal.manage")]
    public async Task<ActionResult<ApiResponse<FinJournalDto>>> Upsert(
        [FromBody] FinJournalUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDto>.Ok(await _svc.UpsertJournalAsync(TenantId, UserId, req, ct)));

    [HttpPost("auto-stub")]
    [AuthorizePermission("fin.journal.manage")]
    public async Task<ActionResult<ApiResponse<FinJournalDto>>> AutoStub(
        [FromBody] FinJournalUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDto>.Ok(await _svc.CreateAutoJournalStubAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("fin.journal.manage")]
    public async Task<ActionResult<ApiResponse<FinJournalDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDto>.Ok(await _svc.PostJournalAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/reverse")]
    [AuthorizePermission("fin.journal.manage")]
    public async Task<ActionResult<ApiResponse<FinJournalDto>>> Reverse(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDto>.Ok(await _svc.ReverseJournalAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/ledgers")]
public sealed class FinLedgerController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinLedgerController(IFinAccountingService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinLedgerRowDto>>>> Ledger(
        [FromQuery] Guid? accountId, [FromQuery] string? partnerCode,
        [FromQuery] Guid? costCenterId, [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinLedgerRowDto>>.Ok(
            await _svc.GetLedgerAsync(TenantId, new FinLedgerQuery(accountId, partnerCode, costCenterId, periodId), ct)));

    [HttpGet("detail")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinDetailLedgerRowDto>>>> Detail(
        [FromQuery] Guid? accountId, [FromQuery] string? partnerCode,
        [FromQuery] Guid? costCenterId, [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinDetailLedgerRowDto>>.Ok(
            await _svc.GetDetailLedgerAsync(TenantId, new FinLedgerQuery(accountId, partnerCode, costCenterId, periodId), ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/reports")]
public sealed class FinReportsController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinReportsController(IFinAccountingService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("trial-balance")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinTrialBalanceRowDto>>>> TrialBalance(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinTrialBalanceRowDto>>.Ok(await _svc.GetTrialBalanceAsync(TenantId, periodId, ct)));

    [HttpGet("balance-sheet")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBalanceSheetRowDto>>>> BalanceSheet(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBalanceSheetRowDto>>.Ok(await _svc.GetBalanceSheetAsync(TenantId, periodId, ct)));

    [HttpGet("profit-loss")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinProfitLossRowDto>>>> ProfitLoss(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinProfitLossRowDto>>.Ok(await _svc.GetProfitLossAsync(TenantId, periodId, ct)));

    [HttpGet("cash-flow")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCashFlowRowDto>>>> CashFlow(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCashFlowRowDto>>.Ok(await _svc.GetCashFlowAsync(TenantId, periodId, ct)));

    [HttpGet("dashboard")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<FinDashboardSummaryDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<FinDashboardSummaryDto>.Ok(await _svc.GetDashboardSummaryAsync(TenantId, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/closing")]
public sealed class FinClosingController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinClosingController(IFinAccountingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("transfer")]
    [AuthorizePermission("fin.journal.manage")]
    public async Task<ActionResult<ApiResponse<FinJournalDto>>> Transfer(
        [FromBody] FinClosingTransferRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinJournalDto>.Ok(await _svc.RunClosingTransferAsync(TenantId, UserId, req, ct)));

    [HttpPost("fiscal-year")]
    [AuthorizePermission("fin.master.manage")]
    public async Task<ActionResult<ApiResponse<bool>>> CloseFiscalYear(
        [FromBody] FinYearEndClosingRequest req, CancellationToken ct)
        => Ok(ApiResponse<bool>.Ok(await _svc.CloseFiscalYearAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/fin/reconciliation")]
public sealed class FinReconciliationController : ControllerBase
{
    private readonly IFinAccountingService _svc;
    public FinReconciliationController(IFinAccountingService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArApReconciliationRowDto>>>> Reconcile(
        [FromQuery] string type, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArApReconciliationRowDto>>.Ok(await _svc.ReconcileArApAsync(TenantId, type, ct)));
}


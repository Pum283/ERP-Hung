using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fin/statement-offset-dunning-bad-debt")]
public sealed class FinStatementOffsetDunningBadDebtController : ControllerBase
{
    private readonly IFinStatementOffsetDunningBadDebtService _svc;

    public FinStatementOffsetDunningBadDebtController(IFinStatementOffsetDunningBadDebtService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FIN_028: Import sao kê
    [HttpPost("bank-statements/import")]
    [AuthorizePermission("fin.bank.write")]
    public async Task<ActionResult<ApiResponse<FinBankStatementImportRecordDto>>> ImportBankStatement([FromBody] FinImportBankStatementRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBankStatementImportRecordDto>.Ok(await _svc.ImportBankStatementAsync(TenantId, req, ct)));

    [HttpGet("bank-statements/import-history")]
    [AuthorizePermission("fin.bank.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBankStatementImportRecordDto>>>> GetBankStatementImports(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBankStatementImportRecordDto>>.Ok(await _svc.GetBankStatementImportsAsync(TenantId, ct)));

    // UC_FIN_033: Bù trừ công nợ
    [HttpPost("arap-offsets")]
    [AuthorizePermission("fin.ar.write")]
    public async Task<ActionResult<ApiResponse<FinArApOffsetSettlementDto>>> CreateArApOffset([FromBody] FinCreateArApOffsetRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinArApOffsetSettlementDto>.Ok(await _svc.CreateArApOffsetAsync(TenantId, req, ct)));

    [HttpGet("arap-offsets")]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinArApOffsetSettlementDto>>>> GetArApOffsets(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinArApOffsetSettlementDto>>.Ok(await _svc.GetArApOffsetsAsync(TenantId, ct)));

    // UC_FIN_034: Nhắc nợ tự động
    [HttpPost("dunning/send")]
    [AuthorizePermission("fin.ar.write")]
    public async Task<ActionResult<ApiResponse<FinDebtDunningNotificationDto>>> SendDunningNotification([FromBody] FinSendDunningNotificationRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinDebtDunningNotificationDto>.Ok(await _svc.SendDunningNotificationAsync(TenantId, req, ct)));

    [HttpGet("dunning/history")]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinDebtDunningNotificationDto>>>> GetDunningNotifications(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinDebtDunningNotificationDto>>.Ok(await _svc.GetDunningNotificationsAsync(TenantId, ct)));

    // UC_FIN_037: Xử lý nợ khó đòi
    [HttpPost("bad-debt/process")]
    [AuthorizePermission("fin.ar.write")]
    public async Task<ActionResult<ApiResponse<FinBadDebtProvisionWriteOffDto>>> ProcessBadDebt([FromBody] FinProcessBadDebtRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinBadDebtProvisionWriteOffDto>.Ok(await _svc.ProcessBadDebtAsync(TenantId, req, ct)));

    [HttpGet("bad-debt/records")]
    [AuthorizePermission("fin.ar.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinBadDebtProvisionWriteOffDto>>>> GetBadDebtRecords(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinBadDebtProvisionWriteOffDto>>.Ok(await _svc.GetBadDebtRecordsAsync(TenantId, ct)));
}

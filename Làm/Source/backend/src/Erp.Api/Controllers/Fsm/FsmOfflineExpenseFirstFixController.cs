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
[Route("api/fsm/offline-expense-first-fix")]
public sealed class FsmOfflineExpenseFirstFixController : ControllerBase
{
    private readonly IFsmOfflineExpenseFirstFixService _svc;

    public FsmOfflineExpenseFirstFixController(IFsmOfflineExpenseFirstFixService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_040: Cảnh báo thất thoát
    [HttpGet("part-loss-warnings")]
    [AuthorizePermission("fsm.part.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmSparePartLossWarningDto>>>> GetSparePartLossWarnings(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmSparePartLossWarningDto>>.Ok(await _svc.GetSparePartLossWarningsAsync(TenantId, ct)));

    // UC_FSM_043: Làm việc offline
    [HttpPost("offline-sync")]
    [AuthorizePermission("fsm.job.write")]
    public async Task<ActionResult<ApiResponse<FsmOfflineSyncAuditLogDto>>> RecordOfflineSync([FromBody] FsmSyncOfflineDataRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmOfflineSyncAuditLogDto>.Ok(await _svc.RecordOfflineSyncAsync(TenantId, req, ct)));

    [HttpGet("offline-sync-logs")]
    [AuthorizePermission("fsm.job.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmOfflineSyncAuditLogDto>>>> GetOfflineSyncLogs(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmOfflineSyncAuditLogDto>>.Ok(await _svc.GetOfflineSyncLogsAsync(TenantId, ct)));

    // UC_FSM_044: Nộp quyết toán ngày
    [HttpPost("daily-settlements")]
    [AuthorizePermission("fsm.cost.write")]
    public async Task<ActionResult<ApiResponse<FsmDailyExpenseSettlementDto>>> SubmitDailySettlement([FromBody] FsmSubmitDailySettlementRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmDailyExpenseSettlementDto>.Ok(await _svc.SubmitDailySettlementAsync(TenantId, req, ct)));

    [HttpGet("daily-settlements")]
    [AuthorizePermission("fsm.cost.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmDailyExpenseSettlementDto>>>> GetDailySettlements(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmDailyExpenseSettlementDto>>.Ok(await _svc.GetDailySettlementsAsync(TenantId, ct)));

    // UC_FSM_048: Tỷ lệ sửa lần đầu
    [HttpGet("ftfr-report")]
    [AuthorizePermission("fsm.report.read")]
    public async Task<ActionResult<ApiResponse<FsmFirstTimeFixRateReportDto>>> GetFirstTimeFixRateReport(CancellationToken ct)
        => Ok(ApiResponse<FsmFirstTimeFixRateReportDto>.Ok(await _svc.GetFirstTimeFixRateReportAsync(TenantId, ct)));
}

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
[Route("api/crm/commission-sync-leaderboard")]
public sealed class CrmCommissionSyncLeaderboardController : ControllerBase
{
    private readonly ICrmCommissionSyncLeaderboardService _svc;

    public CrmCommissionSyncLeaderboardController(ICrmCommissionSyncLeaderboardService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_121: Tính hoa hồng theo kỳ
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("commission-periods")]
    [AuthorizePermission("crm.commission.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCommissionPeriodDto>>>> GetCommissionPeriods(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCommissionPeriodDto>>.Ok(await _svc.GetCommissionPeriodsAsync(TenantId, ct)));

    [HttpPost("commission-periods/calculate")]
    [AuthorizePermission("crm.commission.write")]
    public async Task<ActionResult<ApiResponse<CrmCommissionPeriodDto>>> CalculateCommissionPeriod([FromBody] CrmCalculateCommissionRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCommissionPeriodDto>.Ok(await _svc.CalculateCommissionPeriodAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_122: Duyệt bảng hoa hồng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("commission-periods/approve")]
    [AuthorizePermission("crm.commission.approve")]
    public async Task<ActionResult<ApiResponse<CrmCommissionApprovalResultDto>>> ApproveCommissionPeriod([FromBody] CrmApproveCommissionRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCommissionApprovalResultDto>.Ok(await _svc.ApproveCommissionPeriodAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_123: Đồng bộ hoa hồng sang HRM/FIN
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("commission-periods/sync-hrm-fin")]
    [AuthorizePermission("crm.commission.sync")]
    public async Task<ActionResult<ApiResponse<CrmCommissionSyncResultDto>>> SyncCommissionToHrmFin([FromBody] CrmSyncCommissionHrmFinRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCommissionSyncResultDto>.Ok(await _svc.SyncCommissionToHrmFinAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_125: Bảng xếp hạng sales
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("leaderboard")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmSalesLeaderboardEntryDto>>>> GetSalesLeaderboard([FromQuery] string rankingPeriod = "Monthly", CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<CrmSalesLeaderboardEntryDto>>.Ok(await _svc.GetSalesLeaderboardAsync(TenantId, rankingPeriod, ct)));
}

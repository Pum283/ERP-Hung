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
[Route("api/crm/route-sales-visit-gps")]
public sealed class CrmRouteSalesVisitGpsController : ControllerBase
{
    private readonly ICrmRouteSalesVisitGpsService _svc;

    public CrmRouteSalesVisitGpsController(ICrmRouteSalesVisitGpsService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_089: Phân vùng / tuyến bán hàng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("territories")]
    [AuthorizePermission("crm.territory.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmTerritoryDto>>>> GetTerritories(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmTerritoryDto>>.Ok(await _svc.GetTerritoriesAsync(TenantId, ct)));

    [HttpPost("territories")]
    [AuthorizePermission("crm.territory.write")]
    public async Task<ActionResult<ApiResponse<CrmTerritoryDto>>> CreateTerritory([FromBody] CrmCreateTerritoryRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmTerritoryDto>.Ok(await _svc.CreateTerritoryAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_090: Phân loại tần suất visit
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("classify-frequency")]
    [AuthorizePermission("crm.territory.write")]
    public async Task<ActionResult<ApiResponse<CrmVisitFrequencyDto>>> ClassifyFrequency([FromBody] CrmClassifyFrequencyRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmVisitFrequencyDto>.Ok(await _svc.ClassifyFrequencyAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_091: Lập kế hoạch visit
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("visit-plans")]
    [AuthorizePermission("crm.visit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmVisitPlanDto>>>> GetVisitPlans([FromQuery] DateTime? date, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmVisitPlanDto>>.Ok(await _svc.GetVisitPlansAsync(TenantId, date, ct)));

    [HttpPost("visit-plans")]
    [AuthorizePermission("crm.visit.write")]
    public async Task<ActionResult<ApiResponse<CrmVisitPlanDto>>> CreateVisitPlan([FromBody] CrmCreateVisitPlanRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmVisitPlanDto>.Ok(await _svc.CreateVisitPlanAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_092: Check-in / check-out GPS
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("check-in")]
    [AuthorizePermission("crm.visit.write")]
    public async Task<ActionResult<ApiResponse<CrmGpsCheckResultDto>>> CheckInGps([FromBody] CrmGpsCheckInRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmGpsCheckResultDto>.Ok(await _svc.CheckInGpsAsync(TenantId, req, ct)));

    [HttpPost("check-out")]
    [AuthorizePermission("crm.visit.write")]
    public async Task<ActionResult<ApiResponse<CrmGpsCheckResultDto>>> CheckOutGps([FromBody] CrmGpsCheckOutRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmGpsCheckResultDto>.Ok(await _svc.CheckOutGpsAsync(TenantId, req, ct)));
}

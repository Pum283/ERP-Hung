using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Bi;
using Erp.Application.Interfaces.Services.Bi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Bi;

[ApiController]
[Authorize]
[Route("api/bi/datasets")]
public sealed class BiDatasetController : ControllerBase
{
    private readonly IBiAnalyticsService _svc;
    public BiDatasetController(IBiAnalyticsService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiDatasetDto>>>> List(
        [FromQuery] string? moduleCode, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiDatasetDto>>.Ok(await _svc.ListDatasetsAsync(TenantId, moduleCode, ct)));

    [HttpPost]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiDatasetDto>>> Upsert(
        [FromBody] BiDatasetUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiDatasetDto>.Ok(await _svc.UpsertDatasetAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/refresh")]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiDatasetDto>>> Refresh(
        Guid id, [FromBody] BiRefreshRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiDatasetDto>.Ok(await _svc.RefreshDatasetAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("{id:guid}/refreshes")]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiDatasetRefreshDto>>>> Refreshes(
        Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiDatasetRefreshDto>>.Ok(await _svc.ListRefreshesAsync(TenantId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/bi/reports")]
public sealed class BiReportController : ControllerBase
{
    private readonly IBiAnalyticsService _svc;
    public BiReportController(IBiAnalyticsService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("bi.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiReportDto>>>> List(
        [FromQuery] string? moduleCode, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiReportDto>>.Ok(await _svc.ListReportsAsync(TenantId, moduleCode, ct)));

    [HttpPost]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiReportDto>>> Upsert(
        [FromBody] BiReportUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiReportDto>.Ok(await _svc.UpsertReportAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}/permissions")]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiReportPermissionDto>>>> Permissions(
        Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiReportPermissionDto>>.Ok(await _svc.ListPermissionsAsync(TenantId, id, ct)));

    [HttpPost("permissions")]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiReportPermissionDto>>> UpsertPermission(
        [FromBody] BiReportPermissionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiReportPermissionDto>.Ok(await _svc.UpsertPermissionAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/run")]
    [AuthorizePermission("bi.report.run")]
    public async Task<ActionResult<ApiResponse<BiReportRunDto>>> Run(
        Guid id, [FromBody] BiReportRunRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiReportRunDto>.Ok(await _svc.RunReportAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("runs")]
    [AuthorizePermission("bi.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiReportRunDto>>>> Runs(
        [FromQuery] Guid? reportId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiReportRunDto>>.Ok(await _svc.ListRunsAsync(TenantId, reportId, ct)));
}

[ApiController]
[Authorize]
[Route("api/bi/dashboards")]
public sealed class BiDashboardController : ControllerBase
{
    private readonly IBiAnalyticsService _svc;
    public BiDashboardController(IBiAnalyticsService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiDashboardDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiDashboardDto>>.Ok(await _svc.ListDashboardsAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<BiDashboardDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<BiDashboardDetailDto>.Ok(await _svc.GetDashboardDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiDashboardDto>>> Upsert(
        [FromBody] BiDashboardUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiDashboardDto>.Ok(await _svc.UpsertDashboardAsync(TenantId, UserId, req, ct)));

    [HttpPost("widgets")]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiWidgetDto>>> UpsertWidget(
        [FromBody] BiWidgetUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiWidgetDto>.Ok(await _svc.UpsertWidgetAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/bi/kpi")]
public sealed class BiKpiController : ControllerBase
{
    private readonly IBiAnalyticsService _svc;
    public BiKpiController(IBiAnalyticsService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("targets")]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiKpiTargetDto>>>> Targets(
        [FromQuery] string? periodKey, [FromQuery] string? moduleCode, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiKpiTargetDto>>.Ok(
            await _svc.ListKpiTargetsAsync(TenantId, periodKey, moduleCode, ct)));

    [HttpPost("targets")]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiKpiTargetDto>>> UpsertTarget(
        [FromBody] BiKpiTargetUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiKpiTargetDto>.Ok(await _svc.UpsertKpiTargetAsync(TenantId, UserId, req, ct)));

    [HttpGet("thresholds")]
    [AuthorizePermission("bi.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiAlertThresholdDto>>>> Thresholds(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiAlertThresholdDto>>.Ok(await _svc.ListAlertThresholdsAsync(TenantId, ct)));

    [HttpPost("thresholds")]
    [AuthorizePermission("bi.catalog.manage")]
    public async Task<ActionResult<ApiResponse<BiAlertThresholdDto>>> UpsertThreshold(
        [FromBody] BiAlertThresholdUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiAlertThresholdDto>.Ok(await _svc.UpsertAlertThresholdAsync(TenantId, UserId, req, ct)));

    [HttpPost("compare")]
    [AuthorizePermission("bi.report.read")]
    public async Task<ActionResult<ApiResponse<BiPeriodCompareDto>>> Compare(
        [FromBody] BiPeriodCompareRequest req, CancellationToken ct)
        => Ok(ApiResponse<BiPeriodCompareDto>.Ok(await _svc.ComparePeriodsAsync(TenantId, req, ct)));

    [HttpGet("board")]
    [AuthorizePermission("bi.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BiTargetVsActualRowDto>>>> Board(
        [FromQuery] string periodKey, [FromQuery] string? moduleCode, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<BiTargetVsActualRowDto>>.Ok(
            await _svc.ListTargetVsActualAsync(TenantId, periodKey, moduleCode, ct)));
}

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
[Route("api/log/realtime-gps-internal-transfer")]
public sealed class LogRealtimeGpsInternalTransferController : ControllerBase
{
    private readonly ILogRealtimeGpsInternalTransferService _svc;

    public LogRealtimeGpsInternalTransferController(ILogRealtimeGpsInternalTransferService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_LOG_019: Theo dõi realtime trên bản đồ
    [HttpPost("gps-pings")]
    [AuthorizePermission("log.tracking.write")]
    public async Task<ActionResult<ApiResponse<LogRealtimeGpsPingDto>>> RecordGpsPing([FromBody] LogPingGpsLocationRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogRealtimeGpsPingDto>.Ok(await _svc.RecordGpsPingAsync(TenantId, req, ct)));

    [HttpGet("fleet-locations")]
    [AuthorizePermission("log.tracking.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogRealtimeGpsPingDto>>>> GetLatestFleetLocations(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogRealtimeGpsPingDto>>.Ok(await _svc.GetLatestFleetLocationsAsync(TenantId, ct)));

    // UC_LOG_031 & UC_LOG_032: Lệnh giao nội bộ & Xác nhận nhận hàng
    [HttpPost("internal-deliveries")]
    [AuthorizePermission("log.transfer.write")]
    public async Task<ActionResult<ApiResponse<LogInternalTransferDeliveryDto>>> CreateInternalTransferDelivery([FromBody] LogCreateInternalTransferDeliveryRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogInternalTransferDeliveryDto>.Ok(await _svc.CreateInternalTransferDeliveryAsync(TenantId, req, ct)));

    [HttpPost("internal-deliveries/confirm-receipt")]
    [AuthorizePermission("log.transfer.write")]
    public async Task<ActionResult<ApiResponse<LogInternalTransferDeliveryDto>>> ConfirmInternalReceipt([FromBody] LogConfirmInternalReceiptRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogInternalTransferDeliveryDto>.Ok(await _svc.ConfirmInternalReceiptAsync(TenantId, req, ct)));

    // UC_LOG_033: Đối soát giao nội bộ
    [HttpPost("internal-reconciliations")]
    [AuthorizePermission("log.transfer.write")]
    public async Task<ActionResult<ApiResponse<LogInternalDeliveryReconciliationDto>>> ReconcileInternalDelivery([FromBody] LogCreateInternalReconciliationRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogInternalDeliveryReconciliationDto>.Ok(await _svc.ReconcileInternalDeliveryAsync(TenantId, req, ct)));
}

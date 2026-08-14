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
[Route("api/log/shift-trip-pod-reschedule")]
public sealed class LogShiftTripPodRescheduleController : ControllerBase
{
    private readonly ILogShiftTripPodRescheduleService _svc;

    public LogShiftTripPodRescheduleController(ILogShiftTripPodRescheduleService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_LOG_005: Cấu hình ca giao hàng
    [HttpPost("delivery-shifts")]
    [AuthorizePermission("log.config.write")]
    public async Task<ActionResult<ApiResponse<LogDeliveryShiftDto>>> CreateDeliveryShift([FromBody] LogCreateDeliveryShiftRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryShiftDto>.Ok(await _svc.CreateDeliveryShiftAsync(TenantId, req, ct)));

    [HttpGet("delivery-shifts")]
    [AuthorizePermission("log.config.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDeliveryShiftDto>>>> GetDeliveryShifts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDeliveryShiftDto>>.Ok(await _svc.GetDeliveryShiftsAsync(TenantId, ct)));

    // UC_LOG_007: Gộp nhiều đơn thành chuyến
    [HttpPost("consolidate-trips")]
    [AuthorizePermission("log.trip.write")]
    public async Task<ActionResult<ApiResponse<LogDeliveryTripDto>>> ConsolidateTrip([FromBody] LogConsolidateTripRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryTripDto>.Ok(await _svc.ConsolidateTripAsync(TenantId, req, ct)));

    // UC_LOG_016: Chứng từ ký nhận (POD)
    [HttpPost("proof-of-deliveries")]
    [AuthorizePermission("log.pod.write")]
    public async Task<ActionResult<ApiResponse<LogProofOfDeliveryDto>>> SubmitPod([FromBody] LogSubmitPodRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogProofOfDeliveryDto>.Ok(await _svc.SubmitPodAsync(TenantId, req, ct)));

    // UC_LOG_018: Hẹn giao lại
    [HttpPost("redelivery-requests")]
    [AuthorizePermission("log.order.write")]
    public async Task<ActionResult<ApiResponse<LogRedeliveryRequestDto>>> CreateRedeliveryRequest([FromBody] LogCreateRedeliveryRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogRedeliveryRequestDto>.Ok(await _svc.CreateRedeliveryRequestAsync(TenantId, req, ct)));
}

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
[Route("api/pur/reject-return-delivery-protocol-discrepancy")]
public sealed class PurRejectReturnDeliveryProtocolDiscrepancyController : ControllerBase
{
    private readonly IPurRejectReturnDeliveryProtocolDiscrepancyService _svc;

    public PurRejectReturnDeliveryProtocolDiscrepancyController(IPurRejectReturnDeliveryProtocolDiscrepancyService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PUR_036: Từ chối lô hàng không đạt QC
    [HttpPost("shipments/reject")]
    [AuthorizePermission("pur.qc.write")]
    public async Task<ActionResult<ApiResponse<PurShipmentRejectionDto>>> RejectShipment([FromBody] PurRejectShipmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurShipmentRejectionDto>.Ok(await _svc.RejectShipmentAsync(TenantId, req, ct)));

    [HttpGet("shipments/rejections")]
    [AuthorizePermission("pur.qc.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurShipmentRejectionDto>>>> GetRejections(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurShipmentRejectionDto>>.Ok(await _svc.GetRejectionsAsync(TenantId, ct)));

    // UC_PUR_038: Trả hàng nhà cung cấp (RTV)
    [HttpPost("returns/rtv")]
    [AuthorizePermission("pur.rtv.write")]
    public async Task<ActionResult<ApiResponse<PurVendorReturnDto>>> CreateVendorReturn([FromBody] PurCreateVendorReturnRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorReturnDto>.Ok(await _svc.CreateVendorReturnAsync(TenantId, req, ct)));

    // UC_PUR_039 & UC_PUR_042: Biên bản giao nhận & Xử lý chênh lệch
    [HttpPost("delivery-protocols/settle-discrepancy")]
    [AuthorizePermission("pur.grn.write")]
    public async Task<ActionResult<ApiResponse<PurDeliveryReceivingProtocolDto>>> CreateDeliveryProtocolAndSettleDiscrepancy([FromBody] PurCreateDeliveryProtocolRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurDeliveryReceivingProtocolDto>.Ok(await _svc.CreateDeliveryProtocolAndSettleDiscrepancyAsync(TenantId, req, ct)));
}

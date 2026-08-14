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
[Route("api/inv/location-customer-return-label-technical-dispatch")]
public sealed class InvLocationCustomerReturnLabelTechnicalDispatchController : ControllerBase
{
    private readonly IInvLocationCustomerReturnLabelTechnicalDispatchService _svc;

    public InvLocationCustomerReturnLabelTechnicalDispatchController(IInvLocationCustomerReturnLabelTechnicalDispatchService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_013: Vị trí / kệ / bin
    [HttpPost("bin-locations")]
    [AuthorizePermission("inv.warehouse.write")]
    public async Task<ActionResult<ApiResponse<InvWarehouseBinLocationDto>>> CreateWarehouseBinLocation([FromBody] InvCreateWarehouseBinLocationRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvWarehouseBinLocationDto>.Ok(await _svc.CreateWarehouseBinLocationAsync(TenantId, req, ct)));

    [HttpGet("bin-locations")]
    [AuthorizePermission("inv.warehouse.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvWarehouseBinLocationDto>>>> GetWarehouseBinLocations([FromQuery] Guid warehouseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvWarehouseBinLocationDto>>.Ok(await _svc.GetWarehouseBinLocationsAsync(TenantId, warehouseId, ct)));

    // UC_INV_021: Nhập trả từ khách
    [HttpPost("customer-returns")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvCustomerReturnReceiptDto>>> CreateCustomerReturnReceipt([FromBody] InvCreateCustomerReturnReceiptRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvCustomerReturnReceiptDto>.Ok(await _svc.CreateCustomerReturnReceiptAsync(TenantId, req, ct)));

    // UC_INV_023: In tem lô / serial
    [HttpPost("print-lot-serial-label")]
    [AuthorizePermission("inv.product.write")]
    public async Task<ActionResult<ApiResponse<InvLotSerialLabelPrintDto>>> PrintLotSerialLabel([FromBody] InvPrintLotSerialLabelRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvLotSerialLabelPrintDto>.Ok(await _svc.PrintLotSerialLabelAsync(TenantId, req, ct)));

    // UC_INV_027: Xuất cho dịch vụ kỹ thuật
    [HttpPost("technical-service-dispatches")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvTechnicalServiceDispatchDto>>> CreateTechnicalServiceDispatch([FromBody] InvCreateTechnicalServiceDispatchRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvTechnicalServiceDispatchDto>.Ok(await _svc.CreateTechnicalServiceDispatchAsync(TenantId, req, ct)));
}

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
[Route("api/pos/hardware-printer-drawer")]
public sealed class PosHardwarePrinterDrawerController : ControllerBase
{
    private readonly IPosHardwarePrinterDrawerService _svc;

    public PosHardwarePrinterDrawerController(IPosHardwarePrinterDrawerService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_POS_004: Cấu hình máy in bếp/khu vực
    [HttpGet("kitchen-printers")]
    [AuthorizePermission("pos.config.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosKitchenPrinterConfigDto>>>> GetKitchenPrinterConfigs(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosKitchenPrinterConfigDto>>.Ok(await _svc.GetKitchenPrinterConfigsAsync(TenantId, ct)));

    [HttpPost("kitchen-printers")]
    [AuthorizePermission("pos.config.write")]
    public async Task<ActionResult<ApiResponse<PosKitchenPrinterConfigDto>>> SaveKitchenPrinterConfig([FromBody] PosSaveKitchenPrinterConfigRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosKitchenPrinterConfigDto>.Ok(await _svc.SaveKitchenPrinterConfigAsync(TenantId, req, ct)));

    // UC_POS_005: Cấu hình ngăn kéo tiền
    [HttpGet("cash-drawers")]
    [AuthorizePermission("pos.config.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosCashDrawerConfigDto>>>> GetCashDrawerConfigs(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosCashDrawerConfigDto>>.Ok(await _svc.GetCashDrawerConfigsAsync(TenantId, ct)));

    [HttpPost("cash-drawers")]
    [AuthorizePermission("pos.config.write")]
    public async Task<ActionResult<ApiResponse<PosCashDrawerConfigDto>>> SaveCashDrawerConfig([FromBody] PosSaveCashDrawerConfigRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosCashDrawerConfigDto>.Ok(await _svc.SaveCashDrawerConfigAsync(TenantId, req, ct)));
}

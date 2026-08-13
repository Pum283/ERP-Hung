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
[Route("api/pos/scanner-offline-attr-product")]
public sealed class PosScannerOfflineAttrProductController : ControllerBase
{
    private readonly IPosScannerOfflineAttrProductService _svc;

    public PosScannerOfflineAttrProductController(IPosScannerOfflineAttrProductService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_POS_006: Cấu hình thiết bị quét mã
    [HttpGet("scanners")]
    [AuthorizePermission("pos.config.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosBarcodeScannerConfigDto>>>> GetBarcodeScannerConfigs(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosBarcodeScannerConfigDto>>.Ok(await _svc.GetBarcodeScannerConfigsAsync(TenantId, ct)));

    [HttpPost("scanners")]
    [AuthorizePermission("pos.config.write")]
    public async Task<ActionResult<ApiResponse<PosBarcodeScannerConfigDto>>> SaveBarcodeScannerConfig([FromBody] PosSaveBarcodeScannerConfigRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosBarcodeScannerConfigDto>.Ok(await _svc.SaveBarcodeScannerConfigAsync(TenantId, req, ct)));

    // UC_POS_008: Chế độ offline tạm & Đệm đồng bộ
    [HttpGet("offline-buffer")]
    [AuthorizePermission("pos.offline.read")]
    public async Task<ActionResult<ApiResponse<PosOfflineSyncBufferDto>>> GetOfflineSyncBufferStatus([FromQuery] string terminalCode, CancellationToken ct)
        => Ok(ApiResponse<PosOfflineSyncBufferDto>.Ok(await _svc.GetOfflineSyncBufferStatusAsync(TenantId, terminalCode, ct)));

    [HttpPost("offline-buffer/trigger-sync")]
    [AuthorizePermission("pos.offline.sync")]
    public async Task<ActionResult<ApiResponse<PosOfflineSyncBufferDto>>> TriggerOfflineSync([FromBody] PosTriggerOfflineSyncRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosOfflineSyncBufferDto>.Ok(await _svc.TriggerOfflineSyncAsync(TenantId, req, ct)));

    // UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Ảnh/Thứ tự hiển thị
    [HttpGet("product-attributes")]
    [AuthorizePermission("pos.catalog.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosProductAttributeModifierDto>>>> GetProductAttributes([FromQuery] Guid productId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosProductAttributeModifierDto>>.Ok(await _svc.GetProductAttributesAsync(TenantId, productId, ct)));

    [HttpPost("product-attributes")]
    [AuthorizePermission("pos.catalog.write")]
    public async Task<ActionResult<ApiResponse<PosProductAttributeModifierDto>>> SaveProductAttribute([FromBody] PosSaveProductAttributeRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosProductAttributeModifierDto>.Ok(await _svc.SaveProductAttributeAsync(TenantId, req, ct)));
}

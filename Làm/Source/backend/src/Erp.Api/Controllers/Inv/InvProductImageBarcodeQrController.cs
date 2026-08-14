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
[Route("api/inv/product-image-barcode-qr")]
public sealed class InvProductImageBarcodeQrController : ControllerBase
{
    private readonly IInvProductImageBarcodeQrService _svc;

    public InvProductImageBarcodeQrController(IInvProductImageBarcodeQrService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_006: Ảnh & mô tả sản phẩm
    [HttpPost("product-media")]
    [AuthorizePermission("inv.product.write")]
    public async Task<ActionResult<ApiResponse<InvProductMediaDto>>> UpdateProductMedia([FromBody] InvUpdateProductMediaRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvProductMediaDto>.Ok(await _svc.UpdateProductMediaAsync(TenantId, req, ct)));

    [HttpGet("product-media/{productId:guid}")]
    [AuthorizePermission("inv.product.read")]
    public async Task<ActionResult<ApiResponse<InvProductMediaDto?>>> GetProductMedia([FromRoute] Guid productId, CancellationToken ct)
        => Ok(ApiResponse<InvProductMediaDto?>.Ok(await _svc.GetProductMediaAsync(TenantId, productId, ct)));

    // UC_INV_009: Barcode / QR theo sản phẩm
    [HttpPost("generate-barcode-qr")]
    [AuthorizePermission("inv.product.write")]
    public async Task<ActionResult<ApiResponse<InvProductBarcodeQrDto>>> GenerateProductBarcodeQr([FromBody] InvGenerateBarcodeQrRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvProductBarcodeQrDto>.Ok(await _svc.GenerateProductBarcodeQrAsync(TenantId, req, ct)));

    [HttpGet("barcode-qr/{productId:guid}")]
    [AuthorizePermission("inv.product.read")]
    public async Task<ActionResult<ApiResponse<InvProductBarcodeQrDto?>>> GetProductBarcodeQr([FromRoute] Guid productId, CancellationToken ct)
        => Ok(ApiResponse<InvProductBarcodeQrDto?>.Ok(await _svc.GetProductBarcodeQrAsync(TenantId, productId, ct)));
}

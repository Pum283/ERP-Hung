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
[Route("api/pur/blacklist-import-legal-pricelist")]
public sealed class PurBlacklistImportLegalPricelistController : ControllerBase
{
    private readonly IPurBlacklistImportLegalPricelistService _svc;

    public PurBlacklistImportLegalPricelistController(IPurBlacklistImportLegalPricelistService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PUR_006: Blacklist / ngưng dùng
    [HttpPost("suppliers/blacklist")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurSupplierBlacklistStatusDto>>> BlacklistSupplier([FromBody] PurBlacklistSupplierRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurSupplierBlacklistStatusDto>.Ok(await _svc.BlacklistSupplierAsync(TenantId, req, ct)));

    // UC_PUR_007: Import danh sách nhà cung cấp
    [HttpPost("suppliers/import-batch")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurBatchImportSuppliersResultDto>>> ImportSuppliersBatch([FromBody] PurBatchImportSuppliersRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurBatchImportSuppliersResultDto>.Ok(await _svc.ImportSuppliersBatchAsync(TenantId, req, ct)));

    // UC_PUR_008: Hồ sơ pháp lý
    [HttpGet("suppliers/{supplierId:guid}/legal-documents")]
    [AuthorizePermission("pur.supplier.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurSupplierLegalDocumentDto>>>> GetSupplierLegalDocuments([FromRoute] Guid supplierId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurSupplierLegalDocumentDto>>.Ok(await _svc.GetSupplierLegalDocumentsAsync(TenantId, supplierId, ct)));

    [HttpPost("legal-documents")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurSupplierLegalDocumentDto>>> SaveSupplierLegalDocument([FromBody] PurSaveSupplierLegalDocumentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurSupplierLegalDocumentDto>.Ok(await _svc.SaveSupplierLegalDocumentAsync(TenantId, req, ct)));

    // UC_PUR_011: Hiệu lực bảng giá mua
    [HttpGet("suppliers/{supplierId:guid}/pricelists")]
    [AuthorizePermission("pur.supplier.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPurchasePricelistValidityDto>>>> GetPurchasePricelists([FromRoute] Guid supplierId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurPurchasePricelistValidityDto>>.Ok(await _svc.GetPurchasePricelistsAsync(TenantId, supplierId, ct)));

    [HttpPost("pricelists")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurPurchasePricelistValidityDto>>> SavePurchasePricelistValidity([FromBody] PurSavePurchasePricelistValidityRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchasePricelistValidityDto>.Ok(await _svc.SavePurchasePricelistValidityAsync(TenantId, req, ct)));
}

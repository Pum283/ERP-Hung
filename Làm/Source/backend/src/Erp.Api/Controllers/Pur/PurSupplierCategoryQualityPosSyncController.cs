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
[Route("api/pur/supplier-category-quality-pos-sync")]
public sealed class PurSupplierCategoryQualityPosSyncController : ControllerBase
{
    private readonly IPurSupplierCategoryQualityPosSyncService _svc;

    public PurSupplierCategoryQualityPosSyncController(IPurSupplierCategoryQualityPosSyncService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_POS_060: Đồng bộ đơn sang CRM
    [HttpPost("pos-orders/sync-to-crm")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosSyncOrderToCrmResultDto>>> SyncPosOrderToCrm([FromBody] PosSyncOrderToCrmRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSyncOrderToCrmResultDto>.Ok(await _svc.SyncPosOrderToCrmAsync(TenantId, req, ct)));

    // UC_PUR_002: Phân loại nhóm nhà cung cấp
    [HttpGet("categories")]
    [AuthorizePermission("pur.supplier.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurSupplierCategoryDto>>>> GetSupplierCategories(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurSupplierCategoryDto>>.Ok(await _svc.GetSupplierCategoriesAsync(TenantId, ct)));

    [HttpPost("categories")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurSupplierCategoryDto>>> SaveSupplierCategory([FromBody] PurSaveSupplierCategoryRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurSupplierCategoryDto>.Ok(await _svc.SaveSupplierCategoryAsync(TenantId, req, ct)));

    // UC_PUR_004: Lead time & MOQ
    [HttpGet("suppliers/{supplierId:guid}/lead-time-moq")]
    [AuthorizePermission("pur.supplier.read")]
    public async Task<ActionResult<ApiResponse<PurSupplierLeadTimeMoqDto>>> GetSupplierLeadTimeMoq([FromRoute] Guid supplierId, CancellationToken ct)
        => Ok(ApiResponse<PurSupplierLeadTimeMoqDto>.Ok(await _svc.GetSupplierLeadTimeMoqAsync(TenantId, supplierId, ct)));

    // UC_PUR_005: Đánh giá chất lượng nhà cung cấp
    [HttpGet("suppliers/{supplierId:guid}/evaluations")]
    [AuthorizePermission("pur.supplier.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurSupplierQualityEvaluationDto>>>> GetSupplierQualityEvaluations([FromRoute] Guid supplierId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurSupplierQualityEvaluationDto>>.Ok(await _svc.GetSupplierQualityEvaluationsAsync(TenantId, supplierId, ct)));

    [HttpPost("evaluations")]
    [AuthorizePermission("pur.supplier.write")]
    public async Task<ActionResult<ApiResponse<PurSupplierQualityEvaluationDto>>> EvaluateSupplierQuality([FromBody] PurSaveSupplierQualityEvaluationRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurSupplierQualityEvaluationDto>.Ok(await _svc.EvaluateSupplierQualityAsync(TenantId, UserId, req, ct)));
}

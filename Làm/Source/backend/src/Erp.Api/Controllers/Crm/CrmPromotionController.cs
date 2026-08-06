using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Crm;

/// <summary>Khuyến mại, voucher & chat (UC_CRM_032–038, 047).</summary>
[ApiController]
[Authorize]
[Route("api/crm/promotions")]
public sealed class CrmPromotionController : ControllerBase
{
    private readonly ICrmPromotionService _svc;
    public CrmPromotionController(ICrmPromotionService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.promotion.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmPromotionDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmPromotionDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.promotion.read")]
    public async Task<ActionResult<ApiResponse<CrmPromotionDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmPromotionDto>.Ok(await _svc.GetAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.promotion.manage")]
    public async Task<ActionResult<ApiResponse<CrmPromotionDto>>> Upsert(
        [FromBody] CrmPromotionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmPromotionDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    // ── Voucher (UC_CRM_034, 035) ──
    [HttpPost("{id:guid}/vouchers/generate")]
    [AuthorizePermission("crm.promotion.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmVoucherDto>>>> GenerateVouchers(
        Guid id, [FromBody] CrmVoucherGenerateRequest req, CancellationToken ct)
    {
        var body = req with { PromotionId = id };
        return Ok(ApiResponse<IReadOnlyList<CrmVoucherDto>>.Ok(
            await _svc.GenerateVouchersAsync(TenantId, UserId, body, ct)));
    }

    [HttpGet("{id:guid}/vouchers")]
    [AuthorizePermission("crm.promotion.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmVoucherDto>>>> ListVouchers(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmVoucherDto>>.Ok(await _svc.ListVouchersAsync(TenantId, id, ct)));

    [HttpPost("vouchers/redeem")]
    [AuthorizePermission("crm.promotion.manage")]
    public async Task<ActionResult<ApiResponse<CrmVoucherRedeemResult>>> RedeemVoucher(
        [FromBody] CrmVoucherRedeemRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmVoucherRedeemResult>.Ok(await _svc.RedeemVoucherAsync(TenantId, UserId, req, ct)));

    // ── Apply on quote (UC_CRM_037) ──
    [HttpPost("apply")]
    [AuthorizePermission("crm.promotion.manage")]
    public async Task<ActionResult<ApiResponse<CrmApplyPromotionResult>>> ApplyOnQuote(
        [FromBody] CrmApplyPromotionRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmApplyPromotionResult>.Ok(await _svc.ApplyOnQuoteAsync(TenantId, UserId, req, ct)));

    // ── Sync CRM → POS (UC_CRM_036) ──
    [HttpPost("{id:guid}/sync-pos")]
    [AuthorizePermission("crm.promotion.manage")]
    public async Task<ActionResult<ApiResponse<CrmSyncPromoToPosResult>>> SyncToPos(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmSyncPromoToPosResult>.Ok(await _svc.SyncToPosAsync(TenantId, UserId, id, ct)));

    // ── Voucher usage report (UC_CRM_038) ──
    [HttpGet("voucher-usage-report")]
    [AuthorizePermission("crm.promotion.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmVoucherUsageReportRowDto>>>> VoucherUsageReport(
        [FromQuery] Guid? promotionId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmVoucherUsageReportRowDto>>.Ok(
            await _svc.GetVoucherUsageReportAsync(TenantId, promotionId, from, to, ct)));

    // ── Chat history (UC_CRM_047) ──
    [HttpPost("~/api/crm/chat")]
    [AuthorizePermission("crm.chat.manage")]
    public async Task<ActionResult<ApiResponse<CrmChatHistoryDto>>> SaveChat(
        [FromBody] CrmChatHistoryRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmChatHistoryDto>.Ok(await _svc.SaveChatAsync(TenantId, UserId, req, ct)));

    [HttpGet("~/api/crm/chat")]
    [AuthorizePermission("crm.chat.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmChatHistoryDto>>>> ListChat(
        [FromQuery] Guid? customerId, [FromQuery] string? channel, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmChatHistoryDto>>.Ok(await _svc.ListChatAsync(TenantId, customerId, channel, ct)));
}

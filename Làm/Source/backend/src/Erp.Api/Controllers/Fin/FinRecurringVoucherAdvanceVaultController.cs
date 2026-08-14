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
[Route("api/fin/recurring-voucher-advance-vault")]
public sealed class FinRecurringVoucherAdvanceVaultController : ControllerBase
{
    private readonly IFinRecurringVoucherAdvanceVaultService _svc;

    public FinRecurringVoucherAdvanceVaultController(IFinRecurringVoucherAdvanceVaultService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FIN_011: Bút toán định kỳ / mẫu
    [HttpPost("recurring-templates")]
    [AuthorizePermission("fin.journal.write")]
    public async Task<ActionResult<ApiResponse<FinRecurringTemplateVoucherDto>>> CreateRecurringTemplate([FromBody] FinCreateRecurringTemplateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinRecurringTemplateVoucherDto>.Ok(await _svc.CreateRecurringTemplateAsync(TenantId, req, ct)));

    [HttpGet("recurring-templates")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinRecurringTemplateVoucherDto>>>> GetRecurringTemplates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinRecurringTemplateVoucherDto>>.Ok(await _svc.GetRecurringTemplatesAsync(TenantId, ct)));

    // UC_FIN_017: Đính kèm chứng từ gốc
    [HttpPost("voucher-attachments")]
    [AuthorizePermission("fin.journal.write")]
    public async Task<ActionResult<ApiResponse<FinOriginalVoucherAttachmentDto>>> UploadVoucherAttachment([FromBody] FinUploadVoucherAttachmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinOriginalVoucherAttachmentDto>.Ok(await _svc.UploadVoucherAttachmentAsync(TenantId, req, ct)));

    [HttpGet("voucher-attachments")]
    [AuthorizePermission("fin.journal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinOriginalVoucherAttachmentDto>>>> GetVoucherAttachments([FromQuery] Guid? journalEntryId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinOriginalVoucherAttachmentDto>>.Ok(await _svc.GetVoucherAttachmentsAsync(TenantId, journalEntryId ?? Guid.Empty, ct)));

    // UC_FIN_021: Đề nghị tạm ứng / hoàn ứng
    [HttpPost("advance-settlements")]
    [AuthorizePermission("fin.cash.write")]
    public async Task<ActionResult<ApiResponse<FinAdvanceSettlementRequestDto>>> CreateAdvanceSettlement([FromBody] FinCreateAdvanceSettlementRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinAdvanceSettlementRequestDto>.Ok(await _svc.CreateAdvanceSettlementAsync(TenantId, req, ct)));

    [HttpGet("advance-settlements")]
    [AuthorizePermission("fin.cash.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinAdvanceSettlementRequestDto>>>> GetAdvanceSettlements(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinAdvanceSettlementRequestDto>>.Ok(await _svc.GetAdvanceSettlementsAsync(TenantId, ct)));

    // UC_FIN_022: Kiểm kê quỹ
    [HttpPost("vault-count-audits")]
    [AuthorizePermission("fin.cash.write")]
    public async Task<ActionResult<ApiResponse<FinCashVaultCountAuditDto>>> CreateVaultCountAudit([FromBody] FinCreateVaultCountAuditRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCashVaultCountAuditDto>.Ok(await _svc.CreateVaultCountAuditAsync(TenantId, req, ct)));

    [HttpGet("vault-count-audits")]
    [AuthorizePermission("fin.cash.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCashVaultCountAuditDto>>>> GetVaultCountAudits(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCashVaultCountAuditDto>>.Ok(await _svc.GetVaultCountAuditsAsync(TenantId, ct)));
}

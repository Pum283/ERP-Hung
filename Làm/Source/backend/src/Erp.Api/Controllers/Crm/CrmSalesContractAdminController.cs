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
[Route("api/crm/sales-contract-admin")]
public sealed class CrmSalesContractAdminController : ControllerBase
{
    private readonly ICrmSalesContractAdminService _svc;

    public CrmSalesContractAdminController(ICrmSalesContractAdminService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_105: Báo cáo năng suất Sales Admin
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("productivity-reports")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmSalesAdminProductivityDto>>>> GetProductivityReports(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmSalesAdminProductivityDto>>.Ok(await _svc.GetProductivityReportsAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_106: Quản lý hợp đồng bán
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("contracts")]
    [AuthorizePermission("crm.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmSalesContractDto>>>> GetContracts([FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmSalesContractDto>>.Ok(await _svc.GetContractsAsync(TenantId, customerId, ct)));

    [HttpPost("contracts")]
    [AuthorizePermission("crm.contract.write")]
    public async Task<ActionResult<ApiResponse<CrmSalesContractDto>>> CreateContract([FromBody] CrmCreateSalesContractRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmSalesContractDto>.Ok(await _svc.CreateContractAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_107: Đính kèm file hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("contracts/{contractId:guid}/attachments")]
    [AuthorizePermission("crm.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmContractAttachmentDto>>>> GetAttachments(Guid contractId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmContractAttachmentDto>>.Ok(await _svc.GetAttachmentsAsync(TenantId, contractId, ct)));

    [HttpPost("contracts/attachments")]
    [AuthorizePermission("crm.contract.write")]
    public async Task<ActionResult<ApiResponse<CrmContractAttachmentDto>>> AttachFile([FromBody] CrmAttachContractFileRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmContractAttachmentDto>.Ok(await _svc.AttachFileAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_108: Theo dõi hiệu lực / tái tục
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("contracts/renew")]
    [AuthorizePermission("crm.contract.write")]
    public async Task<ActionResult<ApiResponse<CrmContractRenewalStatusDto>>> RenewContract([FromBody] CrmRenewContractRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmContractRenewalStatusDto>.Ok(await _svc.RenewContractAsync(TenantId, req, ct)));
}

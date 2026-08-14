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
[Route("api/inv/material-requisition-approval-slow-moving")]
public sealed class InvMaterialRequisitionApprovalSlowMovingController : ControllerBase
{
    private readonly IInvMaterialRequisitionApprovalSlowMovingService _svc;

    public InvMaterialRequisitionApprovalSlowMovingController(IInvMaterialRequisitionApprovalSlowMovingService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_057: Đề nghị cấp hàng
    [HttpPost("requisitions")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvMaterialRequisitionDto>>> CreateRequisition([FromBody] InvCreateMaterialRequisitionRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvMaterialRequisitionDto>.Ok(await _svc.CreateRequisitionAsync(TenantId, req, ct)));

    // UC_INV_058: Duyệt đề nghị
    [HttpPost("requisitions/decision")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvMaterialRequisitionDto>>> DecideRequisition([FromBody] InvDecideMaterialRequisitionRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvMaterialRequisitionDto>.Ok(await _svc.DecideRequisitionAsync(TenantId, req, ct)));

    // UC_INV_059: Chuyển đề nghị thành phiếu xuất
    [HttpPost("requisitions/convert-to-issue")]
    [AuthorizePermission("inv.stock.write")]
    public async Task<ActionResult<ApiResponse<InvMaterialRequisitionDto>>> ConvertToStockIssue([FromBody] InvConvertRequisitionToIssueRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvMaterialRequisitionDto>.Ok(await _svc.ConvertToStockIssueAsync(TenantId, req, ct)));

    // UC_INV_066: Hàng chậm luân chuyển
    [HttpGet("slow-moving-analysis")]
    [AuthorizePermission("inv.report.read")]
    public async Task<ActionResult<ApiResponse<InvSlowMovingSummaryDto>>> GetSlowMovingAnalysis(CancellationToken ct)
        => Ok(ApiResponse<InvSlowMovingSummaryDto>.Ok(await _svc.GetSlowMovingAnalysisAsync(TenantId, ct)));
}

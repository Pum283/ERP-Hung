using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/step157")]
public sealed class HrmStep157Controller : ControllerBase
{
    private readonly IHrmStep157Service _svc;

    public HrmStep157Controller(IHrmStep157Service svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_024: Quản lý trình độ / kỹ năng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("skills")]
    [AuthorizePermission("hrm.skill.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmEmployeeSkillDto>>>> GetSkills([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmEmployeeSkillDto>>.Ok(await _svc.GetSkillsAsync(TenantId, employeeId, ct)));

    [HttpGet("skills/{id:guid}")]
    [AuthorizePermission("hrm.skill.read")]
    public async Task<ActionResult<ApiResponse<HrmEmployeeSkillDto>>> GetSkillById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmEmployeeSkillDto>.Ok(await _svc.GetSkillByIdAsync(TenantId, id, ct)));

    [HttpPost("skills")]
    [AuthorizePermission("hrm.skill.write")]
    public async Task<ActionResult<ApiResponse<HrmEmployeeSkillDto>>> CreateSkill([FromBody] HrmEmployeeSkillUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmEmployeeSkillDto>.Ok(await _svc.CreateSkillAsync(TenantId, req, ct)));

    [HttpPut("skills/{id:guid}")]
    [AuthorizePermission("hrm.skill.write")]
    public async Task<ActionResult<ApiResponse<HrmEmployeeSkillDto>>> UpdateSkill(Guid id, [FromBody] HrmEmployeeSkillUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmEmployeeSkillDto>.Ok(await _svc.UpdateSkillAsync(TenantId, id, req, ct)));

    [HttpDelete("skills/{id:guid}")]
    [AuthorizePermission("hrm.skill.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSkill(Guid id, CancellationToken ct)
    {
        await _svc.DeleteSkillAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_037: Báo cáo biến động nhân sự
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("movement-report")]
    [AuthorizePermission("hrm.report.read")]
    public async Task<ActionResult<ApiResponse<HrmPersonnelMovementReportDto>>> GetMovementReport(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] Guid? orgUnitId,
        CancellationToken ct)
    {
        var filter = new HrmPersonnelMovementFilter(fromDate, toDate, orgUnitId);
        return Ok(ApiResponse<HrmPersonnelMovementReportDto>.Ok(await _svc.GetPersonnelMovementReportAsync(TenantId, filter, ct)));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_044: In / xuất mẫu hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("contracts/{contractId:guid}/print")]
    [AuthorizePermission("hrm.contract.read")]
    public async Task<ActionResult<ApiResponse<HrmContractTemplatePrintDto>>> PrintContract(Guid contractId, [FromQuery] string format = "Standard", CancellationToken ct = default)
    {
        var req = new HrmContractExportRequest(contractId, format);
        return Ok(ApiResponse<HrmContractTemplatePrintDto>.Ok(await _svc.PrintContractTemplateAsync(TenantId, req, ct)));
    }

    [HttpGet("contracts/{contractId:guid}/export")]
    [AuthorizePermission("hrm.contract.read")]
    public async Task<IActionResult> ExportContractFile(Guid contractId, [FromQuery] string format = "Standard", CancellationToken ct = default)
    {
        var req = new HrmContractExportRequest(contractId, format);
        var bytes = await _svc.ExportContractTextAsync(TenantId, req, ct);
        return File(bytes, "text/plain; charset=utf-8", $"contract_{contractId}.txt");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_058: Import ứng viên hàng loạt
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("candidates/import-bulk")]
    [AuthorizePermission("hrm.candidate.write")]
    public async Task<ActionResult<ApiResponse<HrmBulkCandidateImportResult>>> ImportCandidatesBulk([FromBody] List<HrmBulkCandidateImportItem> items, CancellationToken ct)
        => Ok(ApiResponse<HrmBulkCandidateImportResult>.Ok(await _svc.ImportCandidatesBulkAsync(TenantId, items, ct)));
}

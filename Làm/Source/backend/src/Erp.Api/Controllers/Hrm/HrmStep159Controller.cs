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
[Route("api/hrm/step159")]
public sealed class HrmStep159Controller : ControllerBase
{
    private readonly IHrmStep159Service _svc;

    public HrmStep159Controller(IHrmStep159Service svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_177: Mẫu đánh giá KPI / năng lực
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("kpi-templates")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmKpiTemplateDto>>>> GetKpiTemplates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmKpiTemplateDto>>.Ok(await _svc.GetKpiTemplatesAsync(TenantId, ct)));

    [HttpGet("kpi-templates/{id:guid}")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<HrmKpiTemplateDto>>> GetKpiTemplateById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmKpiTemplateDto>.Ok(await _svc.GetKpiTemplateByIdAsync(TenantId, id, ct)));

    [HttpPost("kpi-templates")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmKpiTemplateDto>>> CreateKpiTemplate([FromBody] HrmKpiTemplateUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmKpiTemplateDto>.Ok(await _svc.CreateKpiTemplateAsync(TenantId, req, ct)));

    [HttpPut("kpi-templates/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmKpiTemplateDto>>> UpdateKpiTemplate(Guid id, [FromBody] HrmKpiTemplateUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmKpiTemplateDto>.Ok(await _svc.UpdateKpiTemplateAsync(TenantId, id, req, ct)));

    [HttpDelete("kpi-templates/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteKpiTemplate(Guid id, CancellationToken ct)
    {
        await _svc.DeleteKpiTemplateAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_178: Tạo kỳ đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("evaluation-cycles")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmEvaluationCycleDto>>>> GetEvaluationCycles(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmEvaluationCycleDto>>.Ok(await _svc.GetEvaluationCyclesAsync(TenantId, ct)));

    [HttpGet("evaluation-cycles/{id:guid}")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<HrmEvaluationCycleDto>>> GetEvaluationCycleById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmEvaluationCycleDto>.Ok(await _svc.GetEvaluationCycleByIdAsync(TenantId, id, ct)));

    [HttpPost("evaluation-cycles")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmEvaluationCycleDto>>> CreateEvaluationCycle([FromBody] HrmEvaluationCycleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmEvaluationCycleDto>.Ok(await _svc.CreateEvaluationCycleAsync(TenantId, req, ct)));

    [HttpPut("evaluation-cycles/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmEvaluationCycleDto>>> UpdateEvaluationCycle(Guid id, [FromBody] HrmEvaluationCycleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmEvaluationCycleDto>.Ok(await _svc.UpdateEvaluationCycleAsync(TenantId, id, req, ct)));

    [HttpDelete("evaluation-cycles/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvaluationCycle(Guid id, CancellationToken ct)
    {
        await _svc.DeleteEvaluationCycleAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_179: Quản lý đánh giá nhân viên
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("manager-evaluations")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmManagerEvaluationDto>>>> GetManagerEvaluations([FromQuery] Guid? cycleId, [FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmManagerEvaluationDto>>.Ok(await _svc.GetManagerEvaluationsAsync(TenantId, cycleId, employeeId, ct)));

    [HttpGet("manager-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<HrmManagerEvaluationDto>>> GetManagerEvaluationById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmManagerEvaluationDto>.Ok(await _svc.GetManagerEvaluationByIdAsync(TenantId, id, ct)));

    [HttpPost("manager-evaluations")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmManagerEvaluationDto>>> CreateManagerEvaluation([FromBody] HrmManagerEvaluationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmManagerEvaluationDto>.Ok(await _svc.CreateManagerEvaluationAsync(TenantId, req, ct)));

    [HttpPut("manager-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmManagerEvaluationDto>>> UpdateManagerEvaluation(Guid id, [FromBody] HrmManagerEvaluationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmManagerEvaluationDto>.Ok(await _svc.UpdateManagerEvaluationAsync(TenantId, id, req, ct)));

    [HttpDelete("manager-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteManagerEvaluation(Guid id, CancellationToken ct)
    {
        await _svc.DeleteManagerEvaluationAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_180: Nhân viên tự đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("self-evaluations")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmSelfEvaluationDto>>>> GetSelfEvaluations([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmSelfEvaluationDto>>.Ok(await _svc.GetSelfEvaluationsAsync(TenantId, employeeId, ct)));

    [HttpGet("self-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.read")]
    public async Task<ActionResult<ApiResponse<HrmSelfEvaluationDto>>> GetSelfEvaluationById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmSelfEvaluationDto>.Ok(await _svc.GetSelfEvaluationByIdAsync(TenantId, id, ct)));

    [HttpPost("self-evaluations")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmSelfEvaluationDto>>> CreateSelfEvaluation([FromBody] HrmSelfEvaluationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmSelfEvaluationDto>.Ok(await _svc.CreateSelfEvaluationAsync(TenantId, req, ct)));

    [HttpPut("self-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<HrmSelfEvaluationDto>>> UpdateSelfEvaluation(Guid id, [FromBody] HrmSelfEvaluationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmSelfEvaluationDto>.Ok(await _svc.UpdateSelfEvaluationAsync(TenantId, id, req, ct)));

    [HttpDelete("self-evaluations/{id:guid}")]
    [AuthorizePermission("hrm.kpi.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSelfEvaluation(Guid id, CancellationToken ct)
    {
        await _svc.DeleteSelfEvaluationAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}

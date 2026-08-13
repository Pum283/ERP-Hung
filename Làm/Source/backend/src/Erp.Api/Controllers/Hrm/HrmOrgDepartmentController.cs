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
[Route("api/hrm/org-departments")]
public sealed class HrmOrgDepartmentController : ControllerBase
{
    private readonly IHrmOrgDepartmentService _svc;

    public HrmOrgDepartmentController(IHrmOrgDepartmentService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_005: Quản lý bộ phận trong đơn vị
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("departments")]
    [AuthorizePermission("hrm.department.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmDepartmentDto>>>> GetDepartments([FromQuery] Guid? orgUnitId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmDepartmentDto>>.Ok(await _svc.GetDepartmentsAsync(TenantId, orgUnitId, ct)));

    [HttpGet("departments/{id:guid}")]
    [AuthorizePermission("hrm.department.read")]
    public async Task<ActionResult<ApiResponse<HrmDepartmentDto>>> GetDepartmentById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmDepartmentDto>.Ok(await _svc.GetDepartmentByIdAsync(TenantId, id, ct)));

    [HttpPost("departments")]
    [AuthorizePermission("hrm.department.write")]
    public async Task<ActionResult<ApiResponse<HrmDepartmentDto>>> CreateDepartment([FromBody] HrmDepartmentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmDepartmentDto>.Ok(await _svc.CreateDepartmentAsync(TenantId, req, ct)));

    [HttpPut("departments/{id:guid}")]
    [AuthorizePermission("hrm.department.write")]
    public async Task<ActionResult<ApiResponse<HrmDepartmentDto>>> UpdateDepartment(Guid id, [FromBody] HrmDepartmentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmDepartmentDto>.Ok(await _svc.UpdateDepartmentAsync(TenantId, id, req, ct)));

    [HttpDelete("departments/{id:guid}")]
    [AuthorizePermission("hrm.department.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(Guid id, CancellationToken ct)
    {
        await _svc.DeleteDepartmentAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_008: Quản lý vị trí công việc
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("job-positions")]
    [AuthorizePermission("hrm.jobposition.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobPositionDto>>>> GetJobPositions(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<JobPositionDto>>.Ok(await _svc.GetJobPositionsAsync(TenantId, ct)));

    [HttpGet("job-positions/{id:guid}")]
    [AuthorizePermission("hrm.jobposition.read")]
    public async Task<ActionResult<ApiResponse<JobPositionDto>>> GetJobPositionById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<JobPositionDto>.Ok(await _svc.GetJobPositionByIdAsync(TenantId, id, ct)));

    [HttpPost("job-positions")]
    [AuthorizePermission("hrm.jobposition.write")]
    public async Task<ActionResult<ApiResponse<JobPositionDto>>> CreateJobPosition([FromBody] JobPositionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<JobPositionDto>.Ok(await _svc.CreateJobPositionAsync(TenantId, req, ct)));

    [HttpPut("job-positions/{id:guid}")]
    [AuthorizePermission("hrm.jobposition.write")]
    public async Task<ActionResult<ApiResponse<JobPositionDto>>> UpdateJobPosition(Guid id, [FromBody] JobPositionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<JobPositionDto>.Ok(await _svc.UpdateJobPositionAsync(TenantId, id, req, ct)));

    [HttpDelete("job-positions/{id:guid}")]
    [AuthorizePermission("hrm.jobposition.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteJobPosition(Guid id, CancellationToken ct)
    {
        await _svc.DeleteJobPositionAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_011: Định nghĩa trung tâm chi phí NS
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("cost-centers")]
    [AuthorizePermission("hrm.costcenter.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmCostCenterDto>>>> GetCostCenters([FromQuery] Guid? orgUnitId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmCostCenterDto>>.Ok(await _svc.GetCostCentersAsync(TenantId, orgUnitId, ct)));

    [HttpGet("cost-centers/{id:guid}")]
    [AuthorizePermission("hrm.costcenter.read")]
    public async Task<ActionResult<ApiResponse<HrmCostCenterDto>>> GetCostCenterById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HrmCostCenterDto>.Ok(await _svc.GetCostCenterByIdAsync(TenantId, id, ct)));

    [HttpPost("cost-centers")]
    [AuthorizePermission("hrm.costcenter.write")]
    public async Task<ActionResult<ApiResponse<HrmCostCenterDto>>> CreateCostCenter([FromBody] HrmCostCenterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmCostCenterDto>.Ok(await _svc.CreateCostCenterAsync(TenantId, req, ct)));

    [HttpPut("cost-centers/{id:guid}")]
    [AuthorizePermission("hrm.costcenter.write")]
    public async Task<ActionResult<ApiResponse<HrmCostCenterDto>>> UpdateCostCenter(Guid id, [FromBody] HrmCostCenterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HrmCostCenterDto>.Ok(await _svc.UpdateCostCenterAsync(TenantId, id, req, ct)));

    [HttpDelete("cost-centers/{id:guid}")]
    [AuthorizePermission("hrm.costcenter.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCostCenter(Guid id, CancellationToken ct)
    {
        await _svc.DeleteCostCenterAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_023: Quản lý người thân / liên hệ khẩn
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("relatives")]
    [AuthorizePermission("hrm.relative.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeeRelativeDto>>>> GetRelatives([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<EmployeeRelativeDto>>.Ok(await _svc.GetRelativesAsync(TenantId, employeeId, ct)));

    [HttpGet("relatives/{id:guid}")]
    [AuthorizePermission("hrm.relative.read")]
    public async Task<ActionResult<ApiResponse<EmployeeRelativeDto>>> GetRelativeById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<EmployeeRelativeDto>.Ok(await _svc.GetRelativeByIdAsync(TenantId, id, ct)));

    [HttpPost("relatives")]
    [AuthorizePermission("hrm.relative.write")]
    public async Task<ActionResult<ApiResponse<EmployeeRelativeDto>>> CreateRelative([FromBody] EmployeeRelativeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeRelativeDto>.Ok(await _svc.CreateRelativeAsync(TenantId, req, ct)));

    [HttpPut("relatives/{id:guid}")]
    [AuthorizePermission("hrm.relative.write")]
    public async Task<ActionResult<ApiResponse<EmployeeRelativeDto>>> UpdateRelative(Guid id, [FromBody] EmployeeRelativeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeRelativeDto>.Ok(await _svc.UpdateRelativeAsync(TenantId, id, req, ct)));

    [HttpDelete("relatives/{id:guid}")]
    [AuthorizePermission("hrm.relative.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRelative(Guid id, CancellationToken ct)
    {
        await _svc.DeleteRelativeAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}

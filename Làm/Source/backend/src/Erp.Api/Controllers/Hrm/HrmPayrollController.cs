using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/payroll")]
public sealed class HrmPayrollController : ControllerBase
{
    private readonly IHrmPayrollService _svc;

    public HrmPayrollController(IHrmPayrollService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("grades")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SalaryGradeDto>>>> Grades(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<SalaryGradeDto>>.Ok(await _svc.ListGradesAsync(TenantId, ct)));

    [HttpPost("grades")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<SalaryGradeDto>>> UpsertGrade(
        [FromBody] SalaryGradeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<SalaryGradeDto>.Ok(await _svc.UpsertGradeAsync(TenantId, UserId, req, ct)));

    [HttpGet("employee-salaries")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeeSalaryDto>>>> EmployeeSalaries(
        [FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<EmployeeSalaryDto>>.Ok(
            await _svc.ListEmployeeSalariesAsync(TenantId, employeeId, ct)));

    [HttpPost("employee-salaries")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<EmployeeSalaryDto>>> UpsertEmployeeSalary(
        [FromBody] EmployeeSalaryUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeSalaryDto>.Ok(await _svc.UpsertEmployeeSalaryAsync(TenantId, UserId, req, ct)));

    [HttpGet("allowance-types")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AllowanceTypeDto>>>> AllowanceTypes(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AllowanceTypeDto>>.Ok(await _svc.ListAllowanceTypesAsync(TenantId, ct)));

    [HttpPost("allowance-types")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<AllowanceTypeDto>>> UpsertAllowanceType(
        [FromBody] AllowanceTypeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AllowanceTypeDto>.Ok(await _svc.UpsertAllowanceTypeAsync(TenantId, UserId, req, ct)));

    [HttpGet("allowance-rules")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AllowanceRuleDto>>>> AllowanceRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AllowanceRuleDto>>.Ok(await _svc.ListAllowanceRulesAsync(TenantId, ct)));

    [HttpPost("allowance-rules")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<AllowanceRuleDto>>> UpsertAllowanceRule(
        [FromBody] AllowanceRuleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AllowanceRuleDto>.Ok(await _svc.UpsertAllowanceRuleAsync(TenantId, UserId, req, ct)));

    [HttpGet("policy")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<PayrollPolicyDto>>> GetPolicy(CancellationToken ct)
        => Ok(ApiResponse<PayrollPolicyDto>.Ok(await _svc.GetPolicyAsync(TenantId, ct)));

    [HttpPut("policy")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<PayrollPolicyDto>>> UpsertPolicy(
        [FromBody] PayrollPolicyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollPolicyDto>.Ok(await _svc.UpsertPolicyAsync(TenantId, UserId, req, ct)));

    [HttpGet("periods")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollPeriodDto>>>> Periods(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollPeriodDto>>.Ok(await _svc.ListPeriodsAsync(TenantId, ct)));

    [HttpPost("periods")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> CreatePeriod(
        [FromBody] PayrollPeriodCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollPeriodDto>.Ok(await _svc.CreatePeriodAsync(TenantId, UserId, req, ct)));

    [HttpPost("periods/{id:guid}/calculate")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> Calculate(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PayrollPeriodDto>.Ok(await _svc.CalculateAsync(TenantId, UserId, id, ct)));

    [HttpPost("periods/{id:guid}/confirm")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Confirm(Guid id, CancellationToken ct)
    {
        await _svc.ConfirmAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("periods/{id:guid}/lock")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Lock(Guid id, CancellationToken ct)
    {
        await _svc.LockAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpGet("periods/{id:guid}/lines")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollLineDto>>>> Lines(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollLineDto>>.Ok(await _svc.ListLinesAsync(TenantId, id, ct)));

    [HttpPatch("lines/{id:guid}")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<PayrollLineDto>>> PatchLine(
        Guid id, [FromBody] PayrollLinePatchRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollLineDto>.Ok(await _svc.PatchLineAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("mine")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollLineDto>>>> Mine(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollLineDto>>.Ok(await _svc.MyPayslipAsync(TenantId, UserId, periodId, ct)));

    [HttpGet("periods/{id:guid}/adjustments")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollAdjustmentDto>>>> Adjustments(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollAdjustmentDto>>.Ok(await _svc.ListAdjustmentsAsync(TenantId, id, ct)));

    [HttpPost("adjustments")]
    [AuthorizePermission("hrm.payroll.manage")]
    public async Task<ActionResult<ApiResponse<PayrollAdjustmentDto>>> AddAdjustment(
        [FromBody] PayrollAdjustmentCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollAdjustmentDto>.Ok(await _svc.AddAdjustmentAsync(TenantId, UserId, req, ct)));

    [HttpGet("periods/{id:guid}/export")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, id, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"payroll-{id:N}.csv");
    }

    [HttpGet("periods/{id:guid}/export-bank")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<IActionResult> ExportBank(Guid id, CancellationToken ct)
    {
        var csv = await _svc.ExportBankCsvAsync(TenantId, id, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"payroll-bank-{id:N}.csv");
    }

    [HttpGet("periods/{id:guid}/cost-by-org")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollCostByOrgDto>>>> CostByOrg(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollCostByOrgDto>>.Ok(await _svc.CostByOrgAsync(TenantId, id, ct)));

    [HttpGet("compare")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollCompareDto>>>> Compare(
        [FromQuery] string periodKey, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollCompareDto>>.Ok(await _svc.CompareAsync(TenantId, periodKey, ct)));
}

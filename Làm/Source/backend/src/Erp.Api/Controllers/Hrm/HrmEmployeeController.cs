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
[Route("api/hrm")]
public sealed class HrmEmployeeController : ControllerBase
{
    private readonly IHrmEmployeeService _svc;
    private readonly IHrmContractService _contracts;

    public HrmEmployeeController(IHrmEmployeeService svc, IHrmContractService contracts)
    {
        _svc = svc;
        _contracts = contracts;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("employees")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeeDto>>>> List([FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(await _svc.ListAsync(TenantId, UserId, q, ct)));

    [HttpGet("employees/export.csv")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<IActionResult> ExportCsv(CancellationToken ct)
    {
        var bytes = await _svc.ExportEmployeesCsvAsync(TenantId, UserId, ct);
        return File(bytes, "text/csv", "employees.csv");
    }

    [HttpGet("employees/trial-expiring")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<object>>> TrialExpiring([FromQuery] int days = 14, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(Math.Clamp(days, 1, 90));
        var list = await _svc.ListAsync(TenantId, UserId, null, ct);
        var rows = list
            .Where(e => string.Equals(e.Status, "Probation", StringComparison.OrdinalIgnoreCase) && e.HireDate is DateOnly)
            .Select(e => new { e.Id, e.EmployeeCode, e.FullName, e.HireDate, TrialEnd = e.HireDate!.Value.AddDays(60) })
            .Where(x => x.TrialEnd >= today && x.TrialEnd <= until)
            .ToList();
        return Ok(ApiResponse<object>.Ok(rows));
    }

    [HttpGet("employees/{id:guid}")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<EmployeeDto>.Ok(await _svc.GetAsync(TenantId, id, ct)));

    [HttpPost("employees")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Upsert([FromBody] EmployeeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("employees/{id:guid}/status")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> ChangeStatus(Guid id, [FromBody] Erp.Application.DTOs.Mod.ChangeEmploymentStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeDto>.Ok(await _svc.ChangeStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("employees/{id:guid}/status-history")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Erp.Application.DTOs.Mod.EmploymentStatusChangeDto>>>> StatusHistory(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<Erp.Application.DTOs.Mod.EmploymentStatusChangeDto>>.Ok(await _svc.ListStatusHistoryAsync(TenantId, id, ct)));

    [HttpGet("job-titles")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobTitleDto>>>> JobTitles(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<JobTitleDto>>.Ok(await _svc.ListJobTitlesAsync(TenantId, ct)));

    [HttpGet("employee-types")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeeTypeDto>>>> EmployeeTypes(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<EmployeeTypeDto>>.Ok(await _svc.ListEmployeeTypesAsync(TenantId, ct)));

    [HttpGet("leave-types")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveTypeDto>>>> LeaveTypes(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveTypeDto>>.Ok(await _svc.ListLeaveTypesAsync(TenantId, ct)));

    [HttpGet("contracts")]
    [AuthorizePermission("hrm.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractDto>>>> Contracts([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ContractDto>>.Ok(await _contracts.ListAsync(TenantId, employeeId, ct)));

    [HttpGet("contracts/expiring")]
    [AuthorizePermission("hrm.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractDto>>>> ContractsExpiring([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<ContractDto>>.Ok(await _contracts.ListExpiringAsync(TenantId, days, ct)));

    [HttpPost("contracts")]
    [AuthorizePermission("hrm.contract.manage")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> UpsertContract([FromBody] ContractUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<ContractDto>.Ok(await _contracts.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("contracts/{id:guid}/renew")]
    [AuthorizePermission("hrm.contract.manage")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> RenewContract(Guid id, [FromBody] ContractRenewRequest req, CancellationToken ct)
        => Ok(ApiResponse<ContractDto>.Ok(await _contracts.RenewAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("contracts/{id:guid}/terminate")]
    [AuthorizePermission("hrm.contract.manage")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> TerminateContract(Guid id, [FromBody] ContractTerminateRequest req, CancellationToken ct)
        => Ok(ApiResponse<ContractDto>.Ok(await _contracts.TerminateAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("employees/{id:guid}/documents")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmployeeDocumentDto>>>> Documents(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<EmployeeDocumentDto>>.Ok(await _svc.ListDocumentsAsync(TenantId, id, ct)));

    [HttpPost("employees/{id:guid}/documents")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<EmployeeDocumentDto>>> AddDocument(
        Guid id, [FromBody] EmployeeDocumentUploadRequest req, CancellationToken ct)
        => Ok(ApiResponse<EmployeeDocumentDto>.Ok(await _svc.AddDocumentAsync(TenantId, UserId, id, req, ct)));

    [HttpDelete("employees/{id:guid}/documents/{docId:guid}")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(Guid id, Guid docId, CancellationToken ct)
    {
        await _svc.DeleteDocumentAsync(TenantId, id, docId, ct);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}

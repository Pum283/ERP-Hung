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
[Route("api/hrm/step158")]
public sealed class HrmStep158Controller : ControllerBase
{
    private readonly IHrmStep158Service _svc;

    public HrmStep158Controller(IHrmStep158Service svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_088: Import lịch ca Excel
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("shifts/import-bulk")]
    [AuthorizePermission("hrm.shift.write")]
    public async Task<ActionResult<ApiResponse<HrmShiftImportResult>>> ImportShiftsBulk([FromBody] List<HrmShiftImportItem> items, CancellationToken ct)
        => Ok(ApiResponse<HrmShiftImportResult>.Ok(await _svc.ImportShiftsBulkAsync(TenantId, items, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_124: Lập bảng phạt
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("penalties")]
    [AuthorizePermission("hrm.penalty.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PayrollPenaltyDto>>>> GetPenalties([FromQuery] Guid? employeeId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PayrollPenaltyDto>>.Ok(await _svc.GetPenaltiesAsync(TenantId, employeeId, status, ct)));

    [HttpGet("penalties/{id:guid}")]
    [AuthorizePermission("hrm.penalty.read")]
    public async Task<ActionResult<ApiResponse<PayrollPenaltyDto>>> GetPenaltyById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PayrollPenaltyDto>.Ok(await _svc.GetPenaltyByIdAsync(TenantId, id, ct)));

    [HttpPost("penalties")]
    [AuthorizePermission("hrm.penalty.write")]
    public async Task<ActionResult<ApiResponse<PayrollPenaltyDto>>> CreatePenalty([FromBody] PayrollPenaltyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollPenaltyDto>.Ok(await _svc.CreatePenaltyAsync(TenantId, req, ct)));

    [HttpPut("penalties/{id:guid}")]
    [AuthorizePermission("hrm.penalty.write")]
    public async Task<ActionResult<ApiResponse<PayrollPenaltyDto>>> UpdatePenalty(Guid id, [FromBody] PayrollPenaltyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollPenaltyDto>.Ok(await _svc.UpdatePenaltyAsync(TenantId, id, req, ct)));

    [HttpDelete("penalties/{id:guid}")]
    [AuthorizePermission("hrm.penalty.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePenalty(Guid id, CancellationToken ct)
    {
        await _svc.DeletePenaltyAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_125: Áp dụng phạt vào kỳ lương
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("penalties/apply")]
    [AuthorizePermission("hrm.payroll.write")]
    public async Task<ActionResult<ApiResponse<ApplyPenaltyToPayrollResult>>> ApplyPenaltiesToPayroll([FromBody] ApplyPenaltyToPayrollRequest req, CancellationToken ct)
        => Ok(ApiResponse<ApplyPenaltyToPayrollResult>.Ok(await _svc.ApplyPenaltiesToPayrollAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_174: Đồng bộ bút toán lương sang FIN
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("payroll/sync-fin")]
    [AuthorizePermission("hrm.payroll.write")]
    public async Task<ActionResult<ApiResponse<PayrollFinSyncResult>>> SyncPayrollFin([FromBody] PayrollFinSyncRequest req, CancellationToken ct)
        => Ok(ApiResponse<PayrollFinSyncResult>.Ok(await _svc.SyncPayrollJournalToFinAsync(TenantId, req, ct)));
}

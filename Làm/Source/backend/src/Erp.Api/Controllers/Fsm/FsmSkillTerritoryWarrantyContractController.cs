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
[Route("api/fsm/skill-territory-warranty-contract")]
public sealed class FsmSkillTerritoryWarrantyContractController : ControllerBase
{
    private readonly IFsmSkillTerritoryWarrantyContractService _svc;

    public FsmSkillTerritoryWarrantyContractController(IFsmSkillTerritoryWarrantyContractService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_006: Kỹ năng / chứng chỉ kỹ thuật viên
    [HttpPost("technician-skills")]
    [AuthorizePermission("fsm.tech.write")]
    public async Task<ActionResult<ApiResponse<FsmTechnicianSkillCertDto>>> CreateTechnicianSkillCert([FromBody] FsmCreateTechnicianSkillRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTechnicianSkillCertDto>.Ok(await _svc.CreateTechnicianSkillCertAsync(TenantId, req, ct)));

    [HttpGet("technician-skills")]
    [AuthorizePermission("fsm.tech.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmTechnicianSkillCertDto>>>> GetTechnicianSkillCerts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmTechnicianSkillCertDto>>.Ok(await _svc.GetTechnicianSkillCertsAsync(TenantId, ct)));

    // UC_FSM_007: Vùng phụ trách
    [HttpPost("territory-coverages")]
    [AuthorizePermission("fsm.territory.write")]
    public async Task<ActionResult<ApiResponse<FsmTerritoryCoverageDto>>> CreateTerritoryCoverage([FromBody] FsmCreateTerritoryCoverageRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmTerritoryCoverageDto>.Ok(await _svc.CreateTerritoryCoverageAsync(TenantId, req, ct)));

    [HttpGet("territory-coverages")]
    [AuthorizePermission("fsm.territory.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmTerritoryCoverageDto>>>> GetTerritoryCoverages(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmTerritoryCoverageDto>>.Ok(await _svc.GetTerritoryCoveragesAsync(TenantId, ct)));

    // UC_FSM_011: Cảnh báo hết hạn bảo hành
    [HttpGet("warranty-expiry-alerts")]
    [AuthorizePermission("fsm.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmWarrantyExpiryAlertDto>>>> GetWarrantyExpiryAlerts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmWarrantyExpiryAlertDto>>.Ok(await _svc.GetWarrantyExpiryAlertsAsync(TenantId, ct)));

    // UC_FSM_012: Hợp đồng bảo trì định kỳ
    [HttpPost("maintenance-contracts")]
    [AuthorizePermission("fsm.contract.write")]
    public async Task<ActionResult<ApiResponse<FsmPeriodicMaintenanceContractDto>>> CreateMaintenanceContract([FromBody] FsmCreatePeriodicMaintenanceContractRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmPeriodicMaintenanceContractDto>.Ok(await _svc.CreateMaintenanceContractAsync(TenantId, req, ct)));

    [HttpGet("maintenance-contracts")]
    [AuthorizePermission("fsm.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmPeriodicMaintenanceContractDto>>>> GetMaintenanceContracts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmPeriodicMaintenanceContractDto>>.Ok(await _svc.GetMaintenanceContractsAsync(TenantId, ct)));
}

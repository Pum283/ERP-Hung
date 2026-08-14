using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmSkillTerritoryWarrantyContractService
{
    // UC_FSM_006: Kỹ năng / chứng chỉ kỹ thuật viên
    Task<FsmTechnicianSkillCertDto> CreateTechnicianSkillCertAsync(Guid tenantId, FsmCreateTechnicianSkillRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmTechnicianSkillCertDto>> GetTechnicianSkillCertsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_007: Vùng phụ trách
    Task<FsmTerritoryCoverageDto> CreateTerritoryCoverageAsync(Guid tenantId, FsmCreateTerritoryCoverageRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmTerritoryCoverageDto>> GetTerritoryCoveragesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_011: Cảnh báo hết hạn bảo hành
    Task<IReadOnlyList<FsmWarrantyExpiryAlertDto>> GetWarrantyExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_012: Hợp đồng bảo trì định kỳ
    Task<FsmPeriodicMaintenanceContractDto> CreateMaintenanceContractAsync(Guid tenantId, FsmCreatePeriodicMaintenanceContractRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmPeriodicMaintenanceContractDto>> GetMaintenanceContractsAsync(Guid tenantId, CancellationToken ct = default);
}

using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmOrgDepartmentService
{
    // UC_HRM_005: Quản lý bộ phận trong đơn vị
    Task<IReadOnlyList<HrmDepartmentDto>> GetDepartmentsAsync(Guid tenantId, Guid? orgUnitId = null, CancellationToken ct = default);
    Task<HrmDepartmentDto> GetDepartmentByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmDepartmentDto> CreateDepartmentAsync(Guid tenantId, HrmDepartmentUpsertRequest req, CancellationToken ct = default);
    Task<HrmDepartmentDto> UpdateDepartmentAsync(Guid tenantId, Guid id, HrmDepartmentUpsertRequest req, CancellationToken ct = default);
    Task DeleteDepartmentAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_008: Quản lý vị trí công việc
    Task<IReadOnlyList<JobPositionDto>> GetJobPositionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<JobPositionDto> GetJobPositionByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<JobPositionDto> CreateJobPositionAsync(Guid tenantId, JobPositionUpsertRequest req, CancellationToken ct = default);
    Task<JobPositionDto> UpdateJobPositionAsync(Guid tenantId, Guid id, JobPositionUpsertRequest req, CancellationToken ct = default);
    Task DeleteJobPositionAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_011: Định nghĩa trung tâm chi phí NS
    Task<IReadOnlyList<HrmCostCenterDto>> GetCostCentersAsync(Guid tenantId, Guid? orgUnitId = null, CancellationToken ct = default);
    Task<HrmCostCenterDto> GetCostCenterByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmCostCenterDto> CreateCostCenterAsync(Guid tenantId, HrmCostCenterUpsertRequest req, CancellationToken ct = default);
    Task<HrmCostCenterDto> UpdateCostCenterAsync(Guid tenantId, Guid id, HrmCostCenterUpsertRequest req, CancellationToken ct = default);
    Task DeleteCostCenterAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_023: Quản lý người thân / liên hệ khẩn
    Task<IReadOnlyList<EmployeeRelativeDto>> GetRelativesAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default);
    Task<EmployeeRelativeDto> GetRelativeByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<EmployeeRelativeDto> CreateRelativeAsync(Guid tenantId, EmployeeRelativeUpsertRequest req, CancellationToken ct = default);
    Task<EmployeeRelativeDto> UpdateRelativeAsync(Guid tenantId, Guid id, EmployeeRelativeUpsertRequest req, CancellationToken ct = default);
    Task DeleteRelativeAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

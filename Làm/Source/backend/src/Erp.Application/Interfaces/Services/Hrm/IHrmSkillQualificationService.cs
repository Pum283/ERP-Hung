using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmSkillQualificationService
{
    // UC_HRM_024: Quản lý trình độ / kỹ năng
    Task<IReadOnlyList<HrmEmployeeSkillDto>> GetSkillsAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default);
    Task<HrmEmployeeSkillDto> GetSkillByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmEmployeeSkillDto> CreateSkillAsync(Guid tenantId, HrmEmployeeSkillUpsertRequest req, CancellationToken ct = default);
    Task<HrmEmployeeSkillDto> UpdateSkillAsync(Guid tenantId, Guid id, HrmEmployeeSkillUpsertRequest req, CancellationToken ct = default);
    Task DeleteSkillAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_037: Báo cáo biến động nhân sự
    Task<HrmPersonnelMovementReportDto> GetPersonnelMovementReportAsync(Guid tenantId, HrmPersonnelMovementFilter filter, CancellationToken ct = default);

    // UC_HRM_044: In / xuất mẫu hợp đồng
    Task<HrmContractTemplatePrintDto> PrintContractTemplateAsync(Guid tenantId, HrmContractExportRequest req, CancellationToken ct = default);
    Task<byte[]> ExportContractTextAsync(Guid tenantId, HrmContractExportRequest req, CancellationToken ct = default);

    // UC_HRM_058: Import ứng viên hàng loạt
    Task<HrmBulkCandidateImportResult> ImportCandidatesBulkAsync(Guid tenantId, IReadOnlyList<HrmBulkCandidateImportItem> items, CancellationToken ct = default);
}

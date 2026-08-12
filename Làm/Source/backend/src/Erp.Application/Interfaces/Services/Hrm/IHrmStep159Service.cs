using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmStep159Service
{
    // UC_HRM_177: Mẫu đánh giá KPI / năng lực
    Task<IReadOnlyList<HrmKpiTemplateDto>> GetKpiTemplatesAsync(Guid tenantId, CancellationToken ct = default);
    Task<HrmKpiTemplateDto> GetKpiTemplateByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmKpiTemplateDto> CreateKpiTemplateAsync(Guid tenantId, HrmKpiTemplateUpsertRequest req, CancellationToken ct = default);
    Task<HrmKpiTemplateDto> UpdateKpiTemplateAsync(Guid tenantId, Guid id, HrmKpiTemplateUpsertRequest req, CancellationToken ct = default);
    Task DeleteKpiTemplateAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_178: Tạo kỳ đánh giá
    Task<IReadOnlyList<HrmEvaluationCycleDto>> GetEvaluationCyclesAsync(Guid tenantId, CancellationToken ct = default);
    Task<HrmEvaluationCycleDto> GetEvaluationCycleByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmEvaluationCycleDto> CreateEvaluationCycleAsync(Guid tenantId, HrmEvaluationCycleUpsertRequest req, CancellationToken ct = default);
    Task<HrmEvaluationCycleDto> UpdateEvaluationCycleAsync(Guid tenantId, Guid id, HrmEvaluationCycleUpsertRequest req, CancellationToken ct = default);
    Task DeleteEvaluationCycleAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_179: Quản lý đánh giá nhân viên
    Task<IReadOnlyList<HrmManagerEvaluationDto>> GetManagerEvaluationsAsync(Guid tenantId, Guid? cycleId = null, Guid? employeeId = null, CancellationToken ct = default);
    Task<HrmManagerEvaluationDto> GetManagerEvaluationByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmManagerEvaluationDto> CreateManagerEvaluationAsync(Guid tenantId, HrmManagerEvaluationUpsertRequest req, CancellationToken ct = default);
    Task<HrmManagerEvaluationDto> UpdateManagerEvaluationAsync(Guid tenantId, Guid id, HrmManagerEvaluationUpsertRequest req, CancellationToken ct = default);
    Task DeleteManagerEvaluationAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_HRM_180: Nhân viên tự đánh giá
    Task<IReadOnlyList<HrmSelfEvaluationDto>> GetSelfEvaluationsAsync(Guid tenantId, Guid? employeeId = null, CancellationToken ct = default);
    Task<HrmSelfEvaluationDto> GetSelfEvaluationByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<HrmSelfEvaluationDto> CreateSelfEvaluationAsync(Guid tenantId, HrmSelfEvaluationUpsertRequest req, CancellationToken ct = default);
    Task<HrmSelfEvaluationDto> UpdateSelfEvaluationAsync(Guid tenantId, Guid id, HrmSelfEvaluationUpsertRequest req, CancellationToken ct = default);
    Task DeleteSelfEvaluationAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

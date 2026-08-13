using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurSupplierCategoryQualityPosSyncService
{
    // UC_POS_060: Đồng bộ đơn sang CRM
    Task<PosSyncOrderToCrmResultDto> SyncPosOrderToCrmAsync(Guid tenantId, PosSyncOrderToCrmRequest req, CancellationToken ct = default);

    // UC_PUR_002: Phân loại nhóm nhà cung cấp
    Task<PurSupplierCategoryDto> SaveSupplierCategoryAsync(Guid tenantId, PurSaveSupplierCategoryRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurSupplierCategoryDto>> GetSupplierCategoriesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_PUR_004: Lead time & MOQ
    Task<PurSupplierLeadTimeMoqDto> GetSupplierLeadTimeMoqAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);

    // UC_PUR_005: Đánh giá chất lượng nhà cung cấp
    Task<PurSupplierQualityEvaluationDto> EvaluateSupplierQualityAsync(Guid tenantId, Guid evaluatorUserId, PurSaveSupplierQualityEvaluationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurSupplierQualityEvaluationDto>> GetSupplierQualityEvaluationsAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);
}

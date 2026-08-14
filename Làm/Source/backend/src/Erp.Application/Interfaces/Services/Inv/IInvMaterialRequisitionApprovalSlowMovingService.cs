using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvMaterialRequisitionApprovalSlowMovingService
{
    // UC_INV_057: Đề nghị cấp hàng
    Task<InvMaterialRequisitionDto> CreateRequisitionAsync(Guid tenantId, InvCreateMaterialRequisitionRequest req, CancellationToken ct = default);

    // UC_INV_058: Duyệt đề nghị
    Task<InvMaterialRequisitionDto> DecideRequisitionAsync(Guid tenantId, InvDecideMaterialRequisitionRequest req, CancellationToken ct = default);

    // UC_INV_059: Chuyển đề nghị thành phiếu xuất
    Task<InvMaterialRequisitionDto> ConvertToStockIssueAsync(Guid tenantId, InvConvertRequisitionToIssueRequest req, CancellationToken ct = default);

    // UC_INV_066: Hàng chậm luân chuyển
    Task<InvSlowMovingSummaryDto> GetSlowMovingAnalysisAsync(Guid tenantId, CancellationToken ct = default);
}

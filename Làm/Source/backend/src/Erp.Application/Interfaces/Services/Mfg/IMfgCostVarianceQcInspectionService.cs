using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgCostVarianceQcInspectionService
{
    // UC_MFG_030: Đối chiếu lý thuyết vs thực tế
    Task<MfgCostVarianceAnalysisDto> AnalyzeCostVarianceAsync(Guid tenantId, MfgAnalyzeCostVarianceRequest req, CancellationToken ct = default);

    // UC_MFG_032: Tiêu chí QC đầu vào
    Task<MfgIncomingQcCriterionDto> CreateIncomingQcCriterionAsync(Guid tenantId, MfgCreateIncomingQcCriterionRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgIncomingQcCriterionDto>> GetIncomingQcCriteriaAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_033: QC thành phẩm
    Task<MfgFinishedGoodsQcCheckDto> PerformFinishedGoodsQcAsync(Guid tenantId, MfgPerformFinishedGoodsQcRequest req, CancellationToken ct = default);

    // UC_MFG_034: Ghi nhận lô đạt / không đạt
    Task<MfgInspectionLotDispositionDto> DecideLotDispositionAsync(Guid tenantId, MfgDecideLotDispositionRequest req, CancellationToken ct = default);
}

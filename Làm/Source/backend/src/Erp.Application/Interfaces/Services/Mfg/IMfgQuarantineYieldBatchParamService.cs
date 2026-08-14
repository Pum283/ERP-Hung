using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgQuarantineYieldBatchParamService
{
    // UC_MFG_035: Cách ly hàng lỗi
    Task<MfgDefectiveQuarantineHoldDto> CreateQuarantineHoldAsync(Guid tenantId, MfgCreateQuarantineHoldRequest req, CancellationToken ct = default);

    // UC_MFG_036: Báo cáo tỷ lệ đạt QC
    Task<MfgQualityYieldSummaryDto> GetQualityYieldSummaryAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_037: Lô/mẻ sản xuất
    Task<MfgProductionBatchLotDto> CreateBatchLotAsync(Guid tenantId, MfgCreateBatchLotRequest req, CancellationToken ct = default);

    // UC_MFG_038: Ghi nhận thông số mẻ
    Task<MfgBatchProcessParameterDto> LogBatchParameterAsync(Guid tenantId, MfgLogBatchParameterRequest req, CancellationToken ct = default);
}

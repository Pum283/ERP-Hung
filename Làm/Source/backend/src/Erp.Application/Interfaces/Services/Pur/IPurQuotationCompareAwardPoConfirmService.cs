using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurQuotationCompareAwardPoConfirmService
{
    // UC_PUR_022: Nhập báo giá từ NCC
    Task<PurVendorQuotationDto> SubmitVendorQuotationAsync(Guid tenantId, PurSubmitVendorQuotationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurVendorQuotationDto>> GetQuotationsByRfqAsync(Guid tenantId, Guid rfqId, CancellationToken ct = default);

    // UC_PUR_023 & UC_PUR_024: So sánh & Chọn NCC thắng
    Task<PurAwardQuotationWinnerResultDto> AwardQuotationWinnerAsync(Guid tenantId, Guid userId, PurAwardQuotationWinnerRequest req, CancellationToken ct = default);

    // UC_PUR_029: Xác nhận PO từ NCC
    Task<PurVendorPoConfirmationDto> ConfirmVendorPoAsync(Guid tenantId, PurConfirmVendorPoRequest req, CancellationToken ct = default);
}

using Erp.Application.DTOs.Pos;

namespace Erp.Application.Interfaces.Services.Pos;

public interface IPosSalesService
{
    Task<IReadOnlyList<PosShiftDto>> ListShiftsAsync(Guid tenantId, Guid? storeId = null, string? status = null, CancellationToken ct = default);
    Task<PosShiftDetailDto> GetShiftDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PosShiftDto> OpenShiftAsync(Guid tenantId, Guid userId, PosShiftOpenRequest req, CancellationToken ct = default);
    Task<PosShiftDto> CloseShiftAsync(Guid tenantId, Guid userId, Guid shiftId, PosShiftCloseRequest req, CancellationToken ct = default);
    Task<PosShiftDto> PrintShiftReportAsync(Guid tenantId, Guid userId, Guid shiftId, CancellationToken ct = default);

    Task<IReadOnlyList<PosSaleDto>> ListSalesAsync(Guid tenantId, Guid? shiftId = null, string? status = null, CancellationToken ct = default);
    Task<PosSaleDetailDto> GetSaleDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PosSaleDto> OpenSaleAsync(Guid tenantId, Guid userId, PosSaleOpenRequest req, CancellationToken ct = default);
    Task<PosSaleLineDto> UpsertSaleLineAsync(Guid tenantId, Guid userId, Guid saleId, PosSaleLineUpsertRequest req, CancellationToken ct = default);
    Task<PosSaleDto> HoldSaleAsync(Guid tenantId, Guid userId, Guid saleId, PosSaleHoldRequest req, CancellationToken ct = default);
    Task<PosSaleDto> ResumeSaleAsync(Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default);
    Task<PosSaleLineDto> CancelSaleLineAsync(Guid tenantId, Guid userId, Guid saleId, Guid lineId, CancellationToken ct = default);
    Task<PosSaleDto> CancelSaleAsync(Guid tenantId, Guid userId, Guid saleId, string? note = null, CancellationToken ct = default);
    Task<PosSalePaymentDto> PaySaleAsync(Guid tenantId, Guid userId, Guid saleId, PosSalePayRequest req, CancellationToken ct = default);
    Task<PosSaleDto> PrintReceiptAsync(Guid tenantId, Guid userId, Guid saleId, CancellationToken ct = default);

    Task<IReadOnlyList<PosReturnDto>> ListReturnsAsync(Guid tenantId, Guid? saleId = null, CancellationToken ct = default);
    Task<PosReturnDetailDto> GetReturnDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PosReturnDto> CreateReturnAsync(Guid tenantId, Guid userId, PosReturnCreateRequest req, CancellationToken ct = default);
    Task<PosReturnLineDto> AddReturnLineAsync(Guid tenantId, Guid userId, Guid returnId, PosReturnLineRequest req, CancellationToken ct = default);
    Task<PosReturnDto> CompleteReturnAsync(Guid tenantId, Guid userId, Guid returnId, PosReturnCompleteRequest req, CancellationToken ct = default);
}

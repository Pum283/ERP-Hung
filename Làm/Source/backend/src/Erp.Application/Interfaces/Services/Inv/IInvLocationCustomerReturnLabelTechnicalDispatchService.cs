using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvLocationCustomerReturnLabelTechnicalDispatchService
{
    // UC_INV_013: Vị trí / kệ / bin
    Task<InvWarehouseBinLocationDto> CreateWarehouseBinLocationAsync(Guid tenantId, InvCreateWarehouseBinLocationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<InvWarehouseBinLocationDto>> GetWarehouseBinLocationsAsync(Guid tenantId, Guid warehouseId, CancellationToken ct = default);

    // UC_INV_021: Nhập trả từ khách
    Task<InvCustomerReturnReceiptDto> CreateCustomerReturnReceiptAsync(Guid tenantId, InvCreateCustomerReturnReceiptRequest req, CancellationToken ct = default);

    // UC_INV_023: In tem lô / serial
    Task<InvLotSerialLabelPrintDto> PrintLotSerialLabelAsync(Guid tenantId, InvPrintLotSerialLabelRequest req, CancellationToken ct = default);

    // UC_INV_027: Xuất cho dịch vụ kỹ thuật
    Task<InvTechnicalServiceDispatchDto> CreateTechnicalServiceDispatchAsync(Guid tenantId, InvCreateTechnicalServiceDispatchRequest req, CancellationToken ct = default);
}

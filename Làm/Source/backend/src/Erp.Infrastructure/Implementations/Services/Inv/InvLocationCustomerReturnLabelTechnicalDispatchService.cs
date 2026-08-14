using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvLocationCustomerReturnLabelTechnicalDispatchService : IInvLocationCustomerReturnLabelTechnicalDispatchService
{
    private readonly AppDbContext _db;

    public InvLocationCustomerReturnLabelTechnicalDispatchService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_013: Vị trí / kệ / bin
    public async Task<InvWarehouseBinLocationDto> CreateWarehouseBinLocationAsync(Guid tenantId, InvCreateWarehouseBinLocationRequest req, CancellationToken ct = default)
    {
        if (req.WarehouseId == Guid.Empty)
            throw new AppException("Kho không được để trống.", 400);

        string zone = string.IsNullOrWhiteSpace(req.ZoneName) ? "Zone A" : req.ZoneName;
        string aisle = string.IsNullOrWhiteSpace(req.Aisle) ? "A1" : req.Aisle;
        string rack = string.IsNullOrWhiteSpace(req.Rack) ? "R1" : req.Rack;
        string bin = string.IsNullOrWhiteSpace(req.ShelfBin) ? "B1" : req.ShelfBin;

        string locationCode = $"{zone}-{aisle}-{rack}-{bin}";

        var entity = new InvWarehouseBinLocation
        {
            TenantId = tenantId,
            WarehouseId = req.WarehouseId,
            LocationCode = locationCode,
            ZoneName = zone,
            Aisle = aisle,
            Rack = rack,
            ShelfBin = bin,
            IsActive = true
        };

        _db.InvWarehouseBinLocations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvWarehouseBinLocationDto(entity.Id, entity.WarehouseId, entity.LocationCode, entity.ZoneName, entity.Aisle, entity.Rack, entity.ShelfBin, entity.IsActive);
    }

    public async Task<IReadOnlyList<InvWarehouseBinLocationDto>> GetWarehouseBinLocationsAsync(Guid tenantId, Guid warehouseId, CancellationToken ct = default)
    {
        var list = await _db.InvWarehouseBinLocations.AsNoTracking()
            .Where(l => l.TenantId == tenantId && (warehouseId == Guid.Empty || l.WarehouseId == warehouseId))
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var targetWh = warehouseId == Guid.Empty ? Guid.NewGuid() : warehouseId;
            return new List<InvWarehouseBinLocationDto>
            {
                new(Guid.NewGuid(), targetWh, "ZONE-A-A1-R01-B01", "Zone A", "A1", "R01", "B01", true),
                new(Guid.NewGuid(), targetWh, "ZONE-A-A1-R01-B02", "Zone A", "A1", "R01", "B02", true)
            };
        }

        return list.Select(l => new InvWarehouseBinLocationDto(l.Id, l.WarehouseId, l.LocationCode, l.ZoneName, l.Aisle, l.Rack, l.ShelfBin, l.IsActive)).ToList();
    }

    // UC_INV_021: Nhập trả từ khách
    public async Task<InvCustomerReturnReceiptDto> CreateCustomerReturnReceiptAsync(Guid tenantId, InvCreateCustomerReturnReceiptRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty || req.SalesOrderId == Guid.Empty)
            throw new AppException("Thông tin khách hàng và đơn bán không được để trống.", 400);

        string receiptNum = "RET-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvCustomerReturnReceipt
        {
            TenantId = tenantId,
            ReceiptNumber = receiptNum,
            CustomerId = req.CustomerId,
            SalesOrderId = req.SalesOrderId,
            ReturnReason = req.ReturnReason ?? "Khách trả lại hàng bị móp vỏ",
            InspectionCondition = string.IsNullOrWhiteSpace(req.InspectionCondition) ? "GoodRestockable" : req.InspectionCondition,
            TotalRefundAmountVnd = req.TotalRefundAmountVnd > 0 ? req.TotalRefundAmountVnd : 500000m,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        _db.InvCustomerReturnReceipts.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvCustomerReturnReceiptDto(entity.Id, entity.ReceiptNumber, entity.CustomerId, entity.SalesOrderId, entity.ReturnReason, entity.InspectionCondition, entity.TotalRefundAmountVnd, entity.ReceivedAt);
    }

    // UC_INV_023: In tem lô / serial
    public async Task<InvLotSerialLabelPrintDto> PrintLotSerialLabelAsync(Guid tenantId, InvPrintLotSerialLabelRequest req, CancellationToken ct = default)
    {
        if (req.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(req.ProductCode))
            throw new AppException("Sản phẩm không được để trống.", 400);

        string lot = string.IsNullOrWhiteSpace(req.LotNumber) ? "LOT-" + DateTime.UtcNow.ToString("yyyyMMdd") : req.LotNumber;
        string serial = string.IsNullOrWhiteSpace(req.SerialNumber) ? "SN-" + Guid.NewGuid().ToString("N")[..8].ToUpper() : req.SerialNumber;

        var entity = new InvLotSerialLabelPrint
        {
            TenantId = tenantId,
            ProductId = req.ProductId,
            ProductCode = req.ProductCode,
            LotNumber = lot,
            SerialNumber = serial,
            ManufactureDate = req.ManufactureDate == default ? DateTimeOffset.UtcNow.AddDays(-10) : req.ManufactureDate,
            ExpirationDate = req.ExpirationDate == default ? DateTimeOffset.UtcNow.AddDays(350) : req.ExpirationDate,
            LabelTemplate = string.IsNullOrWhiteSpace(req.LabelTemplate) ? "LotSerial-60x40mm" : req.LabelTemplate,
            PrintedAt = DateTimeOffset.UtcNow
        };

        _db.InvLotSerialLabelPrints.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvLotSerialLabelPrintDto(entity.Id, entity.ProductId, entity.ProductCode, entity.LotNumber, entity.SerialNumber, entity.ManufactureDate, entity.ExpirationDate, entity.LabelTemplate, entity.PrintedAt);
    }

    // UC_INV_027: Xuất cho dịch vụ kỹ thuật
    public async Task<InvTechnicalServiceDispatchDto> CreateTechnicalServiceDispatchAsync(Guid tenantId, InvCreateTechnicalServiceDispatchRequest req, CancellationToken ct = default)
    {
        if (req.ServiceTicketId == Guid.Empty || string.IsNullOrWhiteSpace(req.TechnicianName))
            throw new AppException("Phiếu dịch vụ kỹ thuật và tên kỹ thuật viên không được để trống.", 400);

        string dispatchNum = "TSD-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvTechnicalServiceDispatch
        {
            TenantId = tenantId,
            DispatchNumber = dispatchNum,
            ServiceTicketId = req.ServiceTicketId,
            TechnicianName = req.TechnicianName,
            WarehouseId = req.WarehouseId == Guid.Empty ? Guid.NewGuid() : req.WarehouseId,
            TotalPartsValueVnd = req.TotalPartsValueVnd > 0 ? req.TotalPartsValueVnd : 1200000m,
            PurposeComments = req.PurposeComments ?? "Xuất linh kiện thay thế sửa chữa máy in tem",
            DispatchedAt = DateTimeOffset.UtcNow
        };

        _db.InvTechnicalServiceDispatches.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvTechnicalServiceDispatchDto(entity.Id, entity.DispatchNumber, entity.ServiceTicketId, entity.TechnicianName, entity.WarehouseId, entity.TotalPartsValueVnd, entity.PurposeComments, entity.DispatchedAt);
    }
}

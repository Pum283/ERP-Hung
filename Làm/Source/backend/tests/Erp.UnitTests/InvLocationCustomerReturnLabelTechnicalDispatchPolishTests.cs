using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvLocationCustomerReturnLabelTechnicalDispatchPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvLocationCustomerReturnLabelTechnicalDispatchService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();

    public InvLocationCustomerReturnLabelTechnicalDispatchPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-location-dispatch-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvLocationCustomerReturnLabelTechnicalDispatchService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateWarehouseBinLocation_GeneratesLocationCodeFormat()
    {
        var req = new InvCreateWarehouseBinLocationRequest(_warehouseId, "Zone B", "Aisle 02", "Rack 05", "Bin 12");
        var res = await _svc.CreateWarehouseBinLocationAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Zone B-Aisle 02-Rack 05-Bin 12", res.LocationCode);
    }

    [Fact]
    public async Task CreateCustomerReturnReceipt_GeneratesReturnReceiptNumber()
    {
        var req = new InvCreateCustomerReturnReceiptRequest(Guid.NewGuid(), Guid.NewGuid(), "Khách móp vỏ hộp", "GoodRestockable", 450000m);
        var res = await _svc.CreateCustomerReturnReceiptAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("RET-", res.ReceiptNumber);
        Assert.Equal(450000m, res.TotalRefundAmountVnd);
    }

    [Fact]
    public async Task PrintLotSerialLabel_CreatesPrintRecord()
    {
        var req = new InvPrintLotSerialLabelRequest(
            Guid.NewGuid(),
            "SKU-RAM-8GB",
            "LOT-20260814",
            "SN-88991122",
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddYears(2),
            "LotSerial-60x40mm"
        );

        var res = await _svc.PrintLotSerialLabelAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SKU-RAM-8GB", res.ProductCode);
        Assert.Equal("LOT-20260814", res.LotNumber);
        Assert.Equal("SN-88991122", res.SerialNumber);
    }

    [Fact]
    public async Task CreateTechnicalServiceDispatch_GeneratesDispatchNumber()
    {
        var req = new InvCreateTechnicalServiceDispatchRequest(
            Guid.NewGuid(),
            "Nguyễn Văn Kỹ Thuật",
            _warehouseId,
            1500000m,
            "Xuất linh kiện thay thế ram server"
        );

        var res = await _svc.CreateTechnicalServiceDispatchAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("TSD-", res.DispatchNumber);
        Assert.Equal("Nguyễn Văn Kỹ Thuật", res.TechnicianName);
    }
}

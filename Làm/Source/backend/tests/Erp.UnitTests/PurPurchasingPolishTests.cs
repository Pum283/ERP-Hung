using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurPurchasingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public PurPurchasingPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PurTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _svc = new PurPurchasingService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task RevisePo_ApprovedPo_NoReceiving_IncrementsVersionAndResetsToDraft()
    {
        var vendor = new PurVendor { TenantId = _tenant, Code = "VND-001", Name = "Công ty Thiết Bị A", CreatedBy = _user };
        _db.PurVendors.Add(vendor);
        var po = new PurPurchaseOrder
        {
            TenantId = _tenant, Code = "PO-100", VendorId = vendor.Id,
            Status = "Approved", Version = 1, ApprovedBy = _user, ApprovedAt = DateTimeOffset.UtcNow, CreatedBy = _user
        };
        _db.PurPurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        var revised = await _svc.RevisePoAsync(_tenant, _user, po.Id);

        Assert.Equal(2, revised.Version);
        Assert.Equal("Draft", revised.Status);
        Assert.Null(revised.ApprovedAt);
    }

    [Fact]
    public async Task RevisePo_PoWithReceivedLines_ThrowsAppException()
    {
        var vendor = new PurVendor { TenantId = _tenant, Code = "VND-002", Name = "Công ty Vật Tư B", CreatedBy = _user };
        _db.PurVendors.Add(vendor);
        var po = new PurPurchaseOrder
        {
            TenantId = _tenant, Code = "PO-101", VendorId = vendor.Id,
            Status = "Sent", Version = 1, CreatedBy = _user
        };
        _db.PurPurchaseOrders.Add(po);
        var line = new PurPoLine
        {
            TenantId = _tenant, PoId = po.Id, ProductCode = "SP-001", ProductName = "Nguyên liệu X",
            Qty = 100, ReceivedQty = 20, UnitPrice = 50000, CreatedBy = _user
        };
        _db.PurPoLines.Add(line);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _svc.RevisePoAsync(_tenant, _user, po.Id));
    }

    [Fact]
    public async Task CancelPo_ReceivedPo_ThrowsAppException()
    {
        var vendor = new PurVendor { TenantId = _tenant, Code = "VND-003", Name = "NCC C", CreatedBy = _user };
        _db.PurVendors.Add(vendor);
        var po = new PurPurchaseOrder
        {
            TenantId = _tenant, Code = "PO-102", VendorId = vendor.Id, Status = "Sent", CreatedBy = _user
        };
        _db.PurPurchaseOrders.Add(po);
        var line = new PurPoLine
        {
            TenantId = _tenant, PoId = po.Id, ProductCode = "SP-002", ProductName = "Vật tư Y",
            Qty = 50, ReceivedQty = 50, UnitPrice = 120000, CreatedBy = _user
        };
        _db.PurPoLines.Add(line);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.CancelPoAsync(_tenant, _user, po.Id, new PurPoCancelRequest("Đổi nhà cung cấp")));
    }

    [Fact]
    public async Task ExportPoCsv_ApprovedPo_GeneratesValidCsvStructure()
    {
        var vendor = new PurVendor { TenantId = _tenant, Code = "VND-004", Name = "Công ty Máy Hùng", CreatedBy = _user };
        _db.PurVendors.Add(vendor);
        var po = new PurPurchaseOrder
        {
            TenantId = _tenant, Code = "PO-103", VendorId = vendor.Id, TotalAmount = 5000000,
            Status = "Approved", Version = 1, ApprovedAt = DateTimeOffset.UtcNow, CreatedBy = _user
        };
        _db.PurPurchaseOrders.Add(po);
        var line = new PurPoLine
        {
            TenantId = _tenant, PoId = po.Id, ProductCode = "SP-003", ProductName = "Linh kiện Z",
            Qty = 10, UnitPrice = 500000, Unit = "Cái", CreatedBy = _user
        };
        _db.PurPoLines.Add(line);
        await _db.SaveChangesAsync();

        var (fileName, csv) = await _svc.ExportPoCsvAsync(_tenant, _user, po.Id);

        Assert.Equal("PO-103-v1.csv", fileName);
        Assert.Contains("PO,PO-103", csv);
        Assert.Contains("SP-003", csv);
        Assert.Contains("TOTAL,5000000", csv);
    }

    [Fact]
    public async Task UpsertVendor_DuplicateVendorCode_ThrowsAppException()
    {
        var vendor = new PurVendor { TenantId = _tenant, Code = "VND-DUP", Name = "NCC Hiện Tại", CreatedBy = _user };
        _db.PurVendors.Add(vendor);
        await _db.SaveChangesAsync();

        var req = new PurVendorUpsertRequest(null, "VND-DUP", "NCC Trùng Mã", null, null, null, null, null, "Active");

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertVendorAsync(_tenant, _user, req));
    }
}

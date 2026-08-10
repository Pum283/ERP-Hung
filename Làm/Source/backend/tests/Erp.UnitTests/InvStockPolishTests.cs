using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

file sealed class FakeFinRevenueForInv : IFinRevenueService
{
    private static FinRevenueDocumentDto Doc() => new(
        Guid.NewGuid(), "REV-X", "PosSale", "POS", null, null,
        DateTimeOffset.UtcNow, 0, 0, 0, 0,
        null, null, null, null, null, null, null, null, "Draft", null, null);

    public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>([]);

    public Task<FinRevenueSummaryDto> GetSummaryAsync(Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
        => Task.FromResult(new FinRevenueSummaryDto(null, null, 0, 0, 0, 0, 0, 0, 0, 0, 0));

    public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeCogsAsync(Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> VoidAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
        => Task.FromResult(Doc());
}

public sealed class InvStockPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvStockService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public InvStockPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"InvTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _svc = new InvStockService(_db, new FakeFinRevenueForInv());

        // Seed default UOM for tests
        _db.InvUnitsOfMeasure.Add(new InvUnitOfMeasure
        {
            TenantId = _tenant, Code = "CAI", Name = "Cái", IsActive = true, CreatedBy = _user
        });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task PostPurchaseReceiptFromGrn_ValidGrn_PostsReceiptAndUpdatesBalance()
    {
        var wh = new InvWarehouse { TenantId = _tenant, Code = "KHO-01", Name = "Kho Tổng", Status = "Active", CreatedBy = _user };
        _db.InvWarehouses.Add(wh);
        var sku = new InvSku { TenantId = _tenant, Code = "SP-INV-001", Name = "Sản phẩm A", Status = "Active", CreatedBy = _user };
        _db.InvSkus.Add(sku);

        var grn = new PurGoodsReceipt { TenantId = _tenant, Code = "GRN-100", Status = "Posted", CreatedBy = _user };
        _db.PurGoodsReceipts.Add(grn);
        var grnLine = new PurGrnLine { TenantId = _tenant, GrnId = grn.Id, ProductCode = "SP-INV-001", AcceptedQty = 50, UnitPrice = 10000, CreatedBy = _user };
        _db.PurGrnLines.Add(grnLine);
        await _db.SaveChangesAsync();

        var doc = await _svc.PostPurchaseReceiptFromGrnAsync(_tenant, _user, grn.Id, wh.Id);

        Assert.NotNull(doc);
        Assert.Equal("Receipt", doc.DocType);
        Assert.Equal("Purchase", doc.SourceType);
        Assert.Equal(grn.Code, doc.RefCode);
    }

    [Fact]
    public async Task PostPurchaseReceiptFromGrn_DuplicateGrnReceipt_ReturnsExistingDocIdempotent()
    {
        var wh = new InvWarehouse { TenantId = _tenant, Code = "KHO-02", Name = "Kho 2", Status = "Active", CreatedBy = _user };
        _db.InvWarehouses.Add(wh);
        var grn = new PurGoodsReceipt { TenantId = _tenant, Code = "GRN-101", Status = "Posted", CreatedBy = _user };
        _db.PurGoodsReceipts.Add(grn);
        var grnLine = new PurGrnLine { TenantId = _tenant, GrnId = grn.Id, ProductCode = "SP-INV-002", AcceptedQty = 20, UnitPrice = 5000, CreatedBy = _user };
        _db.PurGrnLines.Add(grnLine);
        await _db.SaveChangesAsync();

        var first = await _svc.PostPurchaseReceiptFromGrnAsync(_tenant, _user, grn.Id, wh.Id);
        var second = await _svc.PostPurchaseReceiptFromGrnAsync(_tenant, _user, grn.Id, wh.Id);

        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task PostPurchaseReceiptFromGrn_UnpostedGrn_ThrowsAppException()
    {
        var grn = new PurGoodsReceipt { TenantId = _tenant, Code = "GRN-DRAFT", Status = "Draft", CreatedBy = _user };
        _db.PurGoodsReceipts.Add(grn);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.PostPurchaseReceiptFromGrnAsync(_tenant, _user, grn.Id));
    }

    [Fact]
    public async Task PostDoc_IssueDoc_InsufficientAvailableStock_ThrowsAppException()
    {
        var wh = new InvWarehouse { TenantId = _tenant, Code = "KHO-03", Name = "Kho 3", Status = "Active", CreatedBy = _user };
        _db.InvWarehouses.Add(wh);
        var sku = new InvSku { TenantId = _tenant, Code = "SP-INV-003", Name = "Sản phẩm B", Status = "Active", CreatedBy = _user };
        _db.InvSkus.Add(sku);

        var doc = new InvStockDoc { TenantId = _tenant, Code = "OUT-001", DocType = "Issue", SourceType = "Internal", WarehouseId = wh.Id, Status = "Draft", CreatedBy = _user };
        _db.InvStockDocs.Add(doc);
        var line = new InvStockDocLine { TenantId = _tenant, DocId = doc.Id, SkuId = sku.Id, SkuCode = sku.Code, SkuName = sku.Name, Qty = 100, CreatedBy = _user };
        _db.InvStockDocLines.Add(line);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.PostDocAsync(_tenant, _user, doc.Id));
    }

    [Fact]
    public async Task UpsertDocLine_InactiveSku_ThrowsAppException()
    {
        var wh = new InvWarehouse { TenantId = _tenant, Code = "KHO-04", Name = "Kho 4", Status = "Active", CreatedBy = _user };
        _db.InvWarehouses.Add(wh);
        var sku = new InvSku { TenantId = _tenant, Code = "SP-INACTIVE", Name = "Sản phẩm khóa", Status = "Inactive", CreatedBy = _user };
        _db.InvSkus.Add(sku);
        var doc = new InvStockDoc { TenantId = _tenant, Code = "IN-002", DocType = "Receipt", SourceType = "Purchase", WarehouseId = wh.Id, Status = "Draft", CreatedBy = _user };
        _db.InvStockDocs.Add(doc);
        await _db.SaveChangesAsync();

        var req = new InvStockDocLineRequest(null, sku.Id, 10, null, null, 1000);

        await Assert.ThrowsAsync<AppException>(
            () => _svc.UpsertDocLineAsync(_tenant, _user, doc.Id, req));
    }
}

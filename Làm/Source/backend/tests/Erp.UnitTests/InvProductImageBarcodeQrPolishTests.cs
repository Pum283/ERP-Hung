using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvProductImageBarcodeQrPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvProductImageBarcodeQrService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public InvProductImageBarcodeQrPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-media-barcode-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvProductImageBarcodeQrService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task UpdateProductMedia_UpdatesMediaAndGalleryUrls()
    {
        var req = new InvUpdateProductMediaRequest(
            _productId,
            "SKU-COFFEE-BEAN",
            "https://example.com/img1.jpg",
            new List<string> { "https://example.com/img1.jpg", "https://example.com/img2.jpg" },
            "Cà phê hạt Arabica rang mộc nguyên chất 100%",
            "Hạt Arabica Cầu Đất Đà Lạt"
        );

        var res = await _svc.UpdateProductMediaAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SKU-COFFEE-BEAN", res.ProductCode);
        Assert.Equal(2, res.GalleryImageUrls.Count);
    }

    [Fact]
    public async Task GenerateProductBarcodeQr_GeneratesBarcodeAndQrPayload()
    {
        var req = new InvGenerateBarcodeQrRequest(_productId, "SKU-COFFEE-BEAN", "8935000123456", "Standard-50x30mm");
        var res = await _svc.GenerateProductBarcodeQrAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("8935000123456", res.BarcodeEan13);
        Assert.StartsWith("ERP-PROD|", res.QrCodePayload);
    }
}

using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurAdvanceBlanketContractExpirationPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurAdvanceBlanketContractExpirationService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _poId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();

    public PurAdvanceBlanketContractExpirationPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-advance-blanket-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR190", Name = "Tenant PUR 190" });
        _db.SaveChanges();

        _svc = new PurAdvanceBlanketContractExpirationService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_044: Tạm ứng nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAdvancePayment_CreatesVendorDownPaymentRequest()
    {
        var req = new PurCreateVendorAdvancePaymentRequest(_poId, _supplierId, 50000000m, "Tạm ứng 30% hợp đồng mua cà phê hạt");
        var res = await _svc.CreateAdvancePaymentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(50000000m, res.AdvanceAmountVnd);
        Assert.Equal("Approved", res.Status);
        Assert.StartsWith("ADV-", res.RequestNumber);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_045 & UC_PUR_046: Hợp đồng mua khung & Theo dõi sản lượng/giá trị còn lại
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBlanketContract_CalculatesRemainingQuantityAndValue()
    {
        var req = new PurCreateBlanketContractRequest(
            "BPO-2026-001",
            "Hợp Đồng Khung Nhập Khẩu Bao Bì 2026",
            _supplierId,
            100000000m,
            10000,
            DateTimeOffset.UtcNow.AddMonths(-1),
            DateTimeOffset.UtcNow.AddMonths(11)
        );

        var res = await _svc.CreateBlanketContractAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(100000000m, res.TotalContractValueVnd);
        Assert.Equal(100000000m, res.RemainingValueVnd);
        Assert.Equal(10000, res.RemainingQty);
        Assert.Equal(0, res.ConsumedPercentage);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_047: Cảnh báo hết hạn hợp đồng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetExpiringContractsAlerts_ReturnsExpiringSoonContracts()
    {
        var list = await _svc.GetExpiringContractsAlertsAsync(_tenant, 30);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, c => c.IsExpiringSoon);
    }
}

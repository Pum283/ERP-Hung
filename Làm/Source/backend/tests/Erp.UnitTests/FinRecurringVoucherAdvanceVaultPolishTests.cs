using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FinRecurringVoucherAdvanceVaultPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FinRecurringVoucherAdvanceVaultService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FinRecurringVoucherAdvanceVaultPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fin-voucher-advance-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FinRecurringVoucherAdvanceVaultService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateRecurringTemplate_SavesAccountCodes()
    {
        var req = new FinCreateRecurringTemplateRequest("TMPL-DEPR", "Khấu hao văn phòng", "Monthly", 35000000m, "6424", "2141", true);
        var res = await _svc.CreateRecurringTemplateAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("6424", res.DebitAccountCode);
        Assert.Equal("2141", res.CreditAccountCode);
        Assert.Equal("Monthly", res.Frequency);
    }

    [Fact]
    public async Task UploadVoucherAttachment_SavesMimeTypeAndSize()
    {
        var req = new FinUploadVoucherAttachmentRequest(Guid.NewGuid(), "PKT-001", "Hóa đơn VAT", "/uploads/inv.pdf", "application/pdf", 850000);
        var res = await _svc.UploadVoucherAttachmentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("application/pdf", res.MimeType);
        Assert.Equal(850000, res.FileSizeBytes);
    }

    [Fact]
    public async Task CreateAdvanceSettlement_GeneratesRequestNumber()
    {
        var req = new FinCreateAdvanceSettlementRequest("Nguyễn Văn An", "Tạm ứng công tác", 15000000m, 14200000m, 800000m);
        var res = await _svc.CreateAdvanceSettlementAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("TU-", res.RequestNumber);
        Assert.Equal("Settled", res.Status);
    }

    [Fact]
    public async Task CreateVaultCountAudit_CalculatesVariance()
    {
        var req = new FinCreateVaultCountAuditRequest("QUY-01", "Quỹ chính", 50000000m, 50000000m, 0m, "Thủ Quỹ", "Khớp số dư");
        var res = await _svc.CreateVaultCountAuditAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(0m, res.VarianceVnd);
        Assert.Equal("QUY-01", res.FundCode);
    }
}

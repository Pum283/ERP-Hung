using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_CRM_074 — gửi/xuất báo giá text thật + email queue notification.</summary>
public sealed class CrmQuoteSendTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _svc;
    private readonly Guid _tenant = Guid.Parse("ffffffff-0000-1111-2222-333333333333");
    private readonly Guid _user = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");

    private sealed class NoopFinRevenue : IFinRevenueService
    {
        public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
            Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>(Array.Empty<FinRevenueDocumentDto>());
        public Task<FinRevenueSummaryDto> GetSummaryAsync(Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeCogsAsync(Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> VoidAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
    }

    public CrmQuoteSendTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-quote-send-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var noop = new NoopFinRevenue();
        _svc = new CrmSalesService(_db, noop, new InvStockService(_db, noop), new LogLogisticsService(_db));
    }

    public void Dispose() => _db.Dispose();

    private CrmQuote SeedQuote(string? email = "khach@example.com", decimal total = 1_200_000m)
    {
        var cust = new CrmCustomer
        {
            TenantId = _tenant, Code = "KH-Q1", DisplayName = "Công ty ABC",
            Email = email, Phone = "0901111222", CreatedBy = _user,
        };
        _db.CrmCustomers.Add(cust);
        var quote = new CrmQuote
        {
            TenantId = _tenant, Code = "BG-001", CustomerId = cust.Id,
            Status = "Draft", SubTotal = total, TotalAmount = total,
            CreatedBy = _user,
        };
        _db.CrmQuotes.Add(quote);
        _db.CrmQuoteLines.Add(new CrmQuoteLine
        {
            TenantId = _tenant, QuoteId = quote.Id, ItemCode = "SP1", ItemName = "Sản phẩm demo",
            Quantity = 2, UnitPrice = total / 2, LineAmount = total, LineNo = 1, CreatedBy = _user,
        });
        _db.SaveChanges();
        return quote;
    }

    [Fact]
    public async Task BuildQuoteText_ContainsHeaderLinesAndTotal()
    {
        var quote = SeedQuote();

        var (fileName, content) = await _svc.BuildQuoteTextAsync(_tenant, _user, quote.Id);

        Assert.Equal("BG-001-baogia.txt", fileName);
        Assert.Contains("BÁO GIÁ", content);
        Assert.Contains("Công ty ABC", content);
        Assert.Contains("Sản phẩm demo", content);
        Assert.Contains($"{1_200_000m:N0}", content);
    }

    [Fact]
    public async Task SendQuote_Pdf_StampsSentAndLogsNote()
    {
        var quote = SeedQuote();

        var dto = await _svc.SendQuoteAsync(_tenant, _user, quote.Id, new CrmQuoteSendRequest("Pdf"));

        Assert.Equal("Sent", dto.Status);
        Assert.Equal("Pdf", dto.SentChannel);
        Assert.NotNull(dto.SentAt);
        Assert.Contains("PDF/TEXT", dto.Note);
    }

    [Fact]
    public async Task SendQuote_Email_RequiresCustomerEmail_AndCreatesNotification()
    {
        var quote = SeedQuote(email: "buyer@corp.vn");

        var dto = await _svc.SendQuoteAsync(_tenant, _user, quote.Id, new CrmQuoteSendRequest("Email"));

        Assert.Equal("Email", dto.SentChannel);
        Assert.Contains("EMAIL→buyer@corp.vn", dto.Note);
        Assert.Equal(1, await _db.AppNotifications.CountAsync(x => x.EventType == "CrmQuoteEmail"));
    }

    [Fact]
    public async Task SendQuote_Email_RejectsWhenNoEmail()
    {
        var quote = SeedQuote(email: null);

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _svc.SendQuoteAsync(_tenant, _user, quote.Id, new CrmQuoteSendRequest("Email")));
        Assert.Contains("email", ex.Message);
    }

    [Fact]
    public async Task SendQuote_RejectsEmptyQuote()
    {
        var quote = SeedQuote();
        _db.CrmQuoteLines.RemoveRange(_db.CrmQuoteLines);
        quote.TotalAmount = 0;
        quote.SubTotal = 0;
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.SendQuoteAsync(_tenant, _user, quote.Id, new CrmQuoteSendRequest("Pdf")));
    }

    [Fact]
    public async Task SendQuote_RejectsPendingDiscount()
    {
        var quote = SeedQuote();
        quote.DiscountApprovalStatus = "Pending";
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.SendQuoteAsync(_tenant, _user, quote.Id, new CrmQuoteSendRequest("Pdf")));
    }

    [Fact]
    public async Task BuildQuoteText_StampMarksSentAsPdf()
    {
        var quote = SeedQuote();

        await _svc.BuildQuoteTextAsync(_tenant, _user, quote.Id, stampSent: true);

        var q = await _db.CrmQuotes.SingleAsync(x => x.Id == quote.Id);
        Assert.Equal("Sent", q.Status);
        Assert.Equal("Pdf", q.SentChannel);
    }
}

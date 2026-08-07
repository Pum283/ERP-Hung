using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_FIN_019/025 · UC_CRM_050 · UC_HRM_118 — JE luôn thật · auto-intake · sync máy chi tiết.</summary>
public sealed class FinCrmHrmPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FinAccountingService _fin;
    private readonly FinCashService _cash;
    private readonly FinBankService _bank;
    private readonly CrmLeadService _leads;
    private readonly HrmAttendanceService _att;
    private readonly Guid _tenant = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private readonly Guid _user = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private sealed class NoopSales : ICrmSalesService
    {
        public Task<IReadOnlyList<CrmPriceListDto>> ListPriceListsAsync(Guid tenantId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<CrmPriceListDto>>(Array.Empty<CrmPriceListDto>());
        public Task<CrmPriceListDetailDto> GetPriceListDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmPriceListDto> UpsertPriceListAsync(Guid tenantId, Guid userId, CrmPriceListUpsertRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmPriceListItemDto> UpsertPriceListItemAsync(Guid tenantId, Guid userId, Guid priceListId, CrmPriceListItemUpsertRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<IReadOnlyList<CrmQuoteDto>> ListQuotesAsync(Guid tenantId, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<CrmQuoteDto>>(Array.Empty<CrmQuoteDto>());
        public Task<CrmQuoteDetailDto> GetQuoteDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> UpsertQuoteAsync(Guid tenantId, Guid userId, CrmQuoteUpsertRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> CreateQuoteFromOpportunityAsync(Guid tenantId, Guid userId, Guid opportunityId, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteLineDto> UpsertQuoteLineAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteLineUpsertRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> ApplyPriceListAsync(Guid tenantId, Guid userId, Guid quoteId, Guid priceListId, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> RequestDiscountAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> DecideDiscountAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountDecisionRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmQuoteDto> SendQuoteAsync(Guid tenantId, Guid userId, Guid quoteId, CrmQuoteSendRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<(string FileName, string Content)> BuildQuoteTextAsync(Guid tenantId, Guid userId, Guid quoteId, bool stampSent = false, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmSalesOrderDto> ConvertQuoteToOrderAsync(Guid tenantId, Guid userId, Guid quoteId, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<IReadOnlyList<CrmSalesOrderDto>> ListOrdersAsync(Guid tenantId, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<CrmSalesOrderDto>>(Array.Empty<CrmSalesOrderDto>());
        public Task<CrmSalesOrderDetailDto> GetOrderDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmSalesOrderDto> SetOrderStatusAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderStatusRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmSalesOrderDto> HoldStockAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmSalesOrderDto> CancelOrderAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderCancelRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmOrderPaymentDto> AddPaymentAsync(Guid tenantId, Guid userId, Guid orderId, CrmOrderPaymentRequest req, CancellationToken ct = default)
            => throw new AppException("noop");
        public Task<CrmSalesOrderDto> PushToWarehouseAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
            => throw new AppException("noop");
    }

    public FinCrmHrmPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fin-crm-hrm-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _fin = new FinAccountingService(_db);
        _cash = new FinCashService(_db, _fin);
        _bank = new FinBankService(_db, _fin);
        _leads = new CrmLeadService(_db, new NoopSales());
        _att = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    private FinPeriod SeedPeriodAndAccounts()
    {
        var now = DateTimeOffset.UtcNow;
        var period = new FinPeriod
        {
            TenantId = _tenant, FiscalYearId = Guid.NewGuid(),
            Code = $"{now:yyyy-MM}", Name = "Tháng hiện tại",
            StartDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, TimeSpan.Zero),
            Status = "Open", CreatedBy = _user,
        };
        _db.FinPeriods.Add(period);
        _db.FinAccounts.AddRange(
            new FinAccount { TenantId = _tenant, Code = "1111", Name = "Tiền mặt", AccountType = "Asset", CreatedBy = _user },
            new FinAccount { TenantId = _tenant, Code = "1121", Name = "NH", AccountType = "Asset", CreatedBy = _user },
            new FinAccount { TenantId = _tenant, Code = "1311", Name = "Phải thu", AccountType = "Asset", CreatedBy = _user },
            new FinAccount { TenantId = _tenant, Code = "5111", Name = "DT", AccountType = "Revenue", CreatedBy = _user },
            new FinAccount { TenantId = _tenant, Code = "3311", Name = "Phải trả", AccountType = "Liability", CreatedBy = _user });
        _db.SaveChanges();
        return period;
    }

    [Fact]
    public async Task PostCashReceipt_AlwaysCreatesBalancedAutoJournal()
    {
        SeedPeriodAndAccounts();
        var cash = await _db.FinAccounts.SingleAsync(x => x.Code == "1111");
        var fund = new FinCashFund
        {
            TenantId = _tenant, Code = "Q1", Name = "Quỹ chính", CashAccountId = cash.Id,
            OpeningBalance = 0, Status = "Active", CreatedBy = _user,
        };
        _db.FinCashFunds.Add(fund);
        var voucher = new FinCashVoucher
        {
            TenantId = _tenant, Code = "PT-01", FundId = fund.Id, VoucherType = "Receipt",
            Amount = 500_000, Description = "Thu khách", Status = "Draft",
            CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.FinCashVouchers.Add(voucher);
        await _db.SaveChangesAsync();

        var dto = await _cash.PostVoucherAsync(_tenant, _user, voucher.Id);

        Assert.Equal("Posted", dto.Status);
        Assert.NotNull(dto.FinJournalId);
        Assert.NotNull(dto.FinJournalCode);
        var je = await _db.FinJournals.SingleAsync(x => x.Id == dto.FinJournalId);
        Assert.Equal("Posted", je.Status);
        Assert.Equal("Auto", je.Source);
        var lines = await _db.FinJournalLines.Where(x => x.JournalId == je.Id).ToListAsync();
        Assert.Equal(2, lines.Count);
        Assert.Equal(lines.Sum(x => x.Debit), lines.Sum(x => x.Credit));
    }

    [Fact]
    public async Task PostBankCredit_AlwaysCreatesJournal_EvenWithoutCounter()
    {
        SeedPeriodAndAccounts();
        var gl = await _db.FinAccounts.SingleAsync(x => x.Code == "1121");
        var bank = new FinBankAccount
        {
            TenantId = _tenant, Code = "NH1", Name = "VCB", BankName = "VCB",
            AccountNumber = "001", GlAccountId = gl.Id, Status = "Active", CreatedBy = _user,
        };
        _db.FinBankAccounts.Add(bank);
        var v = new FinBankVoucher
        {
            TenantId = _tenant, Code = "BC-01", BankAccountId = bank.Id, VoucherType = "Credit",
            Amount = 1_000_000, Description = "Thu chuyển khoản", Status = "Draft",
            CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.FinBankVouchers.Add(v);
        await _db.SaveChangesAsync();

        var dto = await _bank.PostVoucherAsync(_tenant, _user, v.Id);

        Assert.NotNull(dto.FinJournalId);
        Assert.Equal("Posted", dto.Status);
    }

    [Fact]
    public async Task AutoIntake_CreatesLeadWithWebsiteSourceAndActivity()
    {
        var dto = await _leads.AutoIntakeAsync(_tenant, _user, new CrmLeadAutoIntakeRequest(
            "Lead Web", "0909999888", null, "CTY A", "WEBSITE", null, "Từ form"));

        Assert.Equal("New", dto.PipelineStatus);
        Assert.Equal("Auto", dto.IntakeChannel);
        Assert.True(await _db.CrmLeadSources.AnyAsync(x => x.Code == "WEBSITE"));
        Assert.True(await _db.CrmLeadActivities.AnyAsync(x => x.LeadId == dto.Id));
    }

    [Fact]
    public async Task AutoIntake_DedupsByPhone_AndLogsReintake()
    {
        var first = await _leads.AutoIntakeAsync(_tenant, _user, new CrmLeadAutoIntakeRequest(
            "Lead 1", "0901111222", null, null, "WEB", null, null));
        var second = await _leads.AutoIntakeAsync(_tenant, _user, new CrmLeadAutoIntakeRequest(
            "Lead 1b", "0901111222", null, null, "WEB", null, "lần 2"));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await _db.CrmLeads.CountAsync(x => x.TenantId == _tenant && !x.IsDeleted));
        Assert.Contains("Re-intake", (await _db.CrmLeadActivities.OrderByDescending(x => x.CreatedAt).FirstAsync()).Content);
    }

    [Fact]
    public async Task AutoIntake_RejectsWithoutPhoneOrEmail()
    {
        await Assert.ThrowsAsync<AppException>(() => _leads.AutoIntakeAsync(
            _tenant, _user, new CrmLeadAutoIntakeRequest("X", null, null, null, null, null, null)));
    }

    [Fact]
    public async Task SyncDevice_ReturnsDetailedCounts()
    {
        var ou = new OrgUnit { TenantId = _tenant, Code = "OU1", Name = "Phòng 1", CreatedBy = _user };
        _db.OrgUnits.Add(ou);
        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "NV01", FullName = "Nguyễn A",
            OrgUnitId = ou.Id, CreatedBy = _user,
        });
        _db.AttendanceDevices.Add(new AttendanceDevice
        {
            TenantId = _tenant, Code = "MAY1", Name = "Máy 1", DeviceType = "Fingerprint",
            IsActive = true, CreatedBy = _user,
        });
        await _db.SaveChangesAsync();

        var r = await _att.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest([
            new("NV01", DateTimeOffset.UtcNow, "in", "MAY1"),
            new("NV01", DateTimeOffset.UtcNow.AddMinutes(1), "in", "MAY1"), // dup
            new("NOPE", DateTimeOffset.UtcNow, "in", "MAY1"), // unknown
            new("NV01", DateTimeOffset.UtcNow.AddHours(8), "bad", "MAY1"), // invalid
        ]));

        Assert.Equal(1, r.Synced);
        Assert.Equal(1, r.SkippedDuplicate);
        Assert.Equal(1, r.SkippedUnknownEmployee);
        Assert.Equal(1, r.SkippedInvalidType);
        Assert.Equal(4, r.Total);
        Assert.Equal(1, await _db.AttendanceRecords.CountAsync());
    }

    [Fact]
    public async Task SyncDevice_AppliesCheckoutAfterCheckin()
    {
        var ou = new OrgUnit { TenantId = _tenant, Code = "OU2", Name = "Phòng 2", CreatedBy = _user };
        _db.OrgUnits.Add(ou);
        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "NV02", FullName = "Trần B",
            OrgUnitId = ou.Id, CreatedBy = _user,
        });
        await _db.SaveChangesAsync();

        var t0 = DateTimeOffset.UtcNow.Date.AddHours(8);
        await _att.SyncDeviceAsync(_tenant, _user, new AttendanceDeviceSyncRequest([
            new("NV02", t0, "checkin", null),
            new("NV02", t0.AddHours(9), "checkout", null),
        ]));

        var rec = await _db.AttendanceRecords.SingleAsync();
        Assert.NotNull(rec.CheckInAt);
        Assert.NotNull(rec.CheckOutAt);
        Assert.Equal("Closed", rec.Status);
        Assert.Equal("DeviceSync", rec.CheckInMethod);
    }
}

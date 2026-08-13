using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmFieldVisitOutcomeOrderPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmFieldVisitOutcomeOrderService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _salespersonId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _visitPlanId = Guid.NewGuid();

    public CrmFieldVisitOutcomeOrderPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-field-visit-outcome-order-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM172", Name = "Tenant CRM 172" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-172",
            DisplayName = "Đại lý Nông Sản Miền Tây",
            Phone = "0908888777"
        });

        _db.CrmVisitPlans.Add(new CrmVisitPlan
        {
            Id = _visitPlanId,
            TenantId = _tenant,
            TerritoryId = Guid.NewGuid(),
            CustomerId = _customerId,
            SalespersonId = _salespersonId,
            PlannedDate = DateTime.UtcNow,
            Status = "InProgress"
        });

        _db.SaveChanges();

        _svc = new CrmFieldVisitOutcomeOrderService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_093: Ghi nhận mục đích – kết quả visit
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordOutcome_RecordsVisitOutcomeSuccessfully()
    {
        var req = new CrmRecordVisitOutcomeRequest(
            _visitPlanId,
            "Thăm đại lý định kỳ & chốt hợp đồng Q3",
            "Successful",
            "Đại lý đồng ý nhập thêm 100 sản phẩm mới",
            "Gửi hợp đồng ký kết trong tuần"
        );

        var res = await _svc.RecordOutcomeAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_visitPlanId, res.VisitPlanId);
        Assert.Equal("Successful", res.OutcomeStatus);
        Assert.Equal("Gửi hợp đồng ký kết trong tuần", res.ActionItems);

        var dbPlan = await _db.CrmVisitPlans.FirstOrDefaultAsync(p => p.TenantId == _tenant && p.Id == _visitPlanId);
        Assert.NotNull(dbPlan);
        Assert.Equal("Completed", dbPlan.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_094: Ghi nhận nhu cầu khách hàng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordDemand_RecordsCustomerDemand()
    {
        var req = new CrmRecordCustomerDemandRequest(
            _visitPlanId,
            _customerId,
            "Phân bón sinh học hữu cơ",
            250,
            "High",
            "Đối thủ A đang giảm giá 5%",
            "Khách hàng cần hàng giao gấp trong 3 ngày"
        );

        var res = await _svc.RecordDemandAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal("Phân bón sinh học hữu cơ", res.ProductInterestCategory);
        Assert.Equal(250, res.EstimatedQuantity);
        Assert.Equal("High", res.Urgency);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_095: Đặt hàng tại điểm thăm
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOnSiteOrder_CreatesOrderAtStoreVisit()
    {
        var items = new List<CrmOnSiteOrderItemRequest>
        {
            new(Guid.NewGuid(), "Sản phẩm A", 10, 500000m),
            new(Guid.NewGuid(), "Sản phẩm B", 5, 1200000m)
        };

        var req = new CrmCreateOnSiteOrderRequest(
            _visitPlanId,
            _customerId,
            items,
            "Đơn đặt hàng trực tiếp tại điểm thăm"
        );

        var res = await _svc.CreateOnSiteOrderAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(11000000m, res.TotalAmount); // (10*500k) + (5*1.2M) = 5M + 6M = 11M
        Assert.Equal("OnSiteSubmitted", res.Status);
        Assert.StartsWith("ORD-ONSITE-", res.OrderCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_096: Xem lịch sử visit
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetVisitHistoryLogs_ReturnsVisitAuditHistory()
    {
        var logs = await _svc.GetVisitHistoryLogsAsync(_tenant, _customerId);

        Assert.NotEmpty(logs);
        Assert.Contains(logs, l => l.CustomerId == _customerId);
    }
}

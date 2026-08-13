using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmFieldSalesOpsDisputePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmFieldSalesOpsDisputeService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public CrmFieldSalesOpsDisputePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-field-sales-ops-dispute-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM173", Name = "Tenant CRM 173" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-173",
            DisplayName = "Chuỗi Bách Hóa An Khang",
            Phone = "0909111333"
        });

        _db.CrmOnlineOrderIntakes.Add(new CrmOnlineOrderIntake
        {
            Id = _orderId,
            TenantId = _tenant,
            Channel = "FieldSales OnSite",
            ExternalOrderCode = "ORD-ONSITE-173",
            CustomerName = "Chuỗi Bách Hóa An Khang",
            Phone = "0909111333",
            TotalAmount = 15000000m,
            Status = "Completed"
        });

        _db.SaveChanges();

        _svc = new CrmFieldSalesOpsDisputeService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_097: AI gợi ý việc ưu tiên
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAiPriorityActions_ReturnsRecommendedActions()
    {
        var actions = await _svc.GetAiPriorityActionsAsync(_tenant);

        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.PriorityLevel == "High");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_098: Dashboard doanh số field
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFieldSalesRevenueMetrics_ReturnsRevenueMetrics()
    {
        var metrics = await _svc.GetFieldSalesRevenueMetricsAsync(_tenant);

        Assert.NotNull(metrics);
        Assert.True(metrics.TotalFieldRevenue > 0);
        Assert.True(metrics.TotalStoreVisitsPlanned > 0);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_102: Đối soát chứng từ đơn
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReconcileDocument_ReconcilesOrderDocument()
    {
        var req = new CrmReconcileOrderDocumentRequest(
            _orderId,
            "VAT-2026-9912",
            "VATInvoice",
            "Matched",
            "Khớp hóa đơn và phiếu giao hàng"
        );

        var res = await _svc.ReconcileDocumentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_orderId, res.OrderId);
        Assert.Equal("VAT-2026-9912", res.DocumentCode);
        Assert.Equal("Matched", res.ReconciliationStatus);

        var list = await _svc.GetReconciliationsAsync(_tenant, _orderId);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_103: Xử lý khiếu nại đơn hàng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAndResolveComplaint_ManagesComplaintLifecycle()
    {
        var createReq = new CrmCreateOrderComplaintRequest(
            _orderId,
            _customerId,
            "Hàng bị móp méo khi vận chuyển",
            "High"
        );

        var created = await _svc.CreateComplaintAsync(_tenant, createReq);

        Assert.NotNull(created);
        Assert.Equal(_orderId, created.OrderId);
        Assert.Equal("Open", created.Status);

        var resolveReq = new CrmResolveComplaintRequest(
            created.Id,
            "Resolved",
            "Đã đổi sản phẩm mới trong ngày"
        );

        var resolved = await _svc.ResolveComplaintAsync(_tenant, resolveReq);

        Assert.NotNull(resolved);
        Assert.Equal("Resolved", resolved.Status);
        Assert.Equal("Đã đổi sản phẩm mới trong ngày", resolved.ResolutionNotes);

        var complaints = await _svc.GetComplaintsAsync(_tenant);
        Assert.NotEmpty(complaints);
    }
}

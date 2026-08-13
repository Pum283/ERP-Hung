using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmCreditFsmCareLoyaltyPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCreditFsmCareLoyaltyService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public CrmCreditFsmCareLoyaltyPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-credit-fsm-care-loyalty-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM175", Name = "Tenant CRM 175" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-175",
            DisplayName = "Đại lý Nông Sản Miền Tây",
            Phone = "0988777666"
        });

        _db.SaveChanges();

        _svc = new CrmCreditFsmCareLoyaltyService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_111: Chặn bán khi vượt công nợ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckCreditLimit_BlocksSalesWhenDebtExceeded()
    {
        var reqBlocked = new CrmCheckCreditLimitRequest(_customerId, 20000000m); // 85m + 20m = 105m > 100m limit
        var resBlocked = await _svc.CheckCreditLimitAsync(_tenant, reqBlocked);

        Assert.True(resBlocked.IsCreditLimitExceeded);
        Assert.Contains("CHẶN ĐƠN HÀNG", resBlocked.DecisionMessage);

        var reqApproved = new CrmCheckCreditLimitRequest(_customerId, 10000000m); // 85m + 10m = 95m <= 100m limit
        var resApproved = await _svc.CheckCreditLimitAsync(_tenant, reqApproved);

        Assert.False(resApproved.IsCreditLimitExceeded);
        Assert.Contains("DUYỆT ĐƠN HÀNG", resApproved.DecisionMessage);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_114: Chuyển ticket sang FSM
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TransferTicketToFsm_TransfersTicketSuccessfully()
    {
        var ticketId = Guid.NewGuid();
        var fsmTechId = Guid.NewGuid();

        var req = new CrmTransferTicketToFsmRequest(
            ticketId,
            fsmTechId,
            "Urgent",
            "Máy bảo vệ thực vật bị hỏng kim phun"
        );

        var res = await _svc.TransferTicketToFsmAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(ticketId, res.TicketId);
        Assert.Equal("TransferredToFsm", res.Status);

        var tickets = await _svc.GetFsmTicketsAsync(_tenant);
        Assert.NotEmpty(tickets);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_115: Lịch chăm sóc / nhắc tái mua
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ScheduleCare_CreatesCustomerCareSchedule()
    {
        var req = new CrmScheduleCustomerCareRequest(
            _customerId,
            "RepurchaseReminder",
            DateTime.UtcNow.AddDays(7),
            "Nhắc chốt đơn phân bón hữu cơ theo chu kỳ",
            Guid.NewGuid()
        );

        var res = await _svc.ScheduleCareAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal("RepurchaseReminder", res.CareType);
        Assert.Equal("Pending", res.Status);

        var list = await _svc.GetCareSchedulesAsync(_tenant, _customerId);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_116: Chương trình loyalty
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateLoyaltyProgram_CreatesActiveProgram()
    {
        var req = new CrmCreateLoyaltyProgramRequest(
            "LOYALTY-VIP-2026",
            "Chương Trình Khách Hàng VIP 2026",
            0.001m,
            100,
            "Thành viên VIP nhận ưu đãi chiết khấu 5%"
        );

        var res = await _svc.CreateLoyaltyProgramAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("LOYALTY-VIP-2026", res.ProgramCode);
        Assert.True(res.IsActive);

        var programs = await _svc.GetLoyaltyProgramsAsync(_tenant);
        Assert.NotEmpty(programs);
    }
}

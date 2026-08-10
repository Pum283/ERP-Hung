using System;
using System.Linq;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fsm;
using Erp.Domain.Entities.Fsm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmFieldPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmFieldService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public FsmFieldPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"FsmTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _svc = new FsmFieldService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task UpsertTicket_ValidPriority_AutoCalculatesSlaDueDates()
    {
        var sla = new FsmSlaPolicy
        {
            TenantId = _tenant, Code = "SLA-CRITICAL", Priority = "Critical",
            ResponseHours = 1, ResolveHours = 4, IsActive = true, CreatedBy = _user
        };
        _db.FsmSlaPolicies.Add(sla);
        await _db.SaveChangesAsync();

        var req = new FsmTicketUpsertRequest(
            null, "TK-001", "Phone", "Sự cố máy nén khí dừng đột ngột", "Hệ thống dừng", "Công ty Kim Loại Hùng",
            "0909123456", null, null, null, "Critical");

        var dto = await _svc.UpsertTicketAsync(_tenant, _user, req);

        Assert.Equal("Critical", dto.Priority);
        Assert.NotNull(dto.DueResponseAt);
        Assert.NotNull(dto.DueResolveAt);
        Assert.Equal("Open", dto.Status);
    }

    [Fact]
    public async Task UpsertTicket_InvalidPriority_ThrowsAppException()
    {
        var req = new FsmTicketUpsertRequest(
            null, "TK-ERR", "Phone", "Tiêu đề ngẫu nhiên", null, "Khách Hàng X",
            null, null, null, null, "SuperUrgent");

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertTicketAsync(_tenant, _user, req));
    }

    [Fact]
    public async Task EscalateTicket_ClosedTicket_ThrowsAppException()
    {
        var newTech = new AppUser { TenantId = _tenant, Username = "senior_tech", DisplayName = "Trần Văn B", CreatedBy = _user };
        _db.Users.Add(newTech);
        var ticket = new FsmTicket
        {
            TenantId = _tenant, Code = "TK-CLOSED", Subject = "Ticket đã đóng", CustomerName = "Khách Hàng Y",
            Status = "Closed", ClosedAt = DateTimeOffset.UtcNow, CreatedBy = _user
        };
        _db.FsmTickets.Add(ticket);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.EscalateTicketAsync(_tenant, _user, ticket.Id, new FsmEscalateRequest(newTech.Id, null, "Cần hỗ trợ chuyên gia cấp cao")));
    }

    [Fact]
    public async Task EscalateTicket_ValidTechnician_UpdatesStatusToEscalated()
    {
        var currentTech = new AppUser { TenantId = _tenant, Username = "junior_tech", DisplayName = "Lê Văn C", CreatedBy = _user };
        var seniorTech = new AppUser { TenantId = _tenant, Username = "master_tech", DisplayName = "Phạm Văn D", CreatedBy = _user };
        _db.Users.AddRange(currentTech, seniorTech);

        var ticket = new FsmTicket
        {
            TenantId = _tenant, Code = "TK-OPEN", Subject = "Lỗi bo mạch điều khiển", CustomerName = "Nhà Máy Z",
            Status = "InProgress", AssignedTechUserId = currentTech.Id, AssignedTechName = "Lê Văn C", CreatedBy = _user
        };
        _db.FsmTickets.Add(ticket);
        await _db.SaveChangesAsync();

        var updated = await _svc.EscalateTicketAsync(_tenant, _user, ticket.Id, new FsmEscalateRequest(seniorTech.Id, "Phạm Văn D", "Vượt quá khả năng xử lý của KTV sơ cấp"));

        Assert.Equal("Escalated", updated.Status);
        Assert.Equal(seniorTech.Id, updated.AssignedTechUserId);
        Assert.Equal("Phạm Văn D", updated.AssignedTechName);

        var dbTicket = await _db.FsmTickets.FindAsync(updated.Id);
        Assert.NotNull(dbTicket);
        Assert.Equal(currentTech.Id, dbTicket!.PreviousTechUserId);
    }

    [Fact]
    public async Task AssignTicket_ClosedTicket_ThrowsAppException()
    {
        var tech = new AppUser { TenantId = _tenant, Username = "tech_01", DisplayName = "Đỗ Văn E", CreatedBy = _user };
        _db.Users.Add(tech);
        var ticket = new FsmTicket
        {
            TenantId = _tenant, Code = "TK-DONE", Subject = "Lỗi đã sửa", CustomerName = "Khách Hàng M",
            Status = "Resolved", CreatedBy = _user
        };
        _db.FsmTickets.Add(ticket);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.AssignTicketAsync(_tenant, _user, ticket.Id, new FsmAssignRequest(tech.Id, "Đỗ Văn E")));
    }
}

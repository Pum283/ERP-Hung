using System;
using System.Linq;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pjm;
using Erp.Domain.Entities.Pjm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PjmProjectPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PjmProjectService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();

    public PjmProjectPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PjmTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _svc = new PjmProjectService(_db);

        var proj = new PjmProject
        {
            Id = _projectId, TenantId = _tenant, Code = "DA-2026-001", Name = "Xây Dựng Hệ Thống ERP",
            StatusCode = "Active", CreatedByUserId = _user
        };
        _db.PjmProjects.Add(proj);
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task UpsertWbsItem_ValidPercentComplete_UpdatesPercentAndAutoDoneStatus()
    {
        var req = new PjmWbsItemUpsertRequest(
            null, "WBS-01", "Thiết kế kiến trúc CSĐL", null, null, null, "Open", 10,
            "Giai đoạn 1", 100m, true, DateTimeOffset.UtcNow.AddDays(7));

        var dto = await _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req);

        Assert.Equal("WBS-01", dto.Code);
        Assert.Equal(100m, dto.PercentComplete);
        Assert.Equal("Done", dto.Status);
        Assert.False(dto.IsOverdue);
    }

    [Fact]
    public async Task UpsertWbsItem_InvalidPercentComplete_Below0OrAbove100_ThrowsAppException()
    {
        var req1 = new PjmWbsItemUpsertRequest(
            null, "WBS-ERR1", "Lỗi âm phần trăm", null, null, null, "Open", 10,
            null, -15m, false, null);

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req1));

        var req2 = new PjmWbsItemUpsertRequest(
            null, "WBS-ERR2", "Lỗi vượt 100 phần trăm", null, null, null, "Open", 20,
            null, 150m, false, null);

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req2));
    }

    [Fact]
    public async Task UpsertWbsItem_OverdueMilestone_CalculatesIsOverdueTrue()
    {
        var pastDueDate = DateTimeOffset.UtcNow.AddDays(-5);
        var req = new PjmWbsItemUpsertRequest(
            null, "WBS-MS-OVERDUE", "Mốc NT1: Nghiệm thu Sprint 1", null, null, null, "InProgress", 1,
            "Cảnh báo trễ mốc", 60m, true, pastDueDate);

        var dto = await _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req);

        Assert.True(dto.IsMilestone);
        Assert.True(dto.IsOverdue);
        Assert.Equal("InProgress", dto.Status);
    }

    [Fact]
    public async Task UpsertWbsItem_InvalidStatus_ThrowsAppException()
    {
        var req = new PjmWbsItemUpsertRequest(
            null, "WBS-STAT-ERR", "Trạng thái không hợp lệ", null, null, null, "UnknownStatus", 1,
            null, 50m, false, null);

        await Assert.ThrowsAsync<AppException>(() => _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req));
    }

    [Fact]
    public async Task UpsertWbsItem_AssigneeUserMapping_PopulatesAssigneeName()
    {
        var assigneeId = Guid.NewGuid();
        var assigneeUser = new AppUser
        {
            Id = assigneeId, TenantId = _tenant, Username = "dev_lead", DisplayName = "Nguyễn Văn Trưởng", CreatedBy = _user
        };
        _db.Users.Add(assigneeUser);
        await _db.SaveChangesAsync();

        var req = new PjmWbsItemUpsertRequest(
            null, "WBS-ASSIGNEE", "Phân công Trưởng nhóm", null, assigneeId, null, "Open", 1,
            null, 25m, false, null);

        var dto = await _svc.UpsertWbsItemAsync(_tenant, _user, _projectId, req);

        Assert.Equal(assigneeId, dto.AssigneeUserId);
        Assert.Equal("Nguyễn Văn Trưởng", dto.AssigneeName);
        Assert.Equal("InProgress", dto.Status);
    }
}

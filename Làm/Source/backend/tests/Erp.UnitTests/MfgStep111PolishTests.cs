using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mfg;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Bước 111 polish:
///   UC_MFG_013 — Kế hoạch SX theo đơn hàng
///   UC_MFG_017 — Tạo lệnh sản xuất
///   UC_MFG_018 — Duyệt lệnh sản xuất
///   UC_MFG_019 — Phát hành lệnh / in phiếu
/// </summary>
public sealed class MfgStep111PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgProductionService _mfg;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public MfgStep111PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-step111-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "admin111", DisplayName = "Admin 111" });
        _db.SaveChanges();
        _mfg = new MfgProductionService(_db, new FinAccountingService(_db));
    }

    public void Dispose() => _db.Dispose();

    private async Task<MfgItemDto> SeedFgAsync(string code = "TP-111")
        => await _mfg.UpsertItemAsync(_tenant, _user, new MfgItemUpsertRequest(null, code, "TP " + code, "FG", "CAI", 100_000m, "Active", null));

    private async Task<MfgItemDto> SeedRmAsync(string code = "RM-111")
        => await _mfg.UpsertItemAsync(_tenant, _user, new MfgItemUpsertRequest(null, code, "NVL " + code, "RM", "MET", 10_000m, "Active", null));

    private async Task<(MfgPlanDto Plan, MfgItemDto Fg)> SeedConfirmedPlanAsync()
    {
        var fg = await SeedFgAsync("TP-CONF");
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "SO-111", null));
        await _mfg.UpsertPlanLineAsync(_tenant, _user, plan.Id, new MfgPlanLineUpsertRequest(null, fg.Id, 12m, null, null));
        plan = await _mfg.ConfirmPlanAsync(_tenant, _user, plan.Id);
        return (plan, fg);
    }

    // ── UC_MFG_013 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_013_CreatePlan_AddLine_Confirm_Success()
    {
        var fg = await SeedFgAsync();
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "so-abc", "note"));
        Assert.Equal("Draft", plan.Status);
        Assert.Equal("SO-ABC", plan.SourceOrderCode);

        await _mfg.UpsertPlanLineAsync(_tenant, _user, plan.Id, new MfgPlanLineUpsertRequest(null, fg.Id, 5m, null, null));
        var confirmed = await _mfg.ConfirmPlanAsync(_tenant, _user, plan.Id);

        Assert.Equal("Confirmed", confirmed.Status);
        Assert.Equal(1, confirmed.LineCount);
    }

    [Fact]
    public async Task UC_MFG_013_ConfirmPlan_WithoutLines_Throws()
    {
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "SO-EMPTY", null));
        var ex = await Assert.ThrowsAsync<AppException>(() => _mfg.ConfirmPlanAsync(_tenant, _user, plan.Id));
        Assert.Contains("1 dòng", ex.Message);
    }

    [Fact]
    public async Task UC_MFG_013_EditConfirmedPlan_Throws()
    {
        var (plan, _) = await SeedConfirmedPlanAsync();
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(plan.Id, plan.Code, "SO-NEW", null)));
        Assert.Contains("Draft", ex.Message);
    }

    [Fact]
    public async Task UC_MFG_013_PlanLine_RawMaterial_Throws()
    {
        var rm = await SeedRmAsync();
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "SO-RM", null));
        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertPlanLineAsync(_tenant, _user, plan.Id, new MfgPlanLineUpsertRequest(null, rm.Id, 1m, null, null)));
    }

    [Fact]
    public async Task UC_MFG_013_CancelDraftPlan_Success()
    {
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "SO-CANCEL", null));
        var cancelled = await _mfg.CancelPlanAsync(_tenant, _user, plan.Id);
        Assert.Equal("Cancelled", cancelled.Status);
    }

    [Fact]
    public async Task UC_MFG_013_CancelPlan_WithActiveWo_Throws()
    {
        var (plan, fg) = await SeedConfirmedPlanAsync();
        await _mfg.UpsertWorkOrderAsync(_tenant, _user, new MfgWorkOrderUpsertRequest(null, null, fg.Id, 3m, null, null, plan.Id, null));
        var ex = await Assert.ThrowsAsync<AppException>(() => _mfg.CancelPlanAsync(_tenant, _user, plan.Id));
        Assert.Contains("lệnh SX", ex.Message);
    }

    // ── UC_MFG_017 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_017_CreateWorkOrder_Draft_AutoBom()
    {
        var fg = await SeedFgAsync("TP-WO");
        var rm = await SeedRmAsync("RM-WO");
        var bom = await _mfg.UpsertBomAsync(_tenant, _user, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));
        await _mfg.UpsertBomLineAsync(_tenant, _user, bom.Id, new MfgBomLineUpsertRequest(null, rm.Id, 1.5m, "MET", 1, null));
        await _mfg.ActivateBomAsync(_tenant, _user, bom.Id);

        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 10m, null, null, null, null));

        Assert.Equal("Draft", wo.Status);
        Assert.Equal(bom.Id, wo.BomId);
        Assert.Equal(10m, wo.Qty);
    }

    [Fact]
    public async Task UC_MFG_017_CreateWo_FromNonConfirmedPlan_Throws()
    {
        var fg = await SeedFgAsync("TP-BADPLAN");
        var plan = await _mfg.UpsertPlanAsync(_tenant, _user, new MfgPlanUpsertRequest(null, null, "SO-DRAFT", null));
        await _mfg.UpsertPlanLineAsync(_tenant, _user, plan.Id, new MfgPlanLineUpsertRequest(null, fg.Id, 2m, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertWorkOrderAsync(_tenant, _user,
                new MfgWorkOrderUpsertRequest(null, null, fg.Id, 2m, null, null, plan.Id, null)));
        Assert.Contains("Confirmed", ex.Message);
    }

    [Fact]
    public async Task UC_MFG_017_CreateWo_FromConfirmedPlan_Success()
    {
        var (plan, fg) = await SeedConfirmedPlanAsync();
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 4m, null, null, plan.Id, null));
        Assert.Equal(plan.Id, wo.PlanId);
        Assert.Equal("Draft", wo.Status);
    }

    // ── UC_MFG_018 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_018_ApproveDraft_SetsApprovedAt()
    {
        var fg = await SeedFgAsync("TP-APPR");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 1m, null, null, null, null));
        var approved = await _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id);
        Assert.Equal("Approved", approved.Status);
        Assert.NotNull(approved.ApprovedAt);
    }

    [Fact]
    public async Task UC_MFG_018_ApproveReleased_Throws()
    {
        var fg = await SeedFgAsync("TP-APPR2");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 1m, null, null, null, null));
        await _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id);
        await _mfg.ReleaseWorkOrderAsync(_tenant, _user, wo.Id);
        await Assert.ThrowsAsync<AppException>(() => _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id));
    }

    // ── UC_MFG_019 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_019_ReleaseApproved_DoesNotStampPrintedAt()
    {
        var fg = await SeedFgAsync("TP-REL");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 2m, null, null, null, null));
        await _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id);
        var released = await _mfg.ReleaseWorkOrderAsync(_tenant, _user, wo.Id);

        Assert.Equal("Released", released.Status);
        Assert.NotNull(released.ReleasedAt);
        Assert.Null(released.PrintedAt);
    }

    [Fact]
    public async Task UC_MFG_019_PrintReleased_SetsPrintedAt_AndSlip()
    {
        var fg = await SeedFgAsync("TP-PRINT");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 2m, null, null, null, null));
        await _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id);
        await _mfg.ReleaseWorkOrderAsync(_tenant, _user, wo.Id);

        var (order, slip) = await _mfg.PrintWorkOrderAsync(_tenant, _user, wo.Id);
        Assert.NotNull(order.PrintedAt);
        Assert.Contains("PHIẾU LỆNH SẢN XUẤT", slip);
        Assert.Contains(order.Code, slip);
    }

    [Fact]
    public async Task UC_MFG_019_PrintDraft_Throws()
    {
        var fg = await SeedFgAsync("TP-PRINT-DRAFT");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 1m, null, null, null, null));
        await Assert.ThrowsAsync<AppException>(() => _mfg.PrintWorkOrderAsync(_tenant, _user, wo.Id));
    }

    [Fact]
    public async Task UC_MFG_019_ExportCsv_Released_ContainsHeader()
    {
        var fg = await SeedFgAsync("TP-CSV");
        var wo = await _mfg.UpsertWorkOrderAsync(_tenant, _user,
            new MfgWorkOrderUpsertRequest(null, null, fg.Id, 1m, null, null, null, null));
        await _mfg.ApproveWorkOrderAsync(_tenant, _user, wo.Id);
        await _mfg.ReleaseWorkOrderAsync(_tenant, _user, wo.Id);

        var (fileName, csv) = await _mfg.ExportWorkOrderCsvAsync(_tenant, _user, wo.Id);
        Assert.Contains("LSX_", fileName);
        Assert.Contains("Code,", csv);
        Assert.Contains(wo.Code, csv);
    }
}

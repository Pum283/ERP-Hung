using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 107:
///   UC_LOG_023 — Bàn giao tiền COD (CreateHandoverAsync & SubmitHandoverAsync)
///   UC_LOG_024 — Đối soát 3 chiều COD (ReconcileHandoverAsync)
///   UC_LOG_026 — Xử lý lệch COD (ResolveVarianceAsync)
///   UC_LOG_027 — Tạo phiếu hoàn về kho (ReturnAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class LogStep107PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _logistics;
    private readonly LogCodService _cod;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public LogStep107PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-step107-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin107", DisplayName = "Admin 107" });
        _db.SaveChanges();

        _logistics = new LogLogisticsService(_db);
        _cod = new LogCodService(_db, _logistics);
    }

    public void Dispose() => _db.Dispose();

    private async Task<LogDeliveryOrderDto> PrepareCollectedOrderAsync(string code, decimal amount)
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, code, "Khách Handover", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-H107", "SP H107", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        var detail = await _logistics.GetDeliveryDetailAsync(_tenant, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(detail.Lines[0].Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(amount, 3, null));
        return await _cod.ConfirmCollectedAsync(_tenant, _userAdmin, order.Id, new LogCodCollectRequest("Đã thu"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_023: Bàn giao tiền COD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_023_CreateHandover_ValidCollectedOrders_CreatesDraftHandover()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-01", 500000m);
        var req = new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, "Tài xế Nam", "Bàn giao ca sáng");

        var handover = await _cod.CreateHandoverAsync(_tenant, _userAdmin, req);

        Assert.NotNull(handover);
        Assert.NotNull(handover.Header);
        Assert.Equal("Draft", handover.Header.Status);
        Assert.Equal(500000m, handover.Header.ExpectedAmount);
    }

    [Fact]
    public async Task UC_LOG_023_SubmitHandover_DraftHandover_TransitionsToSubmitted()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-02", 300000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, "Tài xế Nam", null));

        var submitted = await _cod.SubmitHandoverAsync(_tenant, _userAdmin, created.Header.Id);

        Assert.NotNull(submitted);
        Assert.Equal("Submitted", submitted.Header.Status);
        Assert.NotNull(submitted.Header.SubmittedAt);
    }

    [Fact]
    public async Task UC_LOG_023_CreateHandover_EmptyOrderList_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid>(), null, null, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_024: Đối soát 3 chiều COD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_024_ReconcileHandover_ExactAmount_TransitionsToReconciled()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-03", 400000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, null, null));
        await _cod.SubmitHandoverAsync(_tenant, _userAdmin, created.Header.Id);

        var reconciled = await _cod.ReconcileHandoverAsync(_tenant, _userAdmin, created.Header.Id, new LogCodReconcileRequest(400000m, "Khớp đủ tiền"));

        Assert.NotNull(reconciled);
        Assert.Equal("Reconciled", reconciled.Header.Status);
        Assert.Equal(0m, reconciled.Header.VarianceAmount);
    }

    [Fact]
    public async Task UC_LOG_024_ReconcileHandover_VarianceAmount_TransitionsToVariance()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-04", 600000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, null, null));
        await _cod.SubmitHandoverAsync(_tenant, _userAdmin, created.Header.Id);

        var variance = await _cod.ReconcileHandoverAsync(_tenant, _userAdmin, created.Header.Id, new LogCodReconcileRequest(550000m, "Thiếu 50k"));

        Assert.NotNull(variance);
        Assert.Equal("Variance", variance.Header.Status);
        Assert.Equal(50000m, variance.Header.VarianceAmount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_026: Xử lý lệch COD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_026_ResolveVariance_ValidResolution_TransitionsToReconciled()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-05", 200000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, null, null));
        await _cod.SubmitHandoverAsync(_tenant, _userAdmin, created.Header.Id);
        await _cod.ReconcileHandoverAsync(_tenant, _userAdmin, created.Header.Id, new LogCodReconcileRequest(180000m, "Thiếu 20k"));

        var resolved = await _cod.ResolveVarianceAsync(_tenant, _userAdmin, created.Header.Id, new LogCodResolveVarianceRequest(200000m, "Tài xế đã nộp bổ sung 20k"));

        Assert.NotNull(resolved);
        Assert.Equal("Reconciled", resolved.Header.Status);
        Assert.Equal("Tài xế đã nộp bổ sung 20k", resolved.Header.VarianceNote);
    }

    [Fact]
    public async Task UC_LOG_026_ResolveVariance_ShortNote_ThrowsException()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-06", 100000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, null, null));
        await _cod.SubmitHandoverAsync(_tenant, _userAdmin, created.Header.Id);
        await _cod.ReconcileHandoverAsync(_tenant, _userAdmin, created.Header.Id, new LogCodReconcileRequest(90000m, "Lệch"));

        await Assert.ThrowsAsync<AppException>(() =>
            _cod.ResolveVarianceAsync(_tenant, _userAdmin, created.Header.Id, new LogCodResolveVarianceRequest(100000m, "OK")));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_027: Tạo phiếu hoàn về kho
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_027_Return_DispatchedOrder_TransitionsToReturned()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-107-07", "Khách Return", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-R107", "SP R107", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        var detail = await _logistics.GetDeliveryDetailAsync(_tenant, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(detail.Lines[0].Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);

        var returned = await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", "Khách không nhận hàng, hoàn kho"));

        Assert.NotNull(returned);
        Assert.Equal("Returned", returned.Status);
    }

    [Fact]
    public async Task UC_LOG_027_Return_DraftOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-107-08", "Khách Draft Return", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", null)));
    }

    [Fact]
    public async Task UC_LOG_024_ReconcileHandover_DraftHandover_ThrowsException()
    {
        var o1 = await PrepareCollectedOrderAsync("SO-107-09", 150000m);
        var created = await _cod.CreateHandoverAsync(_tenant, _userAdmin, new LogCodHandoverCreateRequest(new List<Guid> { o1.Id }, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _cod.ReconcileHandoverAsync(_tenant, _userAdmin, created.Header.Id, new LogCodReconcileRequest(150000m, null)));
    }
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Application.Interfaces.Services.Log;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Log;

public sealed class LogCodService : ILogCodService
{
    private readonly AppDbContext _db;
    private readonly ILogLogisticsService _logistics;

    public LogCodService(AppDbContext db, ILogLogisticsService logistics)
    {
        _db = db;
        _logistics = logistics;
    }

    public async Task<LogDeliveryOrderDto> MarkCodAsync(
        Guid tenantId, Guid userId, Guid orderId, LogCodMarkRequest req, CancellationToken ct = default)
    {
        if (req.Amount <= 0) throw new AppException("Số tiền COD phải > 0.");
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is "Cancelled" or "Returned")
            throw new AppException("Không đánh dấu COD trên lệnh đã hủy/hoàn.");
        if (order.CodStatus is "Remitted" or "Reconciled")
            throw new AppException("COD đã nộp/đối soát — không đánh dấu lại.");

        var dueDays = req.DueDays is > 0 and <= 90 ? req.DueDays.Value : 3;
        order.IsCod = true;
        order.CodAmount = decimal.Round(req.Amount, 2);
        order.CodStatus = "Pending";
        order.CodDueAt = DateTimeOffset.UtcNow.Date.AddDays(dueDays);
        order.CodNote = NullIfEmpty(req.Note);
        order.UpdatedBy = userId;
        await AddEvent(tenantId, order.Id, order.Status, $"Đánh dấu COD {order.CodAmount:0.##}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await _logistics.GetDeliveryDetailAsync(tenantId, orderId, ct)).Order;
    }

    public async Task<LogDeliveryOrderDto> SetCodAmountAsync(
        Guid tenantId, Guid userId, Guid orderId, LogCodAmountRequest req, CancellationToken ct = default)
    {
        if (req.Amount <= 0) throw new AppException("Số tiền COD phải > 0.");
        var order = await RequireOrder(tenantId, orderId, ct);
        if (!order.IsCod || order.CodStatus is "None")
            throw new AppException("Lệnh chưa đánh dấu COD.");
        if (order.CodStatus is not ("Pending" or "Collected" or "Variance"))
            throw new AppException("Chỉ sửa tiền COD khi Pending/Collected/Variance.");

        order.CodAmount = decimal.Round(req.Amount, 2);
        if (!string.IsNullOrWhiteSpace(req.Note)) order.CodNote = req.Note.Trim();
        order.UpdatedBy = userId;
        await AddEvent(tenantId, order.Id, order.Status, $"Cập nhật tiền COD {order.CodAmount:0.##}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await _logistics.GetDeliveryDetailAsync(tenantId, orderId, ct)).Order;
    }

    public async Task<LogDeliveryOrderDto> ConfirmCollectedAsync(
        Guid tenantId, Guid userId, Guid orderId, LogCodCollectRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (!order.IsCod || order.CodStatus != "Pending")
            throw new AppException("Chỉ xác nhận thu khi COD đang Pending.");
        if (order.Status is not ("Delivered" or "InTransit" or "Dispatched"))
            throw new AppException("Xác nhận thu COD khi lệnh đã giao / đang giao.");

        order.CodStatus = "Collected";
        order.CodCollectedAt = DateTimeOffset.UtcNow;
        order.CodCollectedByUserId = userId;
        if (!string.IsNullOrWhiteSpace(req.Note)) order.CodNote = req.Note.Trim();
        order.UpdatedBy = userId;
        await AddEvent(tenantId, order.Id, order.Status, $"Đã thu COD {order.CodAmount:0.##}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await _logistics.GetDeliveryDetailAsync(tenantId, orderId, ct)).Order;
    }

    public async Task<IReadOnlyList<LogDeliveryOrderDto>> ListCodDeliveriesAsync(
        Guid tenantId, string? status, CancellationToken ct = default)
    {
        var q = _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsCod);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var st = status.Trim();
            q = q.Where(x => x.CodStatus == st);
        }
        else
        {
            q = q.Where(x => x.CodStatus != "None");
        }
        var list = await q.OrderByDescending(x => x.UpdatedAt).Take(300).ToListAsync(ct);
        return await MapOrdersAsync(tenantId, list, ct);
    }

    public async Task<IReadOnlyList<LogDeliveryOrderDto>> ListOverdueAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var list = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsCod
                        && (x.CodStatus == "Pending" || x.CodStatus == "Collected")
                        && x.CodDueAt != null && x.CodDueAt < now)
            .OrderBy(x => x.CodDueAt)
            .Take(300)
            .ToListAsync(ct);
        return await MapOrdersAsync(tenantId, list, ct);
    }

    public async Task<LogCodReportDto> GetReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsCod && x.CodStatus != "None")
            .Select(x => new { x.CodStatus, x.CodAmount, x.CodDueAt })
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;

        static (decimal amt, int cnt) Agg(IEnumerable<(string St, decimal Amt)> src, string st)
        {
            var m = src.Where(x => x.St == st).ToList();
            return (m.Sum(x => x.Amt), m.Count);
        }

        var tuples = rows.Select(x => (x.CodStatus, x.CodAmount)).ToList();
        var (pAmt, pCnt) = Agg(tuples, "Pending");
        var (cAmt, cCnt) = Agg(tuples, "Collected");
        var (rAmt, rCnt) = Agg(tuples, "Remitted");
        var (rcAmt, rcCnt) = Agg(tuples, "Reconciled");
        var (vAmt, vCnt) = Agg(tuples, "Variance");
        var overdue = rows.Where(x =>
            (x.CodStatus is "Pending" or "Collected") && x.CodDueAt < now).ToList();

        return new LogCodReportDto(
            pAmt, pCnt, cAmt, cCnt, rAmt, rCnt, rcAmt, rcCnt,
            overdue.Sum(x => x.CodAmount), overdue.Count,
            vAmt, vCnt);
    }

    public async Task<IReadOnlyList<LogCodHandoverDto>> ListHandoversAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogCodHandovers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync(ct);
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.LogCodHandoverLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.HandoverId) && !x.IsDeleted)
            .GroupBy(x => x.HandoverId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(h => MapHandover(h, counts.GetValueOrDefault(h.Id))).ToList();
    }

    public async Task<LogCodHandoverDetailDto> GetHandoverAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var h = await _db.LogCodHandovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy bàn giao COD.");
        return await BuildDetail(tenantId, h, ct);
    }

    public async Task<LogCodHandoverDetailDto> CreateHandoverAsync(
        Guid tenantId, Guid userId, LogCodHandoverCreateRequest req, CancellationToken ct = default)
    {
        if (req.DeliveryOrderIds is null || req.DeliveryOrderIds.Count == 0)
            throw new AppException("Chọn ít nhất một lệnh COD đã thu.");

        var ids = req.DeliveryOrderIds.Distinct().ToList();
        var orders = await _db.LogDeliveryOrders
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);
        if (orders.Count != ids.Count) throw new AppException("Có lệnh giao không hợp lệ.");
        if (orders.Any(x => !x.IsCod || x.CodStatus != "Collected"))
            throw new AppException("Chỉ bàn giao lệnh COD ở trạng thái Collected.");
        if (orders.Any(x => x.CodHandoverId.HasValue))
            throw new AppException("Có lệnh đã nằm trong bàn giao khác.");

        string? driverName = NullIfEmpty(req.DriverName);
        if (req.DriverUserId is Guid du)
        {
            var u = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == du && x.TenantId == tenantId, ct);
            driverName ??= u?.DisplayName ?? u?.Username;
        }

        var expected = orders.Sum(x => x.CodAmount);
        var handover = new LogCodHandover
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "COD", ct),
            Status = "Draft",
            DriverUserId = req.DriverUserId,
            DriverName = driverName,
            ExpectedAmount = expected,
            CollectedAmount = expected,
            RemittedAmount = 0,
            VarianceAmount = 0,
            Note = NullIfEmpty(req.Note),
            CreatedByUserId = userId,
            CreatedBy = userId
        };
        _db.LogCodHandovers.Add(handover);
        await _db.SaveChangesAsync(ct);

        foreach (var o in orders)
        {
            _db.LogCodHandoverLines.Add(new LogCodHandoverLine
            {
                TenantId = tenantId,
                HandoverId = handover.Id,
                DeliveryOrderId = o.Id,
                CodAmount = o.CodAmount,
                CreatedBy = userId
            });
            o.CodHandoverId = handover.Id;
            o.UpdatedBy = userId;
        }
        await _db.SaveChangesAsync(ct);
        return await GetHandoverAsync(tenantId, handover.Id, ct);
    }

    public async Task<LogCodHandoverDetailDto> SubmitHandoverAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var h = await RequireHandover(tenantId, id, ct);
        if (h.Status != "Draft") throw new AppException("Chỉ nộp bàn giao khi Draft.");

        var lines = await _db.LogCodHandoverLines
            .Where(x => x.TenantId == tenantId && x.HandoverId == id && !x.IsDeleted)
            .ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Bàn giao trống.");

        var orderIds = lines.Select(x => x.DeliveryOrderId).ToList();
        var orders = await _db.LogDeliveryOrders
            .Where(x => x.TenantId == tenantId && orderIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);

        h.Status = "Submitted";
        h.SubmittedAt = DateTimeOffset.UtcNow;
        h.RemittedAmount = h.CollectedAmount;
        h.UpdatedBy = userId;
        foreach (var o in orders)
        {
            o.CodStatus = "Remitted";
            o.UpdatedBy = userId;
            await AddEvent(tenantId, o.Id, o.Status, $"Bàn giao COD {h.Code}", userId, ct);
        }
        await _db.SaveChangesAsync(ct);
        return await GetHandoverAsync(tenantId, id, ct);
    }

    public async Task<LogCodHandoverDetailDto> ReconcileHandoverAsync(
        Guid tenantId, Guid userId, Guid id, LogCodReconcileRequest req, CancellationToken ct = default)
    {
        var h = await RequireHandover(tenantId, id, ct);
        if (h.Status != "Submitted") throw new AppException("Chỉ đối soát khi đã nộp (Submitted).");
        if (req.RemittedAmount < 0) throw new AppException("Số tiền nộp không hợp lệ.");

        var remitted = decimal.Round(req.RemittedAmount, 2);
        var variance = decimal.Round(h.ExpectedAmount - remitted, 2);
        h.RemittedAmount = remitted;
        h.VarianceAmount = variance;
        h.ReconciledAt = DateTimeOffset.UtcNow;
        h.UpdatedBy = userId;
        if (!string.IsNullOrWhiteSpace(req.Note)) h.Note = req.Note.Trim();

        var lines = await _db.LogCodHandoverLines
            .Where(x => x.TenantId == tenantId && x.HandoverId == id && !x.IsDeleted)
            .ToListAsync(ct);
        var orderIds = lines.Select(x => x.DeliveryOrderId).ToList();
        var orders = await _db.LogDeliveryOrders
            .Where(x => x.TenantId == tenantId && orderIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);

        if (variance == 0)
        {
            h.Status = "Reconciled";
            foreach (var o in orders)
            {
                o.CodStatus = "Reconciled";
                o.UpdatedBy = userId;
                await AddEvent(tenantId, o.Id, o.Status, $"Đối soát COD khớp · {h.Code}", userId, ct);
            }
        }
        else
        {
            h.Status = "Variance";
            h.VarianceNote = NullIfEmpty(req.Note) ?? $"Lệch {variance:0.##}";
            foreach (var o in orders)
            {
                o.CodStatus = "Variance";
                o.UpdatedBy = userId;
                await AddEvent(tenantId, o.Id, o.Status, $"Lệch COD {variance:0.##} · {h.Code}", userId, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        return await GetHandoverAsync(tenantId, id, ct);
    }

    public async Task<LogCodHandoverDetailDto> ResolveVarianceAsync(
        Guid tenantId, Guid userId, Guid id, LogCodResolveVarianceRequest req, CancellationToken ct = default)
    {
        var h = await RequireHandover(tenantId, id, ct);
        if (h.Status != "Variance") throw new AppException("Chỉ xử lý lệch khi Status = Variance.");
        var note = (req.Note ?? "").Trim();
        if (note.Length < 3) throw new AppException("Ghi chú xử lý lệch (≥ 3 ký tự).");

        if (req.RemittedAmount is decimal amt)
        {
            if (amt < 0) throw new AppException("Số tiền nộp không hợp lệ.");
            h.RemittedAmount = decimal.Round(amt, 2);
            h.VarianceAmount = decimal.Round(h.ExpectedAmount - h.RemittedAmount, 2);
        }

        h.Status = "Reconciled";
        h.VarianceNote = note;
        h.ReconciledAt = DateTimeOffset.UtcNow;
        h.UpdatedBy = userId;

        var orderIds = await _db.LogCodHandoverLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.HandoverId == id && !x.IsDeleted)
            .Select(x => x.DeliveryOrderId)
            .ToListAsync(ct);
        var orders = await _db.LogDeliveryOrders
            .Where(x => x.TenantId == tenantId && orderIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var o in orders)
        {
            o.CodStatus = "Reconciled";
            o.CodNote = note;
            o.UpdatedBy = userId;
            await AddEvent(tenantId, o.Id, o.Status, $"Xử lý lệch COD · {h.Code}: {note}", userId, ct);
        }

        await _db.SaveChangesAsync(ct);
        return await GetHandoverAsync(tenantId, id, ct);
    }

    private async Task<IReadOnlyList<LogDeliveryOrderDto>> MapOrdersAsync(
        Guid tenantId, List<LogDeliveryOrder> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<LogDeliveryOrderDto>();
        var ids = list.Select(x => x.Id).ToList();
        var carrierIds = list.Where(x => x.CarrierId.HasValue).Select(x => x.CarrierId!.Value).Distinct().ToList();
        var carriers = carrierIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.LogCarriers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && carrierIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var counts = await _db.LogDeliveryLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.DeliveryOrderId) && !x.IsDeleted)
            .GroupBy(x => x.DeliveryOrderId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var now = DateTimeOffset.UtcNow;
        return list.Select(o => new LogDeliveryOrderDto(
            o.Id, o.Code, o.SourceOrderCode, o.CustomerName, o.ShipAddress, o.Phone,
            o.Status, o.CarrierId,
            o.CarrierId is Guid cid ? carriers.GetValueOrDefault(cid) : null,
            o.DriverUserId, o.DriverName, o.ParentOrderId, o.BatchNo, o.Note, o.FailureReason,
            o.WaybillNo, o.WaybillPrintedAt, o.PickedAt, o.DispatchedAt, o.DeliveredAt,
            o.PromisedAt,
            o.Status == "Delivered" && o.PromisedAt is DateTimeOffset p && o.DeliveredAt is DateTimeOffset d
                ? d <= p : null,
            o.IsCod, o.CodAmount, o.CodStatus, o.CodDueAt, o.CodCollectedAt, o.CodHandoverId,
            o.IsCod && (o.CodStatus is "Pending" or "Collected")
                && o.CodDueAt is DateTimeOffset due && due < now,
            counts.GetValueOrDefault(o.Id))).ToList();
    }

    private async Task<LogCodHandoverDetailDto> BuildDetail(
        Guid tenantId, LogCodHandover h, CancellationToken ct)
    {
        var lines = await _db.LogCodHandoverLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.HandoverId == h.Id && !x.IsDeleted)
            .ToListAsync(ct);
        var orderIds = lines.Select(x => x.DeliveryOrderId).ToList();
        var orders = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && orderIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var lineDtos = lines.Select(l =>
        {
            orders.TryGetValue(l.DeliveryOrderId, out var o);
            return new LogCodHandoverLineDto(
                l.Id, l.HandoverId, l.DeliveryOrderId,
                o?.Code ?? "?", o?.CustomerName ?? "?", l.CodAmount, l.Note);
        }).ToList();

        return new LogCodHandoverDetailDto(MapHandover(h, lineDtos.Count), lineDtos);
    }

    private async Task<LogDeliveryOrder> RequireOrder(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.LogDeliveryOrders.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy lệnh giao.");

    private async Task<LogCodHandover> RequireHandover(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.LogCodHandovers.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy bàn giao COD.");

    private async Task AddEvent(
        Guid tenantId, Guid orderId, string status, string? note, Guid userId, CancellationToken ct)
    {
        _db.LogShipmentEvents.Add(new LogShipmentEvent
        {
            TenantId = tenantId,
            DeliveryOrderId = orderId,
            Status = status,
            Note = note,
            ActorUserId = userId,
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        });
        await Task.CompletedTask;
    }

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.LogCodHandovers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n))
            seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private static LogCodHandoverDto MapHandover(LogCodHandover h, int lineCount) =>
        new(h.Id, h.Code, h.Status, h.DriverUserId, h.DriverName,
            h.ExpectedAmount, h.CollectedAmount, h.RemittedAmount, h.VarianceAmount,
            h.Note, h.VarianceNote, h.SubmittedAt, h.ReconciledAt, lineCount, h.CreatedAt);

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

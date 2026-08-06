using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Application.Interfaces.Services.Log;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Log;

public sealed class LogReturnService : ILogReturnService
{
    private static readonly HashSet<string> AllowCreateFrom =
        new(StringComparer.OrdinalIgnoreCase)
        { "Delivered", "Failed", "Returned", "InTransit", "Dispatched" };

    private readonly AppDbContext _db;
    private readonly IInvStockService _inv;

    public LogReturnService(AppDbContext db, IInvStockService inv)
    {
        _db = db;
        _inv = inv;
    }

    public async Task<IReadOnlyList<LogReturnNoteDto>> ListAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default)
    {
        var q = _db.LogReturnNotes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapNotesAsync(tenantId, list, ct);
    }

    public async Task<LogReturnDetailDto> GetDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var note = await _db.LogReturnNotes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy phiếu hoàn.");
        var lines = await _db.LogReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReturnNoteId == id && !x.IsDeleted)
            .OrderBy(x => x.ProductCode).ToListAsync(ct);
        return new LogReturnDetailDto(
            (await MapNotesAsync(tenantId, [note], ct))[0],
            lines.Select(MapLine).ToList());
    }

    public async Task<LogReturnDetailDto> CreateAsync(
        Guid tenantId, Guid userId, LogReturnCreateRequest req, CancellationToken ct = default)
    {
        var order = await _db.LogDeliveryOrders.FirstOrDefaultAsync(
            x => x.Id == req.DeliveryOrderId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy lệnh giao.");
        if (!AllowCreateFrom.Contains(order.Status))
            throw new AppException("Chỉ tạo phiếu hoàn từ lệnh Delivered/Failed/Returned/InTransit/Dispatched.");

        await RequireWarehouse(tenantId, req.WarehouseId, ct);

        var openExists = await _db.LogReturnNotes.AnyAsync(
            x => x.TenantId == tenantId && x.DeliveryOrderId == order.Id && !x.IsDeleted
                 && x.Status != "Cancelled" && x.Status != "Posted", ct);
        if (openExists) throw new AppException("Lệnh đã có phiếu hoàn đang mở.");

        var dLines = await _db.LogDeliveryLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeliveryOrderId == order.Id && !x.IsDeleted)
            .ToListAsync(ct);
        if (dLines.Count == 0) throw new AppException("Lệnh giao không có dòng hàng.");

        var note = new LogReturnNote
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "HR", ct),
            DeliveryOrderId = order.Id,
            WarehouseId = req.WarehouseId,
            Status = "Draft",
            Reason = NullIfEmpty(req.Reason),
            Note = NullIfEmpty(req.Note),
            CreatedByUserId = userId,
            CreatedBy = userId
        };
        _db.LogReturnNotes.Add(note);
        await _db.SaveChangesAsync(ct);

        foreach (var dl in dLines)
        {
            var expected = dl.QtyPicked > 0 ? dl.QtyPicked : dl.Qty;
            _db.LogReturnLines.Add(new LogReturnLine
            {
                TenantId = tenantId,
                ReturnNoteId = note.Id,
                DeliveryLineId = dl.Id,
                ProductCode = dl.ProductCode,
                ProductName = dl.ProductName,
                Unit = dl.Unit,
                QtyExpected = expected,
                QtyCounted = 0,
                QtyAccepted = 0,
                CreatedBy = userId
            });
        }

        if (order.Status != "Returned")
        {
            order.Status = "Returned";
            order.UpdatedBy = userId;
            _db.LogShipmentEvents.Add(new LogShipmentEvent
            {
                TenantId = tenantId, DeliveryOrderId = order.Id,
                Status = "Returned", Note = $"Tạo phiếu hoàn {note.Code}",
                ActorUserId = userId, OccurredAt = DateTimeOffset.UtcNow, CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync(ct);
        return await GetDetailAsync(tenantId, note.Id, ct);
    }

    public async Task<LogReturnLineDto> CountLineAsync(
        Guid tenantId, Guid userId, Guid noteId, LogReturnCountRequest req, CancellationToken ct = default)
    {
        var note = await RequireNote(tenantId, noteId, ct);
        if (note.Status != "Draft") throw new AppException("Chỉ đếm khi Draft.");
        if (req.QtyCounted < 0) throw new AppException("SL đếm ≥ 0.");

        var line = await _db.LogReturnLines.FirstOrDefaultAsync(
            x => x.Id == req.LineId && x.TenantId == tenantId && x.ReturnNoteId == noteId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy dòng.");

        line.QtyCounted = decimal.Round(req.QtyCounted, 4);
        line.QtyAccepted = decimal.Round(req.QtyAccepted ?? req.QtyCounted, 4);
        if (line.QtyAccepted < 0) throw new AppException("SL Accepted ≥ 0.");
        if (line.QtyAccepted > line.QtyCounted)
            throw new AppException("Accepted không vượt Counted.");
        if (!string.IsNullOrWhiteSpace(req.Note)) line.Note = req.Note.Trim();
        line.UpdatedBy = userId;
        note.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapLine(line);
    }

    public async Task<LogReturnDetailDto> ConfirmCountAsync(
        Guid tenantId, Guid userId, Guid noteId, CancellationToken ct = default)
    {
        var note = await RequireNote(tenantId, noteId, ct);
        if (note.Status != "Draft") throw new AppException("Chỉ xác nhận đếm khi Draft.");
        var lines = await _db.LogReturnLines
            .Where(x => x.TenantId == tenantId && x.ReturnNoteId == noteId && !x.IsDeleted)
            .ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Phiếu trống.");
        foreach (var l in lines)
        {
            if (l.QtyCounted <= 0) l.QtyCounted = l.QtyExpected;
            if (l.QtyAccepted <= 0) l.QtyAccepted = l.QtyCounted;
        }
        if (lines.Sum(x => x.QtyAccepted) <= 0)
            throw new AppException("Tổng Accepted phải > 0.");

        note.Status = "Counted";
        note.CountedAt = DateTimeOffset.UtcNow;
        note.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetDetailAsync(tenantId, noteId, ct);
    }

    public async Task<LogReturnDetailDto> PostAsync(
        Guid tenantId, Guid userId, Guid noteId, CancellationToken ct = default)
    {
        var note = await RequireNote(tenantId, noteId, ct);
        if (note.Status != "Counted") throw new AppException("Chỉ nhập kho khi Counted.");

        var invDoc = await _inv.PostReceiptFromLogReturnAsync(tenantId, userId, noteId, note.WarehouseId, ct);
        note.Status = "Posted";
        note.PostedAt = DateTimeOffset.UtcNow;
        note.InvStockDocId = invDoc.Id;
        note.InvStockDocCode = invDoc.Code;
        note.UpdatedBy = userId;

        _db.LogShipmentEvents.Add(new LogShipmentEvent
        {
            TenantId = tenantId, DeliveryOrderId = note.DeliveryOrderId,
            Status = "Returned", Note = $"Nhập kho hoàn {note.Code} → INV {invDoc.Code}",
            ActorUserId = userId, OccurredAt = DateTimeOffset.UtcNow, CreatedBy = userId
        });
        await _db.SaveChangesAsync(ct);
        return await GetDetailAsync(tenantId, noteId, ct);
    }

    public async Task<LogReturnDetailDto> CancelAsync(
        Guid tenantId, Guid userId, Guid noteId, string? noteText = null, CancellationToken ct = default)
    {
        var note = await RequireNote(tenantId, noteId, ct);
        if (note.Status is "Posted") throw new AppException("Phiếu đã nhập kho — không hủy.");
        if (note.Status == "Cancelled") throw new AppException("Đã hủy.");
        note.Status = "Cancelled";
        if (!string.IsNullOrWhiteSpace(noteText)) note.Note = noteText.Trim();
        note.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetDetailAsync(tenantId, noteId, ct);
    }

    public async Task<LogOpsReportDto> GetOpsReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var statuses = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .Select(g => new { g.Key, C = g.Count() })
            .ToListAsync(ct);
        int C(string s) => statuses.FirstOrDefault(x => x.Key == s)?.C ?? 0;

        var delivered = C("Delivered");
        var failed = C("Failed");
        var returned = C("Returned");
        var inTransit = C("InTransit") + C("Dispatched");
        var open = C("Draft") + C("Confirmed") + C("Picking") + C("Ready");
        var closed = delivered + failed + returned;
        var returnRate = closed == 0 ? 0 : Math.Round(100m * returned / closed, 1);
        var failRate = closed == 0 ? 0 : Math.Round(100m * failed / closed, 1);

        var retNotes = await _db.LogReturnNotes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .Select(g => new { g.Key, C = g.Count() })
            .ToListAsync(ct);
        int Rn(string s) => retNotes.FirstOrDefault(x => x.Key == s)?.C ?? 0;

        var now = DateTimeOffset.UtcNow;
        var codOverdue = await _db.LogDeliveryOrders.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.IsCod
                             && (x.CodStatus == "Pending" || x.CodStatus == "Collected")
                             && x.CodDueAt != null && x.CodDueAt < now, ct);

        var promisedDelivered = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Delivered"
                        && x.PromisedAt != null && x.DeliveredAt != null)
            .Select(x => new { x.PromisedAt, x.DeliveredAt })
            .ToListAsync(ct);
        var onTime = promisedDelivered.Count(x => x.DeliveredAt <= x.PromisedAt);
        var late = promisedDelivered.Count - onTime;
        var onTimeRate = promisedDelivered.Count == 0
            ? 0 : Math.Round(100m * onTime / promisedDelivered.Count, 1);

        return new LogOpsReportDto(
            delivered, failed, returned, inTransit, open,
            returnRate, failRate,
            Rn("Draft"), Rn("Counted"), Rn("Posted"),
            codOverdue,
            onTime, late, promisedDelivered.Count, onTimeRate);
    }

    private async Task RequireWarehouse(Guid tenantId, Guid warehouseId, CancellationToken ct)
    {
        var ok = await _db.InvWarehouses.AsNoTracking()
            .AnyAsync(x => x.Id == warehouseId && x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);
        if (!ok) throw new AppException("Kho không hợp lệ / không Active.");
    }

    private async Task<LogReturnNote> RequireNote(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.LogReturnNotes.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy phiếu hoàn.");

    private async Task<IReadOnlyList<LogReturnNoteDto>> MapNotesAsync(
        Guid tenantId, List<LogReturnNote> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<LogReturnNoteDto>();
        var ids = list.Select(x => x.Id).ToList();
        var dids = list.Select(x => x.DeliveryOrderId).Distinct().ToList();
        var wids = list.Select(x => x.WarehouseId).Distinct().ToList();
        var deliveries = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => dids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var whs = await _db.InvWarehouses.AsNoTracking()
            .Where(x => wids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var aggs = await _db.LogReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ReturnNoteId) && !x.IsDeleted)
            .GroupBy(x => x.ReturnNoteId)
            .Select(g => new
            {
                g.Key,
                C = g.Count(),
                Exp = g.Sum(x => x.QtyExpected),
                Acc = g.Sum(x => x.QtyAccepted)
            }).ToDictionaryAsync(x => x.Key, ct);

        return list.Select(n =>
        {
            aggs.TryGetValue(n.Id, out var a);
            return new LogReturnNoteDto(
                n.Id, n.Code, n.DeliveryOrderId, deliveries.GetValueOrDefault(n.DeliveryOrderId),
                n.WarehouseId, whs.GetValueOrDefault(n.WarehouseId), n.Status,
                n.Reason, n.Note, n.CountedAt, n.PostedAt, n.InvStockDocId, n.InvStockDocCode,
                a?.C ?? 0, a?.Exp ?? 0, a?.Acc ?? 0, n.CreatedAt);
        }).ToList();
    }

    private static LogReturnLineDto MapLine(LogReturnLine l) =>
        new(l.Id, l.ReturnNoteId, l.DeliveryLineId, l.ProductCode, l.ProductName, l.Unit,
            l.QtyExpected, l.QtyCounted, l.QtyAccepted, l.Note);

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.LogReturnNotes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Application.Interfaces.Services.Log;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Log;

public sealed class LogLogisticsService : ILogLogisticsService
{
    private static readonly HashSet<string> Editable =
        new(StringComparer.OrdinalIgnoreCase) { "Draft" };
    private static readonly HashSet<string> TrackStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "InTransit", "Delivered", "Failed", "Dispatched", "Ready", "Picking", "Confirmed"
        };

    private readonly AppDbContext _db;
    public LogLogisticsService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LogCarrierDto>> ListCarriersAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.LogCarriers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.Name.Contains(term)
                || (x.Phone != null && x.Phone.Contains(term)));
        }
        var list = await query.OrderBy(x => x.Code).Take(300).ToListAsync(ct);
        return list.Select(MapCarrier).ToList();
    }

    public async Task<LogCarrierDto> UpsertCarrierAsync(
        Guid tenantId, Guid userId, LogCarrierUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên ĐVVC");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Trạng thái ĐVVC không hợp lệ.");

        LogCarrier entity;
        if (req.Id is Guid id)
        {
            entity = await _db.LogCarriers.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy ĐVVC.");
        }
        else
        {
            if (await _db.LogCarriers.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã ĐVVC đã tồn tại.");
            entity = new LogCarrier { TenantId = tenantId, CreatedBy = userId };
            _db.LogCarriers.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.Phone = NullIfEmpty(req.Phone);
        entity.ContactName = NullIfEmpty(req.ContactName);
        entity.Note = NullIfEmpty(req.Note);
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapCarrier(entity);
    }

    public async Task<IReadOnlyList<LogDeliveryOrderDto>> ListDeliveriesAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.SourceOrderCode.Contains(term)
                || x.CustomerName.Contains(term)
                || (x.WaybillNo != null && x.WaybillNo.Contains(term)));
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapOrdersAsync(tenantId, list, ct);
    }

    public async Task<LogDeliveryDetailDto> GetDeliveryDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, id, ct, track: false);
        var dto = (await MapOrdersAsync(tenantId, [order], ct))[0];

        var lines = await _db.LogDeliveryLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeliveryOrderId == id && !x.IsDeleted)
            .OrderBy(x => x.ProductCode)
            .Select(x => new LogDeliveryLineDto(
                x.Id, x.DeliveryOrderId, x.ProductCode, x.ProductName, x.Qty, x.QtyPicked, x.Unit, x.Note))
            .ToListAsync(ct);

        var events = await _db.LogShipmentEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeliveryOrderId == id && !x.IsDeleted)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(ct);
        var actorIds = events.Select(x => x.ActorUserId).Distinct().ToList();
        var actors = actorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && actorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var eventDtos = events.Select(e => new LogShipmentEventDto(
            e.Id, e.DeliveryOrderId, e.Status, e.Note, e.ActorUserId,
            actors.GetValueOrDefault(e.ActorUserId), e.OccurredAt)).ToList();

        var children = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentOrderId == id && !x.IsDeleted)
            .OrderBy(x => x.BatchNo)
            .ToListAsync(ct);
        var childDtos = await MapOrdersAsync(tenantId, children, ct);

        return new LogDeliveryDetailDto(dto, lines, eventDtos, childDtos);
    }

    public async Task<LogDeliveryOrderDto> UpsertDeliveryAsync(
        Guid tenantId, Guid userId, LogDeliveryUpsertRequest req, CancellationToken ct = default)
    {
        var so = Req(req.SourceOrderCode, 40, "Mã đơn hàng nguồn");
        var customer = Req(req.CustomerName, 200, "Khách hàng");

        LogDeliveryOrder entity;
        if (req.Id is Guid id)
        {
            entity = await RequireOrder(tenantId, id, ct);
            EnsureEditable(entity);
            if (!string.IsNullOrWhiteSpace(req.Code))
            {
                var code = NormCode(req.Code);
                if (await _db.LogDeliveryOrders.AnyAsync(
                        x => x.TenantId == tenantId && x.Code == code && x.Id != id && !x.IsDeleted, ct))
                    throw new AppException("Mã lệnh giao đã tồn tại.");
                entity.Code = code;
            }
        }
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? await NextCodeAsync(tenantId, "DG", ct)
                : NormCode(req.Code);
            if (await _db.LogDeliveryOrders.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã lệnh giao đã tồn tại.");
            entity = new LogDeliveryOrder
            {
                TenantId = tenantId, Code = code, Status = "Draft",
                CreatedByUserId = userId, CreatedBy = userId, BatchNo = 1
            };
            _db.LogDeliveryOrders.Add(entity);
        }

        entity.SourceOrderCode = so.ToUpperInvariant();
        entity.CustomerName = customer;
        entity.ShipAddress = NullIfEmpty(req.ShipAddress);
        entity.Phone = NullIfEmpty(req.Phone);
        entity.Note = NullIfEmpty(req.Note);
        if (req.PromisedAt.HasValue) entity.PromisedAt = req.PromisedAt;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await AddEvent(tenantId, entity.Id, entity.Status, "Lưu lệnh giao", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<LogDeliveryLineDto> UpsertLineAsync(
        Guid tenantId, Guid userId, Guid orderId, LogDeliveryLineUpsertRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        EnsureEditable(order);
        if (req.Qty <= 0) throw new AppException("Số lượng phải > 0.");
        var pCode = NormCode(req.ProductCode);
        var pName = Req(req.ProductName, 200, "Tên SP");
        var unit = string.IsNullOrWhiteSpace(req.Unit) ? "CAI" : req.Unit.Trim().ToUpperInvariant();

        LogDeliveryLine line;
        if (req.Id is Guid lid)
        {
            line = await _db.LogDeliveryLines.FirstOrDefaultAsync(
                x => x.Id == lid && x.TenantId == tenantId && x.DeliveryOrderId == orderId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy dòng hàng.");
        }
        else
        {
            line = new LogDeliveryLine { TenantId = tenantId, DeliveryOrderId = orderId, CreatedBy = userId };
            _db.LogDeliveryLines.Add(line);
        }

        line.ProductCode = pCode;
        line.ProductName = pName;
        line.Qty = req.Qty;
        line.Unit = unit;
        line.Note = NullIfEmpty(req.Note);
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new LogDeliveryLineDto(
            line.Id, line.DeliveryOrderId, line.ProductCode, line.ProductName,
            line.Qty, line.QtyPicked, line.Unit, line.Note);
    }

    public async Task<LogDeliveryOrderDto> ConfirmAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status != "Draft") throw new AppException("Chỉ xác nhận lệnh Draft.");
        var hasLines = await _db.LogDeliveryLines.AnyAsync(
            x => x.TenantId == tenantId && x.DeliveryOrderId == orderId && !x.IsDeleted, ct);
        if (!hasLines) throw new AppException("Cần ít nhất 1 dòng hàng.");
        order.Status = "Confirmed";
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Confirmed", "Xác nhận lệnh giao", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> SplitBatchAsync(
        Guid tenantId, Guid userId, Guid orderId, LogSplitBatchRequest req, CancellationToken ct = default)
    {
        var parent = await RequireOrder(tenantId, orderId, ct);
        if (parent.Status is not ("Draft" or "Confirmed"))
            throw new AppException("Chỉ tách đợt khi Draft/Confirmed.");
        if (req.Lines is null || req.Lines.Count == 0)
            throw new AppException("Chọn dòng hàng để tách.");

        var maxBatch = await _db.LogDeliveryOrders
            .Where(x => x.TenantId == tenantId && (x.Id == orderId || x.ParentOrderId == orderId) && !x.IsDeleted)
            .MaxAsync(x => (int?)x.BatchNo, ct) ?? parent.BatchNo;

        var child = new LogDeliveryOrder
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "DG", ct),
            SourceOrderCode = parent.SourceOrderCode,
            CustomerName = parent.CustomerName,
            ShipAddress = parent.ShipAddress,
            Phone = parent.Phone,
            Status = "Draft",
            ParentOrderId = parent.ParentOrderId ?? parent.Id,
            BatchNo = maxBatch + 1,
            Note = NullIfEmpty(req.Note) ?? $"Tách từ {parent.Code}",
            CreatedByUserId = userId,
            CreatedBy = userId
        };
        _db.LogDeliveryOrders.Add(child);
        await _db.SaveChangesAsync(ct);

        foreach (var item in req.Lines)
        {
            var src = await _db.LogDeliveryLines.FirstOrDefaultAsync(
                x => x.Id == item.LineId && x.TenantId == tenantId
                     && x.DeliveryOrderId == orderId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng hàng không hợp lệ.");
            if (item.Qty <= 0 || item.Qty > src.Qty)
                throw new AppException($"SL tách {src.ProductCode} không hợp lệ.");

            _db.LogDeliveryLines.Add(new LogDeliveryLine
            {
                TenantId = tenantId, DeliveryOrderId = child.Id,
                ProductCode = src.ProductCode, ProductName = src.ProductName,
                Qty = item.Qty, Unit = src.Unit, Note = src.Note, CreatedBy = userId
            });

            src.Qty -= item.Qty;
            src.UpdatedBy = userId;
            if (src.Qty == 0)
            {
                src.IsDeleted = true;
                src.DeletedAt = DateTimeOffset.UtcNow;
            }
        }

        await AddEvent(tenantId, parent.Id, parent.Status, $"Tách đợt → {child.Code}", userId, ct);
        await AddEvent(tenantId, child.Id, "Draft", $"Tách từ {parent.Code}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [child], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> StartPickAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is not ("Confirmed" or "Draft"))
            throw new AppException("Chỉ soạn hàng khi Draft/Confirmed.");
        if (order.Status == "Draft")
        {
            var hasLines = await _db.LogDeliveryLines.AnyAsync(
                x => x.TenantId == tenantId && x.DeliveryOrderId == orderId && !x.IsDeleted, ct);
            if (!hasLines) throw new AppException("Cần ít nhất 1 dòng hàng.");
        }
        order.Status = "Picking";
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Picking", "Tạo pick list / bắt đầu soạn", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> ConfirmPickAsync(
        Guid tenantId, Guid userId, Guid orderId, LogPickRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status != "Picking") throw new AppException("Lệnh chưa ở trạng thái Picking.");
        if (req.Lines is null || req.Lines.Count == 0) throw new AppException("Nhập SL đã soạn.");

        foreach (var p in req.Lines)
        {
            var line = await _db.LogDeliveryLines.FirstOrDefaultAsync(
                x => x.Id == p.LineId && x.TenantId == tenantId
                     && x.DeliveryOrderId == orderId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng pick không hợp lệ.");
            if (p.QtyPicked < 0 || p.QtyPicked > line.Qty)
                throw new AppException($"SL soạn {line.ProductCode} không hợp lệ.");
            line.QtyPicked = p.QtyPicked;
            line.UpdatedBy = userId;
        }

        var lines = await _db.LogDeliveryLines
            .Where(x => x.TenantId == tenantId && x.DeliveryOrderId == orderId && !x.IsDeleted)
            .ToListAsync(ct);
        if (lines.Any(x => x.QtyPicked < x.Qty))
            throw new AppException("Chưa soạn đủ toàn bộ dòng hàng.");

        order.Status = "Ready";
        order.PickedAt = DateTimeOffset.UtcNow;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Ready", "Xác nhận soạn hàng xong", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> DispatchAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status != "Ready") throw new AppException("Chỉ xuất hàng khi Ready.");
        order.Status = "Dispatched";
        order.DispatchedAt = DateTimeOffset.UtcNow;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Dispatched", "Xác nhận xuất hàng giao", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> PrintWaybillAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is "Draft" or "Cancelled")
            throw new AppException("Không in vận đơn ở trạng thái hiện tại.");
        if (string.IsNullOrEmpty(order.WaybillNo))
            order.WaybillNo = $"VD-{order.Code}-{DateTime.UtcNow:yyMMddHHmm}";
        order.WaybillPrintedAt = DateTimeOffset.UtcNow;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, order.Status, $"In vận đơn {order.WaybillNo}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> AssignAsync(
        Guid tenantId, Guid userId, Guid orderId, LogAssignRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is "Cancelled" or "Delivered" or "Returned")
            throw new AppException("Không phân công lệnh đã kết thúc.");

        if (req.CarrierId is Guid cid)
        {
            var ok = await _db.LogCarriers.AnyAsync(
                x => x.Id == cid && x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active", ct);
            if (!ok) throw new AppException("ĐVVC không hợp lệ.");
            order.CarrierId = cid;
        }
        else order.CarrierId = null;

        if (req.DriverUserId is Guid did)
        {
            var ok = await _db.Users.AnyAsync(
                x => x.Id == did && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Tài xế không hợp lệ.");
            order.DriverUserId = did;
            var name = await _db.Users.AsNoTracking()
                .Where(x => x.Id == did).Select(x => x.DisplayName ?? x.Username).FirstAsync(ct);
            order.DriverName = NullIfEmpty(req.DriverName) ?? name;
        }
        else
        {
            order.DriverUserId = null;
            order.DriverName = NullIfEmpty(req.DriverName);
        }

        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, order.Status,
            $"Phân công ĐVVC/TX: {order.DriverName ?? "—"}", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> UpdateStatusAsync(
        Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        var status = (req.Status ?? "").Trim();
        if (!TrackStatuses.Contains(status))
            throw new AppException("Trạng thái cập nhật không hợp lệ.");
        if (order.Status is "Cancelled" or "Returned")
            throw new AppException("Lệnh đã đóng.");
        if (order.Status is "Delivered" && status != "Delivered")
            throw new AppException("Đơn giao hàng đã hoàn thành — không thể đổi trạng thái.");

        order.Status = status;
        if (status == "Delivered") order.DeliveredAt = DateTimeOffset.UtcNow;
        if (status == "Dispatched" && order.DispatchedAt is null) order.DispatchedAt = DateTimeOffset.UtcNow;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, status, NullIfEmpty(req.Note) ?? "Cập nhật trạng thái", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> CancelAsync(
        Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is "Delivered" or "Returned" or "Cancelled")
            throw new AppException("Không hủy lệnh ở trạng thái hiện tại.");
        order.Status = "Cancelled";
        order.Note = NullIfEmpty(req.Note) ?? order.Note;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Cancelled", NullIfEmpty(req.Note) ?? "Hủy lệnh giao", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> ReturnAsync(
        Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is not ("Dispatched" or "InTransit" or "Failed" or "Delivered"))
            throw new AppException("Chỉ hoàn lệnh đã xuất / đang giao / thất bại / đã giao.");
        order.Status = "Returned";
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Returned", NullIfEmpty(req.Note) ?? "Hoàn lệnh giao", userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<LogDeliveryOrderDto> FailAsync(
        Guid tenantId, Guid userId, Guid orderId, LogFailRequest req, CancellationToken ct = default)
    {
        var order = await RequireOrder(tenantId, orderId, ct);
        if (order.Status is not ("Dispatched" or "InTransit"))
            throw new AppException("Chỉ ghi thất bại khi đã xuất / đang giao.");
        var reason = Req(req.Reason, 500, "Lý do thất bại");
        order.Status = "Failed";
        order.FailureReason = reason;
        order.UpdatedBy = userId;
        await AddEvent(tenantId, orderId, "Failed", reason, userId, ct);
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    private async Task<LogDeliveryOrder> RequireOrder(
        Guid tenantId, Guid id, CancellationToken ct, bool track = true)
    {
        var q = track ? _db.LogDeliveryOrders.AsQueryable() : _db.LogDeliveryOrders.AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy lệnh giao.", 404);
    }

    private static void EnsureEditable(LogDeliveryOrder order)
    {
        if (!Editable.Contains(order.Status))
            throw new AppException("Chỉ sửa lệnh ở trạng thái Draft.");
    }

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
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.LogDeliveryOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[(stem.Length)..], out var n))
            seq = n + 1;
        return $"{stem}{seq:D4}";
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

    private static LogCarrierDto MapCarrier(LogCarrier c) =>
        new(c.Id, c.Code, c.Name, c.Phone, c.ContactName, c.Note, c.Status);

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? s, int max, string label)
    {
        var v = (s ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

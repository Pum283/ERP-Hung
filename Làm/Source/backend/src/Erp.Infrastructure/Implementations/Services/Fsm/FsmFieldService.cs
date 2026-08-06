using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fsm;
using Erp.Application.Interfaces.Services.Fsm;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fsm;

public sealed class FsmFieldService : IFsmFieldService
{
    private static readonly HashSet<string> Priorities =
        new(StringComparer.OrdinalIgnoreCase) { "Low", "Normal", "High", "Critical" };
    private static readonly HashSet<string> Channels =
        new(StringComparer.OrdinalIgnoreCase) { "Phone", "Email", "Portal", "WalkIn", "Other" };
    private static readonly HashSet<string> TicketStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        { "Open", "Assigned", "InProgress", "Escalated", "Resolved", "Closed", "Cancelled" };

    private readonly AppDbContext _db;
    public FsmFieldService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<FsmServiceTypeDto>> ListServiceTypesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmServiceTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new FsmServiceTypeDto(x.Id, x.Code, x.Name, x.Status, x.Note)).ToList();
    }

    public async Task<FsmServiceTypeDto> UpsertServiceTypeAsync(
        Guid tenantId, Guid userId, FsmServiceTypeUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên loại DV");
        var status = ActiveOrInactive(req.Status);

        FsmServiceType entity;
        if (req.Id is Guid id)
            entity = await RequireType(tenantId, id, ct);
        else
        {
            if (await _db.FsmServiceTypes.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException($"Mã '{code}' đã tồn tại.");
            entity = new FsmServiceType { TenantId = tenantId, CreatedBy = userId };
            _db.FsmServiceTypes.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Status = status;
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new FsmServiceTypeDto(entity.Id, entity.Code, entity.Name, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<FsmFaultCodeDto>> ListFaultCodesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmFaultCodes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new FsmFaultCodeDto(x.Id, x.Code, x.Name, x.Severity, x.Status, x.Note)).ToList();
    }

    public async Task<FsmFaultCodeDto> UpsertFaultCodeAsync(
        Guid tenantId, Guid userId, FsmFaultCodeUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên mã lỗi");
        var sev = string.IsNullOrWhiteSpace(req.Severity) ? "Medium" : req.Severity.Trim();
        if (sev is not ("Low" or "Medium" or "High")) throw new AppException("Mức nghiêm trọng: Low | Medium | High.");
        var status = ActiveOrInactive(req.Status);

        FsmFaultCode entity;
        if (req.Id is Guid id)
            entity = await RequireFault(tenantId, id, ct);
        else
        {
            if (await _db.FsmFaultCodes.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException($"Mã '{code}' đã tồn tại.");
            entity = new FsmFaultCode { TenantId = tenantId, CreatedBy = userId };
            _db.FsmFaultCodes.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Severity = sev; entity.Status = status;
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new FsmFaultCodeDto(entity.Id, entity.Code, entity.Name, entity.Severity, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<FsmPartDto>> ListPartsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmParts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new FsmPartDto(x.Id, x.Code, x.Name, x.Unit, x.Status, x.Note)).ToList();
    }

    public async Task<FsmPartDto> UpsertPartAsync(
        Guid tenantId, Guid userId, FsmPartUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên linh kiện");
        var status = ActiveOrInactive(req.Status);

        FsmPart entity;
        if (req.Id is Guid id)
            entity = await RequirePart(tenantId, id, ct);
        else
        {
            if (await _db.FsmParts.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException($"Mã '{code}' đã tồn tại.");
            entity = new FsmPart { TenantId = tenantId, CreatedBy = userId };
            _db.FsmParts.Add(entity);
        }

        entity.Code = code; entity.Name = name;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "CAI" : req.Unit.Trim().ToUpperInvariant();
        entity.Status = status; entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new FsmPartDto(entity.Id, entity.Code, entity.Name, entity.Unit, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<FsmSlaPolicyDto>> ListSlaPoliciesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmSlaPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Priority).ToListAsync(ct);
        return list.Select(MapSla).ToList();
    }

    public async Task<FsmSlaPolicyDto> UpsertSlaPolicyAsync(
        Guid tenantId, Guid userId, FsmSlaPolicyUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên SLA");
        var priority = NormPriority(req.Priority);
        if (req.ResponseHours <= 0 || req.ResolveHours <= 0)
            throw new AppException("SLA giờ phản hồi / xử lý phải > 0.");

        FsmSlaPolicy entity;
        if (req.Id is Guid id)
            entity = await RequireSla(tenantId, id, ct);
        else
        {
            if (await _db.FsmSlaPolicies.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException($"Mã '{code}' đã tồn tại.");
            entity = new FsmSlaPolicy { TenantId = tenantId, CreatedBy = userId };
            _db.FsmSlaPolicies.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Priority = priority;
        entity.ResponseHours = req.ResponseHours; entity.ResolveHours = req.ResolveHours;
        entity.IsActive = req.IsActive ?? true; entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapSla(entity);
    }

    public async Task<IReadOnlyList<FsmAssetDto>> ListAssetsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.FsmAssets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.SerialNo.Contains(term) || x.CustomerName.Contains(term)
                || (x.Model != null && x.Model.Contains(term)));
        }
        var list = await query.OrderBy(x => x.Code).Take(300).ToListAsync(ct);
        return list.Select(MapAsset).ToList();
    }

    public async Task<FsmAssetDetailDto> GetAssetDetailAsync(
        Guid tenantId, Guid assetId, CancellationToken ct = default)
    {
        var asset = await _db.FsmAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assetId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy thiết bị.", 404);

        var hist = await _db.FsmAssetHistories.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId && !x.IsDeleted)
            .OrderByDescending(x => x.OccurredAt).Take(100).ToListAsync(ct);
        var actorIds = hist.Select(x => x.ActorUserId).Distinct().ToList();
        var actors = actorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && actorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var history = hist.Select(h => new FsmAssetHistoryDto(
            h.Id, h.AssetId, h.EventType, h.Summary, h.TicketId, h.ActorUserId,
            actors.GetValueOrDefault(h.ActorUserId), h.OccurredAt)).ToList();

        return new FsmAssetDetailDto(MapAsset(asset), history);
    }

    public async Task<FsmAssetDto> UpsertAssetAsync(
        Guid tenantId, Guid userId, FsmAssetUpsertRequest req, CancellationToken ct = default)
    {
        var customer = Req(req.CustomerName, 200, "Khách hàng");
        var serial = Req(req.SerialNo, 80, "Serial");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive" or "Scrapped"))
            throw new AppException("Trạng thái TB: Active | Inactive | Scrapped.");

        FsmAsset entity;
        if (req.Id is Guid id)
            entity = await RequireAsset(tenantId, id, ct);
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? await NextCodeAsync(tenantId, "TB", ct)
                : NormCode(req.Code);
            if (await _db.FsmAssets.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException($"Mã '{code}' đã tồn tại.");
            entity = new FsmAsset { TenantId = tenantId, Code = code, CreatedBy = userId };
            _db.FsmAssets.Add(entity);
        }

        entity.CustomerName = customer;
        entity.CustomerPhone = NullIfEmpty(req.CustomerPhone);
        entity.SerialNo = serial;
        entity.Model = NullIfEmpty(req.Model);
        entity.ActivatedAt = req.ActivatedAt;
        entity.WarrantyEndAt = req.WarrantyEndAt;
        entity.Status = status;
        entity.Address = NullIfEmpty(req.Address);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.Id is null)
        {
            _db.FsmAssetHistories.Add(new FsmAssetHistory
            {
                TenantId = tenantId, AssetId = entity.Id, EventType = "Warranty",
                Summary = $"Kích hoạt BH · serial {serial}", ActorUserId = userId,
                OccurredAt = req.ActivatedAt ?? DateTimeOffset.UtcNow, CreatedBy = userId
            });
            await _db.SaveChangesAsync(ct);
        }

        return MapAsset(entity);
    }

    public async Task<FsmAssetHistoryDto> AddAssetHistoryAsync(
        Guid tenantId, Guid userId, Guid assetId, FsmAssetHistoryCreateRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsset(tenantId, assetId, ct);
        var type = (req.EventType ?? "Note").Trim();
        if (type is not ("Warranty" or "Repair" or "Ticket" or "Note"))
            throw new AppException("Loại lịch sử: Warranty | Repair | Ticket | Note.");
        var summary = Req(req.Summary, 500, "Nội dung");

        var h = new FsmAssetHistory
        {
            TenantId = tenantId, AssetId = assetId, EventType = type, Summary = summary,
            ActorUserId = userId, OccurredAt = DateTimeOffset.UtcNow, CreatedBy = userId
        };
        _db.FsmAssetHistories.Add(h);
        await _db.SaveChangesAsync(ct);

        var name = await _db.Users.AsNoTracking()
            .Where(x => x.Id == userId).Select(x => x.DisplayName ?? x.Username).FirstOrDefaultAsync(ct);
        return new FsmAssetHistoryDto(h.Id, h.AssetId, h.EventType, h.Summary, h.TicketId, h.ActorUserId, name, h.OccurredAt);
    }

    public async Task<IReadOnlyList<FsmTicketDto>> ListTicketsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.FsmTickets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.Subject.Contains(term) || x.CustomerName.Contains(term));
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapTicketsAsync(tenantId, list, ct);
    }

    public async Task<FsmTicketDetailDto> GetTicketDetailAsync(
        Guid tenantId, Guid ticketId, CancellationToken ct = default)
    {
        var t = await _db.FsmTickets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ticketId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy ticket.", 404);
        var partLines = await _db.FsmTicketPartLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TicketId == ticketId && !x.IsDeleted)
            .OrderByDescending(x => x.IssuedAt).ToListAsync(ct);
        var partIds = partLines.Select(x => x.PartId).Distinct().ToList();
        var parts = partIds.Count == 0
            ? new Dictionary<Guid, FsmPart>()
            : await _db.FsmParts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && partIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
        var mappedParts = partLines.Select(x =>
        {
            parts.TryGetValue(x.PartId, out var p);
            return new FsmTicketPartLineDto(
                x.Id, x.TicketId, x.PartId, p?.Code ?? "", p?.Name ?? "",
                x.Qty, x.UnitCost, decimal.Round(x.Qty * x.UnitCost, 2),
                x.Source, x.TechUserId, x.TechName, x.IssuedAt, x.Note);
        }).ToList();
        return new FsmTicketDetailDto((await MapTicketsAsync(tenantId, [t], ct))[0], mappedParts);
    }

    public async Task<FsmTicketDto> UpsertTicketAsync(
        Guid tenantId, Guid userId, FsmTicketUpsertRequest req, CancellationToken ct = default)
    {
        var channel = string.IsNullOrWhiteSpace(req.Channel) ? "Phone" : req.Channel.Trim();
        if (!Channels.Contains(channel)) throw new AppException("Kênh không hợp lệ.");
        var subject = Req(req.Subject, 200, "Tiêu đề");
        var customer = Req(req.CustomerName, 200, "Khách hàng");
        var priority = NormPriority(string.IsNullOrWhiteSpace(req.Priority) ? "Normal" : req.Priority);

        if (req.ServiceTypeId is Guid stId)
            _ = await RequireType(tenantId, stId, ct);
        if (req.FaultCodeId is Guid fcId)
            _ = await RequireFault(tenantId, fcId, ct);

        FsmAsset? asset = null;
        if (req.AssetId is Guid aId)
            asset = await RequireAsset(tenantId, aId, ct);

        var sla = await _db.FsmSlaPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Priority == priority && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Code).FirstOrDefaultAsync(ct);

        FsmTicket entity;
        if (req.Id is Guid id)
        {
            entity = await RequireTicket(tenantId, id, ct);
            if (entity.Status is "Closed" or "Cancelled")
                throw new AppException("Không sửa ticket đã đóng.");
        }
        else
        {
            entity = new FsmTicket
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(tenantId, "TK", ct)
                    : NormCode(req.Code),
                Status = "Open", CreatedByUserId = userId, CreatedBy = userId
            };
            if (await _db.FsmTickets.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã ticket đã tồn tại.");
            _db.FsmTickets.Add(entity);
        }

        entity.Channel = channel;
        entity.Subject = subject;
        entity.Description = NullIfEmpty(req.Description);
        entity.CustomerName = customer;
        entity.CustomerPhone = NullIfEmpty(req.CustomerPhone) ?? asset?.CustomerPhone;
        entity.ServiceTypeId = req.ServiceTypeId;
        entity.FaultCodeId = req.FaultCodeId;
        entity.AssetId = req.AssetId;
        entity.Priority = priority;
        entity.SlaPolicyId = sla?.Id;
        if (sla is not null && (req.Id is null || entity.DueResponseAt is null))
        {
            var now = DateTimeOffset.UtcNow;
            entity.DueResponseAt = now.AddHours(sla.ResponseHours);
            entity.DueResolveAt = now.AddHours(sla.ResolveHours);
        }
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.Id is null && asset is not null)
        {
            _db.FsmAssetHistories.Add(new FsmAssetHistory
            {
                TenantId = tenantId, AssetId = asset.Id, EventType = "Ticket",
                Summary = $"Ticket {entity.Code}: {subject}", TicketId = entity.Id,
                ActorUserId = userId, OccurredAt = DateTimeOffset.UtcNow, CreatedBy = userId
            });
            await _db.SaveChangesAsync(ct);
        }

        return (await MapTicketsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FsmTicketDto> AssignTicketAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmAssignRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled" or "Resolved")
            throw new AppException("Không phân công ticket đã kết thúc.");

        var techOk = await _db.Users.AnyAsync(
            x => x.Id == req.TechUserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!techOk) throw new AppException("Kỹ thuật viên không hợp lệ.");

        var name = NullIfEmpty(req.TechName) ?? await _db.Users.AsNoTracking()
            .Where(x => x.Id == req.TechUserId).Select(x => x.DisplayName ?? x.Username).FirstAsync(ct);

        t.PreviousTechUserId = t.AssignedTechUserId;
        t.AssignedTechUserId = req.TechUserId;
        t.AssignedTechName = name;
        t.Status = t.Status == "Escalated" ? "Escalated" : "Assigned";
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> EscalateTicketAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmEscalateRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled" or "Resolved")
            throw new AppException("Không escalate ticket đã kết thúc.");
        var reason = Req(req.Reason, 500, "Lý do escalate");

        var techOk = await _db.Users.AnyAsync(
            x => x.Id == req.NewTechUserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!techOk) throw new AppException("KTV mới không hợp lệ.");

        var name = NullIfEmpty(req.NewTechName) ?? await _db.Users.AsNoTracking()
            .Where(x => x.Id == req.NewTechUserId).Select(x => x.DisplayName ?? x.Username).FirstAsync(ct);

        t.PreviousTechUserId = t.AssignedTechUserId;
        t.AssignedTechUserId = req.NewTechUserId;
        t.AssignedTechName = name;
        t.EscalateReason = reason;
        t.Status = "Escalated";
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> SetTicketStatusAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmTicketStatusRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        var status = (req.Status ?? "").Trim();
        if (!TicketStatuses.Contains(status)) throw new AppException("Trạng thái ticket không hợp lệ.");
        var now = DateTimeOffset.UtcNow;
        t.Status = status;
        t.UpdatedBy = userId;

        if (status == "Resolved")
        {
            t.ResolvedAt ??= now;
            t.CheckedOutAt ??= now;
            ApplySlaFlags(t, now);
        }
        if (status == "Closed")
        {
            t.ResolvedAt ??= now;
            t.ClosedAt ??= now;
            ApplySlaFlags(t, t.ClosedAt ?? now);
        }

        if ((status is "Resolved" or "Closed") && t.AssetId is Guid assetId)
        {
            _db.FsmAssetHistories.Add(new FsmAssetHistory
            {
                TenantId = tenantId, AssetId = assetId, EventType = "Repair",
                Summary = $"Ticket {t.Code} → {status}" + (NullIfEmpty(req.Note) is { } n ? $": {n}" : ""),
                TicketId = t.Id, ActorUserId = userId, OccurredAt = now, CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> SetAppointmentAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmAppointmentRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled")
            throw new AppException("Ticket đã đóng — không đặt lịch.");
        t.AppointmentAt = req.AppointmentAt;
        t.AppointmentNote = NullIfEmpty(req.Note);
        if (t.Status is "Open" or "Assigned") t.Status = "Assigned";
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> WorkLogAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmWorkLogRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled")
            throw new AppException("Ticket đã đóng — không ghi xử lý.");
        t.RootCause = Req(req.RootCause, 1000, "Nguyên nhân");
        t.ResolutionNote = Req(req.ResolutionNote, 2000, "Cách xử lý");
        if (req.FaultCodeId is Guid fc)
        {
            var ok = await _db.FsmFaultCodes.AnyAsync(x => x.Id == fc && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Mã lỗi không hợp lệ.");
            t.FaultCodeId = fc;
        }
        if (t.Status is "Open" or "Assigned") t.Status = "InProgress";
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> CheckoutAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmCheckoutRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled")
            throw new AppException("Ticket đã đóng.");
        if (string.IsNullOrWhiteSpace(t.RootCause) || string.IsNullOrWhiteSpace(t.ResolutionNote))
            throw new AppException("Cần ghi nguyên nhân & cách xử lý trước khi check-out.");

        var now = DateTimeOffset.UtcNow;
        t.CheckedOutAt = now;
        t.ResolvedAt = now;
        t.Status = "Resolved";
        ApplySlaFlags(t, now);
        t.UpdatedBy = userId;

        if (t.AssetId is Guid assetId)
        {
            _db.FsmAssetHistories.Add(new FsmAssetHistory
            {
                TenantId = tenantId, AssetId = assetId, EventType = "Repair",
                Summary = $"Check-out {t.Code}" + (NullIfEmpty(req.Note) is { } n ? $": {n}" : ""),
                TicketId = t.Id, ActorUserId = userId, OccurredAt = now, CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> AcceptAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmAcceptRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status is "Closed" or "Cancelled")
            throw new AppException("Ticket đã đóng.");
        if (t.Status != "Resolved" && t.CheckedOutAt is null)
            throw new AppException("Cần check-out / Resolved trước khi nghiệm thu.");

        t.AcceptanceSignerName = Req(req.SignerName, 120, "Người ký");
        t.AcceptanceNote = NullIfEmpty(req.Note);
        t.AcceptanceSignedAt = DateTimeOffset.UtcNow;
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    public async Task<FsmTicketDto> CloseAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmCloseRequest req, CancellationToken ct = default)
    {
        var t = await RequireTicket(tenantId, ticketId, ct);
        if (t.Status == "Cancelled") throw new AppException("Ticket đã hủy.");
        if (t.Status == "Closed") throw new AppException("Ticket đã đóng.");
        if (t.Status != "Resolved" && t.CheckedOutAt is null)
            throw new AppException("Cần Resolved / check-out trước khi đóng.");
        if (t.AcceptanceSignedAt is null)
            throw new AppException("Cần nghiệm thu khách trước khi đóng (UC_FSM_028).");

        var now = DateTimeOffset.UtcNow;
        t.ClosedAt = now;
        t.ResolvedAt ??= now;
        t.Status = "Closed";
        ApplySlaFlags(t, now);
        t.UpdatedBy = userId;

        if (t.AssetId is Guid assetId)
        {
            _db.FsmAssetHistories.Add(new FsmAssetHistory
            {
                TenantId = tenantId, AssetId = assetId, EventType = "Repair",
                Summary = $"Đóng ticket {t.Code}" +
                          (t.SlaResolveMet == true ? " · SLA đạt" : t.SlaResolveMet == false ? " · SLA trễ" : "") +
                          (NullIfEmpty(req.Note) is { } n ? $": {n}" : ""),
                TicketId = t.Id, ActorUserId = userId, OccurredAt = now, CreatedBy = userId
            });
        }

        await _db.SaveChangesAsync(ct);
        return (await MapTicketsAsync(tenantId, [t], ct))[0];
    }

    private static void ApplySlaFlags(FsmTicket t, DateTimeOffset at)
    {
        if (t.DueResponseAt is DateTimeOffset dr)
            t.SlaResponseMet = at <= dr;
        if (t.DueResolveAt is DateTimeOffset dx)
            t.SlaResolveMet = at <= dx;
    }

    private async Task<IReadOnlyList<FsmTicketDto>> MapTicketsAsync(
        Guid tenantId, List<FsmTicket> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FsmTicketDto>();
        var stIds = list.Where(x => x.ServiceTypeId.HasValue).Select(x => x.ServiceTypeId!.Value).Distinct().ToList();
        var fcIds = list.Where(x => x.FaultCodeId.HasValue).Select(x => x.FaultCodeId!.Value).Distinct().ToList();
        var aIds = list.Where(x => x.AssetId.HasValue).Select(x => x.AssetId!.Value).Distinct().ToList();
        var slaIds = list.Where(x => x.SlaPolicyId.HasValue).Select(x => x.SlaPolicyId!.Value).Distinct().ToList();

        var types = stIds.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.FsmServiceTypes.AsNoTracking().Where(x => stIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var faults = fcIds.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.FsmFaultCodes.AsNoTracking().Where(x => fcIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var assets = aIds.Count == 0 ? new Dictionary<Guid, FsmAsset>()
            : await _db.FsmAssets.AsNoTracking().Where(x => aIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
        var slas = slaIds.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.FsmSlaPolicies.AsNoTracking().Where(x => slaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return list.Select(t =>
        {
            assets.TryGetValue(t.AssetId ?? Guid.Empty, out var a);
            return new FsmTicketDto(
                t.Id, t.Code, t.Channel, t.Subject, t.Description, t.CustomerName, t.CustomerPhone,
                t.ServiceTypeId, t.ServiceTypeId is Guid s ? types.GetValueOrDefault(s) : null,
                t.FaultCodeId, t.FaultCodeId is Guid f ? faults.GetValueOrDefault(f) : null,
                t.AssetId, a?.Code, a?.SerialNo,
                t.SlaPolicyId, t.SlaPolicyId is Guid p ? slas.GetValueOrDefault(p) : null,
                t.Priority, t.Status, t.AssignedTechUserId, t.AssignedTechName,
                t.DueResponseAt, t.DueResolveAt, t.EscalateReason, t.CreatedAt,
                t.AppointmentAt, t.AppointmentNote, t.RootCause, t.ResolutionNote,
                t.CheckedOutAt, t.AcceptanceSignedAt, t.AcceptanceSignerName, t.AcceptanceNote,
                t.ResolvedAt, t.ClosedAt, t.SlaResponseMet, t.SlaResolveMet);
        }).ToList();
    }

    private static FsmSlaPolicyDto MapSla(FsmSlaPolicy x) =>
        new(x.Id, x.Code, x.Name, x.Priority, x.ResponseHours, x.ResolveHours, x.IsActive, x.Note);

    private static FsmAssetDto MapAsset(FsmAsset x)
    {
        var soon = x.WarrantyEndAt is DateTimeOffset end
            && end <= DateTimeOffset.UtcNow.AddDays(30)
            && end >= DateTimeOffset.UtcNow;
        return new FsmAssetDto(
            x.Id, x.Code, x.CustomerName, x.CustomerPhone, x.SerialNo, x.Model,
            x.ActivatedAt, x.WarrantyEndAt, x.Status, x.Address, x.Note, soon);
    }

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix switch
        {
            "TB" => await _db.FsmAssets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.FsmTickets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct)
        };
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private async Task<FsmServiceType> RequireType(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmServiceTypes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy loại DV.");

    private async Task<FsmFaultCode> RequireFault(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmFaultCodes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy mã lỗi.");

    private async Task<FsmPart> RequirePart(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmParts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy linh kiện.");

    private async Task<FsmSlaPolicy> RequireSla(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmSlaPolicies.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy SLA.");

    private async Task<FsmAsset> RequireAsset(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmAssets.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy thiết bị.");

    private async Task<FsmTicket> RequireTicket(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmTickets.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy ticket.");

    private static string NormPriority(string? p)
    {
        var v = (p ?? "Normal").Trim();
        if (!Priorities.Contains(v)) throw new AppException("Ưu tiên: Low | Normal | High | Critical.");
        return Priorities.First(x => x.Equals(v, StringComparison.OrdinalIgnoreCase));
    }

    private static string ActiveOrInactive(string? s)
    {
        var v = string.IsNullOrWhiteSpace(s) ? "Active" : s.Trim();
        if (v is not ("Active" or "Inactive")) throw new AppException("Trạng thái: Active | Inactive.");
        return v;
    }

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

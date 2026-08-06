using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fsm;
using Erp.Application.Interfaces.Services.Fsm;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fsm;

public sealed class FsmPartsStockService : IFsmPartsStockService
{
    private readonly AppDbContext _db;
    public FsmPartsStockService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<FsmPartStockDto>> ListStockAsync(
        Guid tenantId, string? locationType, Guid? techUserId, CancellationToken ct = default)
    {
        var q = _db.FsmPartStocks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(locationType))
        {
            var loc = locationType.Trim();
            q = q.Where(x => x.LocationType == loc);
        }
        if (techUserId is Guid tid)
            q = q.Where(x => x.TechUserId == tid);

        var rows = await q.OrderBy(x => x.LocationType).ThenBy(x => x.TechName).ToListAsync(ct);
        if (rows.Count == 0) return Array.Empty<FsmPartStockDto>();
        var parts = await PartsMap(tenantId, rows.Select(x => x.PartId).Distinct(), ct);
        return rows.Select(x => MapStock(x, parts)).ToList();
    }

    public async Task<FsmPartStockDto> ReceiptWarehouseAsync(
        Guid tenantId, Guid userId, FsmPartReceiptRequest req, CancellationToken ct = default)
    {
        if (req.Qty <= 0) throw new AppException("Số lượng nhập phải > 0.");
        var part = await RequirePart(tenantId, req.PartId, ct);
        var cost = req.UnitCost is decimal c && c >= 0 ? c : 0m;
        var stock = await GetOrCreateStock(tenantId, userId, part.Id, "Warehouse", null, null, ct);
        var newQty = stock.QtyOnHand + req.Qty;
        stock.UnitCost = newQty <= 0
            ? cost
            : decimal.Round(((stock.QtyOnHand * stock.UnitCost) + (req.Qty * cost)) / newQty, 2);
        stock.QtyOnHand = newQty;
        stock.UpdatedBy = userId;
        stock.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        var parts = await PartsMap(tenantId, [part.Id], ct);
        return MapStock(stock, parts);
    }

    public async Task<IReadOnlyList<FsmPartIssueDocDto>> ListIssuesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var docs = await _db.FsmPartIssueDocs.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        if (docs.Count == 0) return Array.Empty<FsmPartIssueDocDto>();
        var partIds = docs.SelectMany(d => d.Lines).Select(l => l.PartId).Distinct();
        var parts = await PartsMap(tenantId, partIds, ct);
        return docs.Select(d => MapIssue(d, parts)).ToList();
    }

    public async Task<FsmPartIssueDocDto> CreateAndPostIssueAsync(
        Guid tenantId, Guid userId, FsmPartIssueCreateRequest req, CancellationToken ct = default)
    {
        if (req.Lines is null || req.Lines.Count == 0)
            throw new AppException("Cần ít nhất một dòng linh kiện.");
        var techOk = await _db.Users.AnyAsync(
            x => x.Id == req.TechUserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!techOk) throw new AppException("Không tìm thấy KTV.", 404);
        var techName = string.IsNullOrWhiteSpace(req.TechName)
            ? await _db.Users.AsNoTracking()
                .Where(x => x.Id == req.TechUserId)
                .Select(x => x.DisplayName ?? x.Username).FirstAsync(ct)
            : req.TechName.Trim();

        var doc = new FsmPartIssueDoc
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "PI", ct),
            TechUserId = req.TechUserId,
            TechName = techName,
            Status = "Draft",
            Note = Null(req.Note, 1000),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.FsmPartIssueDocs.Add(doc);

        foreach (var line in req.Lines)
        {
            if (line.Qty <= 0) throw new AppException("Số lượng cấp phải > 0.");
            var part = await RequirePart(tenantId, line.PartId, ct);
            var wh = await GetOrCreateStock(tenantId, userId, part.Id, "Warehouse", null, null, ct);
            if (wh.QtyOnHand < line.Qty)
                throw new AppException($"Kho KT không đủ {part.Code} (tồn {wh.QtyOnHand}).");
            var unitCost = line.UnitCost is decimal uc && uc >= 0 ? uc : wh.UnitCost;
            _db.FsmPartIssueLines.Add(new FsmPartIssueLine
            {
                TenantId = tenantId,
                IssueDocId = doc.Id,
                PartId = part.Id,
                Qty = line.Qty,
                UnitCost = unitCost,
                CreatedBy = userId,
                UpdatedBy = userId,
            });
            wh.QtyOnHand -= line.Qty;
            wh.UpdatedBy = userId;
            wh.UpdatedAt = DateTimeOffset.UtcNow;

            var techStock = await GetOrCreateStock(
                tenantId, userId, part.Id, "Tech", req.TechUserId, techName, ct);
            var newQty = techStock.QtyOnHand + line.Qty;
            techStock.UnitCost = newQty <= 0
                ? unitCost
                : decimal.Round(((techStock.QtyOnHand * techStock.UnitCost) + (line.Qty * unitCost)) / newQty, 2);
            techStock.QtyOnHand = newQty;
            techStock.TechName = techName;
            techStock.UpdatedBy = userId;
            techStock.UpdatedAt = DateTimeOffset.UtcNow;
        }

        doc.Status = "Posted";
        doc.PostedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var loaded = await _db.FsmPartIssueDocs.AsNoTracking().Include(x => x.Lines)
            .FirstAsync(x => x.Id == doc.Id, ct);
        var parts = await PartsMap(tenantId, loaded.Lines.Select(l => l.PartId), ct);
        return MapIssue(loaded, parts);
    }

    public async Task<IReadOnlyList<FsmPartReconcileDocDto>> ListReconcilesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var docs = await _db.FsmPartReconcileDocs.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        if (docs.Count == 0) return Array.Empty<FsmPartReconcileDocDto>();
        var parts = await PartsMap(tenantId, docs.SelectMany(d => d.Lines).Select(l => l.PartId).Distinct(), ct);
        return docs.Select(d => MapReconcile(d, parts)).ToList();
    }

    public async Task<FsmPartReconcileDocDto> CreateAndPostReconcileAsync(
        Guid tenantId, Guid userId, FsmPartReconcileCreateRequest req, CancellationToken ct = default)
    {
        if (req.Lines is null || req.Lines.Count == 0)
            throw new AppException("Cần ít nhất một dòng đối soát.");
        var scope = string.IsNullOrWhiteSpace(req.Scope) ? "Warehouse" : req.Scope.Trim();
        if (scope is not ("Warehouse" or "Tech"))
            throw new AppException("Scope: Warehouse | Tech.");

        string? techName = null;
        Guid? techId = null;
        if (scope == "Tech")
        {
            if (req.TechUserId is not Guid tid)
                throw new AppException("Đối soát túi KTV cần TechUserId.");
            techId = tid;
            var techOk = await _db.Users.AnyAsync(
                x => x.Id == tid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!techOk) throw new AppException("Không tìm thấy KTV.", 404);
            techName = string.IsNullOrWhiteSpace(req.TechName)
                ? await _db.Users.AsNoTracking()
                    .Where(x => x.Id == tid)
                    .Select(x => x.DisplayName ?? x.Username).FirstAsync(ct)
                : req.TechName.Trim();
        }

        var doc = new FsmPartReconcileDoc
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "PR", ct),
            Scope = scope,
            TechUserId = techId,
            TechName = techName,
            Status = "Draft",
            Note = Null(req.Note, 1000),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.FsmPartReconcileDocs.Add(doc);

        foreach (var line in req.Lines)
        {
            if (line.CountedQty < 0) throw new AppException("Số đếm không được âm.");
            var part = await RequirePart(tenantId, line.PartId, ct);
            var stock = await GetOrCreateStock(tenantId, userId, part.Id, scope, techId, techName, ct);
            var diff = line.CountedQty - stock.QtyOnHand;
            _db.FsmPartReconcileLines.Add(new FsmPartReconcileLine
            {
                TenantId = tenantId,
                ReconcileDocId = doc.Id,
                PartId = part.Id,
                SystemQty = stock.QtyOnHand,
                CountedQty = line.CountedQty,
                DiffQty = diff,
                UnitCost = stock.UnitCost,
                CreatedBy = userId,
                UpdatedBy = userId,
            });
            stock.QtyOnHand = line.CountedQty;
            stock.UpdatedBy = userId;
            stock.UpdatedAt = DateTimeOffset.UtcNow;
            if (techName is not null) stock.TechName = techName;
        }

        doc.Status = "Posted";
        doc.PostedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var loaded = await _db.FsmPartReconcileDocs.AsNoTracking().Include(x => x.Lines)
            .FirstAsync(x => x.Id == doc.Id, ct);
        var parts = await PartsMap(tenantId, loaded.Lines.Select(l => l.PartId), ct);
        return MapReconcile(loaded, parts);
    }

    public async Task<IReadOnlyList<FsmTicketPartLineDto>> ListTicketPartsAsync(
        Guid tenantId, Guid ticketId, CancellationToken ct = default)
    {
        _ = await RequireTicket(tenantId, ticketId, ct);
        var lines = await _db.FsmTicketPartLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TicketId == ticketId && !x.IsDeleted)
            .OrderByDescending(x => x.IssuedAt).ToListAsync(ct);
        if (lines.Count == 0) return Array.Empty<FsmTicketPartLineDto>();
        var parts = await PartsMap(tenantId, lines.Select(x => x.PartId).Distinct(), ct);
        return lines.Select(x => MapTicketLine(x, parts)).ToList();
    }

    public async Task<FsmTicketPartLineDto> ConsumeTicketPartAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmConsumePartRequest req, CancellationToken ct = default)
    {
        if (req.Qty <= 0) throw new AppException("Số lượng xuất phải > 0.");
        var ticket = await RequireTicket(tenantId, ticketId, ct);
        if (ticket.Status is "Closed" or "Cancelled")
            throw new AppException("Ticket đã đóng/hủy — không xuất linh kiện.");

        var part = await RequirePart(tenantId, req.PartId, ct);
        var source = string.IsNullOrWhiteSpace(req.Source) ? "Tech" : req.Source.Trim();
        if (source is not ("Tech" or "Warehouse"))
            throw new AppException("Source: Tech | Warehouse.");

        Guid? techId = req.TechUserId ?? ticket.AssignedTechUserId;
        string? techName = ticket.AssignedTechName;
        if (source == "Tech")
        {
            if (techId is null)
                throw new AppException("Ticket chưa có KTV — chọn TechUserId hoặc phân công trước.");
            if (req.TechUserId is Guid overrideTech)
            {
                techId = overrideTech;
                techName = await _db.Users.AsNoTracking()
                    .Where(x => x.Id == overrideTech)
                    .Select(x => x.DisplayName ?? x.Username).FirstOrDefaultAsync(ct);
            }
        }

        var stock = source == "Warehouse"
            ? await GetOrCreateStock(tenantId, userId, part.Id, "Warehouse", null, null, ct)
            : await GetOrCreateStock(tenantId, userId, part.Id, "Tech", techId, techName, ct);

        if (stock.QtyOnHand < req.Qty)
            throw new AppException($"Không đủ tồn {part.Code} tại {source} (tồn {stock.QtyOnHand}).");

        var unitCost = req.UnitCost is decimal uc && uc >= 0 ? uc : stock.UnitCost;
        stock.QtyOnHand -= req.Qty;
        stock.UpdatedBy = userId;
        stock.UpdatedAt = DateTimeOffset.UtcNow;

        var line = new FsmTicketPartLine
        {
            TenantId = tenantId,
            TicketId = ticket.Id,
            PartId = part.Id,
            Qty = req.Qty,
            UnitCost = unitCost,
            Source = source,
            TechUserId = techId,
            TechName = techName,
            IssuedAt = DateTimeOffset.UtcNow,
            Note = Null(req.Note, 500),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.FsmTicketPartLines.Add(line);
        await _db.SaveChangesAsync(ct);

        var parts = await PartsMap(tenantId, [part.Id], ct);
        return MapTicketLine(line, parts);
    }

    private async Task<FsmPartStock> GetOrCreateStock(
        Guid tenantId, Guid userId, Guid partId, string locationType,
        Guid? techUserId, string? techName, CancellationToken ct)
    {
        var stock = await _db.FsmPartStocks.FirstOrDefaultAsync(x =>
            x.TenantId == tenantId && !x.IsDeleted
            && x.PartId == partId && x.LocationType == locationType
            && x.TechUserId == techUserId, ct);
        if (stock is not null) return stock;

        stock = new FsmPartStock
        {
            TenantId = tenantId,
            PartId = partId,
            LocationType = locationType,
            TechUserId = techUserId,
            TechName = techName,
            QtyOnHand = 0,
            UnitCost = 0,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.FsmPartStocks.Add(stock);
        await _db.SaveChangesAsync(ct);
        return stock;
    }

    private async Task<Dictionary<Guid, FsmPart>> PartsMap(
        Guid tenantId, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<Guid, FsmPart>();
        return await _db.FsmParts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && list.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }

    private async Task<FsmPart> RequirePart(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmParts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy linh kiện.", 404);

    private async Task<FsmTicket> RequireTicket(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FsmTickets.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy ticket.", 404);

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix switch
        {
            "PI" => await _db.FsmPartIssueDocs.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.FsmPartReconcileDocs.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
        };
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private static FsmPartStockDto MapStock(FsmPartStock x, IReadOnlyDictionary<Guid, FsmPart> parts)
    {
        parts.TryGetValue(x.PartId, out var p);
        return new FsmPartStockDto(
            x.Id, x.PartId, p?.Code ?? "", p?.Name ?? "", p?.Unit ?? "CAI",
            x.LocationType, x.TechUserId, x.TechName,
            x.QtyOnHand, x.UnitCost, decimal.Round(x.QtyOnHand * x.UnitCost, 2));
    }

    private static FsmPartIssueDocDto MapIssue(FsmPartIssueDoc d, IReadOnlyDictionary<Guid, FsmPart> parts) =>
        new(d.Id, d.Code, d.TechUserId, d.TechName, d.Status, d.Note, d.PostedAt, d.CreatedAt,
            d.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                parts.TryGetValue(l.PartId, out var p);
                return new FsmPartIssueLineDto(l.Id, l.PartId, p?.Code ?? "", p?.Name ?? "", l.Qty, l.UnitCost);
            }).ToList());

    private static FsmPartReconcileDocDto MapReconcile(
        FsmPartReconcileDoc d, IReadOnlyDictionary<Guid, FsmPart> parts) =>
        new(d.Id, d.Code, d.Scope, d.TechUserId, d.TechName, d.Status, d.Note, d.PostedAt, d.CreatedAt,
            d.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                parts.TryGetValue(l.PartId, out var p);
                return new FsmPartReconcileLineDto(
                    l.Id, l.PartId, p?.Code ?? "", p?.Name ?? "",
                    l.SystemQty, l.CountedQty, l.DiffQty, l.UnitCost);
            }).ToList());

    private static FsmTicketPartLineDto MapTicketLine(
        FsmTicketPartLine x, IReadOnlyDictionary<Guid, FsmPart> parts)
    {
        parts.TryGetValue(x.PartId, out var p);
        return new FsmTicketPartLineDto(
            x.Id, x.TicketId, x.PartId, p?.Code ?? "", p?.Name ?? "",
            x.Qty, x.UnitCost, decimal.Round(x.Qty * x.UnitCost, 2),
            x.Source, x.TechUserId, x.TechName, x.IssuedAt, x.Note);
    }

    private static string? Null(string? s, int max)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim();
        return t.Length <= max ? t : t[..max];
    }
}

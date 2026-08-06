using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Mfg;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Application.Interfaces.Services.Mfg;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Mfg;

public sealed class MfgProductionService : IMfgProductionService
{
    private static readonly HashSet<string> ItemTypes =
        new(StringComparer.OrdinalIgnoreCase) { "FG", "SFG", "RM" };

    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;

    public MfgProductionService(AppDbContext db, IFinAccountingService fin)
    {
        _db = db;
        _fin = fin;
    }

    public async Task<IReadOnlyList<MfgItemDto>> ListItemsAsync(
        Guid tenantId, string? type, string? q, CancellationToken ct = default)
    {
        var query = _db.MfgItems.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.ItemType == type.Trim());
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        var list = await query.OrderBy(x => x.Code).Take(500).ToListAsync(ct);
        return list.Select(MapItem).ToList();
    }

    public async Task<MfgItemDto> UpsertItemAsync(
        Guid tenantId, Guid userId, MfgItemUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên");
        var type = (req.ItemType ?? "").Trim().ToUpperInvariant();
        if (!ItemTypes.Contains(type)) throw new AppException("Loại: FG | SFG | RM.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Trạng thái không hợp lệ.");

        MfgItem entity;
        if (req.Id is Guid id)
        {
            entity = await RequireItem(tenantId, id, ct);
        }
        else
        {
            if (await _db.MfgItems.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã đã tồn tại.");
            entity = new MfgItem { TenantId = tenantId, CreatedBy = userId };
            _db.MfgItems.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.ItemType = type;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "CAI" : req.Unit.Trim().ToUpperInvariant();
        entity.StandardCost = Math.Max(0, decimal.Round(req.StandardCost ?? entity.StandardCost, 2));
        entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapItem(entity);
    }

    public async Task<IReadOnlyList<MfgWorkshopDto>> ListWorkshopsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgWorkshops.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(MapWs).ToList();
    }

    public async Task<MfgWorkshopDto> UpsertWorkshopAsync(
        Guid tenantId, Guid userId, MfgWorkshopUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên xưởng");
        var wt = string.IsNullOrWhiteSpace(req.WorkshopType) ? "Workshop" : req.WorkshopType.Trim();
        if (wt is not ("Workshop" or "Line")) throw new AppException("Loại: Workshop | Line.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();

        MfgWorkshop entity;
        if (req.Id is Guid id)
        {
            entity = await _db.MfgWorkshops.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy xưởng.");
        }
        else
        {
            if (await _db.MfgWorkshops.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã xưởng đã tồn tại.");
            entity = new MfgWorkshop { TenantId = tenantId, CreatedBy = userId };
            _db.MfgWorkshops.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.WorkshopType = wt;
        entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapWs(entity);
    }

    public async Task<IReadOnlyList<MfgBomDto>> ListBomsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgBoms.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        return await MapBomsAsync(tenantId, list, ct);
    }

    public async Task<MfgBomDetailDto> GetBomDetailAsync(Guid tenantId, Guid bomId, CancellationToken ct = default)
    {
        var bom = await _db.MfgBoms.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bomId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy BOM.", 404);
        var dto = (await MapBomsAsync(tenantId, [bom], ct))[0];
        var lines = await LoadBomLinesAsync(tenantId, bomId, ct);
        return new MfgBomDetailDto(dto, lines);
    }

    public async Task<MfgBomDto> UpsertBomAsync(
        Guid tenantId, Guid userId, MfgBomUpsertRequest req, CancellationToken ct = default)
    {
        var parent = await RequireItem(tenantId, req.ParentItemId, ct);
        if (parent.ItemType is not ("FG" or "SFG"))
            throw new AppException("BOM chỉ gắn TP/BTP.");
        var version = Req(req.Version, 20, "Phiên bản");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim();
        if (status is not ("Draft" or "Active" or "Obsolete"))
            throw new AppException("Trạng thái BOM: Draft | Active | Obsolete.");

        MfgBom entity;
        if (req.Id is Guid id)
        {
            entity = await RequireBom(tenantId, id, ct);
            if (entity.Status == "Active" && status == "Draft")
                throw new AppException("Không hạ Active về Draft — dùng Obsolete.");
        }
        else
        {
            var code = string.IsNullOrWhiteSpace(req.Code)
                ? $"BOM-{parent.Code}-V{version.Replace('.', '_')}"
                : NormCode(req.Code);
            if (await _db.MfgBoms.AnyAsync(
                    x => x.TenantId == tenantId && x.ParentItemId == req.ParentItemId
                         && x.Version == version && !x.IsDeleted, ct))
                throw new AppException("Phiên bản BOM đã tồn tại cho sản phẩm này.");
            entity = new MfgBom
            {
                TenantId = tenantId, Code = code, ParentItemId = req.ParentItemId,
                Version = version, CreatedBy = userId
            };
            _db.MfgBoms.Add(entity);
        }

        entity.Version = version;
        entity.Status = status == "Active" ? entity.Status : status; // Active qua ActivateBom
        if (status is "Draft" or "Obsolete") entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapBomsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<MfgBomLineDto> UpsertBomLineAsync(
        Guid tenantId, Guid userId, Guid bomId, MfgBomLineUpsertRequest req, CancellationToken ct = default)
    {
        var bom = await RequireBom(tenantId, bomId, ct);
        if (bom.Status == "Obsolete") throw new AppException("BOM đã Obsolete.");
        if (req.Qty <= 0) throw new AppException("Định mức phải > 0.");
        var comp = await RequireItem(tenantId, req.ComponentItemId, ct);
        if (comp.Id == bom.ParentItemId) throw new AppException("Không tự tham chiếu parent.");
        if (comp.ItemType == "FG") throw new AppException("Thành phần BOM không thể là FG.");

        MfgBomLine line;
        if (req.Id is Guid lid)
        {
            line = await _db.MfgBomLines.FirstOrDefaultAsync(
                x => x.Id == lid && x.TenantId == tenantId && x.BomId == bomId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy dòng BOM.");
        }
        else
        {
            line = new MfgBomLine { TenantId = tenantId, BomId = bomId, CreatedBy = userId };
            _db.MfgBomLines.Add(line);
        }

        line.ComponentItemId = req.ComponentItemId;
        line.Qty = req.Qty;
        line.Unit = string.IsNullOrWhiteSpace(req.Unit) ? comp.Unit : req.Unit.Trim().ToUpperInvariant();
        line.Level = req.Level is > 0 ? req.Level.Value : 1;
        line.Note = NullIfEmpty(req.Note);
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        return new MfgBomLineDto(
            line.Id, line.BomId, line.ComponentItemId, comp.Code, comp.Name, comp.ItemType,
            line.Qty, line.Unit, line.Level, line.Note);
    }

    public async Task<MfgBomDto> ActivateBomAsync(
        Guid tenantId, Guid userId, Guid bomId, CancellationToken ct = default)
    {
        var bom = await RequireBom(tenantId, bomId, ct);
        var hasLines = await _db.MfgBomLines.AnyAsync(
            x => x.TenantId == tenantId && x.BomId == bomId && !x.IsDeleted, ct);
        if (!hasLines) throw new AppException("BOM cần ít nhất 1 dòng định mức.");

        var others = await _db.MfgBoms
            .Where(x => x.TenantId == tenantId && x.ParentItemId == bom.ParentItemId
                        && x.Id != bomId && x.Status == "Active" && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var o in others)
        {
            o.Status = "Obsolete";
            o.UpdatedBy = userId;
        }

        bom.Status = "Active";
        bom.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapBomsAsync(tenantId, [bom], ct))[0];
    }

    public async Task<IReadOnlyList<MfgPlanDto>> ListPlansAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgPlans.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapPlansAsync(tenantId, list, ct);
    }

    public async Task<MfgPlanDetailDto> GetPlanDetailAsync(Guid tenantId, Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.MfgPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == planId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy KH SX.", 404);
        var dto = (await MapPlansAsync(tenantId, [plan], ct))[0];
        var lines = await LoadPlanLinesAsync(tenantId, planId, ct);
        return new MfgPlanDetailDto(dto, lines);
    }

    public async Task<MfgPlanDto> UpsertPlanAsync(
        Guid tenantId, Guid userId, MfgPlanUpsertRequest req, CancellationToken ct = default)
    {
        var so = Req(req.SourceOrderCode, 40, "Mã đơn hàng").ToUpperInvariant();

        MfgPlan entity;
        if (req.Id is Guid id)
        {
            entity = await RequirePlan(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa KH Draft.");
        }
        else
        {
            entity = new MfgPlan
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "KH", ct) : NormCode(req.Code),
                Status = "Draft", CreatedByUserId = userId, CreatedBy = userId
            };
            if (await _db.MfgPlans.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã KH đã tồn tại.");
            _db.MfgPlans.Add(entity);
        }

        entity.SourceOrderCode = so;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPlansAsync(tenantId, [entity], ct))[0];
    }

    public async Task<MfgPlanLineDto> UpsertPlanLineAsync(
        Guid tenantId, Guid userId, Guid planId, MfgPlanLineUpsertRequest req, CancellationToken ct = default)
    {
        var plan = await RequirePlan(tenantId, planId, ct);
        if (plan.Status != "Draft") throw new AppException("Chỉ sửa KH Draft.");
        if (req.Qty <= 0) throw new AppException("SL phải > 0.");
        var item = await RequireItem(tenantId, req.ItemId, ct);
        if (item.ItemType is not ("FG" or "SFG")) throw new AppException("KH SX chỉ lập cho TP/BTP.");

        if (req.WorkshopId is Guid wid)
        {
            var ok = await _db.MfgWorkshops.AnyAsync(
                x => x.Id == wid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Xưởng không hợp lệ.");
        }

        MfgPlanLine line;
        if (req.Id is Guid lid)
        {
            line = await _db.MfgPlanLines.FirstOrDefaultAsync(
                x => x.Id == lid && x.TenantId == tenantId && x.PlanId == planId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy dòng KH.");
        }
        else
        {
            line = new MfgPlanLine { TenantId = tenantId, PlanId = planId, CreatedBy = userId };
            _db.MfgPlanLines.Add(line);
        }

        line.ItemId = req.ItemId;
        line.Qty = req.Qty;
        line.WorkshopId = req.WorkshopId;
        line.Note = NullIfEmpty(req.Note);
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var lines = await LoadPlanLinesAsync(tenantId, planId, ct);
        return lines.First(x => x.Id == line.Id);
    }

    public async Task<MfgPlanDto> ConfirmPlanAsync(
        Guid tenantId, Guid userId, Guid planId, CancellationToken ct = default)
    {
        var plan = await RequirePlan(tenantId, planId, ct);
        if (plan.Status != "Draft") throw new AppException("Chỉ xác nhận KH Draft.");
        var has = await _db.MfgPlanLines.AnyAsync(
            x => x.TenantId == tenantId && x.PlanId == planId && !x.IsDeleted, ct);
        if (!has) throw new AppException("KH cần ít nhất 1 dòng.");
        plan.Status = "Confirmed";
        plan.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPlansAsync(tenantId, [plan], ct))[0];
    }

    public async Task<IReadOnlyList<MfgWorkOrderDto>> ListWorkOrdersAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.MfgWorkOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term));
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync(ct);
        return await MapWosAsync(tenantId, list, ct);
    }

    public async Task<MfgWorkOrderDetailDto> GetWorkOrderDetailAsync(
        Guid tenantId, Guid woId, CancellationToken ct = default)
    {
        var wo = await _db.MfgWorkOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == woId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy lệnh SX.", 404);
        var dto = (await MapWosAsync(tenantId, [wo], ct))[0];

        var issues = await _db.MfgMaterialIssues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted)
            .OrderByDescending(x => x.IssuedAt).ToListAsync(ct);
        var receipts = await _db.MfgFgReceipts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted)
            .OrderByDescending(x => x.ReceivedAt).ToListAsync(ct);
        var scraps = await _db.MfgScraps.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted)
            .OrderByDescending(x => x.RecordedAt).ToListAsync(ct);

        var itemIds = issues.Select(x => x.ItemId)
            .Concat(receipts.Select(x => x.ItemId))
            .Concat(scraps.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value))
            .Distinct().ToList();
        var items = itemIds.Count == 0
            ? new Dictionary<Guid, MfgItem>()
            : await _db.MfgItems.AsNoTracking()
                .Where(x => x.TenantId == tenantId && itemIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);

        var issueDtos = issues.Select(i =>
        {
            items.TryGetValue(i.ItemId, out var it);
            return new MfgMaterialIssueDto(
                i.Id, i.WorkOrderId, i.ItemId, it?.Code, it?.Name,
                i.Qty, i.UnitCost, decimal.Round(i.Qty * i.UnitCost, 2), i.Unit, i.IssuedAt, i.Note);
        }).ToList();

        var receiptDtos = receipts.Select(r =>
        {
            items.TryGetValue(r.ItemId, out var it);
            return new MfgFgReceiptDto(
                r.Id, r.WorkOrderId, r.ItemId, it?.Code, it?.Name, r.Qty, r.Unit, r.ReceivedAt, r.Note);
        }).ToList();

        var scrapDtos = scraps.Select(s =>
        {
            MfgItem? it = null;
            if (s.ItemId is Guid sid) items.TryGetValue(sid, out it);
            return new MfgScrapDto(
                s.Id, s.WorkOrderId, s.ItemId, it?.Code, it?.Name,
                s.Qty, s.Unit, s.ScrapType, s.RecordedAt, s.Note);
        }).ToList();

        IReadOnlyList<MfgBomLineDto> required = Array.Empty<MfgBomLineDto>();
        if (wo.BomId is Guid bomId)
            required = await LoadBomLinesAsync(tenantId, bomId, ct);

        var cost = await GetCostSheetAsync(tenantId, woId, ct);
        return new MfgWorkOrderDetailDto(dto, issueDtos, receiptDtos, scrapDtos, required, cost);
    }

    public async Task<MfgWorkOrderDto> UpsertWorkOrderAsync(
        Guid tenantId, Guid userId, MfgWorkOrderUpsertRequest req, CancellationToken ct = default)
    {
        var item = await RequireItem(tenantId, req.ItemId, ct);
        if (item.ItemType is not ("FG" or "SFG")) throw new AppException("Lệnh SX chỉ cho TP/BTP.");
        if (req.Qty <= 0) throw new AppException("SL phải > 0.");

        if (req.WorkshopId is Guid wid)
        {
            var ok = await _db.MfgWorkshops.AnyAsync(
                x => x.Id == wid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Xưởng không hợp lệ.");
        }

        Guid? bomId = req.BomId;
        if (bomId is null)
        {
            bomId = await _db.MfgBoms.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ParentItemId == req.ItemId
                            && x.Status == "Active" && !x.IsDeleted)
                .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        }
        else
        {
            var bomOk = await _db.MfgBoms.AnyAsync(
                x => x.Id == bomId && x.TenantId == tenantId && x.ParentItemId == req.ItemId && !x.IsDeleted, ct);
            if (!bomOk) throw new AppException("BOM không khớp sản phẩm.");
        }

        if (req.PlanId is Guid pid)
        {
            var ok = await _db.MfgPlans.AnyAsync(
                x => x.Id == pid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("KH SX không hợp lệ.");
        }

        MfgWorkOrder entity;
        if (req.Id is Guid id)
        {
            entity = await RequireWo(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa lệnh Draft.");
        }
        else
        {
            entity = new MfgWorkOrder
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "LSX", ct) : NormCode(req.Code),
                Status = "Draft", CreatedByUserId = userId, CreatedBy = userId
            };
            if (await _db.MfgWorkOrders.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã lệnh đã tồn tại.");
            _db.MfgWorkOrders.Add(entity);
        }

        entity.ItemId = req.ItemId;
        entity.Qty = req.Qty;
        entity.WorkshopId = req.WorkshopId;
        entity.BomId = bomId;
        entity.PlanId = req.PlanId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [entity], ct))[0];
    }

    public async Task<MfgWorkOrderDto> ApproveWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status != "Draft") throw new AppException("Chỉ duyệt lệnh Draft.");
        wo.Status = "Approved";
        wo.ApprovedBy = userId;
        wo.ApprovedAt = DateTimeOffset.UtcNow;
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> ReleaseWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status != "Approved") throw new AppException("Chỉ phát hành lệnh đã duyệt.");
        wo.Status = "Released";
        wo.ReleasedAt = DateTimeOffset.UtcNow;
        wo.PrintedAt = DateTimeOffset.UtcNow;
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> IssueMaterialsAsync(
        Guid tenantId, Guid userId, Guid woId, MfgMaterialIssueRequest req, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        EnsureWoOperable(wo);
        if (wo.Status is not ("Released" or "MaterialsIssued"))
            throw new AppException("Chỉ xuất NVL khi lệnh đã phát hành.");
        if (req.Qty <= 0) throw new AppException("SL xuất phải > 0.");
        var item = await RequireItem(tenantId, req.ItemId, ct);
        if (item.ItemType is not ("RM" or "SFG"))
            throw new AppException("Chỉ xuất NVL / BTP.");

        var unitCost = req.UnitCost is > 0
            ? decimal.Round(req.UnitCost.Value, 2)
            : await ResolveItemUnitCostAsync(tenantId, item, ct);

        _db.MfgMaterialIssues.Add(new MfgMaterialIssue
        {
            TenantId = tenantId, WorkOrderId = woId, ItemId = req.ItemId,
            Qty = req.Qty,
            UnitCost = unitCost,
            Unit = string.IsNullOrWhiteSpace(req.Unit) ? item.Unit : req.Unit.Trim().ToUpperInvariant(),
            IssuedAt = DateTimeOffset.UtcNow, IssuedBy = userId,
            Note = NullIfEmpty(req.Note), CreatedBy = userId
        });

        wo.QtyIssuedMaterial += req.Qty;
        wo.Status = "MaterialsIssued";
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> ReceiveFgAsync(
        Guid tenantId, Guid userId, Guid woId, MfgFgReceiptRequest req, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        EnsureWoOperable(wo);
        if (wo.Status is not ("Released" or "MaterialsIssued" or "Completed"))
            throw new AppException("Chỉ nhập TP khi lệnh đã phát hành / xuất NVL.");
        if (req.Qty <= 0) throw new AppException("SL nhập phải > 0.");
        if (wo.QtyFgReceived + req.Qty > wo.Qty)
            throw new AppException("Tổng nhập TP vượt SL lệnh.");

        var item = await RequireItem(tenantId, wo.ItemId, ct);
        _db.MfgFgReceipts.Add(new MfgFgReceipt
        {
            TenantId = tenantId, WorkOrderId = woId, ItemId = wo.ItemId,
            Qty = req.Qty, Unit = item.Unit, ReceivedAt = DateTimeOffset.UtcNow,
            ReceivedBy = userId, Note = NullIfEmpty(req.Note), CreatedBy = userId
        });

        wo.QtyFgReceived += req.Qty;
        if (wo.QtyFgReceived >= wo.Qty) wo.Status = "Completed";
        else if (wo.Status == "Released") wo.Status = "MaterialsIssued";
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> RecordScrapAsync(
        Guid tenantId, Guid userId, Guid woId, MfgScrapRequest req, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        EnsureWoOperable(wo);
        if (wo.Status is not ("Released" or "MaterialsIssued" or "Completed"))
            throw new AppException("Chỉ ghi phế/hao hụt khi lệnh đang SX.");
        if (req.Qty <= 0) throw new AppException("SL phế phải > 0.");
        var scrapType = (req.ScrapType ?? "Scrap").Trim();
        if (scrapType is not ("Scrap" or "Loss")) throw new AppException("Loại: Scrap | Loss.");

        string unit = "CAI";
        Guid? itemId = req.ItemId;
        if (itemId is Guid iid)
        {
            var item = await RequireItem(tenantId, iid, ct);
            unit = string.IsNullOrWhiteSpace(req.Unit) ? item.Unit : req.Unit.Trim().ToUpperInvariant();
        }
        else if (!string.IsNullOrWhiteSpace(req.Unit))
            unit = req.Unit.Trim().ToUpperInvariant();

        _db.MfgScraps.Add(new MfgScrap
        {
            TenantId = tenantId, WorkOrderId = woId, ItemId = itemId,
            Qty = decimal.Round(req.Qty, 4), Unit = unit, ScrapType = scrapType,
            RecordedAt = DateTimeOffset.UtcNow, RecordedByUserId = userId,
            Note = NullIfEmpty(req.Note), CreatedBy = userId
        });
        wo.QtyScrap += decimal.Round(req.Qty, 4);
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> PauseWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, MfgWoNoteRequest? req = null, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status is not ("Released" or "MaterialsIssued"))
            throw new AppException("Chỉ tạm dừng lệnh Released / MaterialsIssued.");
        wo.ResumeStatus = wo.Status;
        wo.Status = "Paused";
        wo.PausedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(req?.Note)) wo.Note = req.Note.Trim();
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> ResumeWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status != "Paused") throw new AppException("Chỉ tiếp tục lệnh đang Paused.");
        wo.Status = string.IsNullOrWhiteSpace(wo.ResumeStatus)
            ? (wo.QtyIssuedMaterial > 0 ? "MaterialsIssued" : "Released")
            : wo.ResumeStatus!;
        wo.ResumeStatus = null;
        wo.PausedAt = null;
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> CancelWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, MfgWoCancelRequest req, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status is "Closed" or "Cancelled")
            throw new AppException("Lệnh đã đóng/hủy.");
        if (wo.Status is "Completed")
            throw new AppException("Lệnh đã hoàn thành — dùng Đóng lệnh.");
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length < 3) throw new AppException("Lý do hủy ≥ 3 ký tự.");
        wo.Status = "Cancelled";
        wo.CancelReason = reason;
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    public async Task<MfgWorkOrderDto> CloseWorkOrderAsync(
        Guid tenantId, Guid userId, Guid woId, MfgWoNoteRequest? req = null, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status is "Closed" or "Cancelled")
            throw new AppException("Lệnh đã đóng/hủy.");
        if (wo.Status is not ("Completed" or "MaterialsIssued" or "Released" or "Paused"))
            throw new AppException("Chỉ đóng lệnh đã phát hành / hoàn thành.");
        if (wo.QtyFgReceived <= 0 && wo.QtyScrap <= 0 && wo.QtyIssuedMaterial <= 0)
            throw new AppException("Chưa có xuất NVL / nhập TP / phế — không đóng trống.");

        wo.Status = "Closed";
        wo.ClosedAt = DateTimeOffset.UtcNow;
        wo.ResumeStatus = null;
        wo.PausedAt = null;
        if (!string.IsNullOrWhiteSpace(req?.Note)) wo.Note = req.Note.Trim();
        wo.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapWosAsync(tenantId, [wo], ct))[0];
    }

    private static void EnsureWoOperable(MfgWorkOrder wo)
    {
        if (wo.Status is "Paused") throw new AppException("Lệnh đang tạm dừng — tiếp tục trước.");
        if (wo.Status is "Closed") throw new AppException("Lệnh đã đóng — không chỉnh.");
        if (wo.Status is "Cancelled") throw new AppException("Lệnh đã hủy.");
    }

    private async Task<MfgItem> RequireItem(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.MfgItems.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy sản phẩm SX.");

    private async Task<MfgBom> RequireBom(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.MfgBoms.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy BOM.");

    private async Task<MfgPlan> RequirePlan(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.MfgPlans.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy KH SX.");

    private async Task<MfgWorkOrder> RequireWo(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy lệnh SX.");

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix switch
        {
            "KH" => await _db.MfgPlans.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.MfgWorkOrders.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct)
        };
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private async Task<IReadOnlyList<MfgBomLineDto>> LoadBomLinesAsync(
        Guid tenantId, Guid bomId, CancellationToken ct)
    {
        var lines = await _db.MfgBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BomId == bomId && !x.IsDeleted)
            .OrderBy(x => x.Level).ThenBy(x => x.CreatedAt).ToListAsync(ct);
        var ids = lines.Select(x => x.ComponentItemId).Distinct().ToList();
        var items = await _db.MfgItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return lines.Select(l =>
        {
            items.TryGetValue(l.ComponentItemId, out var it);
            return new MfgBomLineDto(
                l.Id, l.BomId, l.ComponentItemId, it?.Code, it?.Name, it?.ItemType,
                l.Qty, l.Unit, l.Level, l.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<MfgPlanLineDto>> LoadPlanLinesAsync(
        Guid tenantId, Guid planId, CancellationToken ct)
    {
        var lines = await _db.MfgPlanLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PlanId == planId && !x.IsDeleted)
            .ToListAsync(ct);
        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var wsIds = lines.Where(x => x.WorkshopId.HasValue).Select(x => x.WorkshopId!.Value).Distinct().ToList();
        var items = await _db.MfgItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var workshops = wsIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.MfgWorkshops.AsNoTracking()
                .Where(x => x.TenantId == tenantId && wsIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return lines.Select(l =>
        {
            items.TryGetValue(l.ItemId, out var it);
            return new MfgPlanLineDto(
                l.Id, l.PlanId, l.ItemId, it?.Code, it?.Name, l.Qty, l.WorkshopId,
                l.WorkshopId is Guid w ? workshops.GetValueOrDefault(w) : null, l.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<MfgBomDto>> MapBomsAsync(
        Guid tenantId, List<MfgBom> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<MfgBomDto>();
        var ids = list.Select(x => x.Id).ToList();
        var parentIds = list.Select(x => x.ParentItemId).Distinct().ToList();
        var parents = await _db.MfgItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && parentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var counts = await _db.MfgBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.BomId) && !x.IsDeleted)
            .GroupBy(x => x.BomId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(b =>
        {
            parents.TryGetValue(b.ParentItemId, out var p);
            return new MfgBomDto(
                b.Id, b.Code, b.ParentItemId, p?.Code, p?.Name, b.Version, b.Status, b.Note,
                counts.GetValueOrDefault(b.Id));
        }).ToList();
    }

    private async Task<IReadOnlyList<MfgPlanDto>> MapPlansAsync(
        Guid tenantId, List<MfgPlan> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<MfgPlanDto>();
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.MfgPlanLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PlanId) && !x.IsDeleted)
            .GroupBy(x => x.PlanId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(p => new MfgPlanDto(
            p.Id, p.Code, p.SourceOrderCode, p.Status, p.Note, counts.GetValueOrDefault(p.Id))).ToList();
    }

    private async Task<IReadOnlyList<MfgWorkOrderDto>> MapWosAsync(
        Guid tenantId, List<MfgWorkOrder> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<MfgWorkOrderDto>();
        var itemIds = list.Select(x => x.ItemId).Distinct().ToList();
        var wsIds = list.Where(x => x.WorkshopId.HasValue).Select(x => x.WorkshopId!.Value).Distinct().ToList();
        var bomIds = list.Where(x => x.BomId.HasValue).Select(x => x.BomId!.Value).Distinct().ToList();
        var items = await _db.MfgItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && itemIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var workshops = wsIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.MfgWorkshops.AsNoTracking()
                .Where(x => x.TenantId == tenantId && wsIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var boms = bomIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.MfgBoms.AsNoTracking()
                .Where(x => x.TenantId == tenantId && bomIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);

        return list.Select(w =>
        {
            items.TryGetValue(w.ItemId, out var it);
            return new MfgWorkOrderDto(
                w.Id, w.Code, w.ItemId, it?.Code, it?.Name, w.Qty,
                w.WorkshopId, w.WorkshopId is Guid wid ? workshops.GetValueOrDefault(wid) : null,
                w.BomId, w.BomId is Guid bid ? boms.GetValueOrDefault(bid) : null, w.PlanId,
                w.Status, w.Note, w.QtyIssuedMaterial, w.QtyFgReceived, w.QtyScrap,
                w.ApprovedAt, w.ReleasedAt, w.PrintedAt, w.PausedAt, w.ClosedAt, w.CancelReason);
        }).ToList();
    }

    public async Task<MfgCostSheetDto?> GetCostSheetAsync(
        Guid tenantId, Guid woId, CancellationToken ct = default)
    {
        var sheet = await _db.MfgCostSheets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted
                                      && x.Status != "Void", ct);
        if (sheet is null) return null;
        return await MapCostSheetAsync(tenantId, sheet, ct);
    }

    public async Task<MfgCostSheetDto> CalculateCostAsync(
        Guid tenantId, Guid userId, Guid woId, CancellationToken ct = default)
    {
        var wo = await RequireWo(tenantId, woId, ct);
        if (wo.Status is "Cancelled" or "Draft")
            throw new AppException("Lệnh chưa đủ dữ liệu để tính giá thành.");
        if (wo.QtyFgReceived <= 0)
            throw new AppException("Cần nhập TP trước khi tính giá thành.");

        var issues = await _db.MfgMaterialIssues
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted).ToListAsync(ct);
        if (issues.Count == 0) throw new AppException("Chưa có xuất NVL để tập hợp chi phí.");

        var itemIds = issues.Select(x => x.ItemId).Distinct().ToList();
        var items = await _db.MfgItems
            .Where(x => x.TenantId == tenantId && itemIds.Contains(x.Id) && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var iss in issues)
        {
            if (iss.UnitCost > 0) continue;
            items.TryGetValue(iss.ItemId, out var it);
            if (it is null) continue;
            iss.UnitCost = await ResolveItemUnitCostAsync(tenantId, it, ct);
            iss.UpdatedBy = userId;
        }

        var materialCost = decimal.Round(issues.Sum(x => x.Qty * x.UnitCost), 2);
        var goodQty = wo.QtyFgReceived;
        var unitCost = goodQty > 0 ? decimal.Round(materialCost / goodQty, 4) : 0;

        var existing = await _db.MfgCostSheets
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted
                                      && x.Status != "Void", ct);
        if (existing is not null && existing.Status == "Pushed")
            throw new AppException("Giá thành đã đẩy INV/FIN — không tính lại.");

        MfgCostSheet sheet;
        if (existing is not null)
        {
            sheet = existing;
            var oldLines = await _db.MfgCostSheetLines
                .Where(x => x.TenantId == tenantId && x.CostSheetId == sheet.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var ol in oldLines) { ol.IsDeleted = true; ol.UpdatedBy = userId; }
        }
        else
        {
            sheet = new MfgCostSheet
            {
                TenantId = tenantId,
                Code = $"GT-{wo.Code}",
                WorkOrderId = woId,
                CreatedBy = userId
            };
            if (await _db.MfgCostSheets.AnyAsync(x => x.TenantId == tenantId && x.Code == sheet.Code && !x.IsDeleted, ct))
                sheet.Code = $"GT-{wo.Code}-{DateTime.UtcNow:HHmmss}";
            _db.MfgCostSheets.Add(sheet);
        }

        sheet.Status = "Calculated";
        sheet.MaterialCost = materialCost;
        sheet.LaborCost = 0;
        sheet.OverheadCost = 0;
        sheet.TotalCost = materialCost;
        sheet.GoodQty = goodQty;
        sheet.UnitCost = unitCost;
        sheet.CalculatedAt = DateTimeOffset.UtcNow;
        sheet.CalculatedByUserId = userId;
        sheet.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        foreach (var iss in issues)
        {
            _db.MfgCostSheetLines.Add(new MfgCostSheetLine
            {
                TenantId = tenantId,
                CostSheetId = sheet.Id,
                MaterialIssueId = iss.Id,
                ItemId = iss.ItemId,
                Source = "Material",
                Qty = iss.Qty,
                UnitCost = iss.UnitCost,
                Amount = decimal.Round(iss.Qty * iss.UnitCost, 2),
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return await MapCostSheetAsync(tenantId, sheet, ct);
    }

    public async Task<MfgCostSheetDto> PushCostAsync(
        Guid tenantId, Guid userId, Guid woId, MfgCostPushRequest? req = null, CancellationToken ct = default)
    {
        var sheet = await _db.MfgCostSheets
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == woId && !x.IsDeleted
                                      && x.Status != "Void", ct)
            ?? throw new AppException("Chưa có bảng giá thành — hãy tính trước.");
        if (sheet.Status == "Pushed") throw new AppException("Giá thành đã đẩy.");
        if (sheet.Status != "Calculated") throw new AppException("Chỉ đẩy khi đã Calculated.");

        var wo = await RequireWo(tenantId, woId, ct);
        var fgItem = await RequireItem(tenantId, wo.ItemId, ct);

        // INV: cập nhật StandardCost SKU cùng mã (nếu có)
        var sku = await _db.InvSkus
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == fgItem.Code && !x.IsDeleted, ct);
        if (sku is not null)
        {
            sku.StandardCost = sheet.UnitCost;
            sku.UpdatedBy = userId;
            sheet.InvSkuId = sku.Id;
            sheet.InvSkuCode = sku.Code;
        }

        // MFG item FG cũng cập nhật StandardCost
        fgItem.StandardCost = sheet.UnitCost;
        fgItem.UpdatedBy = userId;

        // FIN stub: WIP → TP khi đủ kỳ + TK
        if (req?.PeriodId is Guid periodId && req.WipAccountId is Guid wipId && req.FgAccountId is Guid fgAccId)
        {
            var period = await _db.FinPeriods.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == periodId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy kỳ KT.");
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
            _ = await _db.FinAccounts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == wipId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("TK WIP không hợp lệ.");
            _ = await _db.FinAccounts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fgAccId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("TK TP không hợp lệ.");

            var lines = new List<FinJournalLineUpsertRequest>
            {
                new(null, fgAccId, sheet.TotalCost, 0, wo.Code, null, "Nhập giá thành TP"),
                new(null, wipId, 0, sheet.TotalCost, wo.Code, null, "Kết chuyển WIP"),
            };
            var je = await _fin.CreateAutoJournalStubAsync(tenantId, userId, new FinJournalUpsertRequest(
                null, null, periodId, DateTimeOffset.UtcNow,
                $"GT SX {sheet.Code}: {fgItem.Code} đơn giá {sheet.UnitCost:N4}",
                wo.Code, null, "Auto", lines), ct);
            je = await _fin.PostJournalAsync(tenantId, userId, je.Id, ct);
            sheet.FinJournalId = je.Id;
            sheet.FinJournalCode = je.Code;
        }

        sheet.Status = "Pushed";
        sheet.PushedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(req?.Note)) sheet.Note = req.Note.Trim();
        sheet.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await MapCostSheetAsync(tenantId, sheet, ct);
    }

    private async Task<decimal> ResolveItemUnitCostAsync(Guid tenantId, MfgItem item, CancellationToken ct)
    {
        if (item.StandardCost > 0) return decimal.Round(item.StandardCost, 2);
        var sku = await _db.InvSkus.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == item.Code && !x.IsDeleted, ct);
        return sku is { StandardCost: > 0 } ? decimal.Round(sku.StandardCost, 2) : 0;
    }

    private async Task<MfgCostSheetDto> MapCostSheetAsync(Guid tenantId, MfgCostSheet sheet, CancellationToken ct)
    {
        var woCode = await _db.MfgWorkOrders.AsNoTracking()
            .Where(x => x.Id == sheet.WorkOrderId).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var lines = await _db.MfgCostSheetLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CostSheetId == sheet.Id && !x.IsDeleted).ToListAsync(ct);
        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var items = itemIds.Count == 0
            ? new Dictionary<Guid, MfgItem>()
            : await _db.MfgItems.AsNoTracking()
                .Where(x => itemIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var lineDtos = lines.Select(l =>
        {
            items.TryGetValue(l.ItemId, out var it);
            return new MfgCostSheetLineDto(
                l.Id, l.MaterialIssueId, l.ItemId, it?.Code, it?.Name,
                l.Source, l.Qty, l.UnitCost, l.Amount, l.Note);
        }).ToList();

        return new MfgCostSheetDto(
            sheet.Id, sheet.Code, sheet.WorkOrderId, woCode, sheet.Status,
            sheet.MaterialCost, sheet.LaborCost, sheet.OverheadCost, sheet.TotalCost,
            sheet.GoodQty, sheet.UnitCost, sheet.InvSkuId, sheet.InvSkuCode,
            sheet.FinJournalId, sheet.FinJournalCode, sheet.CalculatedAt, sheet.PushedAt, sheet.Note,
            lineDtos);
    }

    private static MfgItemDto MapItem(MfgItem x) =>
        new(x.Id, x.Code, x.Name, x.ItemType, x.Unit, x.StandardCost, x.Status, x.Note);

    private static MfgWorkshopDto MapWs(MfgWorkshop x) =>
        new(x.Id, x.Code, x.Name, x.WorkshopType, x.Status, x.Note);

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

using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Domain.Base;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Inv;

public sealed class InvMasterService : IInvMasterService
{
    private static readonly HashSet<string> Costing =
        new(StringComparer.OrdinalIgnoreCase) { "Average", "Fifo" };
    private static readonly HashSet<string> SkuStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Active", "Inactive" };
    private static readonly HashSet<string> WhStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Active", "Inactive" };
    private static readonly HashSet<string> KeeperRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Keeper", "Supervisor" };

    private readonly AppDbContext _db;

    public InvMasterService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvItemGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var groups = await _db.InvItemGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .ToListAsync(ct);
        var counts = await _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.GroupId != null)
            .GroupBy(x => x.GroupId!.Value)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return groups.Select(g => new InvItemGroupDto(
            g.Id, g.Code, g.Name, g.SortOrder, g.IsActive, counts.GetValueOrDefault(g.Id))).ToList();
    }

    public async Task<InvItemGroupDto> UpsertGroupAsync(
        Guid tenantId, Guid userId, InvItemGroupUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = NormName(req.Name);
        await EnsureGroupCodeUnique(tenantId, code, req.Id, ct);

        InvItemGroup entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvItemGroups.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy nhóm hàng.");
            entity.Code = code;
            entity.Name = name;
            entity.SortOrder = req.SortOrder ?? entity.SortOrder;
            entity.IsActive = req.IsActive ?? entity.IsActive;
            Touch(entity, userId);
        }
        else
        {
            entity = new InvItemGroup
            {
                TenantId = tenantId, Code = code, Name = name,
                SortOrder = req.SortOrder ?? 0, IsActive = req.IsActive ?? true, CreatedBy = userId
            };
            _db.InvItemGroups.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        var count = await _db.InvSkus.CountAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && x.GroupId == entity.Id, ct);
        return new InvItemGroupDto(entity.Id, entity.Code, entity.Name, entity.SortOrder, entity.IsActive, count);
    }

    public async Task<IReadOnlyList<InvUomDto>> ListUomsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return list.Select(x => new InvUomDto(x.Id, x.Code, x.Name, x.IsActive)).ToList();
    }

    public async Task<InvUomDto> UpsertUomAsync(
        Guid tenantId, Guid userId, InvUomUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = NormName(req.Name, 100);
        await EnsureUomCodeUnique(tenantId, code, req.Id, ct);

        InvUnitOfMeasure entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvUnitsOfMeasure.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy ĐVT.");
            entity.Code = code;
            entity.Name = name;
            entity.IsActive = req.IsActive ?? entity.IsActive;
            Touch(entity, userId);
        }
        else
        {
            entity = new InvUnitOfMeasure
            {
                TenantId = tenantId, Code = code, Name = name,
                IsActive = req.IsActive ?? true, CreatedBy = userId
            };
            _db.InvUnitsOfMeasure.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        return new InvUomDto(entity.Id, entity.Code, entity.Name, entity.IsActive);
    }

    public async Task<IReadOnlyList<InvUnitConversionDto>> ListConversionsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvUnitConversions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToListAsync(ct);
        var uoms = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        return list.Select(c => new InvUnitConversionDto(
            c.Id, c.FromUnitId, uoms.GetValueOrDefault(c.FromUnitId),
            c.ToUnitId, uoms.GetValueOrDefault(c.ToUnitId), c.Factor)).ToList();
    }

    public async Task<InvUnitConversionDto> UpsertConversionAsync(
        Guid tenantId, Guid userId, InvUnitConversionUpsertRequest req, CancellationToken ct = default)
    {
        if (req.FromUnitId == req.ToUnitId) throw new AppException("ĐVT nguồn và đích phải khác nhau.");
        if (req.Factor <= 0) throw new AppException("Hệ số quy đổi phải > 0.");

        await EnsureUnit(tenantId, req.FromUnitId, ct);
        await EnsureUnit(tenantId, req.ToUnitId, ct);

        var dup = await _db.InvUnitConversions.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted
                 && x.FromUnitId == req.FromUnitId && x.ToUnitId == req.ToUnitId
                 && (req.Id == null || x.Id != req.Id), ct);
        if (dup) throw new AppException("Quy đổi ĐVT này đã tồn tại.");

        InvUnitConversion entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvUnitConversions.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy quy đổi.");
            entity.FromUnitId = req.FromUnitId;
            entity.ToUnitId = req.ToUnitId;
            entity.Factor = req.Factor;
            Touch(entity, userId);
        }
        else
        {
            entity = new InvUnitConversion
            {
                TenantId = tenantId, FromUnitId = req.FromUnitId, ToUnitId = req.ToUnitId,
                Factor = req.Factor, CreatedBy = userId
            };
            _db.InvUnitConversions.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        var fromCode = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.Id == entity.FromUnitId).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var toCode = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.Id == entity.ToUnitId).Select(x => x.Code).FirstOrDefaultAsync(ct);
        return new InvUnitConversionDto(
            entity.Id, entity.FromUnitId, fromCode, entity.ToUnitId, toCode, entity.Factor);
    }

    public async Task<IReadOnlyList<InvSkuDto>> ListSkusAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }

        var list = await query.OrderBy(x => x.Code).Take(500).ToListAsync(ct);
        return await MapSkusAsync(tenantId, list, ct);
    }

    public async Task<InvSkuDto> UpsertSkuAsync(
        Guid tenantId, Guid userId, InvSkuUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = NormName(req.Name);
        await EnsureSkuCodeUnique(tenantId, code, req.Id, ct);
        await EnsureUnit(tenantId, req.BaseUnitId, ct);

        if (req.GroupId is Guid gid)
        {
            var ok = await _db.InvItemGroups.AnyAsync(
                x => x.Id == gid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Nhóm hàng không hợp lệ.");
        }

        var costing = string.IsNullOrWhiteSpace(req.CostingMethod) ? "Average" : req.CostingMethod.Trim();
        if (!Costing.Contains(costing)) throw new AppException("Phương pháp giá vốn: Average | Fifo.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (!SkuStatuses.Contains(status)) throw new AppException("Trạng thái SKU: Active | Inactive.");
        if (req.StandardCost < 0) throw new AppException("Giá vốn chuẩn ≥ 0.");
        ValidateMinMax(req.MinQty, req.MaxQty, req.ReorderQty);

        InvSku entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvSkus.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy SKU.");
            entity.Code = code;
            entity.Name = name;
            entity.GroupId = req.GroupId;
            entity.BaseUnitId = req.BaseUnitId;
            entity.TrackLot = req.TrackLot ?? entity.TrackLot;
            entity.TrackSerial = req.TrackSerial ?? entity.TrackSerial;
            entity.TrackExpiry = req.TrackExpiry ?? entity.TrackExpiry;
            entity.CostingMethod = costing;
            entity.StandardCost = req.StandardCost;
            entity.Status = status;
            entity.MinQty = req.MinQty;
            entity.MaxQty = req.MaxQty;
            entity.ReorderQty = req.ReorderQty;
            entity.Note = NullIfEmpty(req.Note);
            Touch(entity, userId);
        }
        else
        {
            entity = new InvSku
            {
                TenantId = tenantId, Code = code, Name = name, GroupId = req.GroupId,
                BaseUnitId = req.BaseUnitId,
                TrackLot = req.TrackLot ?? false,
                TrackSerial = req.TrackSerial ?? false,
                TrackExpiry = req.TrackExpiry ?? false,
                CostingMethod = costing, StandardCost = req.StandardCost, Status = status,
                MinQty = req.MinQty, MaxQty = req.MaxQty, ReorderQty = req.ReorderQty,
                Note = NullIfEmpty(req.Note), CreatedBy = userId
            };
            _db.InvSkus.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        return (await MapSkusAsync(tenantId, [entity], ct))[0];
    }

    public async Task<InvSkuDto> SetSkuStatusAsync(
        Guid tenantId, Guid userId, Guid skuId, InvSkuStatusRequest req, CancellationToken ct = default)
    {
        var status = (req.Status ?? "").Trim();
        if (!SkuStatuses.Contains(status)) throw new AppException("Trạng thái SKU: Active | Inactive.");
        var entity = await _db.InvSkus.FirstOrDefaultAsync(
            x => x.Id == skuId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy SKU.");
        entity.Status = status;
        Touch(entity, userId);
        await _db.SaveChangesAsync(ct);
        return (await MapSkusAsync(tenantId, [entity], ct))[0];
    }

    public async Task<string> ExportSkusCsvAsync(Guid tenantId, CancellationToken ct = default)
    {
        var skus = await ListSkusAsync(tenantId, null, ct);
        var groupCodes = await _db.InvItemGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,GroupCode,BaseUnitCode,TrackLot,TrackSerial,TrackExpiry,CostingMethod,StandardCost,Status,MinQty,MaxQty,ReorderQty");
        foreach (var s in skus)
        {
            var gCode = s.GroupId is Guid gid ? groupCodes.GetValueOrDefault(gid) : null;
            sb.Append(Escape(s.Code)).Append(',')
                .Append(Escape(s.Name)).Append(',')
                .Append(Escape(gCode)).Append(',')
                .Append(Escape(s.BaseUnitCode)).Append(',')
                .Append(s.TrackLot ? "1" : "0").Append(',')
                .Append(s.TrackSerial ? "1" : "0").Append(',')
                .Append(s.TrackExpiry ? "1" : "0").Append(',')
                .Append(Escape(s.CostingMethod)).Append(',')
                .Append(s.StandardCost).Append(',')
                .Append(Escape(s.Status)).Append(',')
                .Append(s.MinQty?.ToString() ?? "").Append(',')
                .Append(s.MaxQty?.ToString() ?? "").Append(',')
                .Append(s.ReorderQty?.ToString() ?? "")
                .AppendLine();
        }
        return sb.ToString();
    }

    public async Task<InvImportResult> ImportSkusCsvAsync(
        Guid tenantId, Guid userId, InvImportRequest req, CancellationToken ct = default)
    {
        var text = req.CsvText ?? "";
        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) throw new AppException("CSV trống.");

        var start = 0;
        if (lines[0].Contains("Code", StringComparison.OrdinalIgnoreCase)
            && lines[0].Contains("Name", StringComparison.OrdinalIgnoreCase))
            start = 1;

        var groups = await _db.InvItemGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);
        var uoms = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);

        var messages = new List<string>();
        var ok = 0;
        var fail = 0;

        for (var i = start; i < lines.Length; i++)
        {
            var cols = SplitCsv(lines[i]);
            if (cols.Count < 4)
            {
                messages.Add($"L{i + 1}: thiếu cột");
                fail++;
                continue;
            }

            var code = cols[0].Trim().ToUpperInvariant();
            try
            {
                var groupCode = NullIfEmpty(cols.ElementAtOrDefault(2));
                Guid? groupId = null;
                if (groupCode is not null)
                {
                    if (!groups.TryGetValue(groupCode, out var gid))
                        throw new AppException($"Nhóm '{groupCode}' chưa có.");
                    groupId = gid;
                }

                var unitCode = (cols.ElementAtOrDefault(3) ?? "").Trim();
                if (!uoms.TryGetValue(unitCode, out var unitId))
                    throw new AppException($"ĐVT '{unitCode}' chưa có.");

                var existing = await _db.InvSkus.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct);

                await UpsertSkuAsync(tenantId, userId, new InvSkuUpsertRequest(
                    existing?.Id,
                    code,
                    cols.ElementAtOrDefault(1) ?? code,
                    groupId,
                    unitId,
                    ParseBool(cols.ElementAtOrDefault(4)),
                    ParseBool(cols.ElementAtOrDefault(5)),
                    ParseBool(cols.ElementAtOrDefault(6)),
                    NullIfEmpty(cols.ElementAtOrDefault(7)) ?? "Average",
                    ParseDec(cols.ElementAtOrDefault(8)) ?? 0,
                    NullIfEmpty(cols.ElementAtOrDefault(9)) ?? "Active",
                    ParseDec(cols.ElementAtOrDefault(10)),
                    ParseDec(cols.ElementAtOrDefault(11)),
                    ParseDec(cols.ElementAtOrDefault(12)),
                    null), ct);

                messages.Add($"{code}: OK");
                ok++;
            }
            catch (Exception ex)
            {
                messages.Add($"{code}: {ex.Message}");
                fail++;
            }
        }

        return new InvImportResult(ok + fail, ok, fail, messages);
    }

    public async Task<IReadOnlyList<InvWarehouseTypeDto>> ListWarehouseTypesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvWarehouseTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return list.Select(x => new InvWarehouseTypeDto(x.Id, x.Code, x.Name, x.IsActive)).ToList();
    }

    public async Task<InvWarehouseTypeDto> UpsertWarehouseTypeAsync(
        Guid tenantId, Guid userId, InvWarehouseTypeUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = NormName(req.Name);
        await EnsureWhTypeCodeUnique(tenantId, code, req.Id, ct);

        InvWarehouseType entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvWarehouseTypes.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy loại kho.");
            entity.Code = code;
            entity.Name = name;
            entity.IsActive = req.IsActive ?? entity.IsActive;
            Touch(entity, userId);
        }
        else
        {
            entity = new InvWarehouseType
            {
                TenantId = tenantId, Code = code, Name = name,
                IsActive = req.IsActive ?? true, CreatedBy = userId
            };
            _db.InvWarehouseTypes.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        return new InvWarehouseTypeDto(entity.Id, entity.Code, entity.Name, entity.IsActive);
    }

    public async Task<IReadOnlyList<InvWarehouseDto>> ListWarehousesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvWarehouses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return await MapWarehousesAsync(tenantId, list, ct);
    }

    public async Task<InvWarehouseDto> UpsertWarehouseAsync(
        Guid tenantId, Guid userId, InvWarehouseUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = NormName(req.Name);
        await EnsureWarehouseCodeUnique(tenantId, code, req.Id, ct);

        if (req.WarehouseTypeId is Guid tid)
        {
            var ok = await _db.InvWarehouseTypes.AnyAsync(
                x => x.Id == tid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Loại kho không hợp lệ.");
        }

        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (!WhStatuses.Contains(status)) throw new AppException("Trạng thái kho: Active | Inactive.");

        InvWarehouse entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvWarehouses.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy kho.");
            entity.Code = code;
            entity.Name = name;
            entity.WarehouseTypeId = req.WarehouseTypeId;
            entity.Address = NullIfEmpty(req.Address);
            entity.Status = status;
            entity.PickPolicy = NormPickPolicy(req.PickPolicy);
            entity.AllowNegativeStock = req.AllowNegativeStock ?? entity.AllowNegativeStock;
            Touch(entity, userId);
        }
        else
        {
            entity = new InvWarehouse
            {
                TenantId = tenantId, Code = code, Name = name,
                WarehouseTypeId = req.WarehouseTypeId, Address = NullIfEmpty(req.Address),
                Status = status, PickPolicy = NormPickPolicy(req.PickPolicy),
                AllowNegativeStock = req.AllowNegativeStock ?? false, CreatedBy = userId
            };
            _db.InvWarehouses.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        return (await MapWarehousesAsync(tenantId, [entity], ct))[0];
    }

    public async Task<InvWarehouseDetailDto> GetWarehouseDetailAsync(
        Guid tenantId, Guid warehouseId, CancellationToken ct = default)
    {
        var wh = await _db.InvWarehouses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == warehouseId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy kho.");
        var dto = (await MapWarehousesAsync(tenantId, [wh], ct))[0];

        var keepers = await _db.InvWarehouseKeepers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WarehouseId == warehouseId && !x.IsDeleted)
            .ToListAsync(ct);
        var userIds = keepers.Select(x => x.UserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var keeperDtos = keepers.Select(k => new InvWarehouseKeeperDto(
            k.Id, k.WarehouseId, k.UserId, users.GetValueOrDefault(k.UserId), k.Role, k.IsActive)).ToList();
        return new InvWarehouseDetailDto(dto, keeperDtos);
    }

    public async Task<InvWarehouseKeeperDto> UpsertKeeperAsync(
        Guid tenantId, Guid userId, Guid warehouseId, InvWarehouseKeeperUpsertRequest req, CancellationToken ct = default)
    {
        var whOk = await _db.InvWarehouses.AnyAsync(
            x => x.Id == warehouseId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!whOk) throw new AppException("Không tìm thấy kho.");

        var userOk = await _db.Users.AnyAsync(
            x => x.Id == req.UserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!userOk) throw new AppException("Người dùng không hợp lệ.");

        var role = string.IsNullOrWhiteSpace(req.Role) ? "Keeper" : req.Role.Trim();
        if (!KeeperRoles.Contains(role)) throw new AppException("Vai trò: Keeper | Supervisor.");

        InvWarehouseKeeper entity;
        if (req.Id is Guid id)
        {
            entity = await _db.InvWarehouseKeepers.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.WarehouseId == warehouseId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy gán thủ kho.");
            entity.UserId = req.UserId;
            entity.Role = role;
            entity.IsActive = req.IsActive ?? entity.IsActive;
            Touch(entity, userId);
        }
        else
        {
            var exists = await _db.InvWarehouseKeepers.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.WarehouseId == warehouseId
                     && x.UserId == req.UserId && !x.IsDeleted, ct);
            if (exists is not null)
            {
                exists.Role = role;
                exists.IsActive = req.IsActive ?? true;
                Touch(exists, userId);
                entity = exists;
            }
            else
            {
                entity = new InvWarehouseKeeper
                {
                    TenantId = tenantId, WarehouseId = warehouseId, UserId = req.UserId,
                    Role = role, IsActive = req.IsActive ?? true, CreatedBy = userId
                };
                _db.InvWarehouseKeepers.Add(entity);
            }
        }

        await _db.SaveChangesAsync(ct);
        var uname = await _db.Users.AsNoTracking()
            .Where(x => x.Id == entity.UserId)
            .Select(x => x.DisplayName ?? x.Username)
            .FirstOrDefaultAsync(ct);
        return new InvWarehouseKeeperDto(
            entity.Id, entity.WarehouseId, entity.UserId, uname, entity.Role, entity.IsActive);
    }

    private async Task<IReadOnlyList<InvSkuDto>> MapSkusAsync(
        Guid tenantId, List<InvSku> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<InvSkuDto>();
        var groupIds = list.Where(x => x.GroupId.HasValue).Select(x => x.GroupId!.Value).Distinct().ToList();
        var unitIds = list.Select(x => x.BaseUnitId).Distinct().ToList();
        var groups = groupIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.InvItemGroups.AsNoTracking()
                .Where(x => x.TenantId == tenantId && groupIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var units = await _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.TenantId == tenantId && unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);

        return list.Select(s => new InvSkuDto(
            s.Id, s.Code, s.Name, s.GroupId,
            s.GroupId is Guid g ? groups.GetValueOrDefault(g) : null,
            s.BaseUnitId, units.GetValueOrDefault(s.BaseUnitId),
            s.TrackLot, s.TrackSerial, s.TrackExpiry,
            s.CostingMethod, s.StandardCost, s.Status,
            s.MinQty, s.MaxQty, s.ReorderQty, s.Note)).ToList();
    }

    private async Task<IReadOnlyList<InvWarehouseDto>> MapWarehousesAsync(
        Guid tenantId, List<InvWarehouse> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<InvWarehouseDto>();
        var ids = list.Select(x => x.Id).ToList();
        var typeIds = list.Where(x => x.WarehouseTypeId.HasValue).Select(x => x.WarehouseTypeId!.Value).Distinct().ToList();
        var types = typeIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.InvWarehouseTypes.AsNoTracking()
                .Where(x => x.TenantId == tenantId && typeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var keeperCounts = await _db.InvWarehouseKeepers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.WarehouseId) && !x.IsDeleted && x.IsActive)
            .GroupBy(x => x.WarehouseId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(w => new InvWarehouseDto(
            w.Id, w.Code, w.Name, w.WarehouseTypeId,
            w.WarehouseTypeId is Guid t ? types.GetValueOrDefault(t) : null,
            w.Address, w.Status, w.PickPolicy, w.AllowNegativeStock,
            keeperCounts.GetValueOrDefault(w.Id))).ToList();
    }

    private static string NormPickPolicy(string? policy)
    {
        var p = string.IsNullOrWhiteSpace(policy) ? "Fifo" : policy.Trim();
        if (!p.Equals("Fifo", StringComparison.OrdinalIgnoreCase)
            && !p.Equals("Fefo", StringComparison.OrdinalIgnoreCase))
            throw new AppException("PickPolicy: Fifo | Fefo.");
        return p.Equals("Fefo", StringComparison.OrdinalIgnoreCase) ? "Fefo" : "Fifo";
    }

    private async Task EnsureUnit(Guid tenantId, Guid unitId, CancellationToken ct)
    {
        var ok = await _db.InvUnitsOfMeasure.AnyAsync(
            x => x.Id == unitId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("ĐVT không hợp lệ.");
    }

    private async Task EnsureGroupCodeUnique(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var q = _db.InvItemGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code);
        if (excludeId is Guid eid) q = q.Where(x => x.Id != eid);
        if (await q.AnyAsync(ct)) throw new AppException($"Mã '{code}' đã tồn tại.");
    }

    private async Task EnsureUomCodeUnique(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var q = _db.InvUnitsOfMeasure.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code);
        if (excludeId is Guid eid) q = q.Where(x => x.Id != eid);
        if (await q.AnyAsync(ct)) throw new AppException($"Mã '{code}' đã tồn tại.");
    }

    private async Task EnsureSkuCodeUnique(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var q = _db.InvSkus.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code);
        if (excludeId is Guid eid) q = q.Where(x => x.Id != eid);
        if (await q.AnyAsync(ct)) throw new AppException($"Mã '{code}' đã tồn tại.");
    }

    private async Task EnsureWhTypeCodeUnique(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var q = _db.InvWarehouseTypes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code);
        if (excludeId is Guid eid) q = q.Where(x => x.Id != eid);
        if (await q.AnyAsync(ct)) throw new AppException($"Mã '{code}' đã tồn tại.");
    }

    private async Task EnsureWarehouseCodeUnique(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var q = _db.InvWarehouses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Code == code);
        if (excludeId is Guid eid) q = q.Where(x => x.Id != eid);
        if (await q.AnyAsync(ct)) throw new AppException($"Mã '{code}' đã tồn tại.");
    }

    private static void ValidateMinMax(decimal? min, decimal? max, decimal? reorder)
    {
        if (min is < 0 || max is < 0 || reorder is < 0)
            throw new AppException("Min / Max / Reorder ≥ 0.");
        if (min is decimal mn && max is decimal mx && mn > mx)
            throw new AppException("MinQty không được lớn hơn MaxQty.");
    }

    private static void Touch(TenantEntity e, Guid userId)
    {
        e.UpdatedAt = DateTimeOffset.UtcNow;
        e.UpdatedBy = userId;
        e.RowVersion++;
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string NormName(string? name, int max = 200)
    {
        var n = (name ?? "").Trim();
        if (n.Length is < 1 || n.Length > max) throw new AppException($"Tên 1–{max} ký tự.");
        return n;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool ParseBool(string? s) =>
        s is "1" or "true" or "True" or "yes" or "YES" or "Y" or "y";

    private static decimal? ParseDec(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return decimal.TryParse(s.Trim(), out var d) ? d : null;
    }

    private static string Escape(string? s)
    {
        s ??= "";
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
            return $"\"{s.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        return s;
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var cur = new StringBuilder();
        var inQ = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQ && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else inQ = !inQ;
            }
            else if (ch == ',' && !inQ)
            {
                result.Add(cur.ToString());
                cur.Clear();
            }
            else cur.Append(ch);
        }
        result.Add(cur.ToString());
        return result;
    }
}

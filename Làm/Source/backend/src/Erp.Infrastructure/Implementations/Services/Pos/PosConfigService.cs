using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pos;

public sealed class PosConfigService : IPosConfigService
{
    private readonly AppDbContext _db;
    public PosConfigService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PosStoreDto>> ListStoresAsync(Guid tenantId, CancellationToken ct = default)
    {
        var stores = await _db.PosStores.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        return await MapStoresAsync(tenantId, stores, ct);
    }

    public async Task<PosStoreDto> UpsertStoreAsync(
        Guid tenantId, Guid userId, PosStoreUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên điểm bán");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Trạng thái điểm bán không hợp lệ.");

        PosStore entity;
        if (req.Id is Guid id)
        {
            entity = await RequireStore(tenantId, id, ct);
        }
        else
        {
            await EnsureUniqueStoreCode(tenantId, code, null, ct);
            entity = new PosStore { TenantId = tenantId, CreatedBy = userId };
            _db.PosStores.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase))
            await EnsureUniqueStoreCode(tenantId, code, entity.Id, ct);

        entity.Code = code;
        entity.Name = name;
        entity.Address = NullIfEmpty(req.Address);
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStoresAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PosStoreDetailDto> GetStoreDetailAsync(
        Guid tenantId, Guid storeId, CancellationToken ct = default)
    {
        var store = await _db.PosStores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Điểm bán không tồn tại.", 404);

        var terminals = await _db.PosTerminals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new PosTerminalDto(x.Id, x.StoreId, x.Code, x.Name, x.Status))
            .ToListAsync(ct);

        var printers = await _db.PosPrinters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new PosPrinterDto(x.Id, x.StoreId, x.Code, x.Name, x.PrinterType, x.ConnectionInfo, x.Status))
            .ToListAsync(ct);

        var cashiers = await _db.PosCashierAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted)
            .ToListAsync(ct);
        var userIds = cashiers.Select(c => c.UserId).Distinct().ToList();
        var names = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var cashierDtos = cashiers
            .OrderBy(c => names.GetValueOrDefault(c.UserId) ?? "")
            .Select(c => new PosCashierDto(
                c.Id, c.StoreId, c.UserId, names.GetValueOrDefault(c.UserId), c.Role, c.IsActive))
            .ToList();

        var storeDto = (await MapStoresAsync(tenantId, [store], ct))[0];
        return new PosStoreDetailDto(storeDto, terminals, printers, cashierDtos);
    }

    public async Task<PosTerminalDto> UpsertTerminalAsync(
        Guid tenantId, Guid userId, Guid storeId, PosTerminalUpsertRequest req, CancellationToken ct = default)
    {
        await RequireStore(tenantId, storeId, ct);
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên quầy");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();

        PosTerminal entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosTerminals.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted, ct)
                ?? throw new AppException("Quầy không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosTerminals.AnyAsync(
                    x => x.TenantId == tenantId && x.StoreId == storeId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã quầy đã tồn tại.");
            entity = new PosTerminal { TenantId = tenantId, StoreId = storeId, CreatedBy = userId };
            _db.PosTerminals.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PosTerminalDto(entity.Id, entity.StoreId, entity.Code, entity.Name, entity.Status);
    }

    public async Task<PosPrinterDto> UpsertPrinterAsync(
        Guid tenantId, Guid userId, Guid storeId, PosPrinterUpsertRequest req, CancellationToken ct = default)
    {
        await RequireStore(tenantId, storeId, ct);
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên máy in");
        var type = string.IsNullOrWhiteSpace(req.PrinterType) ? "Receipt" : req.PrinterType.Trim();
        if (type is not ("Receipt" or "Kitchen")) throw new AppException("Loại máy in không hợp lệ.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();

        PosPrinter entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosPrinters.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted, ct)
                ?? throw new AppException("Máy in không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosPrinters.AnyAsync(
                    x => x.TenantId == tenantId && x.StoreId == storeId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã máy in đã tồn tại.");
            entity = new PosPrinter { TenantId = tenantId, StoreId = storeId, CreatedBy = userId };
            _db.PosPrinters.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.PrinterType = type;
        entity.ConnectionInfo = NullIfEmpty(req.ConnectionInfo);
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PosPrinterDto(
            entity.Id, entity.StoreId, entity.Code, entity.Name, entity.PrinterType,
            entity.ConnectionInfo, entity.Status);
    }

    public async Task<PosCashierDto> UpsertCashierAsync(
        Guid tenantId, Guid userId, Guid storeId, PosCashierUpsertRequest req, CancellationToken ct = default)
    {
        await RequireStore(tenantId, storeId, ct);
        var role = string.IsNullOrWhiteSpace(req.Role) ? "Cashier" : req.Role.Trim();
        if (role is not ("Cashier" or "Supervisor")) throw new AppException("Vai trò thu ngân không hợp lệ.");

        var userOk = await _db.Users.AnyAsync(
            x => x.Id == req.UserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!userOk) throw new AppException("Người dùng không tồn tại.", 404);

        PosCashierAssignment entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosCashierAssignments.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.StoreId == storeId && !x.IsDeleted, ct)
                ?? throw new AppException("Phân quyền không tồn tại.", 404);
        }
        else
        {
            var existing = await _db.PosCashierAssignments.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.StoreId == storeId && x.UserId == req.UserId && !x.IsDeleted, ct);
            if (existing is not null)
            {
                entity = existing;
            }
            else
            {
                entity = new PosCashierAssignment
                {
                    TenantId = tenantId, StoreId = storeId, UserId = req.UserId, CreatedBy = userId
                };
                _db.PosCashierAssignments.Add(entity);
            }
        }

        entity.UserId = req.UserId;
        entity.Role = role;
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var uname = await _db.Users.AsNoTracking()
            .Where(x => x.Id == entity.UserId)
            .Select(x => x.DisplayName ?? x.Username)
            .FirstOrDefaultAsync(ct);
        return new PosCashierDto(entity.Id, entity.StoreId, entity.UserId, uname, entity.Role, entity.IsActive);
    }

    public async Task<IReadOnlyList<PosCategoryDto>> ListCategoriesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var cats = await _db.PosProductCategories.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .ToListAsync(ct);
        var ids = cats.Select(c => c.Id).ToList();
        var counts = await _db.PosProducts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CategoryId != null && ids.Contains(x.CategoryId.Value) && !x.IsDeleted)
            .GroupBy(x => x.CategoryId!.Value)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
        return cats.Select(c => new PosCategoryDto(
            c.Id, c.Code, c.Name, c.SortOrder, c.IsActive, counts.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<PosCategoryDto> UpsertCategoryAsync(
        Guid tenantId, Guid userId, PosCategoryUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên nhóm");

        PosProductCategory entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosProductCategories.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Nhóm SP không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosProductCategories.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã nhóm đã tồn tại.");
            var max = await _db.PosProductCategories
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? 0;
            entity = new PosProductCategory
            {
                TenantId = tenantId, CreatedBy = userId, SortOrder = req.SortOrder ?? max + 1
            };
            _db.PosProductCategories.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        if (req.SortOrder is int so) entity.SortOrder = so;
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var count = await _db.PosProducts.CountAsync(
            x => x.TenantId == tenantId && x.CategoryId == entity.Id && !x.IsDeleted, ct);
        return new PosCategoryDto(entity.Id, entity.Code, entity.Name, entity.SortOrder, entity.IsActive, count);
    }

    public async Task<IReadOnlyList<PosProductDto>> ListProductsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.PosProducts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        var products = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code).Take(500).ToListAsync(ct);
        return await MapProductsAsync(tenantId, products, ct);
    }

    public async Task<PosProductDto> UpsertProductAsync(
        Guid tenantId, Guid userId, PosProductUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên SP");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Suspended")) throw new AppException("Trạng thái SP không hợp lệ.");

        if (req.CategoryId is Guid catId)
        {
            var ok = await _db.PosProductCategories.AnyAsync(
                x => x.Id == catId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Nhóm SP không tồn tại.", 404);
        }

        PosProduct entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosProducts.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Sản phẩm không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosProducts.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã SP đã tồn tại.");
            entity = new PosProduct { TenantId = tenantId, CreatedBy = userId };
            _db.PosProducts.Add(entity);
        }

        entity.CategoryId = req.CategoryId;
        entity.Code = code;
        entity.Name = name;
        entity.Unit = NullIfEmpty(req.Unit);
        entity.Status = status;
        if (req.SortOrder is int so) entity.SortOrder = so;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapProductsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PosProductDto> SetProductStatusAsync(
        Guid tenantId, Guid userId, Guid productId, string status, CancellationToken ct = default)
    {
        if (status is not ("Active" or "Suspended")) throw new AppException("Trạng thái SP không hợp lệ.");
        var entity = await _db.PosProducts.FirstOrDefaultAsync(
            x => x.Id == productId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Sản phẩm không tồn tại.", 404);
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapProductsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<PosBomLineDto>> ListBomAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        await RequireProduct(tenantId, productId, ct);
        return await _db.PosBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProductId == productId && !x.IsDeleted)
            .OrderBy(x => x.MaterialCode)
            .Select(x => new PosBomLineDto(x.Id, x.ProductId, x.MaterialCode, x.MaterialName, x.Qty, x.Unit))
            .ToListAsync(ct);
    }

    public async Task<PosBomLineDto> UpsertBomAsync(
        Guid tenantId, Guid userId, Guid productId, PosBomLineUpsertRequest req, CancellationToken ct = default)
    {
        await RequireProduct(tenantId, productId, ct);
        var mCode = NormCode(req.MaterialCode);
        var mName = Req(req.MaterialName, 200, "Tên NVL");
        if (req.Qty <= 0) throw new AppException("Định mức phải > 0.");

        PosBomLine entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosBomLines.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.ProductId == productId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng BOM không tồn tại.", 404);
        }
        else
        {
            entity = new PosBomLine { TenantId = tenantId, ProductId = productId, CreatedBy = userId };
            _db.PosBomLines.Add(entity);
        }

        entity.MaterialCode = mCode;
        entity.MaterialName = mName;
        entity.Qty = req.Qty;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "cai" : req.Unit.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PosBomLineDto(
            entity.Id, entity.ProductId, entity.MaterialCode, entity.MaterialName, entity.Qty, entity.Unit);
    }

    public async Task<PosSyncResult> SyncCatalogAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        // Cap-1: đánh dấu đồng bộ back-office (stub stamp) — không gọi hệ thống ngoài
        var now = DateTimeOffset.UtcNow;
        var products = await _db.PosProducts
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var p in products)
        {
            p.SyncedAt = now;
            p.UpdatedBy = userId;
        }
        await _db.SaveChangesAsync(ct);
        return new PosSyncResult(products.Count, now);
    }

    public async Task<IReadOnlyList<PosTaxRateDto>> ListTaxRatesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        return await _db.PosTaxRates.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code)
            .Select(x => new PosTaxRateDto(x.Id, x.Code, x.Name, x.RatePct, x.IsDefault, x.IsActive))
            .ToListAsync(ct);
    }

    public async Task<PosTaxRateDto> UpsertTaxRateAsync(
        Guid tenantId, Guid userId, PosTaxRateUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên thuế");
        if (req.RatePct is < 0 or > 100) throw new AppException("Thuế suất 0–100%.");

        PosTaxRate entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosTaxRates.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Thuế không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosTaxRates.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã thuế đã tồn tại.");
            entity = new PosTaxRate { TenantId = tenantId, CreatedBy = userId };
            _db.PosTaxRates.Add(entity);
        }

        var makeDefault = req.IsDefault ?? entity.IsDefault;
        if (makeDefault)
        {
            var others = await _db.PosTaxRates
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id != entity.Id && x.IsDefault)
                .ToListAsync(ct);
            foreach (var o in others) o.IsDefault = false;
        }

        entity.Code = code;
        entity.Name = name;
        entity.RatePct = req.RatePct;
        entity.IsDefault = makeDefault;
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PosTaxRateDto(
            entity.Id, entity.Code, entity.Name, entity.RatePct, entity.IsDefault, entity.IsActive);
    }

    public async Task<IReadOnlyList<PosPriceListDto>> ListPriceListsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var lists = await _db.PosPriceLists.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        if (lists.Count == 0) return Array.Empty<PosPriceListDto>();

        var storeIds = lists.Select(x => x.StoreId).Distinct().ToList();
        var stores = await _db.PosStores.AsNoTracking()
            .Where(x => x.TenantId == tenantId && storeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var ids = lists.Select(x => x.Id).ToList();
        var counts = await _db.PosPriceListItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PriceListId) && !x.IsDeleted)
            .GroupBy(x => x.PriceListId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);

        return lists.Select(x => new PosPriceListDto(
            x.Id, x.StoreId, stores.GetValueOrDefault(x.StoreId), x.Code, x.Name, x.Status,
            counts.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<PosPriceListDto> UpsertPriceListAsync(
        Guid tenantId, Guid userId, PosPriceListUpsertRequest req, CancellationToken ct = default)
    {
        await RequireStore(tenantId, req.StoreId, ct);
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên bảng giá");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();

        PosPriceList entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosPriceLists.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Bảng giá không tồn tại.", 404);
        }
        else
        {
            if (await _db.PosPriceLists.AnyAsync(
                    x => x.TenantId == tenantId && x.StoreId == req.StoreId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã bảng giá đã tồn tại tại điểm bán.");
            entity = new PosPriceList { TenantId = tenantId, CreatedBy = userId };
            _db.PosPriceLists.Add(entity);
        }

        entity.StoreId = req.StoreId;
        entity.Code = code;
        entity.Name = name;
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var storeName = await _db.PosStores.AsNoTracking()
            .Where(x => x.Id == entity.StoreId).Select(x => x.Name).FirstOrDefaultAsync(ct);
        var count = await _db.PosPriceListItems.CountAsync(
            x => x.TenantId == tenantId && x.PriceListId == entity.Id && !x.IsDeleted, ct);
        return new PosPriceListDto(
            entity.Id, entity.StoreId, storeName, entity.Code, entity.Name, entity.Status, count);
    }

    public async Task<IReadOnlyList<PosPriceItemDto>> ListPriceItemsAsync(
        Guid tenantId, Guid priceListId, CancellationToken ct = default)
    {
        await RequirePriceList(tenantId, priceListId, ct);
        var items = await _db.PosPriceListItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PriceListId == priceListId && !x.IsDeleted)
            .ToListAsync(ct);
        var pIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.PosProducts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && pIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var tIds = items.Where(i => i.TaxRateId.HasValue).Select(i => i.TaxRateId!.Value).Distinct().ToList();
        var taxes = tIds.Count == 0
            ? new Dictionary<Guid, PosTaxRate>()
            : await _db.PosTaxRates.AsNoTracking()
                .Where(x => x.TenantId == tenantId && tIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);

        return items.Select(i =>
        {
            products.TryGetValue(i.ProductId, out var p);
            PosTaxRate? t = null;
            if (i.TaxRateId is Guid tid) taxes.TryGetValue(tid, out t);
            return new PosPriceItemDto(
                i.Id, i.PriceListId, i.ProductId, p?.Code, p?.Name, i.Price,
                i.TaxRateId, t?.Code, t?.RatePct);
        }).OrderBy(x => x.ProductCode).ToList();
    }

    public async Task<PosPriceItemDto> UpsertPriceItemAsync(
        Guid tenantId, Guid userId, Guid priceListId, PosPriceItemUpsertRequest req, CancellationToken ct = default)
    {
        await RequirePriceList(tenantId, priceListId, ct);
        await RequireProduct(tenantId, req.ProductId, ct);
        if (req.Price < 0) throw new AppException("Giá không hợp lệ.");
        if (req.TaxRateId is Guid taxId)
        {
            var ok = await _db.PosTaxRates.AnyAsync(
                x => x.Id == taxId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Thuế không tồn tại.", 404);
        }

        PosPriceListItem entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PosPriceListItems.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.PriceListId == priceListId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng giá không tồn tại.", 404);
        }
        else
        {
            var existing = await _db.PosPriceListItems.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PriceListId == priceListId
                     && x.ProductId == req.ProductId && !x.IsDeleted, ct);
            if (existing is not null) entity = existing;
            else
            {
                entity = new PosPriceListItem
                {
                    TenantId = tenantId, PriceListId = priceListId, ProductId = req.ProductId, CreatedBy = userId
                };
                _db.PosPriceListItems.Add(entity);
            }
        }

        entity.ProductId = req.ProductId;
        entity.Price = req.Price;
        entity.TaxRateId = req.TaxRateId;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var items = await ListPriceItemsAsync(tenantId, priceListId, ct);
        return items.First(x => x.Id == entity.Id);
    }

    private async Task<IReadOnlyList<PosStoreDto>> MapStoresAsync(
        Guid tenantId, List<PosStore> stores, CancellationToken ct)
    {
        if (stores.Count == 0) return Array.Empty<PosStoreDto>();
        var ids = stores.Select(s => s.Id).ToList();
        var tCounts = await _db.PosTerminals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.StoreId) && !x.IsDeleted)
            .GroupBy(x => x.StoreId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var pCounts = await _db.PosPrinters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.StoreId) && !x.IsDeleted)
            .GroupBy(x => x.StoreId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var cCounts = await _db.PosCashierAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.StoreId) && !x.IsDeleted && x.IsActive)
            .GroupBy(x => x.StoreId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        return stores.Select(s => new PosStoreDto(
            s.Id, s.Code, s.Name, s.Address, s.Status,
            tCounts.GetValueOrDefault(s.Id), pCounts.GetValueOrDefault(s.Id), cCounts.GetValueOrDefault(s.Id))).ToList();
    }

    private async Task<IReadOnlyList<PosProductDto>> MapProductsAsync(
        Guid tenantId, List<PosProduct> products, CancellationToken ct)
    {
        if (products.Count == 0) return Array.Empty<PosProductDto>();
        var catIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
        var cats = catIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.PosProductCategories.AsNoTracking()
                .Where(x => x.TenantId == tenantId && catIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var ids = products.Select(p => p.Id).ToList();
        var bomCounts = await _db.PosBomLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ProductId) && !x.IsDeleted)
            .GroupBy(x => x.ProductId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        return products.Select(p => new PosProductDto(
            p.Id, p.CategoryId,
            p.CategoryId is Guid cid ? cats.GetValueOrDefault(cid) : null,
            p.Code, p.Name, p.Unit, p.Status, p.SortOrder, p.SyncedAt,
            bomCounts.GetValueOrDefault(p.Id))).ToList();
    }

    private async Task<PosStore> RequireStore(Guid tenantId, Guid storeId, CancellationToken ct) =>
        await _db.PosStores.FirstOrDefaultAsync(
            x => x.Id == storeId && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Điểm bán không tồn tại.", 404);

    private async Task RequireProduct(Guid tenantId, Guid productId, CancellationToken ct)
    {
        var ok = await _db.PosProducts.AnyAsync(
            x => x.Id == productId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Sản phẩm không tồn tại.", 404);
    }

    private async Task RequirePriceList(Guid tenantId, Guid priceListId, CancellationToken ct)
    {
        var ok = await _db.PosPriceLists.AnyAsync(
            x => x.Id == priceListId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Bảng giá không tồn tại.", 404);
    }

    private async Task EnsureUniqueStoreCode(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        var exists = await _db.PosStores.AnyAsync(
            x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted
                 && (excludeId == null || x.Id != excludeId), ct);
        if (exists) throw new AppException("Mã điểm bán đã tồn tại.");
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

using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Application.Interfaces.Services.Pur;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Pur;

public sealed class PurPurchasingService : IPurPurchasingService
{
    // Cap-1: hạn mức duyệt PO đơn giản
    private const decimal PoAutoApproveLimit = 10_000_000m;

    private readonly AppDbContext _db;
    public PurPurchasingService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PurVendorDto>> ListVendorsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.PurVendors.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) || x.Name.Contains(term)
                || (x.TaxCode != null && x.TaxCode.Contains(term))
                || (x.Phone != null && x.Phone.Contains(term)));
        }
        var list = await query.OrderBy(x => x.Code).Take(300).ToListAsync(ct);
        return await MapVendorsAsync(tenantId, list, ct);
    }

    public async Task<PurVendorDto> UpsertVendorAsync(
        Guid tenantId, Guid userId, PurVendorUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên NCC");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Trạng thái NCC không hợp lệ.");

        PurVendor entity;
        if (req.Id is Guid id)
        {
            entity = await RequireVendor(tenantId, id, ct);
        }
        else
        {
            if (await _db.PurVendors.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã NCC đã tồn tại.");
            entity = new PurVendor { TenantId = tenantId, CreatedBy = userId };
            _db.PurVendors.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.TaxCode = NullIfEmpty(req.TaxCode);
        entity.Phone = NullIfEmpty(req.Phone);
        entity.Email = NullIfEmpty(req.Email);
        entity.Address = NullIfEmpty(req.Address);
        entity.PaymentTerms = NullIfEmpty(req.PaymentTerms);
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVendorsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PurVendorDetailDto> GetVendorDetailAsync(
        Guid tenantId, Guid vendorId, CancellationToken ct = default)
    {
        var vendor = await _db.PurVendors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == vendorId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("NCC không tồn tại.", 404);

        var contacts = await _db.PurVendorContacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.FullName)
            .Select(x => new PurVendorContactDto(x.Id, x.VendorId, x.FullName, x.Title, x.Phone, x.Email, x.IsPrimary))
            .ToListAsync(ct);

        var products = await _db.PurVendorProducts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted)
            .OrderBy(x => x.ProductCode)
            .Select(x => new PurVendorProductDto(x.Id, x.VendorId, x.ProductCode, x.ProductName, x.IsPreferred))
            .ToListAsync(ct);

        var prices = await _db.PurVendorPrices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => new PurVendorPriceDto(
                x.Id, x.VendorId, x.ProductCode, x.ProductName, x.UnitPrice, x.Currency,
                x.EffectiveFrom, x.EffectiveTo))
            .ToListAsync(ct);

        return new PurVendorDetailDto(
            (await MapVendorsAsync(tenantId, [vendor], ct))[0], contacts, products, prices);
    }

    public async Task<PurVendorContactDto> UpsertContactAsync(
        Guid tenantId, Guid userId, Guid vendorId, PurVendorContactUpsertRequest req, CancellationToken ct = default)
    {
        await RequireVendor(tenantId, vendorId, ct);
        var name = Req(req.FullName, 200, "Tên liên hệ");

        PurVendorContact entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PurVendorContacts.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted, ct)
                ?? throw new AppException("Liên hệ không tồn tại.", 404);
        }
        else
        {
            entity = new PurVendorContact { TenantId = tenantId, VendorId = vendorId, CreatedBy = userId };
            _db.PurVendorContacts.Add(entity);
        }

        var primary = req.IsPrimary ?? entity.IsPrimary;
        if (primary)
        {
            var others = await _db.PurVendorContacts
                .Where(x => x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted && x.Id != entity.Id)
                .ToListAsync(ct);
            foreach (var o in others) o.IsPrimary = false;
        }

        entity.FullName = name;
        entity.Title = NullIfEmpty(req.Title);
        entity.Phone = NullIfEmpty(req.Phone);
        entity.Email = NullIfEmpty(req.Email);
        entity.IsPrimary = primary;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PurVendorContactDto(
            entity.Id, entity.VendorId, entity.FullName, entity.Title, entity.Phone, entity.Email, entity.IsPrimary);
    }

    public async Task<PurVendorProductDto> UpsertVendorProductAsync(
        Guid tenantId, Guid userId, Guid vendorId, PurVendorProductUpsertRequest req, CancellationToken ct = default)
    {
        await RequireVendor(tenantId, vendorId, ct);
        var pCode = NormCode(req.ProductCode);
        var pName = Req(req.ProductName, 200, "Tên SP");

        PurVendorProduct entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PurVendorProducts.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted, ct)
                ?? throw new AppException("Gắn SP–NCC không tồn tại.", 404);
        }
        else
        {
            var existing = await _db.PurVendorProducts.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.VendorId == vendorId && x.ProductCode == pCode && !x.IsDeleted, ct);
            if (existing is not null) entity = existing;
            else
            {
                entity = new PurVendorProduct { TenantId = tenantId, VendorId = vendorId, CreatedBy = userId };
                _db.PurVendorProducts.Add(entity);
            }
        }

        entity.ProductCode = pCode;
        entity.ProductName = pName;
        entity.IsPreferred = req.IsPreferred ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PurVendorProductDto(
            entity.Id, entity.VendorId, entity.ProductCode, entity.ProductName, entity.IsPreferred);
    }

    public async Task<PurVendorPriceDto> UpsertVendorPriceAsync(
        Guid tenantId, Guid userId, Guid vendorId, PurVendorPriceUpsertRequest req, CancellationToken ct = default)
    {
        await RequireVendor(tenantId, vendorId, ct);
        var pCode = NormCode(req.ProductCode);
        var pName = Req(req.ProductName, 200, "Tên SP");
        if (req.UnitPrice < 0) throw new AppException("Giá mua không hợp lệ.");
        if (req.EffectiveTo is DateOnly to && to < req.EffectiveFrom)
            throw new AppException("Ngày hiệu lực đến phải >= từ.");

        PurVendorPrice entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PurVendorPrices.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.VendorId == vendorId && !x.IsDeleted, ct)
                ?? throw new AppException("Giá mua không tồn tại.", 404);
        }
        else
        {
            entity = new PurVendorPrice { TenantId = tenantId, VendorId = vendorId, CreatedBy = userId };
            _db.PurVendorPrices.Add(entity);
        }

        entity.ProductCode = pCode;
        entity.ProductName = pName;
        entity.UnitPrice = req.UnitPrice;
        entity.Currency = string.IsNullOrWhiteSpace(req.Currency) ? "VND" : req.Currency.Trim().ToUpperInvariant();
        entity.EffectiveFrom = req.EffectiveFrom;
        entity.EffectiveTo = req.EffectiveTo;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PurVendorPriceDto(
            entity.Id, entity.VendorId, entity.ProductCode, entity.ProductName,
            entity.UnitPrice, entity.Currency, entity.EffectiveFrom, entity.EffectiveTo);
    }

    public async Task<IReadOnlyList<PurPurchaseRequestDto>> ListPrsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurPurchaseRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200).ToListAsync(ct);
        return await MapPrsAsync(tenantId, list, ct);
    }

    public async Task<PurPurchaseRequestDto> UpsertPrAsync(
        Guid tenantId, Guid userId, PurPurchaseRequestUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);

        PurPurchaseRequest entity;
        if (req.Id is Guid id)
        {
            entity = await RequirePr(tenantId, id, ct);
            if (entity.Status is not ("Draft" or "Returned"))
                throw new AppException("Chỉ sửa PR Draft/Returned.");
        }
        else
        {
            if (await _db.PurPurchaseRequests.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã PR đã tồn tại.");
            entity = new PurPurchaseRequest
            {
                TenantId = tenantId, CreatedBy = userId, RequestedBy = userId, Status = "Draft"
            };
            _db.PurPurchaseRequests.Add(entity);
        }

        entity.Code = code;
        entity.RequestingUnit = NullIfEmpty(req.RequestingUnit);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPrsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PurPrDetailDto> GetPrDetailAsync(
        Guid tenantId, Guid prId, CancellationToken ct = default)
    {
        var pr = await _db.PurPurchaseRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == prId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("PR không tồn tại.", 404);
        var lines = await _db.PurPrLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PrId == prId && !x.IsDeleted)
            .OrderBy(x => x.ProductCode)
            .Select(x => new PurPrLineDto(x.Id, x.PrId, x.ProductCode, x.ProductName, x.Qty, x.Unit, x.Note))
            .ToListAsync(ct);
        return new PurPrDetailDto((await MapPrsAsync(tenantId, [pr], ct))[0], lines);
    }

    public async Task<PurPrLineDto> UpsertPrLineAsync(
        Guid tenantId, Guid userId, Guid prId, PurPrLineUpsertRequest req, CancellationToken ct = default)
    {
        var pr = await RequirePr(tenantId, prId, ct);
        if (pr.Status is not ("Draft" or "Returned"))
            throw new AppException("Chỉ thêm dòng khi PR Draft/Returned.");
        if (req.Qty <= 0) throw new AppException("Số lượng phải > 0.");

        PurPrLine entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PurPrLines.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.PrId == prId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng PR không tồn tại.", 404);
        }
        else
        {
            entity = new PurPrLine { TenantId = tenantId, PrId = prId, CreatedBy = userId };
            _db.PurPrLines.Add(entity);
        }

        entity.ProductCode = NormCode(req.ProductCode);
        entity.ProductName = Req(req.ProductName, 200, "Tên SP");
        entity.Qty = req.Qty;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "cai" : req.Unit.Trim();
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PurPrLineDto(
            entity.Id, entity.PrId, entity.ProductCode, entity.ProductName, entity.Qty, entity.Unit, entity.Note);
    }

    public async Task<PurPurchaseRequestDto> SubmitPrAsync(
        Guid tenantId, Guid userId, Guid prId, CancellationToken ct = default)
    {
        var pr = await RequirePr(tenantId, prId, ct);
        if (pr.Status is not ("Draft" or "Returned")) throw new AppException("PR không thể gửi duyệt.");
        var hasLine = await _db.PurPrLines.AnyAsync(
            x => x.TenantId == tenantId && x.PrId == prId && !x.IsDeleted, ct);
        if (!hasLine) throw new AppException("PR cần ít nhất 1 dòng.");
        pr.Status = "Submitted";
        pr.DecisionNote = null;
        pr.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPrsAsync(tenantId, [pr], ct))[0];
    }

    public async Task<PurPurchaseRequestDto> ApprovePrAsync(
        Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default)
        => await DecidePrAsync(tenantId, userId, prId, "Approved", req.Note, ct);

    public async Task<PurPurchaseRequestDto> RejectPrAsync(
        Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default)
        => await DecidePrAsync(tenantId, userId, prId, "Rejected", req.Note, ct);

    public async Task<PurPurchaseRequestDto> ReturnPrAsync(
        Guid tenantId, Guid userId, Guid prId, PurPrDecisionRequest req, CancellationToken ct = default)
        => await DecidePrAsync(tenantId, userId, prId, "Returned", req.Note, ct);

    public async Task<IReadOnlyList<PurPurchaseOrderDto>> ListPosAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurPurchaseOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200).ToListAsync(ct);
        return await MapPosAsync(tenantId, list, ct);
    }

    public async Task<PurPurchaseOrderDto> UpsertPoAsync(
        Guid tenantId, Guid userId, PurPurchaseOrderCreateRequest req, CancellationToken ct = default)
    {
        await RequireVendor(tenantId, req.VendorId, ct);
        var code = NormCode(req.Code);

        PurPurchaseOrder entity;
        if (req.Id is Guid id)
        {
            entity = await RequirePo(tenantId, id, ct);
            if (entity.Status is not ("Draft"))
                throw new AppException("Chỉ sửa PO Draft.");
        }
        else
        {
            if (await _db.PurPurchaseOrders.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã PO đã tồn tại.");
            entity = new PurPurchaseOrder
            {
                TenantId = tenantId, CreatedBy = userId, CreatedByUserId = userId, Status = "Draft"
            };
            _db.PurPurchaseOrders.Add(entity);
        }

        if (req.SourcePrId is Guid prId)
        {
            var pr = await RequirePr(tenantId, prId, ct);
            if (pr.Status != "Approved") throw new AppException("Chỉ tạo PO từ PR đã duyệt.");
            entity.SourcePrId = prId;
        }

        entity.Code = code;
        entity.VendorId = req.VendorId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await RecalcPoTotalAsync(tenantId, entity.Id, ct);
        entity = await RequirePo(tenantId, entity.Id, ct);
        return (await MapPosAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PurPoDetailDto> GetPoDetailAsync(
        Guid tenantId, Guid poId, CancellationToken ct = default)
    {
        var po = await _db.PurPurchaseOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == poId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("PO không tồn tại.", 404);
        var lines = await _db.PurPoLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted)
            .OrderBy(x => x.ProductCode)
            .Select(x => new PurPoLineDto(
                x.Id, x.PoId, x.ProductCode, x.ProductName,
                x.Qty, x.ReceivedQty, x.InvoicedQty, x.UnitPrice, x.Unit))
            .ToListAsync(ct);
        return new PurPoDetailDto((await MapPosAsync(tenantId, [po], ct))[0], lines);
    }

    public async Task<PurPoLineDto> UpsertPoLineAsync(
        Guid tenantId, Guid userId, Guid poId, PurPoLineUpsertRequest req, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status != "Draft") throw new AppException("Chỉ thêm dòng khi PO Draft.");
        if (req.Qty <= 0 || req.UnitPrice < 0) throw new AppException("Qty/giá không hợp lệ.");

        PurPoLine entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PurPoLines.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted, ct)
                ?? throw new AppException("Dòng PO không tồn tại.", 404);
        }
        else
        {
            entity = new PurPoLine { TenantId = tenantId, PoId = poId, CreatedBy = userId };
            _db.PurPoLines.Add(entity);
        }

        entity.ProductCode = NormCode(req.ProductCode);
        entity.ProductName = Req(req.ProductName, 200, "Tên SP");
        entity.Qty = req.Qty;
        entity.UnitPrice = req.UnitPrice;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "cai" : req.Unit.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        await RecalcPoTotalAsync(tenantId, poId, ct);
        return new PurPoLineDto(
            entity.Id, entity.PoId, entity.ProductCode, entity.ProductName,
            entity.Qty, entity.ReceivedQty, entity.InvoicedQty, entity.UnitPrice, entity.Unit);
    }

    public async Task<PurPurchaseOrderDto> CreatePoFromPrAsync(
        Guid tenantId, Guid userId, Guid prId, PurCreatePoFromPrRequest req, CancellationToken ct = default)
    {
        var pr = await RequirePr(tenantId, prId, ct);
        if (pr.Status != "Approved") throw new AppException("PR chưa duyệt.");
        await RequireVendor(tenantId, req.VendorId, ct);

        var code = NormCode(req.Code);
        if (await _db.PurPurchaseOrders.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
            throw new AppException("Mã PO đã tồn tại.");

        var prLines = await _db.PurPrLines
            .Where(x => x.TenantId == tenantId && x.PrId == prId && !x.IsDeleted)
            .ToListAsync(ct);
        if (prLines.Count == 0) throw new AppException("PR không có dòng.");

        var po = new PurPurchaseOrder
        {
            TenantId = tenantId,
            Code = code,
            VendorId = req.VendorId,
            SourcePrId = prId,
            Status = "Draft",
            Note = NullIfEmpty(req.Note) ?? $"Từ PR {pr.Code}",
            CreatedBy = userId,
            CreatedByUserId = userId
        };
        _db.PurPurchaseOrders.Add(po);
        await _db.SaveChangesAsync(ct);

        foreach (var l in prLines)
        {
            var price = await _db.PurVendorPrices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.VendorId == req.VendorId
                            && x.ProductCode == l.ProductCode && !x.IsDeleted)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => (decimal?)x.UnitPrice)
                .FirstOrDefaultAsync(ct) ?? 0;

            _db.PurPoLines.Add(new PurPoLine
            {
                TenantId = tenantId,
                PoId = po.Id,
                ProductCode = l.ProductCode,
                ProductName = l.ProductName,
                Qty = l.Qty,
                UnitPrice = price,
                Unit = l.Unit,
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        await RecalcPoTotalAsync(tenantId, po.Id, ct);
        po = await RequirePo(tenantId, po.Id, ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> SubmitPoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status != "Draft") throw new AppException("Chỉ gửi duyệt PO Draft.");
        var hasLine = await _db.PurPoLines.AnyAsync(
            x => x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted, ct);
        if (!hasLine) throw new AppException("PO cần ít nhất 1 dòng.");

        await RecalcPoTotalAsync(tenantId, poId, ct);
        po = await RequirePo(tenantId, poId, ct);

        // UC_027: hạn mức — dưới hạn mức tự duyệt
        if (po.TotalAmount <= PoAutoApproveLimit)
        {
            po.Status = "Approved";
            po.ApprovedBy = userId;
            po.ApprovedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            po.Status = "PendingApproval";
        }
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> ApprovePoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status != "PendingApproval") throw new AppException("PO không chờ duyệt.");
        po.Status = "Approved";
        po.ApprovedBy = userId;
        po.ApprovedAt = DateTimeOffset.UtcNow;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> SendPoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status != "Approved") throw new AppException("Chỉ gửi PO đã duyệt.");
        po.Status = "Sent";
        po.SentAt = DateTimeOffset.UtcNow;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> RevisePoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status is not ("Approved" or "Sent"))
            throw new AppException("Chỉ sửa phiên bản PO Approved/Sent.");
        var received = await _db.PurPoLines.AnyAsync(
            x => x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted && x.ReceivedQty > 0, ct);
        if (received) throw new AppException("PO đã nhận hàng — không revise.");
        po.Version += 1;
        po.Status = "Draft";
        po.ApprovedBy = null;
        po.ApprovedAt = null;
        po.SentAt = null;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> ClosePoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status is not ("Sent" or "Approved"))
            throw new AppException("Chỉ đóng PO Sent/Approved.");
        po.Status = "Closed";
        po.ClosedAt = DateTimeOffset.UtcNow;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> CancelPoAsync(
        Guid tenantId, Guid userId, Guid poId, PurPoCancelRequest req, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status is "Closed" or "Cancelled")
            throw new AppException("PO đã đóng/hủy.");
        var received = await _db.PurPoLines.AnyAsync(
            x => x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted && x.ReceivedQty > 0, ct);
        if (received) throw new AppException("PO đã nhận hàng — không hủy (đóng thay thế).");
        var reason = (req.Reason ?? "").Trim();
        if (reason.Length is < 1 or > 500) throw new AppException("Lý do hủy 1–500 ký tự.");
        po.Status = "Cancelled";
        po.CancelReason = reason;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    public async Task<PurPurchaseOrderDto> PrintPoAsync(
        Guid tenantId, Guid userId, Guid poId, CancellationToken ct = default)
    {
        var po = await RequirePo(tenantId, poId, ct);
        if (po.Status is "Draft" or "Cancelled")
            throw new AppException("Không in PO Draft/Cancelled.");
        po.PrintedAt = DateTimeOffset.UtcNow;
        po.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPosAsync(tenantId, [po], ct))[0];
    }

    private async Task<PurPurchaseRequestDto> DecidePrAsync(
        Guid tenantId, Guid userId, Guid prId, string status, string? note, CancellationToken ct)
    {
        var pr = await RequirePr(tenantId, prId, ct);
        if (pr.Status != "Submitted") throw new AppException("Chỉ quyết định PR đang Submitted.");
        pr.Status = status;
        pr.DecisionNote = NullIfEmpty(note);
        pr.DecidedBy = userId;
        pr.DecidedAt = DateTimeOffset.UtcNow;
        pr.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPrsAsync(tenantId, [pr], ct))[0];
    }

    private async Task RecalcPoTotalAsync(Guid tenantId, Guid poId, CancellationToken ct)
    {
        var total = await _db.PurPoLines
            .Where(x => x.TenantId == tenantId && x.PoId == poId && !x.IsDeleted)
            .SumAsync(x => x.Qty * x.UnitPrice, ct);
        var po = await RequirePo(tenantId, poId, ct);
        po.TotalAmount = Math.Round(total, 2);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<PurVendorDto>> MapVendorsAsync(
        Guid tenantId, List<PurVendor> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<PurVendorDto>();
        var ids = list.Select(x => x.Id).ToList();
        var cCounts = await _db.PurVendorContacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.VendorId) && !x.IsDeleted)
            .GroupBy(x => x.VendorId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var pCounts = await _db.PurVendorProducts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.VendorId) && !x.IsDeleted)
            .GroupBy(x => x.VendorId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return list.Select(v => new PurVendorDto(
            v.Id, v.Code, v.Name, v.TaxCode, v.Phone, v.Email, v.Address, v.PaymentTerms, v.Status,
            cCounts.GetValueOrDefault(v.Id), pCounts.GetValueOrDefault(v.Id))).ToList();
    }

    private async Task<IReadOnlyList<PurPurchaseRequestDto>> MapPrsAsync(
        Guid tenantId, List<PurPurchaseRequest> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<PurPurchaseRequestDto>();
        var ids = list.Select(x => x.Id).ToList();
        var lineCounts = await _db.PurPrLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PrId) && !x.IsDeleted)
            .GroupBy(x => x.PrId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var userIds = list.Select(x => x.RequestedBy)
            .Concat(list.Where(x => x.DecidedBy.HasValue).Select(x => x.DecidedBy!.Value))
            .Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return list.Select(p => new PurPurchaseRequestDto(
            p.Id, p.Code, p.RequestingUnit, p.Note, p.Status, p.DecisionNote,
            p.RequestedBy, names.GetValueOrDefault(p.RequestedBy),
            p.DecidedBy, p.DecidedBy is Guid d ? names.GetValueOrDefault(d) : null,
            p.DecidedAt, lineCounts.GetValueOrDefault(p.Id))).ToList();
    }

    private async Task<IReadOnlyList<PurPurchaseOrderDto>> MapPosAsync(
        Guid tenantId, List<PurPurchaseOrder> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<PurPurchaseOrderDto>();
        var ids = list.Select(x => x.Id).ToList();
        var lineAgg = await _db.PurPoLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PoId) && !x.IsDeleted)
            .GroupBy(x => x.PoId)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                Ordered = g.Sum(x => x.Qty),
                Received = g.Sum(x => x.ReceivedQty)
            }).ToDictionaryAsync(x => x.Key, ct);
        var vendorIds = list.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking()
            .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var prIds = list.Where(x => x.SourcePrId.HasValue).Select(x => x.SourcePrId!.Value).Distinct().ToList();
        var prs = prIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.PurPurchaseRequests.AsNoTracking()
                .Where(x => x.TenantId == tenantId && prIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var userIds = list.Select(x => x.CreatedByUserId)
            .Concat(list.Where(x => x.ApprovedBy.HasValue).Select(x => x.ApprovedBy!.Value))
            .Distinct().ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return list.Select(p =>
        {
            lineAgg.TryGetValue(p.Id, out var agg);
            var pct = agg is null || agg.Ordered <= 0 ? 0
                : Math.Round(100m * agg.Received / agg.Ordered, 1);
            return new PurPurchaseOrderDto(
                p.Id, p.Code, p.VendorId, vendors.GetValueOrDefault(p.VendorId),
                p.SourcePrId, p.SourcePrId is Guid sid ? prs.GetValueOrDefault(sid) : null,
                p.Status, p.Version, p.TotalAmount, p.Currency, p.Note,
                p.CreatedByUserId, names.GetValueOrDefault(p.CreatedByUserId),
                p.ApprovedBy, p.ApprovedBy is Guid a ? names.GetValueOrDefault(a) : null,
                p.ApprovedAt, p.SentAt, p.PrintedAt, p.ClosedAt, p.CancelReason,
                agg?.Count ?? 0, pct);
        }).ToList();
    }

    private async Task<PurVendor> RequireVendor(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PurVendors.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("NCC không tồn tại.", 404);

    private async Task<PurPurchaseRequest> RequirePr(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PurPurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("PR không tồn tại.", 404);

    private async Task<PurPurchaseOrder> RequirePo(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PurPurchaseOrders.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("PO không tồn tại.", 404);

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

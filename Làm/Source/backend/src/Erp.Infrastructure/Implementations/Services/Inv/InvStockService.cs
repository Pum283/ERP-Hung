using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Domain.Base;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Inv;

public sealed class InvStockService : IInvStockService
{
    private static readonly HashSet<string> DocTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Receipt", "Issue" };
    private static readonly HashSet<string> ReceiptSources =
        new(StringComparer.OrdinalIgnoreCase) { "Purchase", "Adjustment", "TransferIn", "Production", "Return" };
    private static readonly HashSet<string> IssueSources =
        new(StringComparer.OrdinalIgnoreCase) { "Internal", "Adjustment", "TransferOut", "Sales", "Production" };

    private readonly AppDbContext _db;
    private readonly IFinRevenueService _rev;
    public InvStockService(AppDbContext db, IFinRevenueService rev)
    {
        _db = db;
        _rev = rev;
    }

    public async Task<IReadOnlyList<InvBalanceDto>> ListBalancesAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var q = _db.InvStockBalances.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (warehouseId is Guid wid) q = q.Where(x => x.WarehouseId == wid);
        var list = await q.OrderBy(x => x.WarehouseId).ThenBy(x => x.SkuId).Take(500).ToListAsync(ct);
        var wids = list.Select(x => x.WarehouseId).Distinct().ToList();
        var sids = list.Select(x => x.SkuId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x, ct);
        return list.Select(b =>
        {
            skus.TryGetValue(b.SkuId, out var sku);
            return new InvBalanceDto(
                b.Id, b.WarehouseId, whs.GetValueOrDefault(b.WarehouseId),
                b.SkuId, sku?.Code ?? "", sku?.Name ?? "",
                string.IsNullOrEmpty(b.LotCode) ? null : b.LotCode, b.ExpiryDate,
                b.QtyOnHand, b.QtyReserved, b.QtyInTransit,
                b.QtyOnHand - b.QtyReserved);
        }).ToList();
    }

    public async Task<IReadOnlyList<InvStockDocDto>> ListDocsAsync(
        Guid tenantId, string? docType = null, CancellationToken ct = default)
    {
        var q = _db.InvStockDocs.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(docType)) q = q.Where(x => x.DocType == docType);
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapDocsAsync(tenantId, list, ct);
    }

    public async Task<InvStockDocDetailDto> GetDocDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var doc = await RequireAsync(_db.InvStockDocs, tenantId, id, "phiếu kho", ct);
        var lines = await _db.InvStockDocLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DocId == id && !x.IsDeleted).ToListAsync(ct);
        return new InvStockDocDetailDto(
            (await MapDocsAsync(tenantId, [doc], ct))[0],
            lines.Select(MapDocLine).ToList());
    }

    public async Task<InvStockDocDto> CreateDocAsync(
        Guid tenantId, Guid userId, InvStockDocCreateRequest req, CancellationToken ct = default)
    {
        var docType = DocTypes.FirstOrDefault(x => x.Equals(req.DocType, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("DocType: Receipt | Issue.");
        var sources = docType == "Receipt" ? ReceiptSources : IssueSources;
        var source = sources.FirstOrDefault(x => x.Equals(req.SourceType, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException($"SourceType không hợp lệ cho {docType}.");
        await RequireAsync(_db.InvWarehouses, tenantId, req.WarehouseId, "kho", ct);

        var prefix = docType == "Receipt" ? "IN" : "OUT";
        var doc = new InvStockDoc
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, prefix, _db.InvStockDocs, ct),
            DocType = docType, SourceType = source, WarehouseId = req.WarehouseId,
            Status = "Draft", Note = Opt(req.Note, 1000), CreatedBy = userId
        };
        _db.InvStockDocs.Add(doc);
        await _db.SaveChangesAsync(ct);
        return (await MapDocsAsync(tenantId, [doc], ct))[0];
    }

    public async Task<InvStockDocLineDto> UpsertDocLineAsync(
        Guid tenantId, Guid userId, Guid docId, InvStockDocLineRequest req, CancellationToken ct = default)
    {
        var doc = await RequireAsync(_db.InvStockDocs, tenantId, docId, "phiếu kho", ct);
        if (doc.Status != "Draft") throw new AppException("Chỉ sửa phiếu Draft.");
        if (req.Qty <= 0) throw new AppException("SL > 0.");
        var sku = await RequireAsync(_db.InvSkus, tenantId, req.SkuId, "SKU", ct);
        if (sku.Status != "Active") throw new AppException("SKU không Active.");
        // Issue: cho phép bỏ LotCode → FEFO/FIFO khi Post (UC_INV_029).
        if (doc.DocType != "Issue" && sku.TrackLot && string.IsNullOrWhiteSpace(req.LotCode))
            throw new AppException("SKU theo dõi lô — bắt buộc LotCode.");
        if (doc.DocType != "Issue" && sku.TrackExpiry && req.ExpiryDate is null)
            throw new AppException("SKU theo dõi HSD — bắt buộc ExpiryDate.");

        InvStockDocLine line;
        if (req.Id is Guid id)
        {
            line = await RequireAsync(_db.InvStockDocLines, tenantId, id, "dòng phiếu", ct);
            if (line.DocId != docId) throw new AppException("Dòng không thuộc phiếu.");
            line.UpdatedBy = userId;
        }
        else
        {
            line = new InvStockDocLine { TenantId = tenantId, DocId = docId, CreatedBy = userId };
            _db.InvStockDocLines.Add(line);
        }
        line.SkuId = sku.Id;
        line.SkuCode = sku.Code;
        line.SkuName = sku.Name;
        line.Qty = req.Qty;
        line.LotCode = string.IsNullOrWhiteSpace(req.LotCode) ? null : req.LotCode.Trim().ToUpperInvariant();
        line.ExpiryDate = req.ExpiryDate;
        line.UnitCost = req.UnitCost ?? sku.StandardCost;
        await _db.SaveChangesAsync(ct);
        return MapDocLine(line);
    }

    public async Task<InvStockDocDto> PostDocAsync(
        Guid tenantId, Guid userId, Guid docId, CancellationToken ct = default)
    {
        var doc = await RequireAsync(_db.InvStockDocs, tenantId, docId, "phiếu kho", ct);
        if (doc.Status != "Draft") throw new AppException("Phiếu đã post.");
        var lines = await _db.InvStockDocLines
            .Where(x => x.TenantId == tenantId && x.DocId == docId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Phiếu trống.");
        var wh = await RequireAsync(_db.InvWarehouses, tenantId, doc.WarehouseId, "kho", ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (doc.DocType == "Receipt")
        {
            foreach (var line in lines)
                await ApplyBalanceAsync(tenantId, userId, wh, line.SkuId, line.LotCode, line.ExpiryDate,
                    line.Qty, 0, 0, ct);
        }
        else
        {
            foreach (var line in lines.ToList())
            {
                var picks = await ResolveIssuePicksAsync(tenantId, wh, line, today, ct);
                if (picks.Count == 0)
                    throw new AppException($"Không đủ tồn khả dụng cho {line.SkuCode} (UC_INV_042).");

                // FEFO có thể tách nhiều lô → thay dòng gốc bằng các dòng pick.
                if (picks.Count > 1
                    || !string.Equals(NormLot(line.LotCode), NormLot(picks[0].LotCode), StringComparison.Ordinal)
                    || line.ExpiryDate != picks[0].ExpiryDate)
                {
                    line.IsDeleted = true;
                    line.DeletedAt = DateTimeOffset.UtcNow;
                    line.UpdatedBy = userId;
                    foreach (var p in picks)
                    {
                        _db.InvStockDocLines.Add(new InvStockDocLine
                        {
                            TenantId = tenantId, DocId = doc.Id, SkuId = line.SkuId,
                            SkuCode = line.SkuCode, SkuName = line.SkuName,
                            Qty = p.Qty, LotCode = p.LotCode, ExpiryDate = p.ExpiryDate,
                            UnitCost = line.UnitCost, CreatedBy = userId, UpdatedBy = userId,
                        });
                    }
                    await _db.SaveChangesAsync(ct);
                }
                else
                {
                    line.LotCode = picks[0].LotCode;
                    line.ExpiryDate = picks[0].ExpiryDate;
                    line.UpdatedBy = userId;
                }

                foreach (var p in picks)
                {
                    if (p.ExpiryDate is DateOnly exp && exp < today)
                        throw new AppException($"Chặn xuất lô quá HSD {p.LotCode} ({exp:yyyy-MM-dd}) — UC_INV_045.");
                    var consumeReserved = await TryConsumeReservationAsync(
                        tenantId, userId, wh, doc, line.SkuId, p.LotCode, p.Qty, ct);
                    await ApplyBalanceAsync(tenantId, userId, wh, line.SkuId, p.LotCode, p.ExpiryDate,
                        -p.Qty, consumeReserved ? -p.Qty : 0, 0, ct, checkAvailable: !consumeReserved);
                }
            }
        }

        doc.Status = "Posted";
        doc.PostedAt = DateTimeOffset.UtcNow;
        doc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        if (doc.DocType == "Issue" && doc.SourceType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
        {
            try { await _rev.RecognizeCogsAsync(tenantId, userId, doc.Id, null, ct); }
            catch (AppException) { /* FIN chưa sẵn sàng — bỏ qua */ }
        }
        return (await MapDocsAsync(tenantId, [doc], ct))[0];
    }

    public async Task<InvStockDocDto> PostPurchaseReceiptFromGrnAsync(
        Guid tenantId, Guid userId, Guid grnId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var existing = await _db.InvStockDocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted
                                      && x.RefModule == "PUR" && x.RefId == grnId, ct);
        if (existing is not null)
            return (await MapDocsAsync(tenantId, [await RequireAsync(_db.InvStockDocs, tenantId, existing.Id, "phiếu", ct)], ct))[0];

        var grn = await _db.PurGoodsReceipts.FirstOrDefaultAsync(
            x => x.Id == grnId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy GRN.", 404);
        if (grn.Status != "Posted") throw new AppException("GRN chưa Posted.");

        var whId = warehouseId ?? await _db.InvWarehouses.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "Active" && !x.IsDeleted)
            .OrderBy(x => x.Code).Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (whId == Guid.Empty) throw new AppException("Chưa có kho Active để nhận hàng.");

        var grnLines = await _db.PurGrnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.GrnId == grnId && !x.IsDeleted && x.AcceptedQty > 0)
            .ToListAsync(ct);
        if (grnLines.Count == 0) throw new AppException("GRN không có SL Accepted.");

        var doc = new InvStockDoc
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "IN", _db.InvStockDocs, ct),
            DocType = "Receipt", SourceType = "Purchase", WarehouseId = whId,
            Status = "Draft", RefModule = "PUR", RefId = grn.Id, RefCode = grn.Code,
            Note = $"Từ GRN {grn.Code}", CreatedBy = userId
        };
        _db.InvStockDocs.Add(doc);
        await _db.SaveChangesAsync(ct);

        foreach (var gl in grnLines)
        {
            var sku = await _db.InvSkus.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted
                     && x.Code == gl.ProductCode, ct);
            if (sku is null)
            {
                // auto-create minimal SKU from GRN product code
                var uomId = await _db.InvUnitsOfMeasure.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .OrderBy(x => x.Code).Select(x => x.Id).FirstOrDefaultAsync(ct);
                if (uomId == Guid.Empty) throw new AppException("Chưa có ĐVT — tạo ĐVT Cap-1 trước.");
                sku = new InvSku
                {
                    TenantId = tenantId, Code = gl.ProductCode, Name = gl.ProductName,
                    BaseUnitId = uomId, Status = "Active", StandardCost = gl.UnitPrice,
                    CreatedBy = userId
                };
                _db.InvSkus.Add(sku);
                await _db.SaveChangesAsync(ct);
            }
            _db.InvStockDocLines.Add(new InvStockDocLine
            {
                TenantId = tenantId, DocId = doc.Id, SkuId = sku.Id,
                SkuCode = sku.Code, SkuName = sku.Name,
                Qty = gl.AcceptedQty,
                LotCode = sku.TrackLot ? $"LOT-{grn.Code}" : null,
                ExpiryDate = sku.TrackExpiry ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)) : null,
                UnitCost = gl.UnitPrice, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return await PostDocAsync(tenantId, userId, doc.Id, ct);
    }

    public async Task<InvStockDocDto> PostReceiptFromLogReturnAsync(
        Guid tenantId, Guid userId, Guid returnNoteId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var existing = await _db.InvStockDocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted
                                      && x.RefModule == "LOG" && x.RefId == returnNoteId, ct);
        if (existing is not null)
            return (await MapDocsAsync(tenantId, [await RequireAsync(_db.InvStockDocs, tenantId, existing.Id, "phiếu", ct)], ct))[0];

        var note = await _db.LogReturnNotes.FirstOrDefaultAsync(
            x => x.Id == returnNoteId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy phiếu hoàn.", 404);
        if (note.Status != "Counted") throw new AppException("Phiếu hoàn chưa Counted.");

        var whId = warehouseId ?? note.WarehouseId;
        await RequireAsync(_db.InvWarehouses, tenantId, whId, "kho", ct);

        var retLines = await _db.LogReturnLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReturnNoteId == returnNoteId && !x.IsDeleted && x.QtyAccepted > 0)
            .ToListAsync(ct);
        if (retLines.Count == 0) throw new AppException("Phiếu hoàn không có SL Accepted.");

        var doc = new InvStockDoc
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "IN", _db.InvStockDocs, ct),
            DocType = "Receipt", SourceType = "Return", WarehouseId = whId,
            Status = "Draft", RefModule = "LOG", RefId = note.Id, RefCode = note.Code,
            Note = $"Hoàn LOG {note.Code}", CreatedBy = userId
        };
        _db.InvStockDocs.Add(doc);
        await _db.SaveChangesAsync(ct);

        foreach (var rl in retLines)
        {
            var sku = await _db.InvSkus.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Code == rl.ProductCode, ct);
            if (sku is null)
            {
                var uomId = await _db.InvUnitsOfMeasure.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .OrderBy(x => x.Code).Select(x => x.Id).FirstOrDefaultAsync(ct);
                if (uomId == Guid.Empty) throw new AppException("Chưa có ĐVT — tạo ĐVT Cap-1 trước.");
                sku = new InvSku
                {
                    TenantId = tenantId, Code = rl.ProductCode, Name = rl.ProductName,
                    BaseUnitId = uomId, Status = "Active", CreatedBy = userId
                };
                _db.InvSkus.Add(sku);
                await _db.SaveChangesAsync(ct);
            }
            _db.InvStockDocLines.Add(new InvStockDocLine
            {
                TenantId = tenantId, DocId = doc.Id, SkuId = sku.Id,
                SkuCode = sku.Code, SkuName = sku.Name,
                Qty = rl.QtyAccepted,
                LotCode = sku.TrackLot ? $"RET-{note.Code}" : null,
                ExpiryDate = sku.TrackExpiry ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)) : null,
                UnitCost = 0, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return await PostDocAsync(tenantId, userId, doc.Id, ct);
    }

    public async Task<IReadOnlyList<InvTransferDto>> ListTransfersAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default)
    {
        var q = _db.InvTransfers.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapTransfersAsync(tenantId, list, ct);
    }

    public async Task<InvTransferDetailDto> GetTransferDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var tr = await RequireAsync(_db.InvTransfers, tenantId, id, "phiếu chuyển", ct);
        var lines = await _db.InvTransferLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TransferId == id && !x.IsDeleted).ToListAsync(ct);
        return new InvTransferDetailDto(
            (await MapTransfersAsync(tenantId, [tr], ct))[0],
            lines.Select(MapTrLine).ToList());
    }

    public async Task<InvTransferDto> CreateTransferAsync(
        Guid tenantId, Guid userId, InvTransferCreateRequest req, CancellationToken ct = default)
    {
        if (req.FromWarehouseId == req.ToWarehouseId)
            throw new AppException("Kho gửi và nhận phải khác nhau.");
        await RequireAsync(_db.InvWarehouses, tenantId, req.FromWarehouseId, "kho gửi", ct);
        await RequireAsync(_db.InvWarehouses, tenantId, req.ToWarehouseId, "kho nhận", ct);
        var tr = new InvTransfer
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "TR", _db.InvTransfers, ct),
            FromWarehouseId = req.FromWarehouseId, ToWarehouseId = req.ToWarehouseId,
            Status = "Draft", Note = Opt(req.Note, 1000), CreatedBy = userId
        };
        _db.InvTransfers.Add(tr);
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [tr], ct))[0];
    }

    public async Task<InvTransferLineDto> UpsertTransferLineAsync(
        Guid tenantId, Guid userId, Guid transferId, InvTransferLineRequest req, CancellationToken ct = default)
    {
        var tr = await RequireAsync(_db.InvTransfers, tenantId, transferId, "phiếu chuyển", ct);
        if (tr.Status != "Draft") throw new AppException("Chỉ sửa Draft.");
        if (req.Qty <= 0) throw new AppException("SL > 0.");
        var sku = await RequireAsync(_db.InvSkus, tenantId, req.SkuId, "SKU", ct);

        InvTransferLine line;
        if (req.Id is Guid id)
        {
            line = await RequireAsync(_db.InvTransferLines, tenantId, id, "dòng chuyển", ct);
            if (line.TransferId != transferId) throw new AppException("Dòng không thuộc phiếu.");
            line.UpdatedBy = userId;
        }
        else
        {
            line = new InvTransferLine { TenantId = tenantId, TransferId = transferId, CreatedBy = userId };
            _db.InvTransferLines.Add(line);
        }
        line.SkuId = sku.Id;
        line.SkuCode = sku.Code;
        line.SkuName = sku.Name;
        line.Qty = req.Qty;
        line.LotCode = string.IsNullOrWhiteSpace(req.LotCode) ? null : req.LotCode.Trim().ToUpperInvariant();
        line.ExpiryDate = req.ExpiryDate;
        await _db.SaveChangesAsync(ct);
        return MapTrLine(line);
    }

    public async Task<InvTransferDto> ShipTransferAsync(
        Guid tenantId, Guid userId, Guid transferId, CancellationToken ct = default)
    {
        var tr = await RequireAsync(_db.InvTransfers, tenantId, transferId, "phiếu chuyển", ct);
        if (tr.Status != "Draft") throw new AppException("Chỉ ship Draft.");
        var lines = await _db.InvTransferLines
            .Where(x => x.TenantId == tenantId && x.TransferId == transferId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Phiếu chuyển trống.");
        var from = await RequireAsync(_db.InvWarehouses, tenantId, tr.FromWarehouseId, "kho gửi", ct);

        foreach (var line in lines)
        {
            await ApplyBalanceAsync(tenantId, userId, from, line.SkuId, line.LotCode, line.ExpiryDate,
                -line.Qty, 0, line.Qty, ct);
        }
        tr.Status = "InTransit";
        tr.ShippedAt = DateTimeOffset.UtcNow;
        tr.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [tr], ct))[0];
    }

    public async Task<InvTransferDto> ReceiveTransferAsync(
        Guid tenantId, Guid userId, Guid transferId, CancellationToken ct = default)
    {
        var tr = await RequireAsync(_db.InvTransfers, tenantId, transferId, "phiếu chuyển", ct);
        if (tr.Status != "InTransit") throw new AppException("Chỉ nhận khi InTransit.");
        var lines = await _db.InvTransferLines
            .Where(x => x.TenantId == tenantId && x.TransferId == transferId && !x.IsDeleted).ToListAsync(ct);
        var from = await RequireAsync(_db.InvWarehouses, tenantId, tr.FromWarehouseId, "kho gửi", ct);
        var to = await RequireAsync(_db.InvWarehouses, tenantId, tr.ToWarehouseId, "kho nhận", ct);

        foreach (var line in lines)
        {
            await ApplyBalanceAsync(tenantId, userId, from, line.SkuId, line.LotCode, line.ExpiryDate,
                0, 0, -line.Qty, ct);
            await ApplyBalanceAsync(tenantId, userId, to, line.SkuId, line.LotCode, line.ExpiryDate,
                line.Qty, 0, 0, ct);
        }
        tr.Status = "Completed";
        tr.ReceivedAt = DateTimeOffset.UtcNow;
        tr.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [tr], ct))[0];
    }

    public async Task<IReadOnlyList<InvStocktakeDto>> ListStocktakesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvStocktakes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        return await MapStocktakesAsync(tenantId, list, ct);
    }

    public async Task<InvStocktakeDetailDto> GetStocktakeDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var st = await RequireAsync(_db.InvStocktakes, tenantId, id, "phiếu kiểm kê", ct);
        var lines = await _db.InvStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StocktakeId == id && !x.IsDeleted).ToListAsync(ct);
        return new InvStocktakeDetailDto(
            (await MapStocktakesAsync(tenantId, [st], ct))[0],
            lines.Select(MapStLine).ToList());
    }

    public async Task<InvStocktakeDto> CreateStocktakeAsync(
        Guid tenantId, Guid userId, InvStocktakeCreateRequest req, CancellationToken ct = default)
    {
        await RequireAsync(_db.InvWarehouses, tenantId, req.WarehouseId, "kho", ct);
        var st = new InvStocktake
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "ST", _db.InvStocktakes, ct),
            WarehouseId = req.WarehouseId, Status = "Counting",
            Note = Opt(req.Note, 1000), CreatedBy = userId
        };
        _db.InvStocktakes.Add(st);
        await _db.SaveChangesAsync(ct);

        var bals = await _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WarehouseId == req.WarehouseId && !x.IsDeleted
                        && (x.QtyOnHand != 0 || x.QtyInTransit != 0))
            .ToListAsync(ct);
        var skuIds = bals.Select(x => x.SkuId).Distinct().ToList();
        var skus = await _db.InvSkus.AsNoTracking().Where(x => skuIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        foreach (var b in bals)
        {
            skus.TryGetValue(b.SkuId, out var sku);
            _db.InvStocktakeLines.Add(new InvStocktakeLine
            {
                TenantId = tenantId, StocktakeId = st.Id, SkuId = b.SkuId,
                SkuCode = sku?.Code ?? "", SkuName = sku?.Name ?? "",
                LotCode = b.LotCode, SystemQty = b.QtyOnHand, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapStocktakesAsync(tenantId, [st], ct))[0];
    }

    public async Task<InvStocktakeLineDto> CountStocktakeLineAsync(
        Guid tenantId, Guid userId, Guid stocktakeId, InvStocktakeCountRequest req, CancellationToken ct = default)
    {
        var st = await RequireAsync(_db.InvStocktakes, tenantId, stocktakeId, "phiếu kiểm kê", ct);
        if (st.Status is not ("Draft" or "Counting")) throw new AppException("Không còn nhập đếm.");
        var line = await RequireAsync(_db.InvStocktakeLines, tenantId, req.LineId, "dòng KK", ct);
        if (line.StocktakeId != stocktakeId) throw new AppException("Dòng không thuộc phiếu.");
        if (req.CountedQty < 0) throw new AppException("SL đếm ≥ 0.");
        line.CountedQty = req.CountedQty;
        line.VarianceQty = req.CountedQty - line.SystemQty;
        line.UpdatedBy = userId;
        st.Status = "Counting";
        st.CountedAt = DateTimeOffset.UtcNow;
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapStLine(line);
    }

    public async Task<InvStocktakeDto> ReviewStocktakeAsync(
        Guid tenantId, Guid userId, Guid stocktakeId, CancellationToken ct = default)
    {
        var st = await RequireAsync(_db.InvStocktakes, tenantId, stocktakeId, "phiếu kiểm kê", ct);
        if (st.Status != "Counting") throw new AppException("Cần đang Counting.");
        var uncounted = await _db.InvStocktakeLines.CountAsync(
            x => x.TenantId == tenantId && x.StocktakeId == stocktakeId && !x.IsDeleted && x.CountedQty == null, ct);
        if (uncounted > 0) throw new AppException($"Còn {uncounted} dòng chưa đếm.");
        st.Status = "Reviewed";
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStocktakesAsync(tenantId, [st], ct))[0];
    }

    public async Task<InvStocktakeDto> PostStocktakeAsync(
        Guid tenantId, Guid userId, Guid stocktakeId, CancellationToken ct = default)
    {
        var st = await RequireAsync(_db.InvStocktakes, tenantId, stocktakeId, "phiếu kiểm kê", ct);
        if (st.Status != "Reviewed") throw new AppException("Cần duyệt (Reviewed) trước khi post.");
        var lines = await _db.InvStocktakeLines
            .Where(x => x.TenantId == tenantId && x.StocktakeId == stocktakeId && !x.IsDeleted
                        && x.VarianceQty != 0).ToListAsync(ct);
        var wh = await RequireAsync(_db.InvWarehouses, tenantId, st.WarehouseId, "kho", ct);

        foreach (var line in lines)
        {
            var docType = line.VarianceQty > 0 ? "Receipt" : "Issue";
            var doc = new InvStockDoc
            {
                TenantId = tenantId,
                Code = await NextCodeAsync(tenantId, docType == "Receipt" ? "IN" : "OUT", _db.InvStockDocs, ct),
                DocType = docType, SourceType = "Adjustment", WarehouseId = st.WarehouseId,
                Status = "Draft", RefModule = "INV", RefId = st.Id, RefCode = st.Code,
                Note = $"Điều chỉnh KK {st.Code}", CreatedBy = userId
            };
            _db.InvStockDocs.Add(doc);
            await _db.SaveChangesAsync(ct);
            _db.InvStockDocLines.Add(new InvStockDocLine
            {
                TenantId = tenantId, DocId = doc.Id, SkuId = line.SkuId,
                SkuCode = line.SkuCode, SkuName = line.SkuName,
                Qty = Math.Abs(line.VarianceQty), LotCode = line.LotCode,
                CreatedBy = userId
            });
            await _db.SaveChangesAsync(ct);
            await PostDocAsync(tenantId, userId, doc.Id, ct);
        }

        st.Status = "Posted";
        st.PostedAt = DateTimeOffset.UtcNow;
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStocktakesAsync(tenantId, [st], ct))[0];
    }

    public async Task<IReadOnlyList<InvLotPickDto>> SuggestLotsAsync(
        Guid tenantId, InvSuggestLotsRequest req, CancellationToken ct = default)
    {
        if (req.Qty <= 0) throw new AppException("SL > 0.");
        var wh = await RequireAsync(_db.InvWarehouses, tenantId, req.WarehouseId, "kho", ct);
        var sku = await RequireAsync(_db.InvSkus, tenantId, req.SkuId, "SKU", ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var picks = await BuildFefoPicksAsync(tenantId, wh, sku.Id, sku.Code, req.Qty, null, today, ct);
        return picks.Select(p => new InvLotPickDto(
            sku.Id, sku.Code, p.LotCode, p.ExpiryDate, p.Available, p.Qty)).ToList();
    }

    public async Task<IReadOnlyList<InvReservationDto>> ListReservationsAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default)
    {
        var q = _db.InvStockReservations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapReservationsAsync(tenantId, list, ct);
    }

    public async Task<InvReservationDetailDto> GetReservationDetailAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var r = await RequireAsync(_db.InvStockReservations, tenantId, id, "giữ hàng", ct);
        var header = (await MapReservationsAsync(tenantId, [r], ct))[0];
        var lines = await _db.InvStockReservationLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReservationId == id && !x.IsDeleted).ToListAsync(ct);
        return new InvReservationDetailDto(header, lines.Select(MapResLine).ToList());
    }

    public async Task<InvReservationDetailDto> CreateReservationAsync(
        Guid tenantId, Guid userId, InvReservationCreateRequest req, CancellationToken ct = default)
    {
        if (req.Lines is null || req.Lines.Count == 0) throw new AppException("Cần ít nhất một dòng giữ hàng.");
        _ = await RequireAsync(_db.InvWarehouses, tenantId, req.WarehouseId, "kho", ct);
        var entity = new InvStockReservation
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "RV", _db.InvStockReservations, ct),
            WarehouseId = req.WarehouseId,
            Status = "Draft",
            RefModule = Opt(req.RefModule, 40),
            RefId = req.RefId,
            RefCode = Opt(req.RefCode, 40)?.ToUpperInvariant(),
            Note = Opt(req.Note, 1000),
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        _db.InvStockReservations.Add(entity);
        foreach (var line in req.Lines)
        {
            if (line.Qty <= 0) throw new AppException("SL giữ hàng > 0.");
            var sku = await RequireAsync(_db.InvSkus, tenantId, line.SkuId, "SKU", ct);
            _db.InvStockReservationLines.Add(new InvStockReservationLine
            {
                TenantId = tenantId, ReservationId = entity.Id, SkuId = sku.Id,
                SkuCode = sku.Code, SkuName = sku.Name, Qty = line.Qty,
                LotCode = string.IsNullOrWhiteSpace(line.LotCode) ? null : NormLot(line.LotCode),
                ExpiryDate = line.ExpiryDate, CreatedBy = userId, UpdatedBy = userId,
            });
        }
        await _db.SaveChangesAsync(ct);
        if (req.Activate)
            return await ActivateReservationAsync(tenantId, userId, entity.Id, ct);
        return await GetReservationDetailAsync(tenantId, entity.Id, ct);
    }

    public async Task<InvReservationDetailDto> ActivateReservationAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var r = await RequireAsync(_db.InvStockReservations, tenantId, id, "giữ hàng", ct);
        if (r.Status != "Draft") throw new AppException("Chỉ Activate phiếu Draft.");
        var wh = await RequireAsync(_db.InvWarehouses, tenantId, r.WarehouseId, "kho", ct);
        var lines = await _db.InvStockReservationLines
            .Where(x => x.TenantId == tenantId && x.ReservationId == id && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Phiếu giữ hàng trống.");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var line in lines)
        {
            if (line.ExpiryDate is DateOnly exp && exp < today)
                throw new AppException($"Không giữ lô quá HSD {line.LotCode}.");
            await ApplyBalanceAsync(tenantId, userId, wh, line.SkuId, line.LotCode, line.ExpiryDate,
                0, line.Qty, 0, ct, checkAvailable: true);
        }
        r.Status = "Active";
        r.ActivatedAt = DateTimeOffset.UtcNow;
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetReservationDetailAsync(tenantId, id, ct);
    }

    public async Task<InvReservationDetailDto> ReleaseReservationAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var r = await RequireAsync(_db.InvStockReservations, tenantId, id, "giữ hàng", ct);
        if (r.Status != "Active") throw new AppException("Chỉ Release phiếu Active.");
        var wh = await RequireAsync(_db.InvWarehouses, tenantId, r.WarehouseId, "kho", ct);
        var lines = await _db.InvStockReservationLines
            .Where(x => x.TenantId == tenantId && x.ReservationId == id && !x.IsDeleted).ToListAsync(ct);
        foreach (var line in lines)
            await ApplyBalanceAsync(tenantId, userId, wh, line.SkuId, line.LotCode, line.ExpiryDate,
                0, -line.Qty, 0, ct, checkAvailable: false);
        r.Status = "Released";
        r.ReleasedAt = DateTimeOffset.UtcNow;
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return await GetReservationDetailAsync(tenantId, id, ct);
    }

    public async Task<IReadOnlyList<InvAtpAlertRowDto>> AtpAlertsAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default)
    {
        var q = _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.QtyOnHand - x.QtyReserved < 0 || x.QtyReserved > x.QtyOnHand + 0.0001m));
        if (warehouseId is Guid wid) q = q.Where(x => x.WarehouseId == wid);
        var bals = await q.Take(300).ToListAsync(ct);
        // Also surface zero-available with reserved demand as Insufficient.
        var shortAvail = await _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.QtyReserved > 0
                        && x.QtyOnHand - x.QtyReserved <= 0)
            .Take(300).ToListAsync(ct);
        bals = bals.Concat(shortAvail).GroupBy(x => x.Id).Select(g => g.First()).ToList();
        if (bals.Count == 0)
        {
            // Soft: active reservation lines that exceed available at activate time already blocked;
            // expose balances with available == 0 and on-hand > 0 reserved-heavy as info via reserved > 0 & available == 0.
            bals = await _db.InvStockBalances.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted
                            && x.QtyReserved > 0 && x.QtyOnHand - x.QtyReserved <= 0)
                .Take(200).ToListAsync(ct);
        }
        if (warehouseId is Guid w2)
            bals = bals.Where(x => x.WarehouseId == w2).ToList();
        if (bals.Count == 0) return Array.Empty<InvAtpAlertRowDto>();
        var wids = bals.Select(x => x.WarehouseId).Distinct().ToList();
        var sids = bals.Select(x => x.SkuId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var skus = await _db.InvSkus.AsNoTracking().Where(x => sids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return bals.Select(b =>
        {
            skus.TryGetValue(b.SkuId, out var sku);
            var avail = b.QtyOnHand - b.QtyReserved;
            return new InvAtpAlertRowDto(
                b.WarehouseId, whs.GetValueOrDefault(b.WarehouseId),
                b.SkuId, sku?.Code ?? "", sku?.Name ?? "",
                string.IsNullOrEmpty(b.LotCode) ? null : b.LotCode, b.ExpiryDate,
                b.QtyOnHand, b.QtyReserved, avail,
                avail < 0 ? "NegativeAtp" : "Insufficient");
        }).ToList();
    }

    private async Task<List<(string? LotCode, DateOnly? ExpiryDate, decimal Qty, decimal Available)>> ResolveIssuePicksAsync(
        Guid tenantId, InvWarehouse wh, InvStockDocLine line, DateOnly today, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(line.LotCode))
        {
            var lot = NormLot(line.LotCode);
            var bal = await _db.InvStockBalances.AsNoTracking().FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.WarehouseId == wh.Id && x.SkuId == line.SkuId
                     && x.LotCode == lot && !x.IsDeleted, ct);
            var avail = bal is null ? 0 : bal.QtyOnHand - bal.QtyReserved;
            if (bal?.ExpiryDate is DateOnly exp && exp < today)
                throw new AppException($"Chặn xuất lô quá HSD {lot} — UC_INV_045.");
            if (!wh.AllowNegativeStock && avail + 0.0001m < line.Qty)
                throw new AppException($"Không đủ tồn khả dụng {line.SkuCode}/{lot} (còn {avail}) — UC_INV_042.");
            return [(lot, bal?.ExpiryDate ?? line.ExpiryDate, line.Qty, avail)];
        }

        var policy = (wh.PickPolicy ?? "Fifo").Trim();
        if (policy.Equals("Fefo", StringComparison.OrdinalIgnoreCase)
            || policy.Equals("Fifo", StringComparison.OrdinalIgnoreCase))
            return await BuildFefoPicksAsync(tenantId, wh, line.SkuId, line.SkuCode, line.Qty, policy, today, ct);

        // Manual / no policy: xuất lô trống
        return await BuildFefoPicksAsync(tenantId, wh, line.SkuId, line.SkuCode, line.Qty, "Fefo", today, ct);
    }

    private async Task<List<(string? LotCode, DateOnly? ExpiryDate, decimal Qty, decimal Available)>> BuildFefoPicksAsync(
        Guid tenantId, InvWarehouse wh, Guid skuId, string skuCode, decimal qty, string? policy, DateOnly today,
        CancellationToken ct)
    {
        var bals = await _db.InvStockBalances.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WarehouseId == wh.Id && x.SkuId == skuId && !x.IsDeleted)
            .ToListAsync(ct);
        bals = bals.Where(x => x.QtyOnHand - x.QtyReserved > 0.0001m
                               && (x.ExpiryDate is null || x.ExpiryDate >= today))
            .ToList();
        bals = policy != null && policy.Equals("Fifo", StringComparison.OrdinalIgnoreCase)
            ? bals.OrderBy(x => x.CreatedAt).ThenBy(x => x.LotCode).ToList()
            : bals.OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue).ThenBy(x => x.LotCode).ToList();

        var remain = qty;
        var picks = new List<(string? LotCode, DateOnly? ExpiryDate, decimal Qty, decimal Available)>();
        foreach (var b in bals)
        {
            if (remain <= 0) break;
            var avail = b.QtyOnHand - b.QtyReserved;
            var take = Math.Min(avail, remain);
            picks.Add((string.IsNullOrEmpty(b.LotCode) ? null : b.LotCode, b.ExpiryDate, take, avail));
            remain -= take;
        }
        if (remain > 0.0001m && !wh.AllowNegativeStock)
            throw new AppException($"Không đủ tồn khả dụng {skuCode} (thiếu {remain}) — UC_INV_042.");
        if (remain > 0.0001m)
            picks.Add((null, null, remain, 0));
        return picks;
    }

    private async Task<bool> TryConsumeReservationAsync(
        Guid tenantId, Guid userId, InvWarehouse wh, InvStockDoc doc,
        Guid skuId, string? lotCode, decimal qty, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(doc.RefCode) && doc.RefId is null) return false;
        var q = _db.InvStockReservations
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active"
                        && x.WarehouseId == wh.Id);
        if (doc.RefId is Guid rid) q = q.Where(x => x.RefId == rid);
        else if (!string.IsNullOrWhiteSpace(doc.RefCode))
        {
            var code = doc.RefCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.RefCode == code);
        }
        var res = await q.OrderBy(x => x.ActivatedAt).FirstOrDefaultAsync(ct);
        if (res is null) return false;
        var lot = NormLot(lotCode);
        var line = await _db.InvStockReservationLines.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ReservationId == res.Id && !x.IsDeleted
                 && x.SkuId == skuId
                 && (x.LotCode == null || x.LotCode == "" || x.LotCode == lot)
                 && x.Qty >= qty - 0.0001m, ct);
        if (line is null) return false;
        line.Qty -= qty;
        line.UpdatedBy = userId;
        if (line.Qty <= 0.0001m)
        {
            line.IsDeleted = true;
            line.DeletedAt = DateTimeOffset.UtcNow;
        }
        var still = await _db.InvStockReservationLines.AnyAsync(
            x => x.TenantId == tenantId && x.ReservationId == res.Id && !x.IsDeleted && x.Qty > 0, ct);
        if (!still)
        {
            res.Status = "Consumed";
            res.UpdatedBy = userId;
        }
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task ApplyBalanceAsync(
        Guid tenantId, Guid userId, InvWarehouse wh, Guid skuId, string? lotCode, DateOnly? expiry,
        decimal deltaOnHand, decimal deltaReserved, decimal deltaInTransit, CancellationToken ct,
        bool checkAvailable = false)
    {
        var lot = NormLot(lotCode) ?? "";
        var bal = await _db.InvStockBalances.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.WarehouseId == wh.Id && x.SkuId == skuId
                 && x.LotCode == lot && !x.IsDeleted, ct);
        if (bal is null)
        {
            bal = new InvStockBalance
            {
                TenantId = tenantId, WarehouseId = wh.Id, SkuId = skuId,
                LotCode = lot, ExpiryDate = expiry, CreatedBy = userId
            };
            _db.InvStockBalances.Add(bal);
        }

        if (checkAvailable)
        {
            var need = 0m;
            if (deltaOnHand < 0) need = Math.Max(need, -deltaOnHand);
            if (deltaReserved > 0) need = Math.Max(need, deltaReserved);
            var avail = bal.QtyOnHand - bal.QtyReserved;
            if (!wh.AllowNegativeStock && avail + 0.0001m < need)
                throw new AppException($"Không đủ tồn khả dụng kho {wh.Code} (còn {avail}) — UC_INV_042.");
        }

        bal.QtyOnHand += deltaOnHand;
        bal.QtyReserved += deltaReserved;
        bal.QtyInTransit += deltaInTransit;
        if (expiry is not null) bal.ExpiryDate = expiry;
        bal.UpdatedBy = userId;

        if (!wh.AllowNegativeStock && bal.QtyOnHand < -0.0001m)
            throw new AppException($"Kho {wh.Code} không cho tồn âm (SKU {skuId}).");
        if (bal.QtyReserved < -0.0001m)
            throw new AppException($"QtyReserved không hợp lệ (SKU {skuId}).");
        await _db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<InvReservationDto>> MapReservationsAsync(
        Guid tenantId, List<InvStockReservation> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<InvReservationDto>();
        var ids = list.Select(x => x.Id).ToList();
        var wids = list.Select(x => x.WarehouseId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var counts = await _db.InvStockReservationLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ReservationId) && !x.IsDeleted)
            .GroupBy(x => x.ReservationId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(r => new InvReservationDto(
            r.Id, r.Code, r.WarehouseId, whs.GetValueOrDefault(r.WarehouseId), r.Status,
            r.RefModule, r.RefId, r.RefCode, r.Note, r.ActivatedAt, r.ReleasedAt,
            counts.GetValueOrDefault(r.Id))).ToList();
    }

    private static InvReservationLineDto MapResLine(InvStockReservationLine x) =>
        new(x.Id, x.ReservationId, x.SkuId, x.SkuCode, x.SkuName, x.Qty, x.LotCode, x.ExpiryDate);

    private static string? NormLot(string? lotCode)
    {
        if (string.IsNullOrWhiteSpace(lotCode)) return null;
        return lotCode.Trim().ToUpperInvariant();
    }

    private async Task<IReadOnlyList<InvStockDocDto>> MapDocsAsync(
        Guid tenantId, List<InvStockDoc> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var wids = list.Select(x => x.WarehouseId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var counts = await _db.InvStockDocLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.DocId) && !x.IsDeleted)
            .GroupBy(x => x.DocId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(d => new InvStockDocDto(
            d.Id, d.Code, d.DocType, d.SourceType, d.WarehouseId, whs.GetValueOrDefault(d.WarehouseId),
            d.Status, d.RefModule, d.RefId, d.RefCode, d.PostedAt, d.Note,
            counts.GetValueOrDefault(d.Id))).ToList();
    }

    private async Task<IReadOnlyList<InvTransferDto>> MapTransfersAsync(
        Guid tenantId, List<InvTransfer> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var wids = list.Select(x => x.FromWarehouseId).Concat(list.Select(x => x.ToWarehouseId)).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var counts = await _db.InvTransferLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.TransferId) && !x.IsDeleted)
            .GroupBy(x => x.TransferId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(t => new InvTransferDto(
            t.Id, t.Code, t.FromWarehouseId, whs.GetValueOrDefault(t.FromWarehouseId),
            t.ToWarehouseId, whs.GetValueOrDefault(t.ToWarehouseId), t.Status,
            t.ShippedAt, t.ReceivedAt, t.Note, counts.GetValueOrDefault(t.Id))).ToList();
    }

    private async Task<IReadOnlyList<InvStocktakeDto>> MapStocktakesAsync(
        Guid tenantId, List<InvStocktake> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var wids = list.Select(x => x.WarehouseId).Distinct().ToList();
        var whs = await _db.InvWarehouses.AsNoTracking().Where(x => wids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var agg = await _db.InvStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.StocktakeId) && !x.IsDeleted)
            .GroupBy(x => x.StocktakeId)
            .Select(g => new { g.Key, C = g.Count(), V = g.Count(x => x.VarianceQty != 0) })
            .ToDictionaryAsync(x => x.Key, ct);
        return list.Select(s =>
        {
            agg.TryGetValue(s.Id, out var a);
            return new InvStocktakeDto(
                s.Id, s.Code, s.WarehouseId, whs.GetValueOrDefault(s.WarehouseId), s.Status,
                s.CountedAt, s.PostedAt, s.Note, a?.C ?? 0, a?.V ?? 0);
        }).ToList();
    }

    private static InvStockDocLineDto MapDocLine(InvStockDocLine l) =>
        new(l.Id, l.DocId, l.SkuId, l.SkuCode, l.SkuName, l.Qty, l.LotCode, l.ExpiryDate, l.UnitCost);
    private static InvTransferLineDto MapTrLine(InvTransferLine l) =>
        new(l.Id, l.TransferId, l.SkuId, l.SkuCode, l.SkuName, l.Qty, l.LotCode, l.ExpiryDate);
    private static InvStocktakeLineDto MapStLine(InvStocktakeLine l) =>
        new(l.Id, l.StocktakeId, l.SkuId, l.SkuCode, l.SkuName, l.LotCode, l.SystemQty, l.CountedQty, l.VarianceQty);

    private static async Task<T> RequireAsync<T>(DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static async Task<string> NextCodeAsync<T>(
        Guid tenantId, string prefix, DbSet<T> set, CancellationToken ct) where T : TenantEntity
    {
        var p = $"{prefix}-{DateTime.UtcNow:yyyyMM}-";
        var last = await set.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && EF.Property<string>(x, "Code").StartsWith(p))
            .OrderByDescending(x => EF.Property<string>(x, "Code"))
            .Select(x => EF.Property<string>(x, "Code")).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static string? Opt(string? value, int max)
    {
        var v = (value ?? "").Trim();
        if (v.Length == 0) return null;
        if (v.Length > max) throw new AppException($"Tối đa {max} ký tự.");
        return v;
    }
}

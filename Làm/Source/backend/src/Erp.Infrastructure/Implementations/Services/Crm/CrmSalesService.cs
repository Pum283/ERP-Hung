using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Base;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Crm;

public sealed class CrmSalesService : ICrmSalesService
{
    private static readonly HashSet<string> QuoteStatuses =
        new(StringComparer.OrdinalIgnoreCase)
            { "Draft", "PendingDiscount", "Sent", "Accepted", "Rejected", "Expired", "Converted" };
    private static readonly HashSet<string> OrderStatuses =
        new(StringComparer.OrdinalIgnoreCase)
            { "Draft", "Confirmed", "Holding", "Released", "Cancelled", "Delivered" };
    private static readonly HashSet<string> SendChannels =
        new(StringComparer.OrdinalIgnoreCase) { "Email", "Pdf" };
    private static readonly HashSet<string> PayMethods =
        new(StringComparer.OrdinalIgnoreCase) { "Cash", "Transfer", "Card", "Other" };

    private readonly AppDbContext _db;
    private readonly IFinRevenueService _rev;
    public CrmSalesService(AppDbContext db, IFinRevenueService rev)
    {
        _db = db;
        _rev = rev;
    }

    public async Task<IReadOnlyList<CrmPriceListDto>> ListPriceListsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmPriceLists.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        var counts = await _db.CrmPriceListItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.PriceListId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(p => MapPriceList(p, counts.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<CrmPriceListDetailDto> GetPriceListDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var pl = await RequireAsync(_db.CrmPriceLists, tenantId, id, "bảng giá", ct);
        var items = await _db.CrmPriceListItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PriceListId == id && !x.IsDeleted)
            .OrderBy(x => x.ItemCode).ToListAsync(ct);
        return new CrmPriceListDetailDto(
            MapPriceList(pl, items.Count),
            items.Select(MapPriceItem).ToList());
    }

    public async Task<CrmPriceListDto> UpsertPriceListAsync(
        Guid tenantId, Guid userId, CrmPriceListUpsertRequest req, CancellationToken ct = default)
    {
        CrmPriceList pl;
        if (req.Id is Guid id)
        {
            pl = await RequireAsync(_db.CrmPriceLists, tenantId, id, "bảng giá", ct);
            pl.UpdatedBy = userId;
        }
        else
        {
            pl = new CrmPriceList { TenantId = tenantId, CreatedBy = userId };
            _db.CrmPriceLists.Add(pl);
        }
        pl.Code = NormCode(req.Code);
        pl.Name = Req(req.Name, 200, "Tên bảng giá");
        pl.Status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        pl.Note = Opt(req.Note, 1000);
        await _db.SaveChangesAsync(ct);
        var count = await _db.CrmPriceListItems.CountAsync(
            x => x.TenantId == tenantId && x.PriceListId == pl.Id && !x.IsDeleted, ct);
        return MapPriceList(pl, count);
    }

    public async Task<CrmPriceListItemDto> UpsertPriceListItemAsync(
        Guid tenantId, Guid userId, Guid priceListId, CrmPriceListItemUpsertRequest req, CancellationToken ct = default)
    {
        await RequireAsync(_db.CrmPriceLists, tenantId, priceListId, "bảng giá", ct);
        var code = NormCode(req.ItemCode);
        CrmPriceListItem item;
        if (req.Id is Guid id)
        {
            item = await RequireAsync(_db.CrmPriceListItems, tenantId, id, "dòng bảng giá", ct);
            if (item.PriceListId != priceListId) throw new AppException("Dòng không thuộc bảng giá.");
            item.UpdatedBy = userId;
        }
        else
        {
            var dup = await _db.CrmPriceListItems.AnyAsync(
                x => x.TenantId == tenantId && x.PriceListId == priceListId && x.ItemCode == code && !x.IsDeleted, ct);
            if (dup) throw new AppException("Mã SP đã có trên bảng giá.");
            item = new CrmPriceListItem { TenantId = tenantId, PriceListId = priceListId, CreatedBy = userId };
            _db.CrmPriceListItems.Add(item);
        }
        item.ItemCode = code;
        item.ItemName = Req(req.ItemName, 200, "Tên SP");
        item.UnitPrice = req.UnitPrice < 0 ? throw new AppException("Đơn giá ≥ 0.") : req.UnitPrice;
        await _db.SaveChangesAsync(ct);
        return MapPriceItem(item);
    }

    public async Task<IReadOnlyList<CrmQuoteDto>> ListQuotesAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default)
    {
        var q = _db.CrmQuotes.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status);
        var list = await q.OrderByDescending(x => x.QuoteDate).Take(200).ToListAsync(ct);
        return await MapQuotesAsync(tenantId, list, ct);
    }

    public async Task<CrmQuoteDetailDto> GetQuoteDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, id, "báo giá", ct);
        var lines = await _db.CrmQuoteLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.QuoteId == id && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var dto = (await MapQuotesAsync(tenantId, [quote], ct))[0];
        return new CrmQuoteDetailDto(dto, lines.Select(MapQuoteLine).ToList());
    }

    public async Task<CrmQuoteDto> UpsertQuoteAsync(
        Guid tenantId, Guid userId, CrmQuoteUpsertRequest req, CancellationToken ct = default)
    {
        CrmQuote quote;
        if (req.Id is Guid id)
        {
            quote = await RequireAsync(_db.CrmQuotes, tenantId, id, "báo giá", ct);
            EnsureQuoteEditable(quote);
            quote.UpdatedBy = userId;
        }
        else
        {
            quote = new CrmQuote
            {
                TenantId = tenantId,
                Code = await NextCodeAsync(tenantId, "QT", _db.CrmQuotes, ct),
                QuoteDate = DateTimeOffset.UtcNow,
                Status = "Draft",
                DiscountApprovalStatus = "None",
                SentChannel = "None",
                Version = 1,
                CreatedBy = userId
            };
            _db.CrmQuotes.Add(quote);
        }

        if (req.OpportunityId is Guid oid)
            await RequireAsync(_db.CrmOpportunities, tenantId, oid, "cơ hội", ct);
        if (req.CustomerId is Guid cid)
            await RequireAsync(_db.CrmCustomers, tenantId, cid, "khách hàng", ct);
        if (req.PriceListId is Guid pid)
            await RequireAsync(_db.CrmPriceLists, tenantId, pid, "bảng giá", ct);

        quote.OpportunityId = req.OpportunityId;
        quote.CustomerId = req.CustomerId;
        quote.PriceListId = req.PriceListId;
        quote.ValidUntil = req.ValidUntil ?? quote.ValidUntil ?? DateTimeOffset.UtcNow.AddDays(14);
        quote.Note = Opt(req.Note, 1000);
        if (req.DiscountPercent is decimal dp)
        {
            if (dp is < 0 or > 100) throw new AppException("Chiết khấu 0–100%.");
            quote.DiscountPercent = dp;
            RecalcQuoteTotals(quote, await SumQuoteLinesAsync(tenantId, quote.Id, ct));
        }
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmQuoteDto> CreateQuoteFromOpportunityAsync(
        Guid tenantId, Guid userId, Guid opportunityId, CancellationToken ct = default)
    {
        var opp = await RequireAsync(_db.CrmOpportunities, tenantId, opportunityId, "cơ hội", ct);
        if (opp.Stage is "Lost") throw new AppException("Cơ hội Lost — không tạo báo giá.");
        if (opp.QuoteId is not null)
        {
            var existing = await RequireAsync(_db.CrmQuotes, tenantId, opp.QuoteId.Value, "báo giá", ct);
            return (await MapQuotesAsync(tenantId, [existing], ct))[0];
        }

        var quote = new CrmQuote
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "QT", _db.CrmQuotes, ct),
            OpportunityId = opp.Id,
            CustomerId = opp.CustomerId,
            QuoteDate = DateTimeOffset.UtcNow,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(14),
            Status = "Draft",
            DiscountApprovalStatus = "None",
            SentChannel = "None",
            Version = 1,
            Note = $"Từ cơ hội {opp.Code}",
            CreatedBy = userId
        };
        _db.CrmQuotes.Add(quote);
        await _db.SaveChangesAsync(ct);

        var oppLines = await _db.CrmOpportunityLines
            .Where(x => x.TenantId == tenantId && x.OpportunityId == opp.Id && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var lineNo = 1;
        foreach (var ol in oppLines)
        {
            _db.CrmQuoteLines.Add(new CrmQuoteLine
            {
                TenantId = tenantId, QuoteId = quote.Id,
                ItemCode = ol.ItemCode, ItemName = ol.ItemName,
                Quantity = ol.Quantity, UnitPrice = ol.UnitPrice,
                LineAmount = ol.LineAmount, LineNo = lineNo++, CreatedBy = userId
            });
        }
        var sub = oppLines.Sum(x => x.LineAmount);
        if (sub <= 0) sub = opp.EstimatedValue;
        quote.SubTotal = sub;
        quote.TotalAmount = sub;
        await _db.SaveChangesAsync(ct);

        opp.QuoteId = quote.Id;
        if (opp.Stage is "Qualification") opp.Stage = "Proposal";
        opp.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmQuoteLineDto> UpsertQuoteLineAsync(
        Guid tenantId, Guid userId, Guid quoteId, CrmQuoteLineUpsertRequest req, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        EnsureQuoteEditable(quote);
        if (req.Quantity <= 0) throw new AppException("Số lượng > 0.");
        if (req.UnitPrice < 0) throw new AppException("Đơn giá ≥ 0.");

        CrmQuoteLine line;
        if (req.Id is Guid id)
        {
            line = await RequireAsync(_db.CrmQuoteLines, tenantId, id, "dòng báo giá", ct);
            if (line.QuoteId != quoteId) throw new AppException("Dòng không thuộc báo giá.");
            line.UpdatedBy = userId;
        }
        else
        {
            var maxNo = await _db.CrmQuoteLines
                .Where(x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted)
                .Select(x => (int?)x.LineNo).MaxAsync(ct) ?? 0;
            line = new CrmQuoteLine
            {
                TenantId = tenantId, QuoteId = quoteId, LineNo = maxNo + 1, CreatedBy = userId
            };
            _db.CrmQuoteLines.Add(line);
        }
        line.ItemCode = NormCode(req.ItemCode);
        line.ItemName = Req(req.ItemName, 200, "Tên SP/DV");
        line.Quantity = req.Quantity;
        line.UnitPrice = req.UnitPrice;
        line.LineAmount = Math.Round(req.Quantity * req.UnitPrice, 2);
        await _db.SaveChangesAsync(ct);

        RecalcQuoteTotals(quote, await SumQuoteLinesAsync(tenantId, quoteId, ct));
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapQuoteLine(line);
    }

    public async Task<CrmQuoteDto> ApplyPriceListAsync(
        Guid tenantId, Guid userId, Guid quoteId, Guid priceListId, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        EnsureQuoteEditable(quote);
        await RequireAsync(_db.CrmPriceLists, tenantId, priceListId, "bảng giá", ct);
        var prices = await _db.CrmPriceListItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PriceListId == priceListId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.ItemCode, x => x, StringComparer.OrdinalIgnoreCase, ct);
        var lines = await _db.CrmQuoteLines
            .Where(x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted).ToListAsync(ct);
        foreach (var line in lines)
        {
            if (!prices.TryGetValue(line.ItemCode, out var p)) continue;
            line.UnitPrice = p.UnitPrice;
            line.ItemName = string.IsNullOrWhiteSpace(line.ItemName) ? p.ItemName : line.ItemName;
            line.LineAmount = Math.Round(line.Quantity * line.UnitPrice, 2);
            line.UpdatedBy = userId;
        }
        quote.PriceListId = priceListId;
        RecalcQuoteTotals(quote, lines.Sum(x => x.LineAmount));
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmQuoteDto> RequestDiscountAsync(
        Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountRequest req, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        EnsureQuoteEditable(quote);
        if (req.DiscountPercent is < 0 or > 100) throw new AppException("Chiết khấu 0–100%.");
        quote.DiscountPercent = req.DiscountPercent;
        RecalcQuoteTotals(quote, await SumQuoteLinesAsync(tenantId, quoteId, ct));
        quote.DiscountApprovalStatus = req.DiscountPercent > 0 ? "Pending" : "None";
        quote.Status = req.DiscountPercent > 0 ? "PendingDiscount" : "Draft";
        if (!string.IsNullOrWhiteSpace(req.Note)) quote.Note = Opt(req.Note, 1000);
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmQuoteDto> DecideDiscountAsync(
        Guid tenantId, Guid userId, Guid quoteId, CrmQuoteDiscountDecisionRequest req, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        if (quote.DiscountApprovalStatus != "Pending")
            throw new AppException("Không có yêu cầu duyệt chiết khấu.");
        if (req.Approved)
        {
            quote.DiscountApprovalStatus = "Approved";
            quote.Status = "Draft";
        }
        else
        {
            quote.DiscountApprovalStatus = "Rejected";
            quote.DiscountPercent = 0;
            RecalcQuoteTotals(quote, await SumQuoteLinesAsync(tenantId, quoteId, ct));
            quote.Status = "Draft";
        }
        if (!string.IsNullOrWhiteSpace(req.Note)) quote.Note = Opt(req.Note, 1000);
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmQuoteDto> SendQuoteAsync(
        Guid tenantId, Guid userId, Guid quoteId, CrmQuoteSendRequest req, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        if (quote.Status is "Converted" or "Rejected" or "Expired")
            throw new AppException("Báo giá không thể gửi.");
        if (quote.DiscountApprovalStatus == "Pending")
            throw new AppException("Đang chờ duyệt chiết khấu.");
        var channel = SendChannels.FirstOrDefault(x => x.Equals(req.Channel, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Kênh gửi: Email | Pdf.");
        var lineCount = await _db.CrmQuoteLines.CountAsync(
            x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted, ct);
        if (lineCount == 0 && quote.TotalAmount <= 0)
            throw new AppException("Báo giá chưa có dòng / giá trị.");
        quote.SentChannel = channel;
        quote.SentAt = DateTimeOffset.UtcNow;
        quote.Status = "Sent";
        quote.Version = Math.Max(1, quote.Version);
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<CrmSalesOrderDto> ConvertQuoteToOrderAsync(
        Guid tenantId, Guid userId, Guid quoteId, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        if (quote.Status == "Converted" && quote.OrderId is Guid existingOid)
            return (await MapOrdersAsync(tenantId, [await RequireAsync(_db.CrmSalesOrders, tenantId, existingOid, "đơn", ct)], ct))[0];
        if (quote.DiscountApprovalStatus == "Pending")
            throw new AppException("Đang chờ duyệt chiết khấu.");
        if (quote.Status is "Rejected" or "Expired")
            throw new AppException("Báo giá không hợp lệ để chuyển đơn.");

        var lines = await _db.CrmQuoteLines
            .Where(x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        if (lines.Count == 0 && quote.TotalAmount <= 0)
            throw new AppException("Báo giá trống — không tạo đơn.");

        Guid? ownerId = null;
        if (quote.OpportunityId is Guid oid)
        {
            var opp = await _db.CrmOpportunities.FirstOrDefaultAsync(
                x => x.Id == oid && x.TenantId == tenantId && !x.IsDeleted, ct);
            ownerId = opp?.OwnerUserId;
        }

        var order = new CrmSalesOrder
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, "SO", _db.CrmSalesOrders, ct),
            QuoteId = quote.Id,
            CustomerId = quote.CustomerId,
            OpportunityId = quote.OpportunityId,
            OwnerUserId = ownerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = "Confirmed",
            SubTotal = quote.SubTotal,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            StockHoldStatus = "None",
            WarehousePushStatus = "None",
            Note = $"Từ báo giá {quote.Code}",
            CreatedBy = userId
        };
        _db.CrmSalesOrders.Add(order);
        await _db.SaveChangesAsync(ct);

        var n = 1;
        foreach (var ql in lines)
        {
            _db.CrmSalesOrderLines.Add(new CrmSalesOrderLine
            {
                TenantId = tenantId, OrderId = order.Id,
                ItemCode = ql.ItemCode, ItemName = ql.ItemName,
                Quantity = ql.Quantity, UnitPrice = ql.UnitPrice,
                LineAmount = ql.LineAmount, LineNo = n++, CreatedBy = userId
            });
        }
        if (lines.Count == 0)
        {
            _db.CrmSalesOrderLines.Add(new CrmSalesOrderLine
            {
                TenantId = tenantId, OrderId = order.Id,
                ItemCode = "PKG", ItemName = "Gói từ báo giá",
                Quantity = 1, UnitPrice = quote.TotalAmount,
                LineAmount = quote.TotalAmount, LineNo = 1, CreatedBy = userId
            });
            order.SubTotal = quote.TotalAmount;
            order.TotalAmount = quote.TotalAmount;
        }
        await _db.SaveChangesAsync(ct);

        quote.Status = "Converted";
        quote.OrderId = order.Id;
        quote.UpdatedBy = userId;
        if (quote.OpportunityId is Guid opid)
        {
            var opp = await _db.CrmOpportunities.FirstOrDefaultAsync(
                x => x.Id == opid && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (opp is not null && opp.Stage is not "Won" and not "Lost")
            {
                opp.Stage = "Negotiation";
                opp.UpdatedBy = userId;
            }
        }
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<IReadOnlyList<CrmSalesOrderDto>> ListOrdersAsync(
        Guid tenantId, string? status = null, CancellationToken ct = default)
    {
        var q = _db.CrmSalesOrders.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status);
        var list = await q.OrderByDescending(x => x.OrderDate).Take(200).ToListAsync(ct);
        return await MapOrdersAsync(tenantId, list, ct);
    }

    public async Task<CrmSalesOrderDetailDto> GetOrderDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, id, "đơn hàng", ct);
        var lines = await _db.CrmSalesOrderLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OrderId == id && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var pays = await _db.CrmOrderPayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OrderId == id && !x.IsDeleted)
            .OrderByDescending(x => x.PaidAt).ToListAsync(ct);
        var dto = (await MapOrdersAsync(tenantId, [order], ct))[0];
        return new CrmSalesOrderDetailDto(
            dto,
            lines.Select(MapOrderLine).ToList(),
            pays.Select(MapPayment).ToList());
    }

    public async Task<CrmSalesOrderDto> SetOrderStatusAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderStatusRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status == "Cancelled") throw new AppException("Đơn đã hủy.");
        var status = OrderStatuses.FirstOrDefault(x => x.Equals(req.Status, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Trạng thái đơn không hợp lệ.");
        if (status == "Cancelled") throw new AppException("Dùng API hủy đơn có lý do.");
        order.Status = status;
        if (status == "Holding") order.StockHoldStatus = "Held";
        if (status == "Released") order.StockHoldStatus = "Released";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        if (status == "Delivered")
        {
            try { await _rev.RecognizeFromSalesOrderAsync(tenantId, userId, order.Id, null, ct); }
            catch (AppException) { /* FIN chưa sẵn sàng — bỏ qua */ }
        }
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<CrmSalesOrderDto> HoldStockAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status is "Cancelled" or "Delivered")
            throw new AppException("Không giữ tồn trên đơn này.");
        order.StockHoldStatus = "Held";
        order.Status = "Holding";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<CrmSalesOrderDto> CancelOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderCancelRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status == "Delivered") throw new AppException("Đơn đã giao — không hủy.");
        if (order.Status == "Cancelled") throw new AppException("Đơn đã hủy.");
        order.Status = "Cancelled";
        order.CancelReason = Req(req.Reason, 500, "Lý do hủy");
        if (order.StockHoldStatus == "Held") order.StockHoldStatus = "Released";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<CrmOrderPaymentDto> AddPaymentAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderPaymentRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status == "Cancelled") throw new AppException("Đơn đã hủy.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        var method = PayMethods.FirstOrDefault(x => x.Equals(req.Method, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("Phương thức: Cash | Transfer | Card | Other.");
        var remain = order.TotalAmount - order.PaidAmount;
        if (req.Amount > remain + 0.01m) throw new AppException($"Thanh toán vượt còn lại ({remain:N0}).");

        var pay = new CrmOrderPayment
        {
            TenantId = tenantId, OrderId = orderId,
            Code = await NextCodeAsync(tenantId, "PAY", _db.CrmOrderPayments, ct),
            PaidAt = DateTimeOffset.UtcNow, Amount = req.Amount, Method = method,
            Note = Opt(req.Note, 1000), CreatedBy = userId
        };
        _db.CrmOrderPayments.Add(pay);
        order.PaidAmount = Math.Round(order.PaidAmount + req.Amount, 2);
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapPayment(pay);
    }

    public async Task<CrmSalesOrderDto> PushToWarehouseAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status is "Cancelled") throw new AppException("Đơn đã hủy.");
        if (order.Status is "Draft") throw new AppException("Cần xác nhận đơn trước khi đẩy kho.");
        // Stub tích hợp INV/LOG Cap-2
        order.WarehousePushStatus = "Pushed";
        if (order.Status is "Confirmed" or "Holding") order.Status = "Released";
        if (order.StockHoldStatus == "Held") order.StockHoldStatus = "Released";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    private async Task<decimal> SumQuoteLinesAsync(Guid tenantId, Guid quoteId, CancellationToken ct)
        => await _db.CrmQuoteLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.LineAmount, ct) ?? 0;

    private static void RecalcQuoteTotals(CrmQuote quote, decimal subTotal)
    {
        quote.SubTotal = Math.Round(subTotal, 2);
        quote.DiscountAmount = Math.Round(quote.SubTotal * quote.DiscountPercent / 100m, 2);
        quote.TotalAmount = Math.Max(0, quote.SubTotal - quote.DiscountAmount);
    }

    private static void EnsureQuoteEditable(CrmQuote quote)
    {
        if (quote.Status is "Converted" or "Rejected" or "Expired")
            throw new AppException("Báo giá đã khóa — không chỉnh sửa.");
    }

    private async Task<IReadOnlyList<CrmQuoteDto>> MapQuotesAsync(
        Guid tenantId, List<CrmQuote> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var oids = list.Where(x => x.OpportunityId.HasValue).Select(x => x.OpportunityId!.Value).Distinct().ToList();
        var cids = list.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();
        var pids = list.Where(x => x.PriceListId.HasValue).Select(x => x.PriceListId!.Value).Distinct().ToList();
        var orids = list.Where(x => x.OrderId.HasValue).Select(x => x.OrderId!.Value).Distinct().ToList();
        var opps = oids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmOpportunities.AsNoTracking().Where(x => oids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var custs = cids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, ct);
        var pls = pids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmPriceLists.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var ords = orids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmSalesOrders.AsNoTracking().Where(x => orids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var lineCounts = await _db.CrmQuoteLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.QuoteId) && !x.IsDeleted)
            .GroupBy(x => x.QuoteId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(q => new CrmQuoteDto(
            q.Id, q.Code, q.OpportunityId, q.OpportunityId is Guid o ? opps.GetValueOrDefault(o) : null,
            q.CustomerId, q.CustomerId is Guid c ? custs.GetValueOrDefault(c) : null,
            q.PriceListId, q.PriceListId is Guid p ? pls.GetValueOrDefault(p) : null,
            q.QuoteDate, q.ValidUntil, q.SubTotal, q.DiscountPercent, q.DiscountAmount, q.TotalAmount,
            q.Status, q.DiscountApprovalStatus, q.Version, q.SentAt, q.SentChannel,
            q.OrderId, q.OrderId is Guid r ? ords.GetValueOrDefault(r) : null, q.Note,
            lineCounts.GetValueOrDefault(q.Id))).ToList();
    }

    private async Task<IReadOnlyList<CrmSalesOrderDto>> MapOrdersAsync(
        Guid tenantId, List<CrmSalesOrder> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var qids = list.Where(x => x.QuoteId.HasValue).Select(x => x.QuoteId!.Value).Distinct().ToList();
        var cids = list.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId!.Value).Distinct().ToList();
        var uids = list.Where(x => x.OwnerUserId.HasValue).Select(x => x.OwnerUserId!.Value).Distinct().ToList();
        var quotes = qids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmQuotes.AsNoTracking().Where(x => qids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var custs = cids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, ct);
        var users = uids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking().Where(x => uids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);
        var lineCounts = await _db.CrmSalesOrderLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.OrderId) && !x.IsDeleted)
            .GroupBy(x => x.OrderId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var payCounts = await _db.CrmOrderPayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.OrderId) && !x.IsDeleted)
            .GroupBy(x => x.OrderId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);

        return list.Select(o => new CrmSalesOrderDto(
            o.Id, o.Code, o.QuoteId, o.QuoteId is Guid q ? quotes.GetValueOrDefault(q) : null,
            o.CustomerId, o.CustomerId is Guid c ? custs.GetValueOrDefault(c) : null,
            o.OpportunityId, o.OwnerUserId, o.OwnerUserId is Guid u ? users.GetValueOrDefault(u) : null,
            o.OrderDate, o.Status, o.SubTotal, o.DiscountAmount, o.TotalAmount, o.PaidAmount,
            o.StockHoldStatus, o.WarehousePushStatus, o.CancelReason, o.Note,
            lineCounts.GetValueOrDefault(o.Id), payCounts.GetValueOrDefault(o.Id))).ToList();
    }

    private static CrmPriceListDto MapPriceList(CrmPriceList p, int count) =>
        new(p.Id, p.Code, p.Name, p.Status, p.Note, count);
    private static CrmPriceListItemDto MapPriceItem(CrmPriceListItem i) =>
        new(i.Id, i.PriceListId, i.ItemCode, i.ItemName, i.UnitPrice);
    private static CrmQuoteLineDto MapQuoteLine(CrmQuoteLine l) =>
        new(l.Id, l.QuoteId, l.ItemCode, l.ItemName, l.Quantity, l.UnitPrice, l.LineAmount, l.LineNo);
    private static CrmSalesOrderLineDto MapOrderLine(CrmSalesOrderLine l) =>
        new(l.Id, l.OrderId, l.ItemCode, l.ItemName, l.Quantity, l.UnitPrice, l.LineAmount, l.LineNo);
    private static CrmOrderPaymentDto MapPayment(CrmOrderPayment p) =>
        new(p.Id, p.OrderId, p.Code, p.PaidAt, p.Amount, p.Method, p.Note);

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

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is 0) throw new AppException($"{label} bắt buộc.");
        if (v.Length > max) throw new AppException($"{label} tối đa {max} ký tự.");
        return v;
    }

    private static string? Opt(string? value, int max)
    {
        var v = (value ?? "").Trim();
        if (v.Length == 0) return null;
        if (v.Length > max) throw new AppException($"Ghi chú tối đa {max} ký tự.");
        return v;
    }
}

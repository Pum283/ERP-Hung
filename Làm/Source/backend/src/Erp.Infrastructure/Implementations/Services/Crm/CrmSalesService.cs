using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.DTOs.Inv;
using Erp.Application.DTOs.Log;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Application.Interfaces.Services.Inv;
using Erp.Application.Interfaces.Services.Log;
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
    private readonly IInvStockService _inv;
    private readonly ILogLogisticsService _log;
    public CrmSalesService(AppDbContext db, IFinRevenueService rev, IInvStockService inv, ILogLogisticsService log)
    {
        _db = db;
        _rev = rev;
        _inv = inv;
        _log = log;
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

        var (fileName, content) = await BuildQuoteDocumentAsync(tenantId, quote, ct);

        if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            string? email = null;
            if (quote.CustomerId is Guid cid)
            {
                email = await _db.CrmCustomers.AsNoTracking()
                    .Where(x => x.Id == cid && x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => x.Email).FirstOrDefaultAsync(ct);
            }
            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("Khách hàng chưa có email — không gửi được.");

            // Ghi outbox nội bộ (AppNotification cho người gửi) + nhật ký trên báo giá.
            _db.AppNotifications.Add(new Domain.Entities.Sys.AppNotification
            {
                TenantId = tenantId, UserId = userId,
                Title = $"Đã xếp hàng gửi BG {quote.Code}",
                Body = $"Tới {email} · {fileName} · {content.Length} ký tự",
                EventType = "CrmQuoteEmail", Link = $"/app/crm/quotes?id={quote.Id}",
                CreatedBy = userId,
            });
            quote.Note = AppendNote(quote.Note, $"EMAIL→{email} @ {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}Z · {fileName}");
        }
        else
        {
            quote.Note = AppendNote(quote.Note, $"PDF/TEXT {fileName} @ {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}Z · {content.Length} ký tự");
        }

        quote.SentChannel = channel;
        quote.SentAt = DateTimeOffset.UtcNow;
        quote.Status = "Sent";
        quote.Version = Math.Max(1, quote.Version);
        quote.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapQuotesAsync(tenantId, [quote], ct))[0];
    }

    public async Task<(string FileName, string Content)> BuildQuoteTextAsync(
        Guid tenantId, Guid userId, Guid quoteId, bool stampSent = false, CancellationToken ct = default)
    {
        var quote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        if (quote.Status is "Converted" or "Rejected" or "Expired")
            throw new AppException("Báo giá không thể xuất.");
        var lineCount = await _db.CrmQuoteLines.CountAsync(
            x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted, ct);
        if (lineCount == 0 && quote.TotalAmount <= 0)
            throw new AppException("Báo giá chưa có dòng / giá trị.");

        var result = await BuildQuoteDocumentAsync(tenantId, quote, ct);
        if (stampSent)
        {
            quote.SentChannel = "Pdf";
            quote.SentAt = DateTimeOffset.UtcNow;
            if (quote.Status is "Draft" or "PendingDiscount") quote.Status = "Sent";
            quote.Note = AppendNote(quote.Note, $"PDF/TEXT {result.FileName} @ {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}Z");
            quote.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
        }
        return result;
    }

    private async Task<(string FileName, string Content)> BuildQuoteDocumentAsync(
        Guid tenantId, CrmQuote quote, CancellationToken ct)
    {
        string? customerName = null, customerEmail = null, customerPhone = null;
        if (quote.CustomerId is Guid cid)
        {
            var cust = await _db.CrmCustomers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == cid && x.TenantId == tenantId && !x.IsDeleted, ct);
            customerName = cust?.DisplayName;
            customerEmail = cust?.Email;
            customerPhone = cust?.Phone;
        }

        var lines = await _db.CrmQuoteLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.QuoteId == quote.Id && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("BÁO GIÁ / QUOTATION");
        sb.AppendLine(new string('=', 48));
        sb.AppendLine($"Số       : {quote.Code}  (v{quote.Version})");
        sb.AppendLine($"Ngày     : {quote.QuoteDate.ToLocalTime():dd/MM/yyyy}");
        if (quote.ValidUntil is DateTimeOffset vu)
            sb.AppendLine($"Hiệu lực : đến {vu.ToLocalTime():dd/MM/yyyy}");
        sb.AppendLine($"Khách hàng: {customerName ?? "—"}");
        if (!string.IsNullOrWhiteSpace(customerPhone)) sb.AppendLine($"Điện thoại: {customerPhone}");
        if (!string.IsNullOrWhiteSpace(customerEmail)) sb.AppendLine($"Email     : {customerEmail}");
        sb.AppendLine(new string('-', 48));
        sb.AppendLine($"{"SP/DV",-24}{"SL",8}{"Đơn giá",12}{"Thành tiền",12}");
        foreach (var l in lines)
        {
            var name = (l.ItemName.Length > 24 ? l.ItemName[..24] : l.ItemName).PadRight(24);
            sb.AppendLine($"{name}{l.Quantity,8:0.##}{l.UnitPrice,12:N0}{l.LineAmount,12:N0}");
        }
        sb.AppendLine(new string('-', 48));
        sb.AppendLine($"Tạm tính     : {quote.SubTotal,14:N0}");
        if (quote.DiscountAmount > 0 || quote.DiscountPercent > 0)
            sb.AppendLine($"Chiết khấu   : {quote.DiscountAmount,14:N0} ({quote.DiscountPercent:0.##}%)");
        sb.AppendLine($"TỔNG CỘNG    : {quote.TotalAmount,14:N0}");
        sb.AppendLine(new string('=', 48));
        if (!string.IsNullOrWhiteSpace(quote.Note) && !quote.Note.Contains("EMAIL→") && !quote.Note.Contains("PDF/TEXT"))
            sb.AppendLine($"Ghi chú: {quote.Note}");
        sb.AppendLine("Trân trọng cảm ơn.");

        return ($"{quote.Code}-baogia.txt", sb.ToString());
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

    public async Task<CrmQuoteDto> CreateNewVersionAsync(
        Guid tenantId, Guid userId, Guid quoteId, CancellationToken ct = default)
    {
        var oldQuote = await RequireAsync(_db.CrmQuotes, tenantId, quoteId, "báo giá", ct);
        var oldLines = await _db.CrmQuoteLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.QuoteId == quoteId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);

        var newVersion = oldQuote.Version + 1;
        var newCode = $"{oldQuote.Code.Split("-v")[0]}-v{newVersion}";

        var newQuote = new CrmQuote
        {
            TenantId = tenantId,
            Code = newCode,
            OpportunityId = oldQuote.OpportunityId,
            CustomerId = oldQuote.CustomerId,
            PriceListId = oldQuote.PriceListId,
            QuoteDate = DateTimeOffset.UtcNow,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(14),
            Status = "Draft",
            DiscountPercent = oldQuote.DiscountPercent,
            DiscountAmount = oldQuote.DiscountAmount,
            SubTotal = oldQuote.SubTotal,
            TotalAmount = oldQuote.TotalAmount,
            DiscountApprovalStatus = "None",
            SentChannel = "None",
            Version = newVersion,
            Note = $"Phiên bản mới v{newVersion} sao chép từ {oldQuote.Code}",
            CreatedBy = userId
        };
        _db.CrmQuotes.Add(newQuote);
        await _db.SaveChangesAsync(ct);

        foreach (var l in oldLines)
        {
            _db.CrmQuoteLines.Add(new CrmQuoteLine
            {
                TenantId = tenantId, QuoteId = newQuote.Id,
                ItemCode = l.ItemCode, ItemName = l.ItemName,
                Quantity = l.Quantity, UnitPrice = l.UnitPrice,
                LineAmount = l.LineAmount, LineNo = l.LineNo, CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);

        return (await MapQuotesAsync(tenantId, [newQuote], ct))[0];
    }

    public async Task<int> CheckAndExpireQuotesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredQuotes = await _db.CrmQuotes
            .Where(x => x.TenantId == tenantId && !x.IsDeleted &&
                        x.ValidUntil < now &&
                        (x.Status == "Draft" || x.Status == "Sent" || x.Status == "PendingDiscount"))
            .ToListAsync(ct);

        foreach (var q in expiredQuotes)
        {
            q.Status = "Expired";
            q.Note = AppendNote(q.Note, $"[Hệ thống] Tự động hết hạn vào {now:yyyy-MM-dd HH:mm}Z");
        }

        if (expiredQuotes.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }
        return expiredQuotes.Count;
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

    /// <summary>UC_CRM_082 — giữ tồn thật qua INV reservation (Active → tăng QtyReserved), idempotent.</summary>
    public async Task<CrmSalesOrderDto> HoldStockAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status is "Cancelled" or "Delivered")
            throw new AppException("Không giữ tồn trên đơn này.");

        var hasActive = await _db.InvStockReservations.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId && !x.IsDeleted
                 && x.RefModule == "CRM" && x.RefId == order.Id && x.Status == "Active", ct);
        if (!hasActive)
        {
            var whId = await _db.InvWarehouses.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Status == "Active" && !x.IsDeleted)
                .OrderBy(x => x.Code).Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (whId == Guid.Empty) throw new AppException("Chưa có kho Active để giữ tồn.");

            var lines = await _db.CrmSalesOrderLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.OrderId == order.Id && !x.IsDeleted && x.Quantity > 0)
                .ToListAsync(ct);
            if (lines.Count == 0) throw new AppException("Đơn chưa có dòng hàng.");

            var codes = lines.Select(x => x.ItemCode.ToUpperInvariant()).Distinct().ToList();
            var skus = await _db.InvSkus.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && codes.Contains(x.Code))
                .ToDictionaryAsync(x => x.Code, ct);
            var reserveLines = lines
                .Where(l => skus.ContainsKey(l.ItemCode.ToUpperInvariant()))
                .Select(l => new InvReservationLineRequest(
                    skus[l.ItemCode.ToUpperInvariant()].Id, l.Quantity, null, null))
                .ToList();
            if (reserveLines.Count == 0)
                throw new AppException("Không có SP nào khớp SKU kho — đồng bộ catalog INV trước khi giữ tồn.");

            try
            {
                var rv = await _inv.CreateReservationAsync(tenantId, userId, new InvReservationCreateRequest(
                    whId, "CRM", order.Id, order.Code, $"Giữ tồn đơn {order.Code}", Activate: true, reserveLines), ct);
                order.Note = AppendNote(order.Note, $"Giữ tồn {rv.Header.Code} ({reserveLines.Count}/{lines.Count} dòng)");
            }
            catch (AppException ex)
            {
                throw new AppException($"Giữ tồn thất bại: {ex.Message}");
            }
        }

        order.StockHoldStatus = "Held";
        order.Status = "Holding";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    private async Task ReleaseCrmReservationAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct)
    {
        var active = await _db.InvStockReservations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && x.RefModule == "CRM" && x.RefId == orderId && x.Status == "Active")
            .Select(x => x.Id).ToListAsync(ct);
        foreach (var id in active)
        {
            try { await _inv.ReleaseReservationAsync(tenantId, userId, id, ct); }
            catch (AppException) { /* đã release song song — bỏ qua */ }
        }
    }

    private static string? AppendNote(string? note, string extra)
    {
        var joined = string.IsNullOrWhiteSpace(note) ? extra : $"{note} · {extra}";
        return joined.Length <= 1000 ? joined : joined[..1000];
    }

    public async Task<CrmSalesOrderDto> CancelOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderCancelRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status == "Delivered") throw new AppException("Đơn đã giao — không hủy.");
        if (order.Status == "Cancelled") throw new AppException("Đơn đã hủy.");
        order.Status = "Cancelled";
        order.CancelReason = Req(req.Reason, 500, "Lý do hủy");
        if (order.StockHoldStatus == "Held")
        {
            await ReleaseCrmReservationAsync(tenantId, userId, order.Id, ct);
            order.StockHoldStatus = "Released";
        }
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

    /// <summary>UC_CRM_088 — tạo lệnh giao LOG thật (Draft→Confirmed) từ đơn, idempotent theo SourceOrderCode.</summary>
    public async Task<CrmSalesOrderDto> PushToWarehouseAsync(
        Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status is "Cancelled") throw new AppException("Đơn đã hủy.");
        if (order.Status is "Draft") throw new AppException("Cần xác nhận đơn trước khi đẩy kho.");

        var existing = await _db.LogDeliveryOrders.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && !x.IsDeleted
                 && x.SourceOrderCode == order.Code.ToUpper(), ct);
        if (existing is null)
        {
            var lines = await _db.CrmSalesOrderLines.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.OrderId == order.Id && !x.IsDeleted && x.Quantity > 0)
                .OrderBy(x => x.LineNo).ToListAsync(ct);
            if (lines.Count == 0) throw new AppException("Đơn chưa có dòng hàng — không thể đẩy kho.");

            var customer = order.CustomerId is Guid cid
                ? await _db.CrmCustomers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == cid && x.TenantId == tenantId && !x.IsDeleted, ct)
                : null;

            try
            {
                var delivery = await _log.UpsertDeliveryAsync(tenantId, userId, new LogDeliveryUpsertRequest(
                    null, null, order.Code, customer?.DisplayName ?? "Khách lẻ",
                    customer?.Address, customer?.Phone, $"Từ đơn CRM {order.Code}", null), ct);
                foreach (var l in lines)
                    await _log.UpsertLineAsync(tenantId, userId, delivery.Id, new LogDeliveryLineUpsertRequest(
                        null, l.ItemCode, l.ItemName, l.Quantity, null, null), ct);
                await _log.ConfirmAsync(tenantId, userId, delivery.Id, ct);
                order.Note = AppendNote(order.Note, $"LOG {delivery.Code}");
            }
            catch (AppException ex)
            {
                order.WarehousePushStatus = "Failed";
                order.UpdatedBy = userId;
                await _db.SaveChangesAsync(ct);
                throw new AppException($"Đẩy kho/LOG thất bại: {ex.Message}");
            }
        }

        if (order.StockHoldStatus == "Held")
            await ReleaseCrmReservationAsync(tenantId, userId, order.Id, ct);
        order.WarehousePushStatus = "Pushed";
        if (order.Status is "Confirmed" or "Holding") order.Status = "Released";
        if (order.StockHoldStatus == "Held") order.StockHoldStatus = "Released";
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<CrmSalesOrderDto> ReturnOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderReturnRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        if (order.Status == "Cancelled") throw new AppException("Đơn hàng đã hủy — không thể trả hàng.");
        var reason = Req(req.Reason, 500, "Lý do trả hàng");
        order.Status = "Returned";
        order.ReturnReason = reason;
        if (order.StockHoldStatus == "Held")
        {
            await ReleaseCrmReservationAsync(tenantId, userId, order.Id, ct);
            order.StockHoldStatus = "Released";
        }
        order.Note = AppendNote(order.Note, $"[Trả hàng/Điều chỉnh] Lý do: {reason}");
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<CrmSalesOrderDto> LinkContractAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderLinkContractRequest req, CancellationToken ct = default)
    {
        var order = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng", ct);
        order.ContractId = req.ContractId;
        order.Note = AppendNote(order.Note, $"[Hợp đồng] Gắn với HĐ ID: {req.ContractId}");
        order.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [order], ct))[0];
    }

    public async Task<(string FileName, string Content)> BuildQuotePdfHtmlAsync(
        Guid tenantId, Guid userId, Guid quoteId, CancellationToken ct = default)
    {
        var (fn, txt) = await BuildQuoteTextAsync(tenantId, userId, quoteId, stampSent: false, ct);
        var html = $"<!DOCTYPE html><html><head><title>{fn}</title><style>body{{font-family:sans-serif;padding:20px;}}pre{{background:#f4f4f4;padding:15px;border-radius:4px;}}</style></head><body><h2>Báo giá Pum's ERP</h2><pre>{System.Net.WebUtility.HtmlEncode(txt)}</pre></body></html>";
        return (fn.Replace(".txt", ".html"), html);
    }

    public async Task<CrmSalesOrderDto> SplitOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, CrmOrderSplitRequest req, CancellationToken ct = default)
    {
        if (req.LineIds == null || req.LineIds.Count == 0)
            throw new AppException("Chỉ định ít nhất 1 dòng hàng để tách.");

        var originalOrder = await RequireAsync(_db.CrmSalesOrders, tenantId, orderId, "đơn hàng gốc", ct);
        if (originalOrder.Status is "Delivered" or "Cancelled")
            throw new AppException("Không thể tách đơn đã hoàn thành hoặc đã hủy.");

        var linesToMove = await _db.CrmSalesOrderLines
            .Where(x => x.TenantId == tenantId && x.OrderId == orderId && req.LineIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(ct);

        if (linesToMove.Count == 0)
            throw new AppException("Không tìm thấy các dòng chỉ định.");

        var newOrderCode = $"{originalOrder.Code}-S1";
        var newOrder = new CrmSalesOrder
        {
            TenantId = tenantId,
            Code = newOrderCode,
            QuoteId = originalOrder.QuoteId,
            CustomerId = originalOrder.CustomerId,
            OpportunityId = originalOrder.OpportunityId,
            OwnerUserId = originalOrder.OwnerUserId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = "Draft",
            Note = $"Tách từ đơn {originalOrder.Code}",
            CreatedBy = userId
        };
        _db.CrmSalesOrders.Add(newOrder);
        await _db.SaveChangesAsync(ct);

        var newSubTotal = 0m;
        var lineNo = 1;
        foreach (var l in linesToMove)
        {
            l.OrderId = newOrder.Id;
            l.LineNo = lineNo++;
            l.UpdatedBy = userId;
            newSubTotal += l.LineAmount;
        }

        newOrder.SubTotal = newSubTotal;
        newOrder.TotalAmount = newSubTotal;
        originalOrder.Note = AppendNote(originalOrder.Note, $"Đã tách bớt {linesToMove.Count} dòng sang {newOrderCode}");
        originalOrder.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [newOrder], ct))[0];
    }

    public async Task<CrmSalesOrderDto> MergeOrdersAsync(
        Guid tenantId, Guid userId, CrmOrderMergeRequest req, CancellationToken ct = default)
    {
        if (req.PrimaryOrderId == req.SecondaryOrderId)
            throw new AppException("Chỉ định 2 đơn hàng khác nhau để gộp.");

        var primary = await RequireAsync(_db.CrmSalesOrders, tenantId, req.PrimaryOrderId, "đơn hàng chính", ct);
        var secondary = await RequireAsync(_db.CrmSalesOrders, tenantId, req.SecondaryOrderId, "đơn hàng phụ", ct);

        if (secondary.Status is "Delivered" or "Cancelled")
            throw new AppException("Không thể gộp đơn phụ đã giao hoặc đã hủy.");

        var secLines = await _db.CrmSalesOrderLines
            .Where(x => x.TenantId == tenantId && x.OrderId == secondary.Id && !x.IsDeleted)
            .ToListAsync(ct);

        var maxLineNo = await _db.CrmSalesOrderLines
            .Where(x => x.TenantId == tenantId && x.OrderId == primary.Id && !x.IsDeleted)
            .Select(x => (int?)x.LineNo).MaxAsync(ct) ?? 0;

        foreach (var l in secLines)
        {
            l.OrderId = primary.Id;
            l.LineNo = ++maxLineNo;
            l.UpdatedBy = userId;
        }

        secondary.Status = "Cancelled";
        secondary.CancelReason = Opt(req.Reason, 500) ?? $"Gộp trùng dòng hàng sang đơn {primary.Code}";
        secondary.UpdatedBy = userId;

        primary.SubTotal += secondary.SubTotal;
        primary.TotalAmount += secondary.TotalAmount;
        primary.Note = AppendNote(primary.Note, $"Gộp dòng hàng từ đơn {secondary.Code}");
        primary.UpdatedBy = userId;

        await _db.SaveChangesAsync(ct);
        return (await MapOrdersAsync(tenantId, [primary], ct))[0];
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

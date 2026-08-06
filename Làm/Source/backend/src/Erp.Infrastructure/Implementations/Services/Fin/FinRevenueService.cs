using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinRevenueService : IFinRevenueService
{
    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;

    public FinRevenueService(AppDbContext db, IFinAccountingService fin)
    {
        _db = db;
        _fin = fin;
    }

    public async Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
        Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null,
        CancellationToken ct = default)
    {
        var q = _db.FinRevenueDocuments.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(kind)) q = q.Where(x => x.Kind == kind.Trim());
        if (periodId is Guid pid) q = q.Where(x => x.PeriodId == pid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Code).Take(500).ToListAsync(ct);
        return await MapAsync(tenantId, list, ct);
    }

    public async Task<FinRevenueSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
    {
        var q = _db.FinRevenueDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted");
        if (periodId is Guid pid) q = q.Where(x => x.PeriodId == pid);
        var list = await q.ToListAsync(ct);
        string? periodCode = null;
        if (periodId is Guid p)
            periodCode = await _db.FinPeriods.AsNoTracking()
                .Where(x => x.Id == p).Select(x => x.Code).FirstOrDefaultAsync(ct);

        var pos = list.Where(x => x.Kind == "PosRevenue").ToList();
        var ord = list.Where(x => x.Kind == "OrderRevenue").ToList();
        var ar = list.Where(x => x.Kind == "ArRevenue").ToList();
        var cogs = list.Where(x => x.Kind == "Cogs").ToList();
        var rev = pos.Sum(x => x.RevenueAmount) + ord.Sum(x => x.RevenueAmount) + ar.Sum(x => x.RevenueAmount);
        var cogsAmt = cogs.Sum(x => x.CogsAmount);
        return new FinRevenueSummaryDto(
            periodId, periodCode,
            pos.Sum(x => x.RevenueAmount), pos.Count,
            ord.Sum(x => x.RevenueAmount), ord.Count,
            ar.Sum(x => x.RevenueAmount), ar.Count,
            cogsAmt, cogs.Count,
            rev - cogsAmt);
    }

    public async Task<FinRevenueDocumentDto> RecognizeFromPosAsync(
        Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
    {
        var existing = await FindExistingAsync(tenantId, "POS", saleId, ct);
        if (existing is not null) return (await MapAsync(tenantId, [existing], ct))[0];

        var sale = await _db.PosSales.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == saleId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy đơn POS.", 404);
        if (sale.Status != "Paid") throw new AppException("Chỉ ghi nhận doanh thu đơn POS đã Paid.");

        var entity = await CreateBaseAsync(tenantId, userId, "PosRevenue", "POS", sale.Id, sale.Code,
            sale.PaidAt ?? DateTimeOffset.UtcNow, sale.SubTotal, sale.TaxAmount, 0, sale.TotalAmount, req, ct);

        await TryPostJeAsync(tenantId, userId, entity,
            $"DT POS {sale.Code}", sale.TotalAmount, "Thu tiền POS", "Doanh thu POS", ct);

        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
    {
        var existing = await FindExistingAsync(tenantId, "CRM", orderId, ct);
        if (existing is not null) return (await MapAsync(tenantId, [existing], ct))[0];

        var order = await _db.CrmSalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy đơn bán.", 404);
        if (order.Status is "Draft" or "Cancelled")
            throw new AppException("Đơn phải Confirmed trở lên để ghi nhận doanh thu.");

        var entity = await CreateBaseAsync(tenantId, userId, "OrderRevenue", "CRM", order.Id, order.Code,
            order.OrderDate, order.SubTotal, 0, 0, order.TotalAmount, req, ct);

        await TryPostJeAsync(tenantId, userId, entity,
            $"DT đơn {order.Code}", order.TotalAmount, "Phải thu / tiền đơn", "Doanh thu đơn", ct);

        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
    {
        var existing = await FindExistingAsync(tenantId, "FIN_AR", arInvoiceId, ct);
        if (existing is not null) return (await MapAsync(tenantId, [existing], ct))[0];

        var inv = await _db.FinArInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == arInvoiceId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy HĐ AR.", 404);
        if (inv.Status is "Draft" or "Void")
            throw new AppException("Chỉ ghi nhận doanh thu HĐ AR đã ghi sổ.");

        var entity = await CreateBaseAsync(tenantId, userId, "ArRevenue", "FIN_AR", inv.Id, inv.Code,
            inv.InvoiceDate, inv.SubTotal, inv.TaxAmount, 0, inv.TotalAmount, req, ct);

        // AR đã đẩy JE doanh thu — liên kết, không tạo BT trùng.
        if (inv.FinJournalId is Guid jeId)
        {
            entity.FinJournalId = jeId;
            entity.FinJournalCode = inv.FinJournalCode;
            entity.PeriodId ??= inv.PeriodId;
            entity.DebitAccountId ??= inv.ArAccountId;
            entity.CreditAccountId ??= inv.RevenueAccountId;
        }
        else
        {
            await TryPostJeAsync(tenantId, userId, entity,
                $"DT AR {inv.Code}", inv.TotalAmount, "Phải thu KH", "Doanh thu AR", ct);
        }

        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinRevenueDocumentDto> RecognizeCogsAsync(
        Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
    {
        var existing = await FindExistingAsync(tenantId, "INV", invStockDocId, ct);
        if (existing is not null) return (await MapAsync(tenantId, [existing], ct))[0];

        var doc = await _db.InvStockDocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invStockDocId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy phiếu kho.", 404);
        if (doc.DocType != "Issue") throw new AppException("COGS chỉ từ phiếu xuất (Issue).");
        if (doc.Status != "Posted") throw new AppException("Phiếu kho chưa Posted.");

        var lines = await _db.InvStockDocLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DocId == invStockDocId && !x.IsDeleted)
            .ToListAsync(ct);
        var cogs = decimal.Round(lines.Sum(x => x.Qty * x.UnitCost), 2);
        if (cogs < 0) throw new AppException("Giá vốn không hợp lệ.");

        var entity = await CreateBaseAsync(tenantId, userId, "Cogs", "INV", doc.Id, doc.Code,
            doc.PostedAt ?? DateTimeOffset.UtcNow, 0, 0, cogs, cogs, req, ct);

        await TryPostJeAsync(tenantId, userId, entity,
            $"GVHB {doc.Code}", cogs, "Giá vốn hàng bán", "Xuất kho bán", ct);

        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinRevenueDocumentDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var entity = await _db.FinRevenueDocuments
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy chứng từ doanh thu.");
        if (entity.Status == "Void") throw new AppException("Chứng từ đã hủy.");
        if (entity.FinJournalId.HasValue)
            throw new AppException("Đã gắn BT — đảo BT trước khi hủy (Cap sau).");
        entity.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) entity.Note = note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    private async Task<FinRevenueDocument?> FindExistingAsync(
        Guid tenantId, string sourceModule, Guid sourceId, CancellationToken ct)
        => await _db.FinRevenueDocuments
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted
                                      && x.SourceModule == sourceModule && x.SourceId == sourceId
                                      && x.Status != "Void", ct);

    private async Task<FinRevenueDocument> CreateBaseAsync(
        Guid tenantId, Guid userId, string kind, string sourceModule, Guid sourceId, string sourceCode,
        DateTimeOffset docDate, decimal revenue, decimal tax, decimal cogs, decimal total,
        FinRevenueRecognizeRequest? req, CancellationToken ct)
    {
        if (req?.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }

        var prefix = kind switch
        {
            "PosRevenue" => "DT-POS",
            "OrderRevenue" => "DT-SO",
            "ArRevenue" => "DT-AR",
            "Cogs" => "GVHB",
            _ => "DT"
        };

        var entity = new FinRevenueDocument
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, prefix, ct),
            Kind = kind,
            SourceModule = sourceModule,
            SourceId = sourceId,
            SourceCode = sourceCode,
            DocDate = docDate,
            RevenueAmount = decimal.Round(revenue, 2),
            TaxAmount = decimal.Round(tax, 2),
            CogsAmount = decimal.Round(cogs, 2),
            TotalAmount = decimal.Round(total, 2),
            PeriodId = req?.PeriodId,
            DebitAccountId = req?.DebitAccountId,
            CreditAccountId = req?.CreditAccountId,
            Status = "Posted",
            PostedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
            CreatedBy = userId,
            Note = string.IsNullOrWhiteSpace(req?.Note) ? null : req.Note.Trim()
        };
        _db.FinRevenueDocuments.Add(entity);
        return entity;
    }

    private async Task TryPostJeAsync(
        Guid tenantId, Guid userId, FinRevenueDocument entity, string description,
        decimal amount, string debitNote, string creditNote, CancellationToken ct)
    {
        if (amount <= 0) return;
        if (entity.DebitAccountId is not Guid dr || entity.CreditAccountId is not Guid cr
            || entity.PeriodId is not Guid periodId)
            return;

        _ = await RequireAccount(tenantId, dr, ct);
        _ = await RequireAccount(tenantId, cr, ct);
        var period = await RequirePeriod(tenantId, periodId, ct);
        if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");

        var lines = new List<FinJournalLineUpsertRequest>
        {
            new(null, dr, amount, 0, null, null, debitNote),
            new(null, cr, 0, amount, null, null, creditNote),
        };
        var je = await _fin.CreateAutoJournalAsync(tenantId, userId, new FinJournalUpsertRequest(
            null, null, periodId, entity.DocDate, description, null, null, "Auto", lines), ct);
        je = await _fin.PostJournalAsync(tenantId, userId, je.Id, ct);
        entity.FinJournalId = je.Id;
        entity.FinJournalCode = je.Code;
    }

    private async Task<IReadOnlyList<FinRevenueDocumentDto>> MapAsync(
        Guid tenantId, List<FinRevenueDocument> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinRevenueDocumentDto>();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var aids = list.SelectMany(x => new[] { x.DebitAccountId, x.CreditAccountId })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var periods = await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var accounts = await _db.FinAccounts.AsNoTracking().Where(x => aids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, ct);

        return list.Select(d => new FinRevenueDocumentDto(
            d.Id, d.Code, d.Kind, d.SourceModule, d.SourceId, d.SourceCode,
            d.DocDate, d.RevenueAmount, d.TaxAmount, d.CogsAmount, d.TotalAmount,
            d.PeriodId, d.PeriodId is Guid pid ? periods.GetValueOrDefault(pid) : null,
            d.DebitAccountId, d.DebitAccountId is Guid dr ? accounts.GetValueOrDefault(dr) : null,
            d.CreditAccountId, d.CreditAccountId is Guid cr ? accounts.GetValueOrDefault(cr) : null,
            d.FinJournalId, d.FinJournalCode, d.Status, d.PostedAt, d.Note)).ToList();
    }

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<FinAccount> RequireAccount(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy tài khoản.");

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.FinRevenueDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }
}

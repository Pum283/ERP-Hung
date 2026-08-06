using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinVatService : IFinVatService
{
    private readonly AppDbContext _db;

    public FinVatService(AppDbContext db) => _db = db;

    public async Task<FinVatCalcResult> CalculateAsync(
        Guid tenantId, FinVatCalcRequest req, CancellationToken ct = default)
    {
        if (req.TaxableAmount < 0) throw new AppException("Tiền trước thuế ≥ 0.");
        decimal rate;
        Guid? taxId = req.TaxId;
        string? taxCode = null;
        if (req.TaxId is Guid tid)
        {
            var tax = await RequireTax(tenantId, tid, ct);
            rate = tax.RatePercent;
            taxCode = tax.Code;
        }
        else if (req.RatePercent is decimal r)
        {
            if (r is < 0 or > 100) throw new AppException("Thuế suất 0–100%.");
            rate = r;
        }
        else
        {
            var def = await _db.FinTaxes.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active" && x.IsDefault)
                .OrderBy(x => x.Code).FirstOrDefaultAsync(ct)
                ?? await _db.FinTaxes.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active")
                    .OrderBy(x => x.Code).FirstOrDefaultAsync(ct)
                ?? throw new AppException("Chưa cấu hình thuế suất.");
            rate = def.RatePercent;
            taxId = def.Id;
            taxCode = def.Code;
        }

        var taxable = decimal.Round(req.TaxableAmount, 2);
        var taxAmt = decimal.Round(taxable * rate / 100m, 2);
        return new FinVatCalcResult(taxable, rate, taxAmt, taxable + taxAmt, taxId, taxCode);
    }

    public async Task<IReadOnlyList<FinVatDocumentDto>> ListDocumentsAsync(
        Guid tenantId, string? direction = null, Guid? periodId = null, string? status = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var q = _db.FinVatDocuments.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(direction)) q = q.Where(x => x.Direction == direction.Trim());
        if (periodId is Guid pid) q = q.Where(x => x.PeriodId == pid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        if (from is DateTimeOffset f) q = q.Where(x => x.InvoiceDate >= f);
        if (to is DateTimeOffset t) q = q.Where(x => x.InvoiceDate <= t);
        var list = await q.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Code).Take(500).ToListAsync(ct);
        return await MapDocsAsync(tenantId, list, ct);
    }

    public async Task<FinVatDocumentDto> UpsertDocumentAsync(
        Guid tenantId, Guid userId, FinVatDocumentUpsertRequest req, CancellationToken ct = default)
    {
        var dir = (req.Direction ?? "").Trim();
        if (dir is not ("Output" or "Input")) throw new AppException("Direction: Output | Input.");
        var invNo = Req(req.InvoiceNo, 80, "Số HĐ");
        if (req.TaxableAmount < 0) throw new AppException("Tiền trước thuế ≥ 0.");
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }

        var calc = await CalculateAsync(tenantId, new FinVatCalcRequest(req.TaxableAmount, req.TaxId, req.RatePercent), ct);

        FinVatDocument entity;
        if (req.Id is Guid id)
        {
            entity = await RequireDoc(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa chứng từ Draft.");
        }
        else
        {
            entity = new FinVatDocument
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(tenantId, dir == "Output" ? "GTGT-R" : "GTGT-V", ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinVatDocuments.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã chứng từ GTGT đã tồn tại.");
            _db.FinVatDocuments.Add(entity);
        }

        entity.Direction = dir;
        entity.TaxId = calc.TaxId;
        entity.RatePercent = calc.RatePercent;
        entity.InvoiceNo = invNo;
        entity.InvoiceSeries = NullIfEmpty(req.InvoiceSeries);
        entity.InvoiceDate = req.InvoiceDate == default ? DateTimeOffset.UtcNow : req.InvoiceDate;
        entity.PartnerCode = NullIfEmpty(req.PartnerCode)?.ToUpperInvariant();
        entity.PartnerName = NullIfEmpty(req.PartnerName);
        entity.PartnerTaxCode = NullIfEmpty(req.PartnerTaxCode);
        entity.TaxableAmount = calc.TaxableAmount;
        entity.TaxAmount = calc.TaxAmount;
        entity.TotalAmount = calc.TotalAmount;
        entity.PeriodId = req.PeriodId;
        entity.ArInvoiceId = req.ArInvoiceId;
        entity.ApInvoiceId = req.ApInvoiceId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapDocsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinVatDocumentDto> PostDocumentAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var doc = await RequireDoc(tenantId, id, ct);
        if (doc.Status != "Draft") throw new AppException("Chỉ ghi nhận chứng từ Draft.");
        if (doc.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }
        doc.Status = "Posted";
        doc.PostedAt = DateTimeOffset.UtcNow;
        doc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapDocsAsync(tenantId, [doc], ct))[0];
    }

    public async Task<FinVatDocumentDto> VoidDocumentAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var doc = await RequireDoc(tenantId, id, ct);
        if (doc.Status == "Void") throw new AppException("Chứng từ đã hủy.");
        doc.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) doc.Note = note.Trim();
        doc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapDocsAsync(tenantId, [doc], ct))[0];
    }

    public async Task<FinVatSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var q = _db.FinVatDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Posted");
        string? periodCode = null;
        if (periodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            periodCode = period.Code;
            q = q.Where(x => x.PeriodId == pid);
        }
        if (from is DateTimeOffset f) q = q.Where(x => x.InvoiceDate >= f);
        if (to is DateTimeOffset t) q = q.Where(x => x.InvoiceDate <= t);

        var rows = await q.Select(x => new { x.Direction, x.TaxableAmount, x.TaxAmount }).ToListAsync(ct);
        var output = rows.Where(x => x.Direction == "Output").ToList();
        var input = rows.Where(x => x.Direction == "Input").ToList();
        var outTax = output.Sum(x => x.TaxAmount);
        var inTax = input.Sum(x => x.TaxAmount);
        return new FinVatSummaryDto(
            from, to, periodId, periodCode,
            output.Sum(x => x.TaxableAmount), outTax, output.Count,
            input.Sum(x => x.TaxableAmount), inTax, input.Count,
            outTax - inTax);
    }

    public async Task<FinVatDocumentDto> RegisterFromArAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, Guid? taxId = null, CancellationToken ct = default)
    {
        var existing = await _db.FinVatDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ArInvoiceId == arInvoiceId
                                      && !x.IsDeleted && x.Status != "Void", ct);
        if (existing is not null) return (await MapDocsAsync(tenantId, [existing], ct))[0];

        var inv = await _db.FinArInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == arInvoiceId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy HĐ AR.");
        if (inv.Status is "Draft" or "Void") throw new AppException("HĐ AR chưa ghi sổ.");

        var customer = await _db.CrmCustomers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == inv.CustomerId, ct);
        var taxable = inv.SubTotal > 0 ? inv.SubTotal : Math.Max(0, inv.TotalAmount - inv.TaxAmount);

        var doc = await UpsertDocumentAsync(tenantId, userId, new FinVatDocumentUpsertRequest(
            null, null, "Output", taxId, null,
            inv.CustomerInvoiceNo ?? inv.Code, null, inv.InvoiceDate,
            customer?.Code, customer?.DisplayName, customer?.TaxCode,
            taxable, inv.PeriodId, inv.Id, null, $"Từ AR {inv.Code}"), ct);
        return await PostDocumentAsync(tenantId, userId, doc.Id, ct);
    }

    public async Task<FinVatDocumentDto> RegisterFromApAsync(
        Guid tenantId, Guid userId, Guid apInvoiceId, Guid? taxId = null, CancellationToken ct = default)
    {
        var existing = await _db.FinVatDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ApInvoiceId == apInvoiceId
                                      && !x.IsDeleted && x.Status != "Void", ct);
        if (existing is not null)
        {
            var tracked = await RequireDoc(tenantId, existing.Id, ct);
            return (await MapDocsAsync(tenantId, [tracked], ct))[0];
        }

        var inv = await _db.FinApInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == apInvoiceId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy HĐ AP.");
        if (inv.Status is "Draft" or "Void") throw new AppException("HĐ AP chưa ghi sổ.");

        var vendor = await _db.PurVendors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == inv.VendorId, ct);
        var taxable = inv.SubTotal > 0 ? inv.SubTotal : Math.Max(0, inv.TotalAmount - inv.TaxAmount);

        var doc = await UpsertDocumentAsync(tenantId, userId, new FinVatDocumentUpsertRequest(
            null, null, "Input", taxId, null,
            inv.VendorInvoiceNo ?? inv.Code, null, inv.InvoiceDate,
            vendor?.Code, vendor?.Name, vendor?.TaxCode,
            taxable, inv.PeriodId, null, inv.Id, $"Từ AP {inv.Code}"), ct);
        return await PostDocumentAsync(tenantId, userId, doc.Id, ct);
    }

    private async Task<IReadOnlyList<FinVatDocumentDto>> MapDocsAsync(
        Guid tenantId, List<FinVatDocument> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinVatDocumentDto>();
        var taxIds = list.Where(x => x.TaxId.HasValue).Select(x => x.TaxId!.Value).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var taxes = taxIds.Count == 0 ? new Dictionary<Guid, FinTax>()
            : await _db.FinTaxes.AsNoTracking().Where(x => taxIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(d =>
        {
            FinTax? tax = null;
            if (d.TaxId is Guid tid) taxes.TryGetValue(tid, out tax);
            FinPeriod? pe = null;
            if (d.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            return new FinVatDocumentDto(
                d.Id, d.Code, d.Direction, d.TaxId, tax?.Code, d.RatePercent,
                d.InvoiceNo, d.InvoiceSeries, d.InvoiceDate,
                d.PartnerCode, d.PartnerName, d.PartnerTaxCode,
                d.TaxableAmount, d.TaxAmount, d.TotalAmount,
                d.PeriodId, pe?.Code, d.ArInvoiceId, d.ApInvoiceId,
                d.Status, d.PostedAt, d.Note);
        }).ToList();
    }

    private async Task<FinVatDocument> RequireDoc(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinVatDocuments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy chứng từ GTGT.");

    private async Task<FinTax> RequireTax(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinTaxes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy thuế suất.");

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.FinVatDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
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

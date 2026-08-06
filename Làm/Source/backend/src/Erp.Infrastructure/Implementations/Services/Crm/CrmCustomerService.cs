using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Crm;

public sealed class CrmCustomerService : ICrmCustomerService
{
    private static readonly HashSet<string> Types =
        new(StringComparer.OrdinalIgnoreCase) { "Person", "Organization" };
    private static readonly HashSet<string> Segments =
        new(StringComparer.OrdinalIgnoreCase) { "Lead", "Prospect", "Customer", "Partner" };
    private static readonly HashSet<string> Statuses =
        new(StringComparer.OrdinalIgnoreCase) { "Active", "Inactive", "Merged" };

    private readonly AppDbContext _db;

    public CrmCustomerService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CrmCustomerDto>> SearchAsync(
        Guid tenantId, CrmCustomerSearchRequest req, CancellationToken ct = default)
    {
        var q = _db.CrmCustomers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (!req.IncludeMerged)
            q = q.Where(x => x.Status != "Merged");

        if (!string.IsNullOrWhiteSpace(req.CustomerType))
            q = q.Where(x => x.CustomerType == req.CustomerType.Trim());
        if (!string.IsNullOrWhiteSpace(req.Segment))
            q = q.Where(x => x.Segment == req.Segment.Trim());
        if (!string.IsNullOrWhiteSpace(req.Status))
            q = q.Where(x => x.Status == req.Status.Trim());
        if (req.OwnerUserId is Guid oid)
            q = q.Where(x => x.OwnerUserId == oid);
        if (!string.IsNullOrWhiteSpace(req.Phone))
        {
            var phone = NormalizePhone(req.Phone) ?? req.Phone.Trim();
            q = q.Where(x => x.Phone != null && x.Phone.Contains(phone));
        }
        if (!string.IsNullOrWhiteSpace(req.TaxCode))
        {
            var tax = req.TaxCode.Trim();
            q = q.Where(x => x.TaxCode != null && x.TaxCode.Contains(tax));
        }
        if (!string.IsNullOrWhiteSpace(req.Q))
        {
            var term = req.Q.Trim();
            q = q.Where(x =>
                x.Code.Contains(term) || x.DisplayName.Contains(term)
                || (x.CompanyName != null && x.CompanyName.Contains(term))
                || (x.Email != null && x.Email.Contains(term))
                || (x.Phone != null && x.Phone.Contains(term))
                || (x.TaxCode != null && x.TaxCode.Contains(term)));
        }

        var list = await q.OrderBy(x => x.Code).Take(500).ToListAsync(ct);
        return await MapCustomersAsync(tenantId, list, ct);
    }

    public async Task<CrmCustomerDto> UpsertAsync(
        Guid tenantId, Guid userId, CrmCustomerUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        var name = (req.DisplayName ?? "").Trim();
        if (code.Length is < 1 or > 40) throw new AppException("Mã KH 1–40 ký tự.");
        if (name.Length is < 1 or > 200) throw new AppException("Tên hiển thị 1–200 ký tự.");

        var type = string.IsNullOrWhiteSpace(req.CustomerType) ? "Person" : req.CustomerType.Trim();
        if (!Types.Contains(type)) throw new AppException("Loại KH không hợp lệ.");
        var segment = string.IsNullOrWhiteSpace(req.Segment) ? "Prospect" : req.Segment.Trim();
        if (!Segments.Contains(segment)) throw new AppException("Phân loại tệp không hợp lệ.");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (!Statuses.Contains(status) || status == "Merged")
            throw new AppException("Trạng thái không hợp lệ.");

        if (string.Equals(type, "Organization", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(req.CompanyName) && string.IsNullOrWhiteSpace(name))
            throw new AppException("Khách DN cần tên công ty / tên hiển thị.");

        var phone = NormalizePhone(req.Phone);
        var tax = string.IsNullOrWhiteSpace(req.TaxCode) ? null : req.TaxCode.Trim();

        // UC_004 — chặn trùng SĐT / MST khi tạo hoặc đổi
        if (!string.IsNullOrEmpty(phone))
        {
            var dupPhone = await _db.CrmCustomers.AsNoTracking().AnyAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Merged"
                     && x.Phone == phone && (req.Id == null || x.Id != req.Id), ct);
            if (dupPhone) throw new AppException("SĐT đã tồn tại ở khách khác.");
        }
        if (!string.IsNullOrEmpty(tax))
        {
            var dupTax = await _db.CrmCustomers.AsNoTracking().AnyAsync(
                x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Merged"
                     && x.TaxCode == tax && (req.Id == null || x.Id != req.Id), ct);
            if (dupTax) throw new AppException("MST đã tồn tại ở khách khác.");
        }

        if (req.OwnerUserId is Guid ownerId)
        {
            var ok = await _db.Users.AnyAsync(x => x.Id == ownerId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (!ok) throw new AppException("Người phụ trách không tồn tại.", 404);
        }

        CrmCustomer entity;
        if (req.Id is Guid id)
        {
            entity = await _db.CrmCustomers.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Khách hàng không tồn tại.", 404);
            if (entity.Status == "Merged") throw new AppException("Khách đã gộp — không sửa.");
        }
        else
        {
            if (await _db.CrmCustomers.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã KH đã tồn tại.");
            entity = new CrmCustomer { TenantId = tenantId, CreatedBy = userId };
            _db.CrmCustomers.Add(entity);
        }

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase)
            && await _db.CrmCustomers.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && x.Id != entity.Id && !x.IsDeleted, ct))
            throw new AppException("Mã KH đã tồn tại.");

        entity.Code = code;
        entity.CustomerType = type;
        entity.DisplayName = name;
        entity.CompanyName = string.IsNullOrWhiteSpace(req.CompanyName) ? null : req.CompanyName.Trim();
        entity.Phone = phone;
        entity.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        entity.TaxCode = tax;
        entity.Segment = segment;
        entity.OwnerUserId = req.OwnerUserId;
        entity.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.PotentialScore = req.PotentialScore is >= 1 and <= 5 ? req.PotentialScore : null;
        entity.Status = status;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        return (await MapCustomersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<CrmCustomer360Dto> Get360Async(
        Guid tenantId, Guid customerId, CancellationToken ct = default)
    {
        var customer = await _db.CrmCustomers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == customerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách hàng không tồn tại.", 404);

        var dto = (await MapCustomersAsync(tenantId, [customer], ct))[0];
        var contacts = await ListContactsAsync(tenantId, customerId, ct);

        var handovers = await _db.CrmCustomerHandovers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.HandedAt)
            .ToListAsync(ct);
        var userIds = handovers
            .SelectMany(h => new[] { h.FromUserId, (Guid?)h.ToUserId })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var names = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        var handoverDtos = handovers.Select(h => new CrmHandoverDto(
            h.Id, h.CustomerId, h.FromUserId,
            h.FromUserId is Guid f ? names.GetValueOrDefault(f) : null,
            h.ToUserId, names.GetValueOrDefault(h.ToUserId),
            h.Note, h.HandedAt)).ToList();

        var dups = await FindDuplicatesAsync(tenantId, customer.Phone, customer.TaxCode, customer.Id, ct);
        return new CrmCustomer360Dto(dto, contacts, handoverDtos, dups);
    }

    public async Task<IReadOnlyList<CrmDuplicateHitDto>> FindDuplicatesAsync(
        Guid tenantId, string? phone, string? taxCode, Guid? excludeId, CancellationToken ct = default)
    {
        var hits = new List<CrmDuplicateHitDto>();
        var normPhone = NormalizePhone(phone);
        if (!string.IsNullOrEmpty(normPhone))
        {
            var byPhone = await _db.CrmCustomers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Merged"
                            && x.Phone == normPhone && (excludeId == null || x.Id != excludeId))
                .Take(20).ToListAsync(ct);
            hits.AddRange(byPhone.Select(x => new CrmDuplicateHitDto(
                x.Id, x.Code, x.DisplayName, x.Phone, x.TaxCode, "Phone")));
        }
        if (!string.IsNullOrWhiteSpace(taxCode))
        {
            var tax = taxCode.Trim();
            var byTax = await _db.CrmCustomers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Merged"
                            && x.TaxCode == tax && (excludeId == null || x.Id != excludeId))
                .Take(20).ToListAsync(ct);
            foreach (var x in byTax)
            {
                if (hits.Any(h => h.Id == x.Id)) continue;
                hits.Add(new CrmDuplicateHitDto(x.Id, x.Code, x.DisplayName, x.Phone, x.TaxCode, "TaxCode"));
            }
        }
        return hits;
    }

    public async Task<CrmCustomerDto> AssignOwnerAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmAssignOwnerRequest req, CancellationToken ct = default)
    {
        var entity = await _db.CrmCustomers.FirstOrDefaultAsync(
            x => x.Id == customerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách hàng không tồn tại.", 404);
        if (entity.Status == "Merged") throw new AppException("Khách đã gộp.");

        var ok = await _db.Users.AnyAsync(
            x => x.Id == req.OwnerUserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Người phụ trách không tồn tại.", 404);

        entity.OwnerUserId = req.OwnerUserId;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapCustomersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<CrmHandoverDto> HandoverAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmHandoverRequest req, CancellationToken ct = default)
    {
        var entity = await _db.CrmCustomers.FirstOrDefaultAsync(
            x => x.Id == customerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách hàng không tồn tại.", 404);
        if (entity.Status == "Merged") throw new AppException("Khách đã gộp.");

        var toOk = await _db.Users.AnyAsync(
            x => x.Id == req.ToUserId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!toOk) throw new AppException("Người nhận bàn giao không tồn tại.", 404);

        var from = entity.OwnerUserId;
        entity.OwnerUserId = req.ToUserId;
        entity.UpdatedBy = userId;

        var log = new CrmCustomerHandover
        {
            TenantId = tenantId,
            CustomerId = customerId,
            FromUserId = from,
            ToUserId = req.ToUserId,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            HandedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };
        _db.CrmCustomerHandovers.Add(log);
        await _db.SaveChangesAsync(ct);

        var names = await _db.Users.AsNoTracking()
            .Where(x => x.TenantId == tenantId && (x.Id == req.ToUserId || (from != null && x.Id == from)))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return new CrmHandoverDto(
            log.Id, log.CustomerId, log.FromUserId,
            from is Guid f ? names.GetValueOrDefault(f) : null,
            log.ToUserId, names.GetValueOrDefault(log.ToUserId),
            log.Note, log.HandedAt);
    }

    public async Task<CrmCustomerDto> MergeAsync(
        Guid tenantId, Guid userId, CrmMergeRequest req, CancellationToken ct = default)
    {
        if (req.SourceCustomerId == req.TargetCustomerId)
            throw new AppException("Không thể gộp khách vào chính nó.");

        var source = await _db.CrmCustomers.FirstOrDefaultAsync(
            x => x.Id == req.SourceCustomerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách nguồn không tồn tại.", 404);
        var target = await _db.CrmCustomers.FirstOrDefaultAsync(
            x => x.Id == req.TargetCustomerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách đích không tồn tại.", 404);
        if (source.Status == "Merged" || target.Status == "Merged")
            throw new AppException("Không gộp khách đã Merged.");

        var contacts = await _db.CrmContacts
            .Where(x => x.TenantId == tenantId && x.CustomerId == source.Id && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var c in contacts)
        {
            c.CustomerId = target.Id;
            c.UpdatedBy = userId;
        }

        var handovers = await _db.CrmCustomerHandovers
            .Where(x => x.TenantId == tenantId && x.CustomerId == source.Id && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var h in handovers)
        {
            h.CustomerId = target.Id;
            h.UpdatedBy = userId;
        }

        // Điền thông tin trống từ nguồn
        target.Phone ??= source.Phone;
        target.Email ??= source.Email;
        target.TaxCode ??= source.TaxCode;
        target.Address ??= source.Address;
        target.CompanyName ??= source.CompanyName;
        target.OwnerUserId ??= source.OwnerUserId;
        if (string.IsNullOrWhiteSpace(target.Note) && !string.IsNullOrWhiteSpace(source.Note))
            target.Note = source.Note;
        target.UpdatedBy = userId;

        source.Status = "Merged";
        source.MergedIntoId = target.Id;
        source.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        return (await MapCustomersAsync(tenantId, [target], ct))[0];
    }

    public async Task<IReadOnlyList<CrmContactDto>> ListContactsAsync(
        Guid tenantId, Guid customerId, CancellationToken ct = default)
    {
        var ok = await _db.CrmCustomers.AnyAsync(
            x => x.Id == customerId && x.TenantId == tenantId && !x.IsDeleted, ct);
        if (!ok) throw new AppException("Khách hàng không tồn tại.", 404);

        return await _db.CrmContacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.FullName)
            .Select(x => new CrmContactDto(x.Id, x.CustomerId, x.FullName, x.Title, x.Phone, x.Email, x.IsPrimary))
            .ToListAsync(ct);
    }

    public async Task<CrmContactDto> UpsertContactAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmContactUpsertRequest req, CancellationToken ct = default)
    {
        var customer = await _db.CrmCustomers.FirstOrDefaultAsync(
            x => x.Id == customerId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Khách hàng không tồn tại.", 404);
        if (customer.Status == "Merged") throw new AppException("Khách đã gộp.");

        var name = (req.FullName ?? "").Trim();
        if (name.Length is < 1 or > 200) throw new AppException("Tên liên hệ 1–200 ký tự.");

        CrmContact entity;
        if (req.Id is Guid id)
        {
            entity = await _db.CrmContacts.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && x.CustomerId == customerId && !x.IsDeleted, ct)
                ?? throw new AppException("Liên hệ không tồn tại.", 404);
        }
        else
        {
            entity = new CrmContact { TenantId = tenantId, CustomerId = customerId, CreatedBy = userId };
            _db.CrmContacts.Add(entity);
        }

        var makePrimary = req.IsPrimary ?? entity.IsPrimary;
        if (makePrimary)
        {
            var others = await _db.CrmContacts
                .Where(x => x.TenantId == tenantId && x.CustomerId == customerId && !x.IsDeleted && x.Id != entity.Id)
                .ToListAsync(ct);
            foreach (var o in others) o.IsPrimary = false;
        }

        entity.FullName = name;
        entity.Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim();
        entity.Phone = NormalizePhone(req.Phone);
        entity.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        entity.IsPrimary = makePrimary;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        return new CrmContactDto(
            entity.Id, entity.CustomerId, entity.FullName, entity.Title, entity.Phone, entity.Email, entity.IsPrimary);
    }

    public async Task<string> ExportCsvAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmCustomers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Merged")
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Code,CustomerType,DisplayName,CompanyName,Phone,Email,TaxCode,Segment,Status,Address");
        foreach (var c in list)
        {
            sb.Append(Escape(c.Code)).Append(',')
                .Append(Escape(c.CustomerType)).Append(',')
                .Append(Escape(c.DisplayName)).Append(',')
                .Append(Escape(c.CompanyName)).Append(',')
                .Append(Escape(c.Phone)).Append(',')
                .Append(Escape(c.Email)).Append(',')
                .Append(Escape(c.TaxCode)).Append(',')
                .Append(Escape(c.Segment)).Append(',')
                .Append(Escape(c.Status)).Append(',')
                .Append(Escape(c.Address))
                .AppendLine();
        }
        return sb.ToString();
    }

    public async Task<CrmImportResult> ImportCsvAsync(
        Guid tenantId, Guid userId, CrmImportRequest req, CancellationToken ct = default)
    {
        var text = req.CsvText ?? "";
        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) throw new AppException("CSV trống.");

        var start = 0;
        if (lines[0].Contains("Code", StringComparison.OrdinalIgnoreCase)
            && lines[0].Contains("DisplayName", StringComparison.OrdinalIgnoreCase))
            start = 1;

        var rows = new List<CrmImportRowResult>();
        var ok = 0;
        var fail = 0;

        for (var i = start; i < lines.Length; i++)
        {
            var cols = SplitCsv(lines[i]);
            if (cols.Count < 3)
            {
                rows.Add(new CrmImportRowResult($"L{i + 1}", false, "Thiếu cột"));
                fail++;
                continue;
            }

            var code = cols[0].Trim().ToUpperInvariant();
            try
            {
                await UpsertAsync(tenantId, userId, new CrmCustomerUpsertRequest(
                    null,
                    code,
                    cols.ElementAtOrDefault(1) is { Length: > 0 } t ? t : "Person",
                    cols.ElementAtOrDefault(2) ?? code,
                    NullIfEmpty(cols.ElementAtOrDefault(3)),
                    NullIfEmpty(cols.ElementAtOrDefault(4)),
                    NullIfEmpty(cols.ElementAtOrDefault(5)),
                    NullIfEmpty(cols.ElementAtOrDefault(6)),
                    NullIfEmpty(cols.ElementAtOrDefault(7)) ?? "Prospect",
                    null,
                    NullIfEmpty(cols.ElementAtOrDefault(9)),
                    null,
                    null,
                    "Active"), ct);
                rows.Add(new CrmImportRowResult(code, true, "OK"));
                ok++;
            }
            catch (Exception ex)
            {
                // Nếu mã trùng → cập nhật
                try
                {
                    var existing = await _db.CrmCustomers.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct);
                    if (existing is null) throw;

                    await UpsertAsync(tenantId, userId, new CrmCustomerUpsertRequest(
                        existing.Id,
                        code,
                        cols.ElementAtOrDefault(1) is { Length: > 0 } t2 ? t2 : existing.CustomerType,
                        cols.ElementAtOrDefault(2) ?? existing.DisplayName,
                        NullIfEmpty(cols.ElementAtOrDefault(3)) ?? existing.CompanyName,
                        NullIfEmpty(cols.ElementAtOrDefault(4)) ?? existing.Phone,
                        NullIfEmpty(cols.ElementAtOrDefault(5)) ?? existing.Email,
                        NullIfEmpty(cols.ElementAtOrDefault(6)) ?? existing.TaxCode,
                        NullIfEmpty(cols.ElementAtOrDefault(7)) ?? existing.Segment,
                        existing.OwnerUserId,
                        NullIfEmpty(cols.ElementAtOrDefault(9)) ?? existing.Address,
                        existing.Note,
                        existing.PotentialScore,
                        existing.Status == "Merged" ? "Active" : existing.Status), ct);
                    rows.Add(new CrmImportRowResult(code, true, "Updated"));
                    ok++;
                }
                catch
                {
                    rows.Add(new CrmImportRowResult(code, false, ex.Message));
                    fail++;
                }
            }
        }

        return new CrmImportResult(ok + fail, ok, fail, rows);
    }

    private async Task<IReadOnlyList<CrmCustomerDto>> MapCustomersAsync(
        Guid tenantId, List<CrmCustomer> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<CrmCustomerDto>();
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.CrmContacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.CustomerId) && !x.IsDeleted)
            .GroupBy(x => x.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count, ct);

        var ownerIds = list.Where(x => x.OwnerUserId.HasValue).Select(x => x.OwnerUserId!.Value).Distinct().ToList();
        var owners = ownerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.TenantId == tenantId && ownerIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return list.Select(c => new CrmCustomerDto(
            c.Id, c.Code, c.CustomerType, c.DisplayName, c.CompanyName, c.Phone, c.Email, c.TaxCode,
            c.Segment, c.OwnerUserId,
            c.OwnerUserId is Guid oid ? owners.GetValueOrDefault(oid) : null,
            c.Status, c.MergedIntoId, c.Address, c.Note, c.PotentialScore,
            counts.GetValueOrDefault(c.Id))).ToList();
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

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

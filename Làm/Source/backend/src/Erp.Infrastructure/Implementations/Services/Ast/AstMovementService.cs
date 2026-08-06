using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Erp.Domain.Entities.Ast;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Ast;

public sealed class AstMovementService : IAstMovementService
{
    private static readonly HashSet<string> DocTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Transfer", "Handover", "Disposal" };
    private static readonly HashSet<string> DisposalKinds =
        new(StringComparer.OrdinalIgnoreCase) { "Scrap", "Sale" };

    private readonly AppDbContext _db;
    public AstMovementService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AstMovementDocDto>> ListAsync(
        Guid tenantId, string? docType = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.AstMovementDocs.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(docType)) q = q.Where(x => x.DocType == docType.Trim());
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Code).Take(300).ToListAsync(ct);
        return await MapAsync(tenantId, list, ct);
    }

    public async Task<AstMovementDocDto> UpsertAsync(
        Guid tenantId, Guid userId, AstMovementUpsertRequest req, CancellationToken ct = default)
    {
        var docType = DocTypes.FirstOrDefault(x => x.Equals(req.DocType, StringComparison.OrdinalIgnoreCase))
            ?? throw new AppException("DocType: Transfer | Handover | Disposal.");
        var asset = await _db.AstAssets
            .FirstOrDefaultAsync(x => x.Id == req.AssetId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy tài sản.", 404);
        if (asset.Status == "Disposed" && docType != "Disposal")
            throw new AppException("TS đã thanh lý — không điều chuyển/bàn giao.");

        AstMovementDoc entity;
        if (req.Id is Guid id)
        {
            entity = await RequireDoc(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa chứng từ Draft.");
        }
        else
        {
            var prefix = docType switch
            {
                "Transfer" => "DC",
                "Handover" => "BG",
                _ => "TL"
            };
            entity = new AstMovementDoc
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(tenantId, prefix, ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.AstMovementDocs.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã chứng từ đã tồn tại.");
            _db.AstMovementDocs.Add(entity);
        }

        entity.DocType = docType;
        entity.DocDate = req.DocDate ?? DateTimeOffset.UtcNow;
        entity.AssetId = asset.Id;
        entity.FromLocationId = asset.LocationId;
        entity.FromEmployeeId = asset.AssignedEmployeeId;
        entity.FromEmployeeName = asset.AssignedEmployeeName;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;

        if (docType == "Transfer")
        {
            if (req.ToLocationId is not Guid toLoc) throw new AppException("Điều chuyển cần vị trí đích.");
            _ = await RequireLoc(tenantId, toLoc, ct);
            entity.ToLocationId = toLoc;
            entity.ToEmployeeId = null;
            entity.ToEmployeeName = null;
            entity.DisposalKind = null;
            entity.DisposalAmount = null;
        }
        else if (docType == "Handover")
        {
            if (req.ToEmployeeId is not Guid empId) throw new AppException("Bàn giao cần nhân viên nhận.");
            var emp = await _db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == empId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy nhân viên.", 404);
            entity.ToEmployeeId = emp.Id;
            entity.ToEmployeeName = string.IsNullOrWhiteSpace(req.ToEmployeeName)
                ? emp.FullName
                : req.ToEmployeeName.Trim();
            entity.ToLocationId = req.ToLocationId;
            if (req.ToLocationId is Guid lid) _ = await RequireLoc(tenantId, lid, ct);
            entity.DisposalKind = null;
            entity.DisposalAmount = null;
        }
        else
        {
            var kind = DisposalKinds.FirstOrDefault(x =>
                x.Equals(req.DisposalKind ?? "Scrap", StringComparison.OrdinalIgnoreCase))
                ?? throw new AppException("DisposalKind: Scrap | Sale.");
            if (req.DisposalAmount is decimal amt && amt < 0) throw new AppException("Số tiền thanh lý ≥ 0.");
            entity.DisposalKind = kind;
            entity.DisposalAmount = req.DisposalAmount ?? 0;
            entity.BookValueSnapshot = asset.BookValue;
            entity.ToLocationId = null;
            entity.ToEmployeeId = null;
            entity.ToEmployeeName = null;
        }

        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [entity], ct))[0];
    }

    public async Task<AstMovementDocDto> PostAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var doc = await RequireDoc(tenantId, id, ct);
        if (doc.Status != "Draft") throw new AppException("Chỉ ghi sổ chứng từ Draft.");
        var asset = await _db.AstAssets
            .FirstOrDefaultAsync(x => x.Id == doc.AssetId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy tài sản.", 404);
        if (asset.Status == "Disposed") throw new AppException("TS đã thanh lý.");

        if (doc.DocType == "Transfer")
        {
            if (doc.ToLocationId is not Guid toLoc) throw new AppException("Thiếu vị trí đích.");
            asset.LocationId = toLoc;
        }
        else if (doc.DocType == "Handover")
        {
            if (doc.ToEmployeeId is not Guid) throw new AppException("Thiếu nhân viên nhận.");
            asset.AssignedEmployeeId = doc.ToEmployeeId;
            asset.AssignedEmployeeName = doc.ToEmployeeName;
            if (doc.ToLocationId is Guid loc) asset.LocationId = loc;
        }
        else if (doc.DocType == "Disposal")
        {
            if (asset.Status != "Active") throw new AppException("Chỉ thanh lý TS Active.");
            asset.Status = "Disposed";
            asset.DisposedAt = doc.DocDate;
            asset.DisposalAmount = doc.DisposalAmount ?? 0;
            doc.BookValueSnapshot = asset.BookValue;
        }

        doc.Status = "Posted";
        doc.PostedAt = DateTimeOffset.UtcNow;
        doc.UpdatedBy = userId;
        asset.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [doc], ct))[0];
    }

    public async Task<AstMovementDocDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var doc = await RequireDoc(tenantId, id, ct);
        if (doc.Status == "Void") throw new AppException("Chứng từ đã hủy.");
        if (doc.Status == "Posted")
            throw new AppException("Đã ghi sổ — không hủy (Cap sau: đảo điều chuyển).");
        doc.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) doc.Note = note.Trim();
        doc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAsync(tenantId, [doc], ct))[0];
    }

    private async Task<IReadOnlyList<AstMovementDocDto>> MapAsync(
        Guid tenantId, List<AstMovementDoc> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<AstMovementDocDto>();
        var aids = list.Select(x => x.AssetId).Distinct().ToList();
        var lids = list.SelectMany(x => new[] { x.FromLocationId, x.ToLocationId })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var assets = await _db.AstAssets.AsNoTracking().Where(x => aids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return list.Select(d =>
        {
            assets.TryGetValue(d.AssetId, out var a);
            return new AstMovementDocDto(
                d.Id, d.Code, d.DocType, d.DocDate, d.AssetId, a?.Code, a?.Name,
                d.FromLocationId, d.FromLocationId is Guid fl ? locs.GetValueOrDefault(fl) : null,
                d.ToLocationId, d.ToLocationId is Guid tl ? locs.GetValueOrDefault(tl) : null,
                d.FromEmployeeId, d.FromEmployeeName, d.ToEmployeeId, d.ToEmployeeName,
                d.DisposalKind, d.DisposalAmount, d.BookValueSnapshot,
                d.Status, d.PostedAt, d.Note);
        }).ToList();
    }

    private async Task<AstMovementDoc> RequireDoc(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.AstMovementDocs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy chứng từ AST.");

    private async Task<AstLocation> RequireLoc(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.AstLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy vị trí.");

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.AstMovementDocs.AsNoTracking()
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
}

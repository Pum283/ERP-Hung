using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Erp.Domain.Entities.Ast;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Ast;

public sealed class AstStocktakeService : IAstStocktakeService
{
    private readonly AppDbContext _db;
    public AstStocktakeService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AstStocktakeDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.AstStocktakes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        return await MapHeadersAsync(tenantId, list, ct);
    }

    public async Task<AstStocktakeDetailDto> GetDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var st = await Require(tenantId, id, ct);
        var lines = await _db.AstStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StocktakeId == id && !x.IsDeleted)
            .OrderBy(x => x.AssetCode).ToListAsync(ct);
        var header = (await MapHeadersAsync(tenantId, [st], ct))[0];
        return new AstStocktakeDetailDto(header, lines.Select(MapLine).ToList());
    }

    public async Task<AstStocktakeDto> CreateAsync(
        Guid tenantId, Guid userId, AstStocktakeCreateRequest req, CancellationToken ct = default)
    {
        if (req.LocationId is Guid lid)
            _ = await RequireLoc(tenantId, lid, ct);

        var st = new AstStocktake
        {
            TenantId = tenantId,
            Code = await NextCodeAsync(tenantId, ct),
            LocationId = req.LocationId,
            Status = "Counting",
            CreatedByUserId = userId,
            CreatedBy = userId,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim()
        };
        _db.AstStocktakes.Add(st);
        await _db.SaveChangesAsync(ct);

        var q = _db.AstAssets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active");
        if (req.LocationId is Guid locFilter)
            q = q.Where(x => x.LocationId == locFilter);

        var assets = await q.OrderBy(x => x.Code).Take(2000).ToListAsync(ct);
        if (assets.Count == 0) throw new AppException("Không có TS Active để kiểm kê.");

        var lids = assets.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        foreach (var a in assets)
        {
            _db.AstStocktakeLines.Add(new AstStocktakeLine
            {
                TenantId = tenantId,
                StocktakeId = st.Id,
                AssetId = a.Id,
                AssetCode = a.Code,
                AssetName = a.Name,
                LocationId = a.LocationId,
                LocationName = a.LocationId is Guid l ? locs.GetValueOrDefault(l) : null,
                ExpectedPresent = 1,
                CountedPresent = null,
                Variance = 0,
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapHeadersAsync(tenantId, [st], ct))[0];
    }

    public async Task<AstStocktakeLineDto> CountLineAsync(
        Guid tenantId, Guid userId, Guid stocktakeId, AstStocktakeCountRequest req, CancellationToken ct = default)
    {
        var st = await Require(tenantId, stocktakeId, ct);
        if (st.Status is not "Counting") throw new AppException("Chỉ đếm khi đang Counting.");
        var line = await _db.AstStocktakeLines
            .FirstOrDefaultAsync(x => x.Id == req.LineId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy dòng kiểm kê.");
        if (line.StocktakeId != stocktakeId) throw new AppException("Dòng không thuộc đợt KK.");

        line.CountedPresent = req.CountedPresent;
        line.Variance = (req.CountedPresent ? 1 : 0) - line.ExpectedPresent;
        if (!string.IsNullOrWhiteSpace(req.Note)) line.Note = req.Note.Trim();
        line.UpdatedBy = userId;
        st.CountedAt = DateTimeOffset.UtcNow;
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapLine(line);
    }

    public async Task<AstStocktakeDto> ReviewAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var st = await Require(tenantId, id, ct);
        if (st.Status != "Counting") throw new AppException("Cần đang Counting.");
        var uncounted = await _db.AstStocktakeLines.CountAsync(
            x => x.TenantId == tenantId && x.StocktakeId == id && !x.IsDeleted && x.CountedPresent == null, ct);
        if (uncounted > 0) throw new AppException($"Còn {uncounted} dòng chưa đếm.");
        st.Status = "Reviewed";
        st.ReviewedAt = DateTimeOffset.UtcNow;
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapHeadersAsync(tenantId, [st], ct))[0];
    }

    public async Task<AstStocktakeDto> CloseAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var st = await Require(tenantId, id, ct);
        if (st.Status != "Reviewed") throw new AppException("Cần Reviewed trước khi đóng.");
        st.Status = "Closed";
        st.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapHeadersAsync(tenantId, [st], ct))[0];
    }

    public async Task<IReadOnlyList<AstStocktakeLineDto>> ListVariancesAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        _ = await Require(tenantId, id, ct);
        var lines = await _db.AstStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StocktakeId == id && !x.IsDeleted
                        && x.CountedPresent != null && x.Variance != 0)
            .OrderBy(x => x.AssetCode).ToListAsync(ct);
        return lines.Select(MapLine).ToList();
    }

    private async Task<IReadOnlyList<AstStocktakeDto>> MapHeadersAsync(
        Guid tenantId, List<AstStocktake> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<AstStocktakeDto>();
        var ids = list.Select(x => x.Id).ToList();
        var lids = list.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var stats = await _db.AstStocktakeLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.StocktakeId) && !x.IsDeleted)
            .GroupBy(x => x.StocktakeId)
            .Select(g => new
            {
                g.Key,
                Total = g.Count(),
                Counted = g.Count(x => x.CountedPresent != null),
                Variance = g.Count(x => x.CountedPresent != null && x.Variance != 0)
            }).ToDictionaryAsync(x => x.Key, ct);

        return list.Select(st =>
        {
            stats.TryGetValue(st.Id, out var s);
            return new AstStocktakeDto(
                st.Id, st.Code, st.LocationId,
                st.LocationId is Guid l ? locs.GetValueOrDefault(l) : "Tất cả vị trí",
                st.Status, s?.Total ?? 0, s?.Counted ?? 0, s?.Variance ?? 0,
                st.CountedAt, st.ReviewedAt, st.Note);
        }).ToList();
    }

    private static AstStocktakeLineDto MapLine(AstStocktakeLine x) =>
        new(x.Id, x.StocktakeId, x.AssetId, x.AssetCode, x.AssetName,
            x.LocationId, x.LocationName, x.ExpectedPresent, x.CountedPresent, x.Variance, x.Note);

    private async Task<AstStocktake> Require(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.AstStocktakes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy đợt kiểm kê.", 404);

    private async Task<AstLocation> RequireLoc(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.AstLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy vị trí.");

    private async Task<string> NextCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var stem = $"KK-{DateTime.UtcNow:yyyyMMdd}-";
        var last = await _db.AstStocktakes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }
}

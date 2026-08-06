using System.Globalization;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Ast;

public sealed class AstReportService : IAstReportService
{
    private readonly AppDbContext _db;
    private readonly IAstAssetService _assets;

    public AstReportService(AppDbContext db, IAstAssetService assets)
    {
        _db = db;
        _assets = assets;
    }

    public async Task<IReadOnlyList<AstRegisterRowDto>> RegisterAsync(
        Guid tenantId, string? status = null, Guid? locationId = null, Guid? groupId = null,
        CancellationToken ct = default)
    {
        var q = _db.AstAssets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        if (locationId is Guid lid) q = q.Where(x => x.LocationId == lid);
        if (groupId is Guid gid) q = q.Where(x => x.GroupId == gid);
        var list = await q.OrderBy(x => x.Code).Take(2000).ToListAsync(ct);
        return await MapRegisterAsync(list, ct);
    }

    public async Task<AstDepreciationReportDto> DepreciationAsync(
        Guid tenantId, int year, int month, CancellationToken ct = default)
    {
        if (year < 2000 || month is < 1 or > 12) throw new AppException("Năm/tháng không hợp lệ.");
        var run = await _db.AstDepreciationRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted
                                      && x.Year == year && x.Month == month, ct);
        if (run is null)
            return new AstDepreciationReportDto(null, null, year, month, null, 0, 0, Array.Empty<AstDepreciationLineDto>());

        var detail = await _assets.GetRunDetailAsync(tenantId, run.Id, ct);
        return new AstDepreciationReportDto(
            run.Id, run.Code, year, month, run.Status, run.TotalAmount, run.LineCount, detail.Lines);
    }

    public async Task<IReadOnlyList<AstByLocationRowDto>> ByLocationAsync(
        Guid tenantId, Guid? locationId = null, CancellationToken ct = default)
    {
        var q = _db.AstAssets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Disposed");
        if (locationId is Guid lid) q = q.Where(x => x.LocationId == lid);
        var assets = await q.ToListAsync(ct);
        var lids = assets.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return assets.GroupBy(x => x.LocationId).Select(g =>
        {
            var name = g.Key is Guid id ? locs.GetValueOrDefault(id) ?? "(không tên)" : "(chưa gắn vị trí)";
            return new AstByLocationRowDto(
                g.Key, name, g.Count(),
                g.Sum(x => x.OriginalCost),
                g.Sum(x => x.AccumulatedDepreciation),
                g.Sum(x => x.BookValue));
        }).OrderBy(x => x.LocationName).ToList();
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId, string report, string? status = null, Guid? locationId = null,
        Guid? groupId = null, int? year = null, int? month = null, CancellationToken ct = default)
    {
        var kind = (report ?? "").Trim().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        if (kind is "register" or "so" or "030")
        {
            var rows = await RegisterAsync(tenantId, status, locationId, groupId, ct);
            sb.AppendLine("Code,Name,Group,Location,Method,Assignee,OriginalCost,AccumDep,BookValue,Status,CapitalizeDate,DisposedAt");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(',',
                    Csv(r.Code), Csv(r.Name), Csv(r.GroupName), Csv(r.LocationName), Csv(r.MethodName),
                    Csv(r.AssignedEmployeeName), N(r.OriginalCost), N(r.AccumulatedDepreciation), N(r.BookValue),
                    Csv(r.Status), Csv(r.CapitalizeDate?.ToString("yyyy-MM-dd")), Csv(r.DisposedAt?.ToString("yyyy-MM-dd"))));
            }
            return sb.ToString();
        }

        if (kind is "by-location" or "location" or "032")
        {
            var rows = await ByLocationAsync(tenantId, locationId, ct);
            sb.AppendLine("Location,AssetCount,OriginalCost,AccumDep,BookValue");
            foreach (var r in rows)
                sb.AppendLine(string.Join(',', Csv(r.LocationName), r.AssetCount, N(r.OriginalCost), N(r.AccumulatedDepreciation), N(r.BookValue)));
            return sb.ToString();
        }

        if (kind is "depreciation" or "kh" or "031")
        {
            if (year is not int y || month is not int m)
                throw new AppException("Xuất KH cần year & month.");
            var rep = await DepreciationAsync(tenantId, y, m, ct);
            sb.AppendLine($"Run,{Csv(rep.RunCode)},Year,{rep.Year},Month,{rep.Month},Status,{Csv(rep.Status)},Total,{N(rep.TotalAmount)}");
            sb.AppendLine("LineNo,AssetCode,AssetName,Amount,BookValueBefore,BookValueAfter");
            foreach (var l in rep.Lines)
                sb.AppendLine(string.Join(',', l.LineNo, Csv(l.AssetCode), Csv(l.AssetName), N(l.Amount), N(l.BookValueBefore), N(l.BookValueAfter)));
            return sb.ToString();
        }

        throw new AppException("report: register | depreciation | by-location.");
    }

    private async Task<IReadOnlyList<AstRegisterRowDto>> MapRegisterAsync(
        List<Domain.Entities.Ast.AstAsset> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<AstRegisterRowDto>();
        var gids = list.Where(x => x.GroupId.HasValue).Select(x => x.GroupId!.Value).Distinct().ToList();
        var lids = list.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var mids = list.Where(x => x.DepreciationMethodId.HasValue).Select(x => x.DepreciationMethodId!.Value).Distinct().ToList();
        var groups = gids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstAssetGroups.AsNoTracking().Where(x => gids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var methods = mids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstDepreciationMethods.AsNoTracking().Where(x => mids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return list.Select(a => new AstRegisterRowDto(
            a.Id, a.Code, a.Name,
            a.GroupId is Guid g ? groups.GetValueOrDefault(g) : null,
            a.LocationId is Guid l ? locs.GetValueOrDefault(l) : null,
            a.DepreciationMethodId is Guid m ? methods.GetValueOrDefault(m) : null,
            a.AssignedEmployeeName, a.OriginalCost, a.AccumulatedDepreciation, a.BookValue,
            a.Status, a.CapitalizeDate, a.DisposedAt)).ToList();
    }

    private static string Csv(string? s)
    {
        var v = s ?? "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }

    private static string N(decimal n) => n.ToString(CultureInfo.InvariantCulture);
}

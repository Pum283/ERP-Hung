using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Prt;
using Erp.Application.Interfaces.Services.Prt;
using Erp.Domain.Entities.Prt;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Prt;

public sealed class PrtPackageService : IPrtPackageService
{
    private static readonly string[] DefaultPlans = ["STARTER", "STANDARD", "ENTERPRISE"];
    private static readonly Dictionary<string, Dictionary<string, bool>> PlanDefaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["STARTER"] = new() { ["orders"] = true, ["ar"] = true, ["tickets"] = false, ["vendor"] = false, ["docs"] = false },
        ["STANDARD"] = new() { ["orders"] = true, ["ar"] = true, ["tickets"] = true, ["vendor"] = false, ["docs"] = true },
        ["ENTERPRISE"] = new() { ["orders"] = true, ["ar"] = true, ["tickets"] = true, ["vendor"] = true, ["docs"] = true },
    };

    private readonly AppDbContext _db;
    public PrtPackageService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PrtPortalPackageDto>> ListPackagesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        await EnsureDefaultsAsync(tenantId, ct);
        var list = await _db.PrtPortalPackages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.PlanCode).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<PrtPortalPackageDto> UpsertPackageAsync(
        Guid tenantId, Guid userId, PrtPortalPackageUpsertRequest req, CancellationToken ct = default)
    {
        var plan = (req.PlanCode ?? "").Trim().ToUpperInvariant();
        if (plan.Length is < 2 or > 40) throw new AppException("PlanCode 2–40 ký tự.");
        var name = (req.Name ?? "").Trim();
        if (name.Length is < 1 or > 200) throw new AppException("Tên gói 1–200 ký tự.");

        var features = req.Features ?? ParseFeatures(req.FeaturesJson);
        var json = JsonSerializer.Serialize(features);

        PrtPortalPackage entity;
        if (req.Id is Guid id)
        {
            entity = await _db.PrtPortalPackages
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy gói portal.", 404);
        }
        else
        {
            entity = await _db.PrtPortalPackages
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PlanCode == plan && !x.IsDeleted, ct)
                ?? new PrtPortalPackage { TenantId = tenantId, PlanCode = plan, CreatedBy = userId };
            if (entity.Id == Guid.Empty) _db.PrtPortalPackages.Add(entity);
        }

        entity.PlanCode = plan;
        entity.Name = name;
        entity.FeaturesJson = json;
        entity.IsActive = req.IsActive ?? true;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PrtEnabledFeaturesDto> GetEnabledFeaturesAsync(
        Guid tenantId, string? planCode = null, CancellationToken ct = default)
    {
        await EnsureDefaultsAsync(tenantId, ct);
        var plan = planCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(plan))
        {
            plan = await _db.Licenses.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active")
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.PlanCode)
                .FirstOrDefaultAsync(ct) ?? "STANDARD";
        }

        var pkg = await _db.PrtPortalPackages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PlanCode == plan && !x.IsDeleted && x.IsActive, ct)
            ?? await _db.PrtPortalPackages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PlanCode == "STANDARD" && !x.IsDeleted, ct);

        var features = pkg is null ? PlanDefaults["STANDARD"] : ParseFeatures(pkg.FeaturesJson);
        var enabled = features.Where(kv => kv.Value).Select(kv => kv.Key).OrderBy(x => x).ToList();
        return new PrtEnabledFeaturesDto(pkg?.PlanCode ?? plan, enabled);
    }

    private async Task EnsureDefaultsAsync(Guid tenantId, CancellationToken ct)
    {
        var existing = await _db.PrtPortalPackages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => x.PlanCode).ToListAsync(ct);
        var added = false;
        foreach (var plan in DefaultPlans)
        {
            if (existing.Any(x => x.Equals(plan, StringComparison.OrdinalIgnoreCase))) continue;
            var feats = PlanDefaults[plan];
            _db.PrtPortalPackages.Add(new PrtPortalPackage
            {
                TenantId = tenantId,
                PlanCode = plan,
                Name = plan switch
                {
                    "STARTER" => "Gói Starter",
                    "ENTERPRISE" => "Gói Enterprise",
                    _ => "Gói Standard"
                },
                FeaturesJson = JsonSerializer.Serialize(feats),
                IsActive = true,
                CreatedBy = Guid.Empty
            });
            added = true;
        }
        if (added) await _db.SaveChangesAsync(ct);
    }

    private static PrtPortalPackageDto Map(PrtPortalPackage p)
    {
        var feats = ParseFeatures(p.FeaturesJson);
        return new PrtPortalPackageDto(p.Id, p.PlanCode, p.Name, p.FeaturesJson, feats, p.IsActive, p.Note);
    }

    private static Dictionary<string, bool> ParseFeatures(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, bool>>(json)
                ?? new Dictionary<string, bool>();
            return new Dictionary<string, bool>(dict, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

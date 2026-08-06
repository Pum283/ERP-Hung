using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mod;
using Erp.Application.Interfaces.Services.Mod;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Mod;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Mod;

public sealed class ModModuleService : IModModuleService
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "LMS", "CRM", "POS", "PUR", "INV", "LOG", "MFG", "FSM", "PJM", "FIN", "AST", "BI", "PRT", "HRM", "WF"
    };

    private readonly AppDbContext _db;
    private readonly ISysPlatformService _platform;

    public ModModuleService(AppDbContext db, ISysPlatformService platform)
    {
        _db = db;
        _platform = platform;
    }

    public async Task<IReadOnlyList<ModMasterDto>> ListMastersAsync(Guid tenantId, string moduleCode, string? recordType, CancellationToken ct = default)
    {
        EnsureModule(moduleCode);
        var q = _db.ModMasters.AsNoTracking().Where(x => x.TenantId == tenantId && x.ModuleCode == moduleCode.ToUpperInvariant() && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(recordType))
            q = q.Where(x => x.RecordType == recordType);
        return await q.OrderBy(x => x.RecordType).ThenBy(x => x.Code)
            .Select(x => new ModMasterDto(x.Id, x.ModuleCode, x.RecordType, x.Code, x.Name, x.Status, x.PayloadJson))
            .ToListAsync(ct);
    }

    public async Task<ModMasterDto> UpsertMasterAsync(Guid tenantId, Guid? actorId, string moduleCode, ModMasterUpsertRequest req, CancellationToken ct = default)
    {
        var mod = EnsureModule(moduleCode);
        ModMaster e;
        if (req.Id is Guid id)
            e = await _db.ModMasters.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.ModuleCode == mod && !x.IsDeleted, ct)
                ?? throw new AppException("Master không tồn tại.", 404);
        else
        {
            e = new ModMaster { TenantId = tenantId, ModuleCode = mod, CreatedBy = actorId };
            _db.ModMasters.Add(e);
        }
        e.RecordType = req.RecordType.Trim();
        e.Code = req.Code.Trim();
        e.Name = req.Name.Trim();
        e.Status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status;
        e.PayloadJson = req.PayloadJson;
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new ModMasterDto(e.Id, e.ModuleCode, e.RecordType, e.Code, e.Name, e.Status, e.PayloadJson);
    }

    public async Task<IReadOnlyList<ModDocumentDto>> ListDocumentsAsync(Guid tenantId, string moduleCode, string? docType, string? status, CancellationToken ct = default)
    {
        var mod = EnsureModule(moduleCode);
        var q = _db.ModDocuments.AsNoTracking().Where(x => x.TenantId == tenantId && x.ModuleCode == mod && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(docType)) q = q.Where(x => x.DocType == docType);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
        return await q.OrderByDescending(x => x.CreatedAt).Take(200)
            .Select(x => new ModDocumentDto(x.Id, x.ModuleCode, x.DocType, x.DocNo, x.Title, x.Status, x.OwnerUserId, x.RefMasterId, x.PayloadJson, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ModDocumentDto> UpsertDocumentAsync(Guid tenantId, Guid? actorId, string moduleCode, ModDocumentUpsertRequest req, CancellationToken ct = default)
    {
        var mod = EnsureModule(moduleCode);
        ModDocument e;
        if (req.Id is Guid id)
            e = await _db.ModDocuments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.ModuleCode == mod && !x.IsDeleted, ct)
                ?? throw new AppException("Chứng từ không tồn tại.", 404);
        else
        {
            var docNo = string.IsNullOrWhiteSpace(req.DocNo)
                ? await _platform.NextNumberAsync(tenantId, $"{mod}.{req.DocType}", ct)
                : req.DocNo.Trim();
            e = new ModDocument
            {
                TenantId = tenantId, ModuleCode = mod, DocType = req.DocType.Trim(), DocNo = docNo, CreatedBy = actorId
            };
            _db.ModDocuments.Add(e);
        }
        e.Title = req.Title.Trim();
        e.Status = string.IsNullOrWhiteSpace(req.Status) ? e.Status : req.Status;
        e.OwnerUserId = req.OwnerUserId ?? actorId;
        e.RefMasterId = req.RefMasterId;
        e.PayloadJson = req.PayloadJson;
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new ModDocumentDto(e.Id, e.ModuleCode, e.DocType, e.DocNo, e.Title, e.Status, e.OwnerUserId, e.RefMasterId, e.PayloadJson, e.CreatedAt);
    }

    public async Task<ModDocumentDto> TransitionDocumentAsync(Guid tenantId, Guid id, string newStatus, CancellationToken ct = default)
    {
        var e = await _db.ModDocuments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Chứng từ không tồn tại.", 404);
        e.Status = newStatus.Trim();
        e.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new ModDocumentDto(e.Id, e.ModuleCode, e.DocType, e.DocNo, e.Title, e.Status, e.OwnerUserId, e.RefMasterId, e.PayloadJson, e.CreatedAt);
    }

    private static string EnsureModule(string moduleCode)
    {
        var m = moduleCode.Trim().ToUpperInvariant();
        if (!Allowed.Contains(m)) throw new AppException($"Module `{m}` không hỗ trợ.");
        return m;
    }
}

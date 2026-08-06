using System.Text.Json;
using Erp.Domain.Base;
using Erp.Domain.Entities.Sys;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Erp.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;

    public AuditSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        WriteAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        WriteAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAudit(DbContext? context)
    {
        if (context is null) return;

        var userIdStr = _http.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? _http.HttpContext?.User?.FindFirst("sub")?.Value;
        Guid? actor = Guid.TryParse(userIdStr, out var uid) ? uid : null;
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity and not AuditLog
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is not BaseEntity entity) continue;

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            Guid tenantId = entity is TenantEntity te ? te.TenantId
                : Guid.TryParse(_http.HttpContext?.User?.FindFirst("tenant_id")?.Value, out var tid) ? tid : Guid.Empty;

            if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTimeOffset.UtcNow;
                entity.UpdatedBy = actor;
                entity.RowVersion += 1;
            }

            if (entry.State == EntityState.Deleted && entity is BaseEntity soft)
            {
                // soft-delete pattern: convert delete → modify flags
                entry.State = EntityState.Modified;
                soft.IsDeleted = true;
                soft.DeletedAt = DateTimeOffset.UtcNow;
                soft.UpdatedAt = DateTimeOffset.UtcNow;
                soft.UpdatedBy = actor;
                action = "SoftDelete";
            }

            context.Set<AuditLog>().Add(new AuditLog
            {
                TenantId = tenantId,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entity.Id,
                Action = action,
                BeforeJson = entry.State == EntityState.Added ? null : SafeSerialize(entry.OriginalValues.ToObject()),
                AfterJson = action == "SoftDelete" ? null : SafeSerialize(entry.CurrentValues.ToObject()),
                ActorUserId = actor,
                IpAddress = ip,
                CreatedBy = actor,
                UpdatedBy = actor
            });
        }
    }

    private static string? SafeSerialize(object? obj)
    {
        try { return obj is null ? null : JsonSerializer.Serialize(obj); }
        catch { return null; }
    }
}

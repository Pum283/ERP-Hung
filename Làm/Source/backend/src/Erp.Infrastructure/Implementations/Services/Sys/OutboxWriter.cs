using System.Text.Json;
using Erp.Application.Common;
using Erp.Application.Interfaces.Realtime;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly AppDbContext _db;

    public OutboxWriter(AppDbContext db) => _db = db;

    public async Task EnqueueAsync(
        Guid tenantId,
        string eventType,
        string sourceModule,
        object payload,
        Guid? correlationId = null,
        CancellationToken ct = default)
    {
        _db.OutboxMessages.Add(new OutboxMessage
        {
            TenantId = tenantId,
            EventType = eventType,
            SourceModule = sourceModule,
            CorrelationId = correlationId ?? CorrelationContext.Current,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = "Pending",
            NextAttemptAt = DateTimeOffset.UtcNow
        });
        // Không SaveChanges — ghi cùng transaction với nghiệp vụ gọi.
        await Task.CompletedTask;
    }
}

public sealed class InboxStore : IInboxStore
{
    private readonly AppDbContext _db;

    public InboxStore(AppDbContext db) => _db = db;

    public async Task<bool> TryBeginProcessAsync(
        Guid tenantId,
        Guid eventId,
        string consumer,
        string eventType,
        CancellationToken ct = default)
    {
        var exists = await _db.InboxMessages.AnyAsync(
            x => x.TenantId == tenantId && x.EventId == eventId && x.Consumer == consumer && !x.IsDeleted, ct);
        if (exists) return false;

        _db.InboxMessages.Add(new InboxMessage
        {
            TenantId = tenantId,
            EventId = eventId,
            Consumer = consumer,
            EventType = eventType,
            Status = "Processed",
            ProcessedAt = DateTimeOffset.UtcNow
        });
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}

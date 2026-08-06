using Erp.Application.Interfaces.Realtime;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Erp.Infrastructure.Background;

/// <summary>Dispatcher Outbox tối thiểu — publish in-process (đánh dấu Published).</summary>
public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _log;

    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherHostedService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogWarning(ex, "Outbox dispatcher lỗi tạm");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var inbox = scope.ServiceProvider.GetRequiredService<IInboxStore>();

        var batch = await db.OutboxMessages
            .Where(x => x.Status == "Pending" && !x.IsDeleted
                        && (x.NextAttemptAt == null || x.NextAttemptAt <= DateTimeOffset.UtcNow))
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var msg in batch)
        {
            try
            {
                // Bus Day-1: ghi nhận inbox consumer "sys.audit" (idempotent)
                var first = await inbox.TryBeginProcessAsync(
                    msg.TenantId, msg.Id, "sys.audit", msg.EventType, ct);

                msg.AttemptCount++;
                msg.Status = "Published";
                msg.PublishedAt = DateTimeOffset.UtcNow;
                msg.LastError = first ? null : "duplicate_skip";
                _log.LogInformation(
                    "Outbox published {EventType} id={Id} corr={CorrelationId}",
                    msg.EventType, msg.Id, msg.CorrelationId);
            }
            catch (Exception ex)
            {
                msg.AttemptCount++;
                msg.LastError = ex.Message.Length > 900 ? ex.Message[..900] : ex.Message;
                msg.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(Math.Min(900, 15 * msg.AttemptCount));
                if (msg.AttemptCount >= 8)
                    msg.Status = "Dead";
                else
                    msg.Status = "Pending";
                _log.LogWarning(ex, "Outbox fail {Id} attempt {N}", msg.Id, msg.AttemptCount);
            }
        }

        if (batch.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}

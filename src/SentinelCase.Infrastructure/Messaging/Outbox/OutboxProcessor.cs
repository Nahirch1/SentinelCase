using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SentinelCase.Infrastructure.Persistence;

namespace SentinelCase.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(
                    stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unexpected error while processing outbox messages.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var messages =
            await dbContext.OutboxMessages
                .Where(x =>
                    x.ProcessedAt == null &&
                    x.RetryCount < 5)
                .OrderBy(x => x.OccurredAt)
                .Take(20)
                .ToListAsync(
                    cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation(
                    "Processing outbox message {MessageId} of type {MessageType}.",
                    message.Id,
                    message.Type);

                message.ProcessedAt =
                    DateTimeOffset.UtcNow;

                message.Error = null;
            }
            catch (Exception exception)
            {
                message.RetryCount++;

                message.Error =
                    exception.Message;

                _logger.LogError(
                    exception,
                    "Failed to process outbox message {MessageId}.",
                    message.Id);
            }
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }
}

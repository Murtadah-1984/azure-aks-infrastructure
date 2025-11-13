using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.DomainEvents;
using IdentityService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IdentityService.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job to process outbox messages and publish them to RabbitMQ
/// </summary>
public class OutboxProcessorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorJob> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);

    public OutboxProcessorJob(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor job stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<Domain.Interfaces.IOutboxRepository>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get unprocessed messages
        var messages = await outboxRepository.GetUnprocessedMessagesAsync(100, cancellationToken);

        if (messages.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize domain event
                var domainEvent = DeserializeDomainEvent(message.MessageType, message.Payload);
                if (domainEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize domain event: {MessageType}", message.MessageType);
                    message.MarkAsFailed("Failed to deserialize domain event");
                    await outboxRepository.UpdateAsync(message, cancellationToken);
                    continue;
                }

                // Publish to event bus
                await eventBus.PublishAsync(domainEvent, cancellationToken);

                // Mark as processed
                message.MarkAsProcessed();
                await outboxRepository.UpdateAsync(message, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Processed outbox message: {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message: {MessageId}", message.Id);
                
                // Mark as failed with retry logic
                var nextRetryAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, message.RetryCount) * 5); // Exponential backoff
                message.MarkAsFailed(ex.Message, nextRetryAt);
                await outboxRepository.UpdateAsync(message, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private IDomainEvent? DeserializeDomainEvent(string messageType, string payload)
    {
        try
        {
            // Get the type from the assembly
            var type = Type.GetType(messageType);
            if (type == null)
            {
                // Try to find in DomainEvents namespace
                var domainEventsAssembly = typeof(IDomainEvent).Assembly;
                type = domainEventsAssembly.GetType(messageType);
            }

            if (type == null || !typeof(IDomainEvent).IsAssignableFrom(type))
            {
                _logger.LogWarning("Unknown domain event type: {MessageType}", messageType);
                return null;
            }

            return JsonSerializer.Deserialize(payload, type) as IDomainEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize domain event: {MessageType}", messageType);
            return null;
        }
    }
}


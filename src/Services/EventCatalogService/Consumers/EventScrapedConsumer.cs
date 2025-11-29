using MassTransit;
using EventBus.Messages;

namespace EventCatalogService.Consumers;

public class EventScrapedConsumer : IConsumer<EventScraped>
{
    private readonly ILogger<EventScrapedConsumer> _logger;

    public EventScrapedConsumer(ILogger<EventScrapedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<EventScraped> context)
    {
        _logger.LogInformation("Event consumed: {EventName} from {Source}", context.Message.Name, context.Message.Source);
        
        // TODO: Save to database
        
        return Task.CompletedTask;
    }
}

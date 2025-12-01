using MassTransit;
using EventBus.Messages;
using EventCatalogService.Core.Application.Interfaces;
using DomainEvent = EventCatalogService.Core.Domain.Event;

namespace EventCatalogService.Consumers;

public class EventScrapedConsumer : IConsumer<EventScraped>
{
    private readonly IEventRepository _repository;
    private readonly ILogger<EventScrapedConsumer> _logger;

    public EventScrapedConsumer(IEventRepository repository, ILogger<EventScrapedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventScraped> context)
    {
        var message = context.Message;
        _logger.LogInformation("Event consumed: {EventName} from {Source}", message.Name, message.Source);

        bool exists = await _repository.EventExistsAsync(message.Name, message.Date);

        if (exists)
        {
            _logger.LogInformation("Event already exists, skipping: {EventName}", message.Name);
            return;
        }

        var eventEntity = new DomainEvent
        {
            Name = message.Name,
            Description = message.Description,
            EventDate = message.Date,
            Location = message.Location,
            City = message.City,
            TicketUrl = message.Url,
            ImageUrl = message.ImageUrl,
            Source = message.Source ?? "Unknown",
            CreatedDate = DateTime.UtcNow
        };

        await _repository.CreateEventAsync(eventEntity);

        _logger.LogInformation("✅ Event saved via Clean Architecture: {EventName}", eventEntity.Name);
    }
}

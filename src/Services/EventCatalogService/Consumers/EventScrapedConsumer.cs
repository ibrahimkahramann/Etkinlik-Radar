using MassTransit;
using EventBus.Messages;
using EventCatalogService.Data;
using MongoDB.Driver;
using Entities = EventCatalogService.Entities;

namespace EventCatalogService.Consumers;

public class EventScrapedConsumer : IConsumer<EventScraped>
{
    private readonly ILogger<EventScrapedConsumer> _logger;
    private readonly IEventContext _context;

    public EventScrapedConsumer(ILogger<EventScrapedConsumer> logger, IEventContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task Consume(ConsumeContext<EventScraped> context)
    {
        _logger.LogInformation("Event consumed: {EventName} from {Source}", context.Message.Name, context.Message.Source);

        var eventEntity = new Entities.Event
        {
            Name = context.Message.Name,
            Description = context.Message.Description,
            Date = context.Message.Date,
            Location = context.Message.Location,
            City = context.Message.City,
            Url = context.Message.Url,
            ImageUrl = context.Message.ImageUrl,
            Source = context.Message.Source
        };
        var filter = Builders<Entities.Event>.Filter.And(
            Builders<Entities.Event>.Filter.Eq(e => e.Name, context.Message.Name),
            Builders<Entities.Event>.Filter.Eq(e => e.Date, context.Message.Date),
            Builders<Entities.Event>.Filter.Eq(e => e.Location, context.Message.Location),
            Builders<Entities.Event>.Filter.Eq(e => e.City, context.Message.City)
        );
        var update = Builders<Entities.Event>.Update
            .Set(e => e.Name, eventEntity.Name)
            .Set(e => e.Description, eventEntity.Description)
            .Set(e => e.Date, eventEntity.Date)
            .Set(e => e.Location, eventEntity.Location)
            .Set(e => e.City, eventEntity.City)
            .Set(e => e.Url, eventEntity.Url)
            .Set(e => e.ImageUrl, eventEntity.ImageUrl)
            .Set(e => e.Source, eventEntity.Source)
            .SetOnInsert(e => e.CreatedAt, DateTime.UtcNow); // Only set CreatedAt on insert

        var updateOptions = new UpdateOptions { IsUpsert = true };

        await _context.Events.UpdateOneAsync(filter, update, updateOptions);
        
        _logger.LogInformation("Event upserted (saved/updated) to database: {EventName}", eventEntity.Name);
    }
}

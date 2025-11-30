using EventCatalogService.Entities;
using MongoDB.Driver;

namespace EventCatalogService.Data;

public class EventContext : IEventContext
{
    public EventContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("MongoDB:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("MongoDB:DatabaseName"));

        Events = database.GetCollection<Event>(configuration.GetValue<string>("MongoDB:CollectionName"));
    }

    public IMongoCollection<Event> Events { get; }
}

public interface IEventContext
{
    IMongoCollection<Event> Events { get; }
}

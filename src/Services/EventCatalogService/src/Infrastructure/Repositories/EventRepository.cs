using EventCatalogService.Core.Application.Interfaces;
using EventCatalogService.Core.Domain;
using MongoDB.Driver;

namespace EventCatalogService.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly IMongoCollection<Event> _context;

    public EventRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString");
        var databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
        var collectionName = configuration.GetValue<string>("DatabaseSettings:CollectionName");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _context = database.GetCollection<Event>(collectionName);
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Find(e => true).ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(string id)
    {
        return await _context.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsByCityAsync(string city)
    {
        return await _context.Find(e => e.City == city).ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsBySourceAsync(string source)
    {
        return await _context.Find(e => e.Source == source).ToListAsync();
    }

    public async Task CreateEventAsync(Event eventEntity)
    {
        await _context.InsertOneAsync(eventEntity);
    }

    public async Task UpdateEventAsync(string id, Event eventEntity)
    {
        await _context.ReplaceOneAsync(e => e.Id == id, eventEntity);
    }

    public async Task DeleteEventAsync(string id)
    {
        await _context.DeleteOneAsync(e => e.Id == id);
    }

    public async Task<bool> EventExistsAsync(string name, DateTime eventDate)
    {
        var filter = Builders<Event>.Filter.And(
            Builders<Event>.Filter.Eq(e => e.Name, name),
            Builders<Event>.Filter.Eq(e => e.EventDate, eventDate)
        );
        return await _context.Find(filter).AnyAsync();
    }

    public async Task<long> GetTotalEventsCountAsync()
    {
        return await _context.CountDocumentsAsync(e => true);
    }
}

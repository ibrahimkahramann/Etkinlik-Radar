using EventCatalogService.Core.Domain;

namespace EventCatalogService.Core.Application.Interfaces;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(string id);
    Task<IEnumerable<Event>> GetEventsByCityAsync(string city);
    Task<IEnumerable<Event>> GetEventsBySourceAsync(string source);

    Task CreateEventAsync(Event eventEntity);
    Task UpdateEventAsync(string id, Event eventEntity);
    Task DeleteEventAsync(string id);

    Task<bool> EventExistsAsync(string name, DateTime eventDate);
    Task<long> GetTotalEventsCountAsync();
}

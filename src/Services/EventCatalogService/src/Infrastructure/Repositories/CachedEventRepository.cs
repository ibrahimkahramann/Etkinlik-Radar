using EventCatalogService.Core.Application.Interfaces;
using EventCatalogService.Core.Domain;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EventCatalogService.Infrastructure.Repositories;

public class CachedEventRepository : IEventRepository
{
    private readonly EventRepository _repository; 
    private readonly IDistributedCache _cache;  
    private readonly ILogger<CachedEventRepository> _logger;

    private readonly DistributedCacheEntryOptions _cacheOptions;

    public CachedEventRepository(
        EventRepository repository,
        IDistributedCache cache,
        ILogger<CachedEventRepository> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), 
            SlidingExpiration = TimeSpan.FromMinutes(2) 
        };
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        string cacheKey = "all_events";

  
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("✅ Data fetched from Redis Cache");
            return JsonSerializer.Deserialize<IEnumerable<Event>>(cachedData) ?? Enumerable.Empty<Event>();
        }

        var events = await _repository.GetAllEventsAsync();

        string serializedData = JsonSerializer.Serialize(events);
        await _cache.SetStringAsync(cacheKey, serializedData, _cacheOptions);

        _logger.LogInformation("⬇️ Data fetched from Database and cached");

        return events;
    }

    public async Task<Event?> GetEventByIdAsync(string id)
    {
        string cacheKey = $"event_{id}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("✅ Event {EventId} fetched from Redis Cache", id);
            return JsonSerializer.Deserialize<Event>(cachedData);
        }

        var eventEntity = await _repository.GetEventByIdAsync(id);

        if (eventEntity != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(eventEntity), _cacheOptions);
            _logger.LogInformation("⬇️ Event {EventId} fetched from Database and cached", id);
        }

        return eventEntity;
    }

    public async Task<IEnumerable<Event>> GetEventsByCityAsync(string city)
    {
        string cacheKey = $"events_city_{city.ToLowerInvariant()}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("✅ Events for city {City} fetched from Redis Cache", city);
            return JsonSerializer.Deserialize<IEnumerable<Event>>(cachedData) ?? Enumerable.Empty<Event>();
        }

        var events = await _repository.GetEventsByCityAsync(city);

        string serializedData = JsonSerializer.Serialize(events);
        await _cache.SetStringAsync(cacheKey, serializedData, _cacheOptions);
        _logger.LogInformation("⬇️ Events for city {City} fetched from Database and cached", city);

        return events;
    }

    public async Task<IEnumerable<Event>> GetEventsBySourceAsync(string source)
    {
        string cacheKey = $"events_source_{source.ToLowerInvariant()}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("✅ Events for source {Source} fetched from Redis Cache", source);
            return JsonSerializer.Deserialize<IEnumerable<Event>>(cachedData) ?? Enumerable.Empty<Event>();
        }

        var events = await _repository.GetEventsBySourceAsync(source);

        string serializedData = JsonSerializer.Serialize(events);
        await _cache.SetStringAsync(cacheKey, serializedData, _cacheOptions);
        _logger.LogInformation("⬇️ Events for source {Source} fetched from Database and cached", source);

        return events;
    }

    public async Task CreateEventAsync(Event eventEntity)
    {
        await _repository.CreateEventAsync(eventEntity);

        await _cache.RemoveAsync("all_events");

        if (!string.IsNullOrEmpty(eventEntity.City))
        {
            await _cache.RemoveAsync($"events_city_{eventEntity.City.ToLowerInvariant()}");
        }
        if (!string.IsNullOrEmpty(eventEntity.Source))
        {
            await _cache.RemoveAsync($"events_source_{eventEntity.Source.ToLowerInvariant()}");
        }

        _logger.LogInformation("🗑️ Cache invalidated due to new event creation");
    }

    public async Task UpdateEventAsync(string id, Event eventEntity)
    {
        await _repository.UpdateEventAsync(id, eventEntity);

        await _cache.RemoveAsync($"event_{id}");
        await _cache.RemoveAsync("all_events");

        if (!string.IsNullOrEmpty(eventEntity.City))
        {
            await _cache.RemoveAsync($"events_city_{eventEntity.City.ToLowerInvariant()}");
        }
        if (!string.IsNullOrEmpty(eventEntity.Source))
        {
            await _cache.RemoveAsync($"events_source_{eventEntity.Source.ToLowerInvariant()}");
        }

        _logger.LogInformation("🗑️ Cache invalidated for event {EventId}", id);
    }

    public async Task DeleteEventAsync(string id)
    {
        var eventEntity = await _repository.GetEventByIdAsync(id);

        await _repository.DeleteEventAsync(id);

        await _cache.RemoveAsync($"event_{id}");
        await _cache.RemoveAsync("all_events");

        if (eventEntity != null)
        {
            if (!string.IsNullOrEmpty(eventEntity.City))
            {
                await _cache.RemoveAsync($"events_city_{eventEntity.City.ToLowerInvariant()}");
            }
            if (!string.IsNullOrEmpty(eventEntity.Source))
            {
                await _cache.RemoveAsync($"events_source_{eventEntity.Source.ToLowerInvariant()}");
            }
        }

        _logger.LogInformation("🗑️ Cache invalidated for deleted event {EventId}", id);
    }

    public async Task<bool> EventExistsAsync(string name, DateTime eventDate)
    {
        return await _repository.EventExistsAsync(name, eventDate);
    }

    public async Task<bool> EventExistsByTicketUrlAsync(string ticketUrl)
    {
        return await _repository.EventExistsByTicketUrlAsync(ticketUrl);
    }

    public async Task<long> GetTotalEventsCountAsync()
    {
        string cacheKey = "total_events_count";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("✅ Total events count fetched from Redis Cache");
            return long.Parse(cachedData);
        }

        var count = await _repository.GetTotalEventsCountAsync();

        await _cache.SetStringAsync(cacheKey, count.ToString(), _cacheOptions);
        _logger.LogInformation("⬇️ Total events count fetched from Database and cached");

        return count;
    }
}

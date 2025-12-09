using EventCatalogService.Core.Application.Interfaces;
using EventCatalogService.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventRepository eventRepository, ILogger<EventsController> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Getting events - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        // Sayfa numarası en az 1 olmalı
        if (page < 1) page = 1;
        // Sayfa boyutu 1-1000 arası olmalı
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        var allEvents = await _eventRepository.GetAllEventsAsync();

        // Pagination uygula
        var paginatedEvents = allEvents
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(paginatedEvents);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetEventById(string id)
    {
        _logger.LogInformation("Getting event with ID: {Id}", id);
        var eventEntity = await _eventRepository.GetEventByIdAsync(id);

        if (eventEntity == null)
        {
            return NotFound($"Event with ID {id} not found");
        }

        return Ok(eventEntity);
    }

    [HttpGet("city/{city}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetEventsByCity(string city)
    {
        _logger.LogInformation("Getting events for city: {City}", city);
        var events = await _eventRepository.GetEventsByCityAsync(city);
        return Ok(events);
    }

    [HttpGet("source/{source}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetEventsBySource(string source)
    {
        _logger.LogInformation("Getting events from source: {Source}", source);
        var events = await _eventRepository.GetEventsBySourceAsync(source);
        return Ok(events);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] Event eventEntity)
    {
        if (eventEntity == null)
        {
            return BadRequest("Event cannot be null");
        }

        _logger.LogInformation("Creating new event: {EventName}", eventEntity.Name);

        var exists = await _eventRepository.EventExistsAsync(eventEntity.Name, eventEntity.EventDate);
        if (exists)
        {
            _logger.LogWarning("Event already exists: {EventName} on {EventDate}",
                eventEntity.Name, eventEntity.EventDate);
            return BadRequest("Event with same name and date already exists");
        }

        await _eventRepository.CreateEventAsync(eventEntity);

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = eventEntity.Id },
            eventEntity);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] Event eventEntity)
    {
        _logger.LogInformation("Updating event with ID: {Id}", id);

        var existingEvent = await _eventRepository.GetEventByIdAsync(id);
        if (existingEvent == null)
        {
            return NotFound($"Event with ID {id} not found");
        }

        eventEntity.Id = id;
        await _eventRepository.UpdateEventAsync(id, eventEntity);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        _logger.LogInformation("Deleting event with ID: {Id}", id);

        var existingEvent = await _eventRepository.GetEventByIdAsync(id);
        if (existingEvent == null)
        {
            return NotFound($"Event with ID {id} not found");
        }

        await _eventRepository.DeleteEventAsync(id);

        return NoContent();
    }

    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> GetTotalEventsCount()
    {
        _logger.LogInformation("Getting total events count");
        var count = await _eventRepository.GetTotalEventsCountAsync();
        return Ok(new { totalEvents = count });
    }
}

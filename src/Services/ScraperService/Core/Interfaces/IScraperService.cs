using EventBus.Messages;

namespace ScraperService.Core.Interfaces;

public interface IScraperService
{
    string Name { get; }
    Task<List<EventScraped>> ScrapeEventsAsync(string city);
}

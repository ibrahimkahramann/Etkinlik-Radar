using MassTransit;
using ScraperService.Core.Interfaces;
using EventBus.Messages;
using ScraperService.Core.Options;
using ScraperService.Infrastructure.Adapters;

namespace ScraperService.Application.Features.Scraping.Consumers;

public class ScrapeAllConsumer : IConsumer<ScrapeAllCommand>
{
    private readonly ILogger<ScrapeAllConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ScraperOptions _options;

    public ScrapeAllConsumer(
        ILogger<ScrapeAllConsumer> logger, 
        IPublishEndpoint publishEndpoint, 
        IServiceScopeFactory scopeFactory,
        Microsoft.Extensions.Options.IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<ScrapeAllCommand> context)
    {
        _logger.LogInformation("ScrapeAllCommand received. Triggered at: {Time}, Source: {Source}", 
            context.Message.TriggeredAt, context.Message.Source);
        
        var scraperNames = new[] { "Bubilet", "Biletinial", "Biletino", "Biletix" };
        
        _logger.LogInformation("Will run {Count} scrapers sequentially", scraperNames.Length);
        
        foreach (var city in _options.Cities)
        {
            _logger.LogInformation("Starting scraping for city: {City}", city);
            
            foreach (var scraperName in scraperNames)
            {
                _logger.LogInformation("Creating scope for scraper: {ScraperName}", scraperName);
                
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        _logger.LogInformation("Resolving scraper: {ScraperName}", scraperName);
                        
                        IScraperService scraper;
                        
                        if (scraperName == "Bubilet")
                        {
                            _logger.LogInformation("Matched Bubilet");
                            scraper = scope.ServiceProvider.GetRequiredService<BubiletScraper>();
                        }
                        else if (scraperName == "Biletinial")
                        {
                            _logger.LogInformation("Matched Biletinial");
                            scraper = scope.ServiceProvider.GetRequiredService<BiletinialScraper>();
                        }
                        else if (scraperName == "Biletino")
                        {
                            _logger.LogInformation("Matched Biletino");
                            scraper = scope.ServiceProvider.GetRequiredService<BiletinoScraper>();
                        }
                        else if (scraperName == "Biletix")
                        {
                            _logger.LogInformation("Matched Biletix");
                            scraper = scope.ServiceProvider.GetRequiredService<BiletixScraper>();
                        }
                        else
                        {
                            _logger.LogWarning("No match for scraper: {ScraperName}", scraperName);
                            scraper = null;
                        }
                        
                        if (scraper == null)
                        {
                            _logger.LogWarning("Scraper resolved to null: {ScraperName}", scraperName);
                            continue;
                        }
                        
                        _logger.LogInformation("Running scraper: {ScraperName} for city: {City}", scraper.Name, city);
                        
                        var events = await scraper.ScrapeEventsAsync(city);
                        
                        _logger.LogInformation("Scraper {ScraperName} found {Count} events for {City}", scraper.Name, events.Count, city);
                        
                        foreach (var evt in events)
                        {
                            await _publishEndpoint.Publish(evt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scraping {City} with {ScraperName}", city, scraperName);
                }
            }
        }

        _logger.LogInformation("Scraping completed for all sites and cities");
    }
}

public record ScrapeAllCommand
{
    public DateTime TriggeredAt { get; init; }
    public string Source { get; init; } = string.Empty;
}

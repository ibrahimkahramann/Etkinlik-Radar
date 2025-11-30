using MassTransit;
using ScraperService.Core.Interfaces;
using EventBus.Messages;
using ScraperService.Core.Options;

namespace ScraperService.Application.Features.Scraping.Consumers;

public class ScrapeSiteConsumer : IConsumer<ScrapeSiteCommand>
{
    private readonly ILogger<ScrapeSiteConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ScraperOptions _options;

    public ScrapeSiteConsumer(ILogger<ScrapeSiteConsumer> logger, IServiceProvider serviceProvider, IPublishEndpoint publishEndpoint, Microsoft.Extensions.Options.IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _publishEndpoint = publishEndpoint;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<ScrapeSiteCommand> context)
    {
        var siteName = context.Message.SiteName;
        _logger.LogInformation("ScrapeSiteCommand received for {SiteName}. Triggered at: {Time}", 
            siteName, context.Message.TriggeredAt);

        IScraperService scraper = null;
        
        _logger.LogInformation("Creating scope...");
        using (var scope = _serviceProvider.CreateScope())
        {
            _logger.LogInformation("Scope created. Resolving scraper...");
            try
            {
                if (siteName.Equals("Bubilet", StringComparison.OrdinalIgnoreCase))
                {
                    scraper = scope.ServiceProvider.GetRequiredService<ScraperService.Infrastructure.Adapters.BubiletScraper>();
                }
                else if (siteName.Equals("Biletinial", StringComparison.OrdinalIgnoreCase))
                {
                    scraper = scope.ServiceProvider.GetRequiredService<ScraperService.Infrastructure.Adapters.BiletinialScraper>();
                }
                else if (siteName.Equals("Biletino", StringComparison.OrdinalIgnoreCase))
                {
                    scraper = scope.ServiceProvider.GetRequiredService<ScraperService.Infrastructure.Adapters.BiletinoScraper>();
                }
                else if (siteName.Equals("Biletix", StringComparison.OrdinalIgnoreCase))
                {
                    scraper = scope.ServiceProvider.GetRequiredService<ScraperService.Infrastructure.Adapters.BiletixScraper>();
                }

                if (scraper == null)
                {
                    _logger.LogWarning("No scraper found for site: {SiteName}", siteName);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving scraper for site: {SiteName}", siteName);
                return;
            }
        var citiesToScrape = !string.IsNullOrEmpty(context.Message.City) 
            ? new List<string> { context.Message.City } 
            : _options.Cities;

        foreach (var city in citiesToScrape)
        {
            try
            {
                _logger.LogInformation("Executing scraper: {ScraperName} for city: {City}", scraper.Name, city);
                var events = await scraper.ScrapeEventsAsync(city);

                if (events != null && events.Any())
                {
                    _logger.LogInformation("Publishing {Count} events from {ScraperName} ({City})", events.Count, scraper.Name, city);
                    foreach (var evt in events)
                    {
                        await _publishEndpoint.Publish(evt);
                    }
                }
                else
                {
                    _logger.LogInformation("No events found by {ScraperName} for {City}", scraper.Name, city);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scraper {ScraperName} for {City}", scraper.Name, city);
            }
        }
    } // End of using scope
    }
}

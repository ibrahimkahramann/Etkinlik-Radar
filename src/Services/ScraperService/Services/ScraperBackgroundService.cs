using MassTransit;
using Microsoft.Extensions.Options;
using ScraperService.Application.Features.Scraping.Consumers;
using ScraperService.Core.Options;

namespace ScraperService.Services;

public class ScraperBackgroundService : BackgroundService
{
    private readonly ILogger<ScraperBackgroundService> _logger;
    private readonly IBus _bus;
    private readonly ScraperOptions _options;

    public ScraperBackgroundService(
        ILogger<ScraperBackgroundService> logger,
        IBus bus,
        IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _bus = bus;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scraper Background Service started. AutoScrape: {Enabled}, Interval: {Interval} min",
            _options.AutoScrapeEnabled, _options.IntervalMinutes);

        if (!_options.AutoScrapeEnabled)
        {
            _logger.LogInformation("Auto scraping is disabled. Waiting for manual triggers via Admin API.");
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Auto-triggering ScrapeAllCommand...");

            await _bus.Publish(new ScrapeAllCommand
            {
                TriggeredAt = DateTime.UtcNow,
                Source = "BackgroundService-Auto"
            }, stoppingToken);

            var interval = TimeSpan.FromMinutes(_options.IntervalMinutes);
            _logger.LogInformation("Next auto-scrape in {Interval} minutes", _options.IntervalMinutes);
            await Task.Delay(interval, stoppingToken);
        }
    }
}

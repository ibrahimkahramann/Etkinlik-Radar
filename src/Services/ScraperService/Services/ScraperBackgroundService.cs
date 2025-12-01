using MassTransit;
using ScraperService.Application.Features.Scraping.Consumers;

namespace ScraperService.Services;

public class ScraperBackgroundService : BackgroundService
{
    private readonly ILogger<ScraperBackgroundService> _logger;
    private readonly IBus _bus;

    public ScraperBackgroundService(ILogger<ScraperBackgroundService> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scraper Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Triggering ScrapeAllCommand...");
            
            await _bus.Publish(new ScrapeAllCommand 
            { 
                TriggeredAt = DateTime.UtcNow, 
                Source = "BackgroundService" 
            }, stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

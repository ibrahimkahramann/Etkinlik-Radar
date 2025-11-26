using MassTransit;

namespace ScraperService.Services;

public class ScraperBackgroundService : BackgroundService
{
    private readonly ILogger<ScraperBackgroundService> _logger;

    public ScraperBackgroundService(ILogger<ScraperBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scraper Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}

public class ScrapeAllConsumer : IConsumer<ScrapeAllCommand>
{
    private readonly ILogger<ScrapeAllConsumer> _logger;

    public ScrapeAllConsumer(ILogger<ScrapeAllConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ScrapeAllCommand> context)
    {
        _logger.LogInformation("ScrapeAllCommand received. Triggered at: {Time}, Source: {Source}", 
            context.Message.TriggeredAt, context.Message.Source);
        
        await Task.Delay(2000);
        
        _logger.LogInformation("Scraping completed for all sites");
    }
}

public class ScrapeSiteConsumer : IConsumer<ScrapeSiteCommand>
{
    private readonly ILogger<ScrapeSiteConsumer> _logger;

    public ScrapeSiteConsumer(ILogger<ScrapeSiteConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ScrapeSiteCommand> context)
    {
        _logger.LogInformation("ScrapeSiteCommand received for {SiteName}. Triggered at: {Time}", 
            context.Message.SiteName, context.Message.TriggeredAt);
        
        await Task.Delay(1000);
        
        _logger.LogInformation("Scraping completed for {SiteName}", context.Message.SiteName);
    }
}

public record ScrapeAllCommand
{
    public DateTime TriggeredAt { get; init; }
    public string Source { get; init; } = string.Empty;
}

public record ScrapeSiteCommand
{
    public string SiteName { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
    public string Source { get; init; } = string.Empty;
}

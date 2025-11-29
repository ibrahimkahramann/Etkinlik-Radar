using MassTransit;
using EventBus.Messages;

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
    private readonly IPublishEndpoint _publishEndpoint;

    public ScrapeAllConsumer(ILogger<ScrapeAllConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ScrapeAllCommand> context)
    {
        _logger.LogInformation("ScrapeAllCommand received. Triggered at: {Time}, Source: {Source}", 
            context.Message.TriggeredAt, context.Message.Source);
        
        await Task.Delay(2000);
        
        var eventScraped = new EventScraped
        {
            Name = "Tarkan Konseri",
            Description = "Harbiye Açıkhava Tarkan Konseri",
            Date = DateTime.Now.AddDays(7),
            Location = "Harbiye Cemil Topuzlu Açıkhava Tiyatrosu",
            Url = "https://biletix.com/etkinlik/123",
            ImageUrl = "https://example.com/tarkan.jpg",
            Source = "Biletix"
        };

        await _publishEndpoint.Publish(eventScraped);
        _logger.LogInformation("Published EventScraped event for {EventName}", eventScraped.Name);

        _logger.LogInformation("Scraping completed for all sites");
    }
}

public class ScrapeSiteConsumer : IConsumer<ScrapeSiteCommand>
{
    private readonly ILogger<ScrapeSiteConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ScrapeSiteConsumer(ILogger<ScrapeSiteConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ScrapeSiteCommand> context)
    {
        _logger.LogInformation("ScrapeSiteCommand received for {SiteName}. Triggered at: {Time}", 
            context.Message.SiteName, context.Message.TriggeredAt);
        
        await Task.Delay(1000);
        
        var eventScraped = new EventScraped
        {
            Name = "Fazıl Say Resitali",
            Description = "Fazıl Say Piyano Resitali",
            Date = DateTime.Now.AddDays(14),
            Location = "AKM",
            Url = "https://passo.com.tr/etkinlik/456",
            ImageUrl = "https://example.com/fazilsay.jpg",
            Source = context.Message.SiteName
        };

        await _publishEndpoint.Publish(eventScraped);
        _logger.LogInformation("Published EventScraped event for {EventName}", eventScraped.Name);

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

using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ScraperService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ScraperController> _logger;

    public ScraperController(IPublishEndpoint publishEndpoint, ILogger<ScraperController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartScraping()
    {
        _logger.LogInformation("Scraping manually triggered");
        
        await _publishEndpoint.Publish(new ScrapeAllCommand
        {
            TriggeredAt = DateTime.UtcNow,
            Source = "Manual"
        });

        return Ok(new { message = "Scraping işlemi başlatıldı", triggeredAt = DateTime.UtcNow });
    }

    [HttpPost("start/{siteName}")]
    public async Task<IActionResult> StartScrapingSite(string siteName)
    {
        _logger.LogInformation("Scraping for {SiteName} manually triggered", siteName);
        
        await _publishEndpoint.Publish(new ScrapeSiteCommand
        {
            SiteName = siteName,
            TriggeredAt = DateTime.UtcNow,
            Source = "Manual"
        });

        return Ok(new { message = $"{siteName} için scraping başlatıldı", triggeredAt = DateTime.UtcNow });
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

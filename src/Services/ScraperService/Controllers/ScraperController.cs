using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ScraperService.Application.Features.Scraping.Consumers;
using EventBus.Messages;

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
    public async Task<IActionResult> StartScrapingSite(string siteName, [FromQuery] string? city = null)
    {
        _logger.LogInformation("Scraping for {SiteName} manually triggered. City: {City}", siteName, city ?? "All");
        
        await _publishEndpoint.Publish(new ScrapeSiteCommand
        {
            SiteName = siteName,
            City = city,
            TriggeredAt = DateTime.UtcNow,
            Source = "Manual"
        });

        return Ok(new { message = $"{siteName} için scraping başlatıldı", city = city ?? "All", triggeredAt = DateTime.UtcNow });
    }
}



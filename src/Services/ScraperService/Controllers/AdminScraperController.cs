using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScraperService.Application.Features.Scraping.Consumers;
using ScraperService.Core.DTOs;
using ScraperService.Core.Options;
using ScraperService.Core.Services;
using EventBus.Messages;

namespace ScraperService.Controllers;

[ApiController]
[Route("api/admin/scraper")]
public class AdminScraperController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AdminScraperController> _logger;
    private readonly IScrapeJobTracker _jobTracker;
    private readonly ScraperOptions _options;

    private static readonly string[] AvailableScrapers = { "Bubilet", "Biletinial", "Biletino", "Biletix" };

    public AdminScraperController(
        IPublishEndpoint publishEndpoint,
        ILogger<AdminScraperController> logger,
        IScrapeJobTracker jobTracker,
        IOptions<ScraperOptions> options)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _jobTracker = jobTracker;
        _options = options.Value;
    }

    [HttpPost("trigger")]
    [ProducesResponseType(typeof(ScrapeResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScrapeResponseDto>> TriggerScrape([FromBody] ScrapeRequestDto request)
    {
        if (request.ScrapeAll)
        {
            return await TriggerAllScrapers(request.City);
        }

        if (string.IsNullOrWhiteSpace(request.SiteName))
        {
            return BadRequest(new { error = "SiteName is required when ScrapeAll is false" });
        }

        if (!AvailableScrapers.Contains(request.SiteName, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = $"Invalid scraper. Available: {string.Join(", ", AvailableScrapers)}" });
        }

        var jobId = _jobTracker.CreateJob(request.SiteName, request.City);

        _logger.LogInformation("[Job:{JobId}] Admin triggered scrape for {SiteName}, City: {City}",
            jobId, request.SiteName, request.City ?? "All");

        await _publishEndpoint.Publish(new ScrapeSiteCommand
        {
            SiteName = request.SiteName,
            City = request.City,
            TriggeredAt = DateTime.UtcNow,
            Source = $"Admin-{jobId}"
        });

        var response = new ScrapeResponseDto
        {
            JobId = jobId,
            Status = "Accepted",
            Message = $"Scrape job queued for {request.SiteName}",
            TriggeredAt = DateTime.UtcNow,
            SiteName = request.SiteName,
            City = request.City
        };

        return Accepted(response);
    }

    [HttpPost("trigger/all")]
    [ProducesResponseType(typeof(ScrapeResponseDto), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<ScrapeResponseDto>> TriggerAllScrapers([FromQuery] string? city = null)
    {
        var jobId = _jobTracker.CreateJob(null, city);

        _logger.LogInformation("[Job:{JobId}] Admin triggered scrape for ALL scrapers, City: {City}",
            jobId, city ?? "All");

        await _publishEndpoint.Publish(new ScrapeAllCommand
        {
            TriggeredAt = DateTime.UtcNow,
            Source = $"Admin-{jobId}"
        });

        var response = new ScrapeResponseDto
        {
            JobId = jobId,
            Status = "Accepted",
            Message = "Scrape job queued for all scrapers",
            TriggeredAt = DateTime.UtcNow,
            SiteName = "All",
            City = city
        };

        return Accepted(response);
    }

    [HttpGet("jobs/{jobId}")]
    [ProducesResponseType(typeof(ScrapeStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ScrapeStatusDto> GetJobStatus(string jobId)
    {
        var status = _jobTracker.GetJobStatus(jobId);

        if (status == null)
        {
            return NotFound(new { error = $"Job {jobId} not found" });
        }

        return Ok(status);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(IEnumerable<ScrapeStatusDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ScrapeStatusDto>> GetRecentJobs([FromQuery] int count = 10)
    {
        var jobs = _jobTracker.GetRecentJobs(Math.Min(count, 50));
        return Ok(jobs);
    }

    [HttpGet("scrapers")]
    [ProducesResponseType(typeof(IEnumerable<ScraperInfoDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ScraperInfoDto>> GetAvailableScrapers()
    {
        var scrapers = AvailableScrapers.Select(name => new ScraperInfoDto
        {
            Name = name,
            IsAvailable = true,
            SupportedCities = _options.Cities
        });

        return Ok(scrapers);
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(ScraperSettingsDto), StatusCodes.Status200OK)]
    public ActionResult<ScraperSettingsDto> GetSettings()
    {
        var settings = new ScraperSettingsDto
        {
            AutoScrapeEnabled = _options.AutoScrapeEnabled,
            IntervalMinutes = _options.IntervalMinutes,
            EnabledScrapers = AvailableScrapers.ToList(),
            Cities = _options.Cities
        };

        return Ok(settings);
    }

    [HttpGet("cities")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetSupportedCities()
    {
        return Ok(_options.Cities);
    }
}

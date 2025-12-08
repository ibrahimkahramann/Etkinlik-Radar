namespace ScraperService.Core.DTOs;

public record ScrapeRequestDto
{
    public string? SiteName { get; init; }
    public string? City { get; init; }
    public bool ScrapeAll { get; init; } = false;
}

public record ScrapeResponseDto
{
    public string JobId { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string Message { get; init; } = default!;
    public DateTime TriggeredAt { get; init; }
    public string? SiteName { get; init; }
    public string? City { get; init; }
}

public record ScrapeStatusDto
{
    public string JobId { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int EventsFound { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> ProcessedSites { get; init; } = new();
}

public record ScraperInfoDto
{
    public string Name { get; init; } = default!;
    public bool IsAvailable { get; init; }
    public List<string> SupportedCities { get; init; } = new();
}

public record ScraperSettingsDto
{
    public bool AutoScrapeEnabled { get; init; }
    public int IntervalMinutes { get; init; }
    public List<string> EnabledScrapers { get; init; } = new();
    public List<string> Cities { get; init; } = new();
}

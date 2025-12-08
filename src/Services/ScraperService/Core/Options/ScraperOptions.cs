namespace ScraperService.Core.Options;

public class ScraperOptions
{
    public List<string> Cities { get; set; } = new();
    public bool AutoScrapeEnabled { get; set; } = false;
    public int IntervalMinutes { get; set; } = 60;
}

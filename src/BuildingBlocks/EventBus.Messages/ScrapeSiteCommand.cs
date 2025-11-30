namespace EventBus.Messages;

public record ScrapeSiteCommand
{
    public string SiteName { get; init; } = string.Empty;
    public string? City { get; init; }
    public DateTime TriggeredAt { get; init; }
    public string Source { get; init; } = string.Empty;
}

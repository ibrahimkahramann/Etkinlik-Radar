namespace EventBus.Messages;

public record EventScraped
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public DateTime Date { get; init; }
    public string Location { get; init; } = default!;
    public string City { get; init; } = default!;
    public string Url { get; init; } = default!;
    public string ImageUrl { get; init; } = default!;
    public string Source { get; init; } = default!;
}

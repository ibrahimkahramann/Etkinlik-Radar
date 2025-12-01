using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventCatalogService.Core.Domain;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [BsonElement("Date")]
    public DateTime EventDate { get; set; }

    public string? Location { get; set; }

    public string? City { get; set; }

    [BsonElement("Url")]
    public string? TicketUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Source { get; set; }

    [BsonElement("CreatedAt")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

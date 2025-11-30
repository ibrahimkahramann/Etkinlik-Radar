using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventCatalogService.Entities;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

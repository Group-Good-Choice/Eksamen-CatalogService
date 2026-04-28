using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProductUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

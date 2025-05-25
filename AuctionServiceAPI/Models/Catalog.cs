using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Models;

public class Catalog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid CatalogId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public CatalogStatus Status { get; set; } = CatalogStatus.Active;
}

public enum CatalogStatus
{
    Active, // Kataloget er aktivt og kan indeholde auktioner
    Closed, // Kataloget er lukket og kan ikke indeholde auktioner
}
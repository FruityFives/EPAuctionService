using Models;
using MongoDB.Bson.Serialization.Attributes;
namespace Models;

public class Auction
{
    [BsonId]
    public Guid AuctionId { get; set; }
    public AuctionStatus Status { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? CatalogId { get; set; }  // FK
    public List<BidDTO> BidHistory { get; set; } = new(); // FK - Historik over bud

    public double MinPrice { get; set; }    // Det nuværende højeste bud - FK

    public BidDTO? CurrentBid { get; set; }  // Det nuværende højeste bud - FK

    public EffectDTO Effect { get; set; } = null!; // Det tilknyttede auktionsobjekt FK
}
public enum AuctionStatus
{
    Inactive, // Auktionen er inaktiv og kan ikke bydes på
    Active, // Auktionen er aktiv og kan bydes på
    Closed, // Auktionen er lukket og kan ikke bydes på
}
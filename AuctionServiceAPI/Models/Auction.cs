using Auction;
namespace AuctionServiceAPI.Models;

public class Auction
{
    public Guid Id { get; set; }
    public AuctionStatus Status { get; set; }

    public Catalog CatalogId { get; set; } = null!; // FK til kataloget

    // Historik over bud
    public List<BidDTO> BidHistory { get; set; } = new();

    // Det nuværende højeste bud
    public BidDTO? CurrentBid { get; set; }

    // Det tilknyttede auktionsobjekt
    public AuctionEffect Effect { get; set; } = null!;

    // Constructor
    public Auction(AuctionEffect effect)
    {
        Effect = effect;
    }
}
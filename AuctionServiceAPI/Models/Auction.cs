using Auction;
namespace AuctionServiceAPI.Models;

public class Auction
{
    public Guid Id { get; set; }
    public AuctionStatus Status { get; set; }

    // Historik over bud
    public List<Bid> BidHistory { get; set; } = new();

    // Det nuværende højeste bud
    public Bid? MinimumBid { get; set; }

    // Det tilknyttede auktionsobjekt
    public AuctionEffect Effect { get; set; } = null!;

    // Constructor
    public Auction(AuctionEffect effect)
    {
        Effect = effect;
    }
}
using Auction;
namespace AuctionServiceAPI.Models;

public class Auction
{
    public Guid Id { get; set; }
    public AuctionStatus Status { get; set; }

    public Catalog CatalogId { get; set; } = null!;

    // Historik over bud
    public List<BidDTO> BidHistory { get; set; } = new();


    public BidDTO? MinimumBid { get; set; }    // Det nuværende højeste bud


    public EffectDTO EffectId { get; set; } = null!; // Det tilknyttede auktionsobjekt

  
}
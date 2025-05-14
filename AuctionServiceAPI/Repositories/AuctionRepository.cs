using Models;

namespace AuctionServiceAPI.Repositories;

public class AuctionRepository
{
    private readonly List<Auction> ListOfAuctions = new();
    private readonly List<Catalog> ListOfCatalogs = new();
    List<Catalog> TestList = new CatalogRepository().SeedData();
    
    public List<Auction> SeedDataAuction()
    {
        // Sample data
        ListOfAuctions.Add(new Auction
        {
            AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3a7"),
            Name = "Auction 1",
            Status = AuctionStatus.Active,
            CatalogId = TestList[0].CatalogId,
            BidHistory = new List<BidDTO>(),
            MinPrice = 5000, 
            EffectId = new EffectDTO
            {
                EffectId = Guid.NewGuid()
            }
        });
        // Second auction
        ListOfAuctions.Add(new Auction
        {
            AuctionId = Guid.Parse("b68e3d5f-1a12-4c0e-99e4-92793f3040d6"),
            Name = "Auction 2",
            Status = AuctionStatus.Closed,
            CatalogId = TestList[1].CatalogId,
            BidHistory = new List<BidDTO>(),
            MinPrice = 10000,
            EffectId = new EffectDTO
            {
                EffectId = Guid.NewGuid()
            }
        });
        return ListOfAuctions;
    }
    
    public Task<Auction> AddAuction(Auction auction)
    {

        ListOfAuctions.Add(auction);
        return Task.FromResult(auction);
    }
    
    public Task<bool> RemoveAuction(Guid id)
    {
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == id);
        if (auction == null)
            return Task.FromResult(false);

        ListOfAuctions.Remove(auction);
        return Task.FromResult(true);
    }

    public Task<Auction> UpdateAuctionStatus(Guid id)
    {
        // Find the auction by ID
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == id);
        if (auction != null)
        {
            // Update the status
            auction.Status = AuctionStatus.Closed;
            return Task.FromResult(auction);
        }
        // If not found, return null or throw an exception
        return Task.FromResult<Auction>(null);
        
    }

    public Task<Auction> AddBidToAuctionById(Guid auctionId, BidDTO bid)
    {
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == auctionId);
        if (auction != null)
        {
            auction.BidHistory.Add(bid);
            auction.CurrentBid = bid;
            //ListOfAuctions. mangler en metode til at opdatere auction listen i in-memory databasen
            return Task.FromResult(auction);
        }
        
        // If not found, return null or throw an exception
        return Task.FromResult<Auction>(null);
        
    }
    
    public Task<List<Auction>> SendAuctionBasedOnStatus( AuctionStatus status)
    {
        var auctions = ListOfAuctions.Where(a => a.Status == status).ToList();
        return Task.FromResult(auctions);
    }
}
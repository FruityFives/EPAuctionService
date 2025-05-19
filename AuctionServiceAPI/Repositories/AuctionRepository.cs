using Models;
using IAuctionRepository = AuctionServiceAPI.Repositories.IAuctionRepository;

namespace AuctionServiceAPI.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly List<Auction> ListOfAuctions = new();
    List<Catalog> TestList = new CatalogRepository().SeedData();

    public AuctionRepository()
    {
        Console.WriteLine(" AuctionRepository seeded");
        SeedDataAuction();
    }
    public Task<Auction> AddAuction(Auction auction)
    {
        ListOfAuctions.Add(auction);
        return Task.FromResult(auction);
    }
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

    public Task<bool> RemoveAuction(Guid id)
    {
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == id);
        if (auction == null)
            return Task.FromResult(false);

        ListOfAuctions.Remove(auction);
        return Task.FromResult(true);
    }

    public Task<Auction?> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == id);
        if (auction != null)
        {
            auction.Status = status;
            return Task.FromResult(auction);
        }
        return Task.FromResult<Auction?>(null);
    }

    public Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        var auctions = ListOfAuctions
            .Where(a => a.CatalogId == catalogId && a.Status == status)
            .ToList();
        return Task.FromResult(auctions);
    }

    public Task<Auction> GetAuctionById(Guid id)
    {
        Console.WriteLine($"Auction RepositoryLooking for auction with ID: {id}");
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == id);
        return Task.FromResult(auction);
    }

    public Task<Auction?> AddBidToAuctionById(BidDTO bid)
    {
        var auctionId = bid.AuctionId;
        // Find the auction by ID
        var auction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == auctionId);
        if (auction != null)
        {
            auction.BidHistory.Add(bid);
            auction.CurrentBid = bid;
            return Task.FromResult(auction);
        }
        return Task.FromResult<Auction?>(null);
    }

    public Task<Auction?> UpdateAuction(Auction auction)
    {
        var existingAuction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == auction.AuctionId);
        if (existingAuction != null)
        {
            existingAuction.Name = auction.Name;
            existingAuction.Status = auction.Status;
            existingAuction.CatalogId = auction.CatalogId;
            existingAuction.BidHistory = auction.BidHistory;
            existingAuction.MinPrice = auction.MinPrice;
            existingAuction.EffectId = auction.EffectId;
            return Task.FromResult(existingAuction);
        }
        return Task.FromResult<Auction?>(null);
    }

}
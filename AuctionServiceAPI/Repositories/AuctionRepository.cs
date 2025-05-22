using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly List<Auction> ListOfAuctions = new();
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly ILogger<AuctionRepository> _logger;

    public AuctionRepository(MongoDbContext context, ILogger<AuctionRepository> logger)
    {
        _logger = logger;
        _auctionCollection = context.AuctionCollection;
        ListOfAuctions = _auctionCollection.AsQueryable().ToList();
        Console.WriteLine("AuctionRepository seeded");
    }

    public async Task<Auction> AddAuction(Auction auction)
    {
        await _auctionCollection.InsertOneAsync(auction);
        _logger.LogInformation($"Auction '{auction.Name}' added with ID: {auction.AuctionId}");
        return auction;
    }

    public List<Auction> SeedDataAuction()
    {
        ListOfAuctions.Add(new Auction
        {
            AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3a7"),
            Name = "Auction 1",
            Status = AuctionStatus.Active,
            // CatalogId = TestList[0].CatalogId, // Kommenteret ud for at undgå NullReferenceException
            BidHistory = new List<BidDTO>(),
            MinPrice = 5000,
            EffectId = new EffectDTO { EffectId = Guid.NewGuid() }
        });

        ListOfAuctions.Add(new Auction
        {
            AuctionId = Guid.Parse("b68e3d5f-1a12-4c0e-99e4-92793f3040d6"),
            Name = "Auction 2",
            Status = AuctionStatus.Closed,
            // CatalogId = TestList[1].CatalogId, // Kommenteret ud for at undgå NullReferenceException
            BidHistory = new List<BidDTO>(),
            MinPrice = 10000,
            EffectId = new EffectDTO { EffectId = Guid.NewGuid() }
        });

        _logger.LogInformation($"Seeded {ListOfAuctions.Count} auctions into ListOfAuctions");
        return ListOfAuctions;
    }

    public async Task<bool> RemoveAuction(Guid id)
    {
        var result = await _auctionCollection.DeleteOneAsync(a => a.AuctionId == id);

        if (result.DeletedCount == 0)
        {
            _logger.LogWarning($"No auction found with ID: {id} to delete.");
            return false;
        }

        _logger.LogInformation($"Auction with ID: {id} deleted successfully.");
        return true;
    }

    public async Task<Auction?> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        _logger.LogInformation($"Updating auction status. AuctionId: {id}, NewStatus: {status}");

        var update = Builders<Auction>.Update.Set(a => a.Status, status);

        var updatedAuction = await _auctionCollection.FindOneAndUpdateAsync(
            a => a.AuctionId == id,
            update,
            new FindOneAndUpdateOptions<Auction> { ReturnDocument = ReturnDocument.After }
        );

        if (updatedAuction != null)
            _logger.LogInformation($"Auction status updated successfully. AuctionId: {id}");
        else
            _logger.LogWarning($"Auction not found for update. AuctionId: {id}");

        return updatedAuction;
    }

    public async Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        _logger.LogInformation($"Fetching active auctions. CatalogId: {catalogId}, Status: {status}");

        var auctions = await _auctionCollection
            .Find(a => a.CatalogId == catalogId && a.Status == status)
            .ToListAsync();

        _logger.LogInformation($"Found {auctions.Count} active auctions for CatalogId: {catalogId} with Status: {status}");

        return auctions;
    }

    public async Task<Auction> GetAuctionById(Guid id)
    {
        var auction = await _auctionCollection.Find(a => a.AuctionId == id).FirstOrDefaultAsync();
        if (auction == null)
            _logger.LogWarning($"Auction with ID: {id} not found.");
        else
            _logger.LogInformation($"Auction with ID: {id} retrieved.");
        return auction;
    }

    public Task<Auction?> UpdateAuction(Auction auction)
    {
        _logger.LogInformation($"Attempting to update auction with ID: {auction.AuctionId}");

        var existingAuction = ListOfAuctions.FirstOrDefault(a => a.AuctionId == auction.AuctionId);

        if (existingAuction != null)
        {
            existingAuction.Name = auction.Name;
            existingAuction.Status = auction.Status;
            existingAuction.CatalogId = auction.CatalogId;
            existingAuction.BidHistory = auction.BidHistory;
            existingAuction.MinPrice = auction.MinPrice;
            existingAuction.EffectId = auction.EffectId;
            existingAuction.CurrentBid = auction.CurrentBid;

            _logger.LogInformation($"Auction with ID: {auction.AuctionId} updated successfully.");
            return Task.FromResult(existingAuction);
        }

        _logger.LogWarning($"Auction with ID: {auction.AuctionId} not found. Update failed.");
        return Task.FromResult<Auction?>(null);
    }

    public async Task SaveAuction(Auction auction)
    {
        var filter = Builders<Auction>.Filter.Eq(a => a.AuctionId, auction.AuctionId);
        var result = await _auctionCollection.ReplaceOneAsync(filter, auction);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            _logger.LogInformation($"Auction with ID: {auction.AuctionId} saved successfully.");
        }
        else
        {
            _logger.LogWarning($"Failed to save auction with ID: {auction.AuctionId}. It may not exist.");
        }
    }
}

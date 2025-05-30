using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Repositories;

/// <summary>
/// Repository til håndtering af auktioner i MongoDB.
/// </summary>
public class AuctionRepository : IAuctionRepository
{
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly ILogger<AuctionRepository> _logger;

    public AuctionRepository(MongoDbContext context, ILogger<AuctionRepository> logger)
    {
        _logger = logger;
        _auctionCollection = context.AuctionCollection;

        Console.WriteLine("AuctionRepository seeded");
    }

    /// <summary>
    /// Tilføjer en ny auktion til databasen.
    /// </summary>
    /// <param name="auction">Auktionsobjektet der skal tilføjes.</param>
    /// <returns>Den tilføjede auktion.</returns>
    public async Task<Auction> AddAuction(Auction auction)
    {
        await _auctionCollection.InsertOneAsync(auction);
        _logger.LogInformation($"Auction '{auction.Name}' added with ID: {auction.AuctionId}");
        return auction;
    }

    /// <summary>
    /// Seeder databasen med to eksempel-auktioner.
    /// </summary>
    /// <returns>Liste over de seedede auktioner.</returns>
    public async Task<List<Auction>> SeedDataAuction()
    {
        var seedAuctions = new List<Auction>
        {
            new Auction
            {
                AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3a7"),
                Name = "Auction 1",
                Status = AuctionStatus.Active,
                BidHistory = new List<BidDTO>(),
                MinPrice = 5000,
                Effect = new EffectDTO { EffectId = Guid.NewGuid() }
            },
            new Auction
            {
                AuctionId = Guid.Parse("b68e3d5f-1a12-4c0e-99e4-92793f3040d6"),
                Name = "Auction 2",
                Status = AuctionStatus.Closed,
                BidHistory = new List<BidDTO>(),
                MinPrice = 10000,
                Effect = new EffectDTO { EffectId = Guid.NewGuid() }
            }
        };

        await _auctionCollection.InsertManyAsync(seedAuctions);
        _logger.LogInformation($"Seeded {seedAuctions.Count} auctions into MongoDB");

        return seedAuctions;
    }

    /// <summary>
    /// Fjerner en auktion ud fra dens ID.
    /// </summary>
    /// <param name="id">ID på auktionen der skal fjernes.</param>
    /// <returns>True hvis den blev fjernet, ellers false.</returns>
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

    /// <summary>
    /// Henter alle auktioner knyttet til et givent katalog-ID.
    /// </summary>
    /// <param name="catalogId">Katalogets ID.</param>
    /// <returns>Liste over auktioner.</returns>
    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        _logger.LogInformation($"Fetching auctions for Catalog ID: {catalogId}");
        var auctions = await _auctionCollection.Find(a => a.CatalogId == catalogId).ToListAsync();
        _logger.LogInformation($"Found {auctions.Count} auctions for Catalog ID: {catalogId}");
        return auctions;
    }

    /// <summary>
    /// Opdaterer status på en specifik auktion.
    /// </summary>
    /// <param name="id">ID på auktionen der skal opdateres.</param>
    /// <param name="status">Ny status der skal sættes.</param>
    /// <returns>Den opdaterede auktion, eller null hvis ikke fundet.</returns>
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

    /// <summary>
    /// Henter alle aktive auktioner med en specifik status for et katalog.
    /// </summary>
    /// <param name="catalogId">ID på kataloget.</param>
    /// <param name="status">Status som skal filtreres på.</param>
    /// <returns>Liste over auktioner.</returns>
    public async Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        _logger.LogInformation($"Fetching active auctions. CatalogId: {catalogId}, Status: {status}");

        var auctions = await _auctionCollection
            .Find(a => a.CatalogId == catalogId && a.Status == status)
            .ToListAsync();

        _logger.LogInformation($"Found {auctions.Count} active auctions for CatalogId: {catalogId} with Status: {status}");

        return auctions;
    }

    /// <summary>
    /// Henter en auktion baseret på ID.
    /// </summary>
    /// <param name="id">ID på auktionen.</param>
    /// <returns>Auktionsobjektet hvis fundet, ellers null.</returns>
    public async Task<Auction> GetAuctionById(Guid id)
    {
        var auction = await _auctionCollection.Find(a => a.AuctionId == id).FirstOrDefaultAsync();
        if (auction == null)
            _logger.LogWarning($"Auction with ID: {id} not found.");
        else
            _logger.LogInformation($"Auction with ID: {id} retrieved.");
        return auction;
    }

    /// <summary>
    /// Opdaterer en eksisterende auktion.
    /// </summary>
    /// <param name="auction">Auktion med nye data.</param>
    /// <returns>Den opdaterede auktion, eller null hvis den ikke fandtes.</returns>
    public async Task<Auction?> UpdateAuction(Auction auction)
    {
        _logger.LogInformation($"Updating auction with ID: {auction.AuctionId}");

        var filter = Builders<Auction>.Filter.Eq(a => a.AuctionId, auction.AuctionId);
        var result = await _auctionCollection.ReplaceOneAsync(filter, auction);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            _logger.LogInformation($"Auction with ID: {auction.AuctionId} updated successfully.");
            return auction;
        }

        _logger.LogWarning($"Auction with ID: {auction.AuctionId} not found. Update failed.");
        return null;
    }

    /// <summary>
    /// Gemmer en auktion ved at overskrive den eksisterende.
    /// </summary>
    /// <param name="auction">Auktion der skal gemmes.</param>
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


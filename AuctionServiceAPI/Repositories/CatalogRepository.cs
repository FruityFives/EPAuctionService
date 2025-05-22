using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

public class CatalogRepository : ICatalogRepository
{
    private readonly List<Catalog> ListOfCatalogs;
    private readonly List<Auction> ListOfAuctions = new();
    private readonly IMongoCollection<Catalog> _catalogCollection;
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly ILogger<CatalogRepository> _logger;

    /// Constructor
    public CatalogRepository(MongoDbContext context, ILogger<CatalogRepository> logger)
    {
        _logger = logger;
        _catalogCollection = context.CatalogCollection;
        _auctionCollection = context.AuctionCollection;
        ListOfCatalogs = context.CatalogCollection.AsQueryable().ToList();
        Console.WriteLine("CatalogRepository seeded");
    }

    public async Task<Catalog> AddCatalog(Catalog catalog)
    {
        await _catalogCollection.InsertOneAsync(catalog);
        _logger.LogInformation($"Catalog {catalog.Name} added with ID: {catalog.CatalogId}");
        return catalog; //s
    }

    public List<Catalog> SeedDataCatalog()
    {
        ListOfCatalogs.Add(new Catalog
        {
            CatalogId = Guid.Parse("d1f8c03f-8405-4d0e-b86b-6ad94ea4a3a7"),
            Name = "Catalog 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = CatalogStatus.Active
        });

        ListOfCatalogs.Add(new Catalog
        {
            CatalogId = Guid.Parse("e68e3d5f-1a12-4c0e-99e4-92793f3040d6"),
            Name = "Catalog 2",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = CatalogStatus.Closed
        });

        return ListOfCatalogs;
    }

    public async Task<bool> RemoveCatalog(Guid id)
    {
        var result = await _catalogCollection.DeleteOneAsync(c => c.CatalogId == id);
        if (result.DeletedCount == 0)
        {
            _logger.LogWarning($"No catalog found to delete with ID: {id}");
            return false;
        }

        _logger.LogInformation($"Catalog with ID: {id} removed");
        return true;
    }

    public async Task<Catalog> GetCatalogById(Guid id)
    {
        var catalog = await _catalogCollection.Find(c => c.CatalogId == id).FirstOrDefaultAsync();
        if (catalog == null)
        {
            _logger.LogWarning($"Catalog with ID: {id} not found.");
        }
        else
        {
            _logger.LogInformation($"Catalog with ID: {id} retrieved.");
        }
        return catalog;
    }

    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        _logger.LogInformation($"Fetching auctions for Catalog ID: {catalogId}");
        var auctions = await _auctionCollection.Find(a => a.CatalogId == catalogId).ToListAsync();
        _logger.LogInformation($"Found {auctions.Count} auctions for Catalog ID: {catalogId}");
        return auctions;
    }

    public async Task<Catalog?> UpdateCatalog(Catalog catalog)
    {
        var filter = Builders<Catalog>.Filter.Eq(c => c.CatalogId, catalog.CatalogId);
        var result = await _catalogCollection.ReplaceOneAsync(filter, catalog);
        bool updateSucceeded = result.IsAcknowledged && result.ModifiedCount > 0;
        if (updateSucceeded)
        {
            _logger.LogInformation($"Catalog with ID: {catalog.CatalogId} updated successfully.");
            return catalog;
        }
        _logger.LogWarning($"Update failed for Catalog with ID: {catalog.CatalogId}. Catalog may not exist.");
        return null;
    }

    public async Task<List<Catalog>> GetAllCatalogs()
    {
        _logger.LogInformation("Retrieving all catalogs.");
        var catalogs = await _catalogCollection.Find(c => true).ToListAsync();
        _logger.LogInformation($"Retrieved {catalogs.Count} catalogs.");
        return catalogs;
    }

    public async Task SaveCatalog(Catalog catalog)
    {
        var filter = Builders<Catalog>.Filter.Eq(c => c.CatalogId, catalog.CatalogId);
        var result = await _catalogCollection.ReplaceOneAsync(filter, catalog);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            _logger.LogInformation($"Catalog with ID: {catalog.CatalogId} saved successfully.");
        }
        else
        {
            _logger.LogWarning($"Failed to save catalog with ID: {catalog.CatalogId}. It may not exist.");
        }
    }
}

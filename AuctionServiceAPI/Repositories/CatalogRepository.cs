using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AuctionServiceAPI.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly List<Catalog> ListOfCatalogs;
    private readonly List<Auction> ListOfAuctions = new();
    private readonly IMongoCollection<Catalog> _catalogCollection;

    private readonly IMongoCollection<Auction> _auctionCollection;

    public CatalogRepository(MongoDbContext context)
    {
        _catalogCollection = context.CatalogCollection;
        _auctionCollection = context.AuctionCollection;
        ListOfCatalogs = context.CatalogCollection.AsQueryable().ToList();
        Console.WriteLine("CatalogRepository seeded");
    }

    public async Task<Catalog> AddCatalog(Catalog catalog)
    {
        await _catalogCollection.InsertOneAsync(catalog);
        return catalog;
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

    public Task<bool> RemoveCatalog(Guid id)
    {
        return _catalogCollection.DeleteOneAsync(c => c.CatalogId == id)
            .ContinueWith(task => task.Result.DeletedCount > 0);

        /*
    var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id);
    if (catalog == null) return Task.FromResult(false);
    ListOfCatalogs.Remove(catalog);
    return Task.FromResult(true);
    */
    }

    public async Task<Catalog> GetCatalogById(Guid id)
    {
        return await _catalogCollection.Find(c => c.CatalogId == id).FirstOrDefaultAsync();
        /*
        var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id);
        return Task.FromResult(catalog);
        */
    }

    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        return await _auctionCollection
            .Find(a => a.CatalogId == catalogId)
            .ToListAsync();
    }

    public async Task<Catalog?> UpdateCatalog(Catalog catalog)
    {
        var filter = Builders<Catalog>.Filter.Eq(c => c.CatalogId, catalog.CatalogId);
        var result = await _catalogCollection.ReplaceOneAsync(filter, catalog);

        bool updateSucceeded = result.IsAcknowledged && result.ModifiedCount > 0;

        if (updateSucceeded)
        {
            return catalog;
        }

        return null;

        /*
        var existing = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalog.CatalogId);
        if (existing != null)
        {
            existing.Name = catalog.Name;
            existing.StartDate = catalog.StartDate;
            existing.EndDate = catalog.EndDate;
            existing.Status = catalog.Status;
            return Task.FromResult(existing);
        }
        return Task.FromResult<Catalog?>(null);
        */
    }

    public Task<List<Catalog>> GetAllCatalogs()
    {
        return _catalogCollection
            .Find(c => true)
            .ToListAsync();
        /*
        return Task.FromResult(ListOfCatalogs);
        */
    }

    public Task SaveAuction(Auction auction)
        => Task.CompletedTask;

    public async Task SaveCatalog(Catalog catalog)
    {
        var filter = Builders<Catalog>.Filter.Eq(c => c.CatalogId, catalog.CatalogId);
        await _catalogCollection.ReplaceOneAsync(filter, catalog);

    }
}

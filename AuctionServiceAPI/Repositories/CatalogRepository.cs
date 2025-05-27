using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Repository til håndtering af kataloger og tilhørende auktioner i MongoDB.
/// </summary>
public class CatalogRepository : ICatalogRepository
{
    private readonly List<Catalog> ListOfCatalogs;
    private readonly List<Auction> ListOfAuctions = new();
    private readonly IMongoCollection<Catalog> _catalogCollection;
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly ILogger<CatalogRepository> _logger;

    /// <summary>
    /// Initialiserer CatalogRepository med MongoDB context og logger.
    /// </summary>
    public CatalogRepository(MongoDbContext context, ILogger<CatalogRepository> logger)
    {
        _logger = logger;
        _catalogCollection = context.CatalogCollection;
        _auctionCollection = context.AuctionCollection;
        ListOfCatalogs = context.CatalogCollection.AsQueryable().ToList();
        Console.WriteLine("CatalogRepository seeded");
    }

    /// <summary>
    /// Tilføjer et nyt katalog til databasen.
    /// </summary>
    /// <param name="catalog">Det katalog der skal tilføjes.</param>
    /// <returns>Det tilføjede katalog.</returns>
    public async Task<Catalog> AddCatalog(Catalog catalog)
    {
        await _catalogCollection.InsertOneAsync(catalog);
        _logger.LogInformation($"Catalog {catalog.Name} added with ID: {catalog.CatalogId}");
        return catalog;
    }

    /// <summary>
    /// Seeder lokale katalogdata med eksempler.
    /// </summary>
    /// <returns>Listen over seedede kataloger.</returns>
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

    /// <summary>
    /// Fjerner et katalog fra databasen baseret på ID.
    /// </summary>
    /// <param name="id">ID på kataloget der skal fjernes.</param>
    /// <returns>True hvis kataloget blev fjernet, ellers false.</returns>
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

    /// <summary>
    /// Henter et katalog baseret på dets ID.
    /// </summary>
    /// <param name="id">ID på kataloget.</param>
    /// <returns>Returnerer kataloget hvis fundet, ellers null.</returns>
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

    /// <summary>
    /// Henter alle auktioner, der er knyttet til et specifikt katalog-ID.
    /// </summary>
    /// <param name="catalogId">ID på kataloget.</param>
    /// <returns>Liste over auktioner.</returns>
    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        _logger.LogInformation($"Fetching auctions for Catalog ID: {catalogId}");
        var auctions = await _auctionCollection.Find(a => a.CatalogId == catalogId).ToListAsync();
        _logger.LogInformation($"Found {auctions.Count} auctions for Catalog ID: {catalogId}");
        return auctions;
    }

    /// <summary>
    /// Opdaterer et katalog i databasen.
    /// </summary>
    /// <param name="catalog">Kataloget med opdaterede værdier.</param>
    /// <returns>Returnerer det opdaterede katalog eller null hvis det ikke blev fundet.</returns>
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

    /// <summary>
    /// Henter alle kataloger fra databasen.
    /// </summary>
    /// <returns>Liste over alle kataloger.</returns>
    public async Task<List<Catalog>> GetAllCatalogs()
    {
        _logger.LogInformation("Retrieving all catalogs.");
        var catalogs = await _catalogCollection.Find(c => true).ToListAsync();
        _logger.LogInformation($"Retrieved {catalogs.Count} catalogs.");
        return catalogs;
    }

    /// <summary>
    /// Gemmer et katalog ved at overskrive det eksisterende i databasen.
    /// </summary>
    /// <param name="catalog">Kataloget der skal gemmes.</param>
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

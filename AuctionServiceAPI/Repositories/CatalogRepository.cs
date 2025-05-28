using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Repository til håndtering af katalogdata i MongoDB.
/// </summary>
public class CatalogRepository : ICatalogRepository
{
    private readonly List<Catalog> ListOfCatalogs;
    private readonly List<Auction> ListOfAuctions = new();
    private readonly IMongoCollection<Catalog> _catalogCollection;
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly ILogger<CatalogRepository> _logger;

    /// <summary>
    /// Initialiserer CatalogRepository med MongoDB-kontekst og logger.
    /// </summary>
    /// <param name="context">MongoDB-kontekst</param>
    /// <param name="logger">Logger til logning</param>
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
    /// <param name="catalog">Kataloget der skal tilføjes</param>
    /// <returns>Det oprettede katalog</returns>
    public async Task<Catalog> AddCatalog(Catalog catalog)
    {
        await _catalogCollection.InsertOneAsync(catalog);
        _logger.LogInformation($"Catalog {catalog.Name} added with ID: {catalog.CatalogId}");
        return catalog;
    }


    /// <summary>
    /// Fjerner et katalog fra databasen.
    /// </summary>
    /// <param name="id">ID for kataloget der skal fjernes</param>
    /// <returns>True hvis kataloget blev fjernet, ellers false</returns>
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
    /// Henter et katalog ud fra dets ID.
    /// </summary>
    /// <param name="id">ID på det ønskede katalog</param>
    /// <returns>Kataloget hvis det findes, ellers null</returns>
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
    /// Henter alle auktioner tilknyttet et katalog.
    /// </summary>
    /// <param name="catalogId">ID på det ønskede katalog</param>
    /// <returns>Liste med auktioner i kataloget</returns>
    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        _logger.LogInformation($"Fetching auctions for Catalog ID: {catalogId}");
        var auctions = await _auctionCollection.Find(a => a.CatalogId == catalogId).ToListAsync();
        _logger.LogInformation($"Found {auctions.Count} auctions for Catalog ID: {catalogId}");
        return auctions;
    }

    /// <summary>
    /// Opdaterer et eksisterende katalog i databasen.
    /// </summary>
    /// <param name="catalog">Kataloget der skal opdateres</param>
    /// <returns>Det opdaterede katalog hvis opdatering lykkedes, ellers null</returns>
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
    /// <returns>Liste med alle kataloger</returns>
    public async Task<List<Catalog>> GetAllCatalogs()
    {
        _logger.LogInformation("Retrieving all catalogs.");
        var catalogs = await _catalogCollection.Find(c => true).ToListAsync();
        _logger.LogInformation($"Retrieved {catalogs.Count} catalogs.");
        return catalogs;
    }

    /// <summary>
    /// Gemmer (overskriver) et katalog i databasen.
    /// </summary>
    /// <param name="catalog">Kataloget der skal gemmes</param>
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

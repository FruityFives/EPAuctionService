using Models;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Service til håndtering af kataloger og tilknyttede auktioner.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IAuctionService _auctionService;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IStoragePublisherRabbit _storagePublisher;
    private readonly ILogger<CatalogService> _logger;

    /// <summary>
    /// Initialiserer CatalogService med nødvendige repositories og services.
    /// </summary>
    public CatalogService(
      IAuctionService auctionService,
      ICatalogRepository catalogRepository,
      IAuctionRepository auctionRepository,
      IStoragePublisherRabbit storagePublisher,
      ILogger<CatalogService> logger)
    {
        _auctionService = auctionService;
        _catalogRepository = catalogRepository;
        _auctionRepository = auctionRepository;
        _storagePublisher = storagePublisher;
        _logger = logger;
    }

    /// <summary>
    /// Henter alle aktive auktioner fra alle kataloger.
    /// </summary>
    public async Task<List<Auction>> GetAllActiveAuctions()
    {
        _logger.LogInformation("Fetching all active auctions from all catalogs");

        var catalogs = await _catalogRepository.GetAllCatalogs();
        var allAuctions = new List<Auction>();

        _logger.LogInformation("Found {Count} catalogs", catalogs.Count);
        foreach (var catalog in catalogs)
        {
            var activeAuctions = await _auctionRepository.SendActiveAuctions(catalog.CatalogId, AuctionStatus.Active);
            _logger.LogInformation("Catalog {CatalogId} has {Count} active auctions", catalog.CatalogId, activeAuctions.Count);
            allAuctions.AddRange(activeAuctions);
        }

        return allAuctions;
    }

    /// <summary>
    /// Importerer effekter fra StorageService og opretter auktioner.
    /// </summary>
    public async Task<List<Auction>> ImportEffectsFromStorageAsync()
    {
        return await _auctionService.ImportEffectsFromStorageAsync();
    }

    /// <summary>
    /// Opretter et nyt katalog.
    /// </summary>
    public async Task<Catalog> CreateCatalog(Catalog catalog)
    {
        catalog.CatalogId = Guid.NewGuid();
        var created = await _catalogRepository.AddCatalog(catalog);
        _logger.LogInformation("Created new catalog with ID: {CatalogId}", created.CatalogId);
        return created;
    }

    /// <summary>
    /// Sletter et katalog baseret på dets ID.
    /// </summary>
    public Task<bool> DeleteCatalog(Guid id)
    {
        _logger.LogInformation("Deleting catalog with ID: {CatalogId}", id);
        return _catalogRepository.RemoveCatalog(id);
    }

    /// <summary>
    /// Henter både aktive og lukkede auktioner for et givent katalog.
    /// </summary>
    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        _logger.LogInformation("Getting auctions for catalog ID: {CatalogId}", catalogId);

        var catalog = await _catalogRepository.GetCatalogById(catalogId);
        if (catalog == null)
        {
            _logger.LogWarning("Catalog not found with ID: {CatalogId}", catalogId);
            throw new Exception("Catalog not found");
        }

        var active = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Active);
        var closed = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Closed);

        return active.Concat(closed).ToList();
    }

    /// <summary>
    /// Henter et katalog baseret på dets ID.
    /// </summary>
    public Task<Catalog> GetCatalogById(Guid id)
    {
        _logger.LogInformation("Fetching catalog with ID: {CatalogId}", id);
        return _catalogRepository.GetCatalogById(id);
    }

    /// <summary>
    /// Opdaterer et eksisterende katalog.
    /// </summary>
    public Task<Catalog?> UpdateCatalog(Catalog catalog)
    {
        if (catalog == null)
        {
            _logger.LogWarning("Catalog is null, cannot update.");
            throw new ArgumentNullException(nameof(catalog));
        }
        _logger.LogInformation("Updating catalog with ID: {CatalogId}", catalog.CatalogId);
        return _catalogRepository.UpdateCatalog(catalog);
    }

    /// <summary>
    /// Henter alle kataloger.
    /// </summary>
    public Task<List<Catalog>> GetAllCatalogs()
    {
        _logger.LogInformation("Fetching all catalogs");
        return _catalogRepository.GetAllCatalogs();
    }

    /// <summary>
    /// Afslutter et katalog og opdaterer tilhørende auktioner som lukkede.
    /// Publicerer resultater til StorageService via RabbitMQ.
    /// </summary>
    public async Task EndCatalog(Guid catalogId)
    {
        _logger.LogInformation("Ending catalog with ID: {CatalogId}", catalogId);

        var catalog = await _catalogRepository.GetCatalogById(catalogId)
                      ?? throw new Exception("Catalog not found");

        var auctions = await _catalogRepository.GetAuctionsByCatalogId(catalogId);
        if (auctions.Count == 0)
        {
            _logger.LogWarning("No auctions found for catalog ID: {CatalogId}", catalogId);
            throw new Exception("No auctions found for this catalog");
        }

        catalog.Status = CatalogStatus.Closed;
        await _catalogRepository.SaveCatalog(catalog);
        _logger.LogInformation("Catalog marked as closed: {CatalogId}", catalogId);

        foreach (var auction in auctions)
        {
            auction.Status = AuctionStatus.Closed;
            await _auctionRepository.SaveAuction(auction);
            _logger.LogInformation("Closed auction with ID: {AuctionId}", auction.AuctionId);

            if (auction.Effect == null)
            {
                _logger.LogWarning("Auction with ID {AuctionId} has null Effect. Skipping publish.", auction.AuctionId);
                continue;
            }

            var dto = new AuctionDTO
            {
                EffectId = auction.Effect.EffectId,
                WinnerUserId = auction.CurrentBid?.UserId,
                FinalPrice = (decimal?)auction.CurrentBid?.Amount,
                IsSold = auction.CurrentBid != null
            };

            await _storagePublisher.PublishAuctionAsync(dto);
            _logger.LogInformation("Published auction result for Effect ID: {EffectId}, Sold: {IsSold}, Final Price: {FinalPrice}",
                dto.EffectId, dto.IsSold, dto.FinalPrice);
        }
    }

    /// <summary>
    /// Håndterer afslutningen af auktioner uden nødvendigvis at lukke kataloget selv.
    /// Publicerer resultater til StorageService.
    /// </summary>
    public async Task HandleAuctionFinish(Guid catalogId)
    {
        _logger.LogInformation("Handling auction finish for catalog ID: {CatalogId}", catalogId);

        var catalog = await _catalogRepository.GetCatalogById(catalogId)
                      ?? throw new Exception("Catalog not found");

        var auctions = await _catalogRepository.GetAuctionsByCatalogId(catalogId);

        foreach (var auction in auctions)
        {
            var dto = new AuctionDTO
            {
                EffectId = auction.Effect.EffectId,
                WinnerUserId = auction.CurrentBid?.UserId,
                FinalPrice = (decimal?)auction.CurrentBid?.Amount,
                IsSold = auction.CurrentBid != null
            };

            await _storagePublisher.PublishAuctionAsync(dto);
            _logger.LogInformation("Published auction result for Effect ID: {EffectId}, Sold: {IsSold}, Final Price: {FinalPrice}",
                dto.EffectId, dto.IsSold, dto.FinalPrice);
        }

        catalog.Status = CatalogStatus.Closed;
        await _catalogRepository.SaveCatalog(catalog);
        _logger.LogInformation("Catalog status updated to 'Closed' for catalog ID: {CatalogId}", catalogId);
    }
}

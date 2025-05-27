using AuctionServiceAPI.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Service-lag for logik relateret til auktioner, herunder oprettelse, bud, statusopdateringer m.m.
/// </summary>
public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ILogger<AuctionService> _logger;

    /// <summary>
    /// Initialiserer AuctionService med nødvendige repositories og logger.
    /// </summary>
    public AuctionService(
        IAuctionRepository auctionRepository,
        ICatalogRepository catalogRepository,
        ILogger<AuctionService> logger)
    {
        _auctionRepository = auctionRepository;
        _catalogRepository = catalogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Opretter en ny auktion og knytter den til et katalog, hvis angivet.
    /// </summary>
    /// <param name="auction">Auktionsobjektet der skal oprettes.</param>
    /// <returns>Returnerer den oprettede auktion.</returns>
    public async Task<Auction> CreateAuction(Auction auction)
    {
        auction.AuctionId = Guid.NewGuid();
        auction.Status = AuctionStatus.Active;

        var createdAuction = await _auctionRepository.AddAuction(auction);
        _logger.LogInformation($"Auction created with ID: {createdAuction.AuctionId}");

        if (auction.CatalogId.HasValue)
        {
            var catalog = await _catalogRepository.GetCatalogById(auction.CatalogId.Value);
            if (catalog != null)
            {
                _logger.LogInformation($"Updating catalog with ID: {catalog.CatalogId} after auction creation.");
                await _catalogRepository.UpdateCatalog(catalog);
            }
            else
            {
                _logger.LogWarning($"Catalog with ID: {auction.CatalogId} not found when creating auction.");
            }
        }
        else
        {
            _logger.LogInformation("Auction created without a catalog assignment.");
        }

        return createdAuction;
    }

    /// <summary>
    /// Importerer effekter fra StorageService og konverterer dem til inaktive auktioner.
    /// </summary>
    /// <returns>Liste af de auktioner der er blevet oprettet.</returns>
    public async Task<List<Auction>> ImportEffectsFromStorageAsync()
    {
        using var httpClient = new HttpClient();
        var url = "http://storage-service:5000/api/storage/effectsforauction";

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Could not fetch effects: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var effects = JsonSerializer.Deserialize<List<EffectDTO>>(content, options);
        if (effects == null || effects.Count == 0)
            return new List<Auction>();

        var createdAuctions = new List<Auction>();

        foreach (var effect in effects)
        {
            effect.Status = EffectDTOStatus.InAuction;
            var auction = new Auction
            {
                AuctionId = Guid.NewGuid(),
                Name = effect.Title,
                MinPrice = (double)effect.AssessmentPrice,
                Status = AuctionStatus.Inactive,
                CatalogId = null,
                BidHistory = new List<BidDTO>(),
                Effect = effect,
            };

            await _auctionRepository.AddAuction(auction);
            createdAuctions.Add(auction);
        }

        return createdAuctions;
    }

    /// <summary>
    /// Tilføjer en eksisterende auktion til et specifikt katalog og sætter minimumspris.
    /// </summary>
    /// <param name="auctionId">ID på auktionen der skal tilføjes.</param>
    /// <param name="catalogId">ID på kataloget.</param>
    /// <param name="minPrice">Minimumspris for auktionen.</param>
    /// <returns>Returnerer den opdaterede auktion, eller null hvis den ikke findes.</returns>
    public async Task<Auction?> AddAuctionToCatalog(Guid auctionId, Guid catalogId, double minPrice)
    {
        var auction = await _auctionRepository.GetAuctionById(auctionId);
        if (auction == null) return null;

        var catalog = await _catalogRepository.GetCatalogById(catalogId);
        if (catalog == null) return null;

        auction.CatalogId = catalogId;
        auction.MinPrice = minPrice;
        auction.Status = AuctionStatus.Active;
        auction.EndDate = catalog.EndDate;

        await _auctionRepository.SaveAuction(auction);
        return auction;
    }

    /// <summary>
    /// Henter en auktion baseret på ID.
    /// </summary>
    public Task<Auction> GetAuctionById(Guid id)
    {
        _logger.LogInformation($"Fetching auction with ID: {id}");
        return _auctionRepository.GetAuctionById(id);
    }

    /// <summary>
    /// Sletter en auktion baseret på ID.
    /// </summary>
    public Task<bool> DeleteAuction(Guid id)
    {
        _logger.LogInformation($"Deleting auction with ID: {id}");
        return _auctionRepository.RemoveAuction(id);
    }

    /// <summary>
    /// Opdaterer status på en auktion.
    /// </summary>
    public Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        _logger.LogInformation($"Updating auction status. Auction ID: {id}, New Status: {status}");
        return _auctionRepository.UpdateAuctionStatus(id, status);
    }

    /// <summary>
    /// Returnerer aktive auktioner for et bestemt katalog og status.
    /// </summary>
    public Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        _logger.LogInformation($"Sending active auctions for Catalog ID: {catalogId} with Status: {status}");
        return _auctionRepository.SendActiveAuctions(catalogId, status);
    }

    /// <summary>
    /// Opdaterer en hel auktion (f.eks. ved redigering).
    /// </summary>
    public Task<Auction?> UpdateAuction(Auction auction)
    {
        _logger.LogInformation($"Updating auction with ID: {auction.AuctionId}");
        return _auctionRepository.UpdateAuction(auction);
    }

    /// <summary>
    /// Opretter et bud på en auktion, hvis auktionen er aktiv og ikke afsluttet.
    /// </summary>
    /// <param name="bid">Det bud der skal placeres.</param>
    /// <returns>Den opdaterede auktion med det nye bud.</returns>
    public async Task<Auction> CreateBidToAuctionById(BidDTO bid)
    {
        _logger.LogInformation($"Creating bid for Auction ID: {bid.AuctionId}");

        var auction = await _auctionRepository.GetAuctionById(bid.AuctionId)
                      ?? throw new Exception("Auction not found");

        if (auction.Status != AuctionStatus.Active)
        {
            _logger.LogWarning($"Cannot place bid. Auction with ID: {bid.AuctionId} is not active.");
            throw new Exception("Auction is not active");
        }

        if (auction.EndDate < DateTime.UtcNow)
        {
            _logger.LogWarning($"Cannot place bid. Auction with ID: {bid.AuctionId} is expired. EndDate: {auction.EndDate}");
            throw new Exception("Auction has ended");
        }

        bid.Timestamp = DateTime.UtcNow;
        auction.BidHistory.Add(bid);
        auction.CurrentBid = bid;

        await _auctionRepository.SaveAuction(auction);
        _logger.LogInformation($"Bid for Auction ID: {bid.AuctionId} created successfully.");
        return auction;
    }
}





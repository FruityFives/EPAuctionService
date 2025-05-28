using AuctionServiceAPI.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Service til håndtering af forretningslogik for auktioner.
/// </summary>
public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly IAuctionPublisherRabbit _publisher;
    private readonly IAuctionPublisherRabbit _syncPublisher;
    private readonly ILogger<AuctionService> _logger;



    private readonly IConfiguration _config;


    /// <summary>
    /// Initialiserer AuctionService med nødvendige repositories, RabbitMQ publishers og logger.
    /// </summary>
    /// <param name="auctionRepository">Repository til auktioner</param>
    /// <param name="catalogRepository">Repository til kataloger</param>
    /// <param name="publisher">RabbitMQ publisher</param>
    /// <param name="syncPublisher">RabbitMQ publisher til synkronisering</param>
    /// <param name="logger">Logger til logning</param>
    public AuctionService(
        IAuctionRepository auctionRepository,
        ICatalogRepository catalogRepository,
        IAuctionPublisherRabbit publisher,
        IAuctionPublisherRabbit syncPublisher,
        ILogger<AuctionService> logger,
        IConfiguration config)
    {
        _auctionRepository = auctionRepository;
        _catalogRepository = catalogRepository;
        _publisher = publisher;
        _syncPublisher = syncPublisher;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Importerer effekter fra StorageService og opretter nye auktioner.
    /// </summary>
    /// <returns>Liste over oprettede auktioner</returns>
    public async Task<List<Auction>> ImportEffectsFromStorageAsync()
    {
        using var httpClient = new HttpClient();

        var baseUrl = _config["STORAGE_SERVICE_BASE_URL"];
        var url = $"{baseUrl}/effectsforauction";

        _logger.LogInformation("Sender GET-request til: {Url}", url);

        var response = await httpClient.GetAsync(url);
        _logger.LogInformation("Modtaget svar med statuskode: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Fejl ved hentning af effekter. Status: {StatusCode}, Body: {Body}", response.StatusCode, errorContent);
            throw new Exception($"Could not fetch effects: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Modtaget body fra StorageService: {Content}", content);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var effects = JsonSerializer.Deserialize<List<EffectDTO>>(content, options);

        if (effects == null || effects.Count == 0)
        {
            _logger.LogWarning("Ingen effekter blev returneret fra StorageService.");
            return new List<Auction>();
        }

        _logger.LogInformation("Antal effekter modtaget: {Count}", effects.Count);

        var createdAuctions = new List<Auction>();

        foreach (var effect in effects)
        {
            _logger.LogInformation("Behandler effekt med ID: {EffectId}", effect.EffectId);

            var markAsInAuctionUrl = $"{baseUrl}/markAsInAuction/{effect.EffectId}";
            _logger.LogInformation("Sender POST-request til: {Url}", markAsInAuctionUrl);

            var updateResponse = await httpClient.PostAsync(markAsInAuctionUrl, null);

            if (!updateResponse.IsSuccessStatusCode)
            {
                var responseBody = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Kunne ikke opdatere status for effekt {EffectId}. Status: {StatusCode}, Body: {Body}",
                    effect.EffectId, updateResponse.StatusCode, responseBody);
                continue;
            }

            _logger.LogInformation("Effekt {EffectId} markeret som InAuction", effect.EffectId);
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
            _logger.LogInformation("Auktion oprettet med ID: {AuctionId} for effekt {EffectId}", auction.AuctionId, effect.EffectId);
            createdAuctions.Add(auction);
        }
        _logger.LogInformation("Total auktioner oprettet: {Count}", createdAuctions.Count);
        return createdAuctions;
    }


    /// <summary>
    /// Tildeler en auktion til et katalog og opdaterer dens status.
    /// </summary>
    /// <param name="auctionId">ID på auktionen</param>
    /// <param name="catalogId">ID på kataloget</param>
    /// <param name="minPrice">Minimumspris for auktionen</param>
    /// <returns>Den opdaterede auktion eller null, hvis auktion eller katalog ikke blev fundet</returns>
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

        var syncDto = new AuctionDTO
        {
            AuctionId = auction.AuctionId,
            Status = auction.Status,
            MinBid = Convert.ToDecimal(auction.MinPrice),
            CurrentBid = Convert.ToDecimal(auction.CurrentBid?.Amount ?? 0),
            EndDate = auction.EndDate
        };

        await _syncPublisher.PublishAuctionAsync(syncDto);
        _logger.LogInformation("Auction {AuctionId} synced to BidService", auction.AuctionId);

        return auction;
    }

    /// <summary>
    /// Henter en auktion baseret på ID.
    /// </summary>
    /// <param name="id">ID på auktionen</param>
    /// <returns>Den fundne auktion</returns>
    public Task<Auction> GetAuctionById(Guid id)
    {
        _logger.LogInformation($"Fetching auction with ID: {id}");
        return _auctionRepository.GetAuctionById(id);
    }

    /// <summary>
    /// Sletter en auktion ud fra ID.
    /// </summary>
    /// <param name="id">ID på auktionen</param>
    /// <returns>True hvis auktionen blev slettet, ellers false</returns>
    public Task<bool> DeleteAuction(Guid id)
    {
        _logger.LogInformation($"Deleting auction with ID: {id}");
        return _auctionRepository.RemoveAuction(id);
    }

    /// <summary>
    /// Opdaterer status for en auktion.
    /// </summary>
    /// <param name="id">ID på auktionen</param>
    /// <param name="status">Ny status</param>
    /// <returns>Den opdaterede auktion</returns>
    public Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        _logger.LogInformation($"Updating auction status. Auction ID: {id}, New Status: {status}");
        return _auctionRepository.UpdateAuctionStatus(id, status);
    }

    /// <summary>
    /// Sender aktive auktioner i et katalog til en ekstern service.
    /// </summary>
    /// <param name="catalogId">ID på kataloget</param>
    /// <param name="status">Filtrering efter auktionens status</param>
    /// <returns>Liste over aktive auktioner</returns>
    public Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        _logger.LogInformation($"Sending active auctions for Catalog ID: {catalogId} with Status: {status}");
        return _auctionRepository.SendActiveAuctions(catalogId, status);
    }

    /// <summary>
    /// Opdaterer en eksisterende auktion.
    /// </summary>
    /// <param name="auction">Auktionsobjekt der skal opdateres</param>
    /// <returns>Den opdaterede auktion eller null</returns>
    public Task<Auction?> UpdateAuction(Auction auction)
    {
        _logger.LogInformation($"Updating auction with ID: {auction.AuctionId}");
        return _auctionRepository.UpdateAuction(auction);
    }

    /// <summary>
    /// Opretter et bud på en specifik auktion.
    /// </summary>
    /// <param name="bid">Buddata, inkl. auktionens ID og beløb</param>
    /// <returns>Den opdaterede auktion med buddet tilføjet</returns>
    /// <exception cref="Exception">Kastes hvis auktionen ikke findes, er inaktiv eller afsluttet</exception>
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
            _logger.LogWarning($"Cannot place bid. Auction with ID: {bid.AuctionId} has ended.");
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

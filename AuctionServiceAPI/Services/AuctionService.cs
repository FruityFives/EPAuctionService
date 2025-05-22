using AuctionServiceAPI.Repositories;
using Models;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ILogger<AuctionService> _logger;

    public AuctionService(
        IAuctionRepository auctionRepository,
        ICatalogRepository catalogRepository,
        ILogger<AuctionService> logger)
    {
        _auctionRepository = auctionRepository;
        _catalogRepository = catalogRepository;
        _logger = logger;
    }

    public async Task<Auction> CreateAuction(Auction auction)
    {
        auction.AuctionId = Guid.NewGuid();
        auction.Status = AuctionStatus.Active;

        var createdAuction = await _auctionRepository.AddAuction(auction);
        _logger.LogInformation($"Auction created with ID: {createdAuction.AuctionId}");

        var catalog = await _catalogRepository.GetCatalogById(auction.CatalogId);
        if (catalog != null)
        {
            _logger.LogInformation($"Updating catalog with ID: {catalog.CatalogId} after auction creation.");
            await _catalogRepository.UpdateCatalog(catalog);
        }
        else
        {
            _logger.LogWarning($"Catalog with ID: {auction.CatalogId} not found when creating auction.");
        }

        return createdAuction;
    }

    public Task<Auction> GetAuctionById(Guid id)
    {
        _logger.LogInformation($"Fetching auction with ID: {id}");
        return _auctionRepository.GetAuctionById(id);
    }

    public Task<bool> DeleteAuction(Guid id)
    {
        _logger.LogInformation($"Deleting auction with ID: {id}");
        return _auctionRepository.RemoveAuction(id);
    }

    public Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        _logger.LogInformation($"Updating auction status. Auction ID: {id}, New Status: {status}");
        return _auctionRepository.UpdateAuctionStatus(id, status);
    }

    public Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        _logger.LogInformation($"Sending active auctions for Catalog ID: {catalogId} with Status: {status}");
        return _auctionRepository.SendActiveAuctions(catalogId, status);
    }

    public Task<Auction?> UpdateAuction(Auction auction)
    {
        _logger.LogInformation($"Updating auction with ID: {auction.AuctionId}");
        return _auctionRepository.UpdateAuction(auction);
    }

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

        bid.Timestamp = DateTime.UtcNow;
        auction.BidHistory.Add(bid);
        auction.CurrentBid = bid;

        await _auctionRepository.SaveAuction(auction);
        _logger.LogInformation($"Bid for Auction ID: {bid.AuctionId} created successfully.");
        return auction;
    }
}

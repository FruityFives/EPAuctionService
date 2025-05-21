using AuctionServiceAPI.Repositories;
using Models;

namespace AuctionServiceAPI.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICatalogRepository _catalogRepository;

    public AuctionService(IAuctionRepository auctionRepository, ICatalogRepository catalogRepository)
    {
        _auctionRepository = auctionRepository;
        _catalogRepository = catalogRepository;
    }

    public async Task<Auction> CreateAuction(Auction auction)
    {
        auction.AuctionId = Guid.NewGuid();
        auction.Status = AuctionStatus.Active;

        var createdAuction = await _auctionRepository.AddAuction(auction);

        var catalog = await _catalogRepository.GetCatalogById(auction.CatalogId);
        if (catalog != null)
        {
            await _catalogRepository.UpdateCatalog(catalog);
        }

        return createdAuction;
    }

    public Task<Auction> GetAuctionById(Guid id)
        => _auctionRepository.GetAuctionById(id);

    public Task<bool> DeleteAuction(Guid id)
        => _auctionRepository.RemoveAuction(id);

    public Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
        => _auctionRepository.UpdateAuctionStatus(id, status);

    public Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
        => _auctionRepository.SendActiveAuctions(catalogId, status);

    public Task<Auction?> UpdateAuction(Auction auction)
        => _auctionRepository.UpdateAuction(auction);

    public async Task<Auction> CreateBidToAuctionById(BidDTO bid)
    {
        var auction = await _auctionRepository.GetAuctionById(bid.AuctionId)
                      ?? throw new Exception("Auction not found");

        if (auction.Status != AuctionStatus.Active)
            throw new Exception("Auction is not active");

        bid.Timestamp = DateTime.UtcNow;
        auction.BidHistory.Add(bid);
        auction.CurrentBid = bid;

        await _auctionRepository.SaveAuction(auction);
        return auction;
    }
}

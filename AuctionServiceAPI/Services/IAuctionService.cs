using Models;

namespace AuctionServiceAPI.Services;

public interface IAuctionService
{
    Task<Auction> CreateAuction(Auction auction);
    Task<Auction?> AddAuctionToCatalog(Guid auctionId, Guid catalogId, double minPrice);

    Task<List<Auction>> ImportEffectsFromStorageAsync();
    Task<Auction> GetAuctionById(Guid id);
    Task<bool> DeleteAuction(Guid id);
    Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status);
    Task<Auction> CreateBidToAuctionById(BidDTO bid);
    Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status);
    Task<Auction?> UpdateAuction(Auction auction);
}
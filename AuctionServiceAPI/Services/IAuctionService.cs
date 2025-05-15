using Models;

namespace AuctionServiceAPI.Services;

public interface IAuctionService
{
    Task<Auction> CreateAuction(Auction auction);
    Task<bool> DeleteAuction(Guid id);
    Task<Auction> UpdateAuctionStatus(Guid id);
    Task<Auction> CreateBidToAuctionById(Guid auctionId, BidDTO bid);
    Task<List<Auction>> SendAuctionBasedOnStatus(AuctionStatus status);
}
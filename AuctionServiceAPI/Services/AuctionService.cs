using Models;

namespace AuctionServiceAPI.Services;

public class AuctionService : IAuctionService
{
    public Task<Auction> CreateAuction(Auction auction)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAuction(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Auction> UpdateAuctionStatus(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Auction> CreateBidToAuctionById(Guid auctionId, BidDTO bid)
    {
        throw new NotImplementedException();
    }

    public Task<List<Auction>> SendAuctionBasedOnStatus(AuctionStatus status)
    {
        throw new NotImplementedException();
    }
}
using AuctionServiceAPI.Repositories;
using Models;

namespace AuctionServiceAPI.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;

    public AuctionService(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }
    
    public async Task<Auction> CreateAuction(Auction auction)
    {
        return await _auctionRepository.AddAuction(auction);
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
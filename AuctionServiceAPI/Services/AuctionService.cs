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

    public async Task<bool> DeleteAuction(Guid id)
    {
        return await _auctionRepository.RemoveAuction(id);
    }

    public async Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        return await _auctionRepository.UpdateAuctionStatus(id, status);
    }

    public async Task<Auction> CreateBidToAuctionById(Guid auctionId, BidDTO bid)
    {
        return await _auctionRepository.AddBidToAuctionById(auctionId, bid);
    }

    public async Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        return await _auctionRepository.SendActiveAuctions(catalogId, status);
    }


}
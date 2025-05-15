using Models;

namespace AuctionServiceAPI.Repositories;
public interface IAuctionRepository
{
    Task<Auction> AddAuction(Auction auction);
    Task<bool> RemoveAuction(Guid id);
    Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status); // Denne metode opdaterer kun status og den bruges ved AuctionFinish
    
    Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status); // Henter alle auctions tilhørende et katalog
    
    // Til service
    Task<Auction> GetAuctionById(Guid id);
    
    Task<Auction> AddBidToAuctionById(Guid auctionId, BidDTO bid);
    Task<List<Auction>> SendAuctionBasedOnStatus(AuctionStatus status);
}

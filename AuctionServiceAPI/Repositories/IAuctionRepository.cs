using Models;
using MongoDB.Driver;
using System.Linq;

namespace AuctionServiceAPI.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> AddAuction(Auction auction);
    Task<bool> RemoveAuction(Guid id);
    Task<Auction?> UpdateAuctionStatus(Guid id, AuctionStatus status);
    Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status);
    Task<Auction> GetAuctionById(Guid id);
    Task<Auction?> UpdateAuction(Auction auction);
    Task SaveAuction(Auction auction); // ny metode til at gemme opdateret data
}
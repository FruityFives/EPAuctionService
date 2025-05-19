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
        // Sæt ID og status
        auction.AuctionId = Guid.NewGuid();
        auction.Status = AuctionStatus.Active;

        // Gem auktionen i auktion-repo
        var createdAuction = await _auctionRepository.AddAuction(auction);

        // Find kataloget og tilføj auktionen til dets liste
        var catalog = await _catalogRepository.GetCatalogById(auction.CatalogId);
        if (catalog != null)
        {
            catalog.Auctions.Add(createdAuction);
            await _catalogRepository.UpdateCatalog(catalog);
        }

        return createdAuction;
    }

    public async Task<Auction> GetAuctionById(Guid id)
    {
        return await _auctionRepository.GetAuctionById(id);
    }

    public async Task<bool> DeleteAuction(Guid id)
    {
        return await _auctionRepository.RemoveAuction(id);
    }

    public async Task<Auction> UpdateAuctionStatus(Guid id, AuctionStatus status)
    {
        return await _auctionRepository.UpdateAuctionStatus(id, status);
    }

    public async Task<List<Auction>> SendActiveAuctions(Guid catalogId, AuctionStatus status)
    {
        return await _auctionRepository.SendActiveAuctions(catalogId, status);
    }

    public async Task<Auction?> UpdateAuction(Auction auction)
    {
        return await _auctionRepository.UpdateAuction(auction);
    }


    public async Task<Auction> CreateBidToAuctionById(Guid auctionId, BidDTO bid)
    {
        var auction = await _auctionRepository.GetAuctionById(auctionId);
        if (auction == null)
            throw new Exception("Auction not found");

        if (auction.Status != AuctionStatus.Active)
        {
            throw new Exception("Auction is not active");
        }
        var currentMax = (auction.CurrentBid?.Amount)
                         ?? (auction.BidHistory.Count > 0 ? auction.BidHistory.Max(b => b.Amount) : (double?)null)
                         ?? auction.MinPrice;

        if (bid.Amount <= currentMax)
            throw new Exception($"Bid must be higher than current max bid ({currentMax})");

        //først her laver vi ændringer
        bid.Timestamp = DateTime.UtcNow;
        auction.BidHistory.Add(bid);
        auction.CurrentBid = bid;

        return await _auctionRepository.UpdateAuction(auction);
    }



   

}
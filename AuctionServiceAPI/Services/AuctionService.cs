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
            await _catalogRepository.UpdateCatalog(catalog); // hvis du har sådan én
        }

        return createdAuction;
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
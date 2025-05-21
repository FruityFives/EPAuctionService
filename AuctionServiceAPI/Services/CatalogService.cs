using Models;
using AuctionServiceAPI.Repositories;

namespace AuctionServiceAPI.Services;
public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IStoragePublisherRabbit _storagePublisher;

    public CatalogService(ICatalogRepository catalogRepository, IAuctionRepository auctionRepository, IStoragePublisherRabbit storagePublisher)
    {
        _catalogRepository = catalogRepository;
        _auctionRepository = auctionRepository;
        _storagePublisher = storagePublisher;
    }

    public async Task<Catalog> CreateCatalog(Catalog catalog)
    {
        catalog.CatalogId = Guid.NewGuid();
        return await _catalogRepository.AddCatalog(catalog);
    }

    public Task<bool> DeleteCatalog(Guid id) => _catalogRepository.RemoveCatalog(id);

   public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
    var catalog = await _catalogRepository.GetCatalogById(catalogId);
    if (catalog == null) throw new Exception("Catalog not found");

    var allAuctions = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Active);
    var closedAuctions = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Closed);

    return allAuctions.Concat(closedAuctions).ToList(); // returnér begge typer
    }

    public Task<Catalog> GetCatalogById(Guid id)
        => _catalogRepository.GetCatalogById(id);

    public Task<Catalog?> UpdateCatalog(Catalog catalog)
        => _catalogRepository.UpdateCatalog(catalog);

    public Task<List<Catalog>> GetAllCatalogs()
        => _catalogRepository.GetAllCatalogs();

public async Task EndCatalog(Guid catalogId)
{
    var catalog = await _catalogRepository.GetCatalogById(catalogId)
                  ?? throw new Exception("Catalog not found");

    var activeAuctions = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Active);
    var closedAuctions = await _auctionRepository.SendActiveAuctions(catalogId, AuctionStatus.Closed);
    var auctions = activeAuctions.Concat(closedAuctions).ToList();

    catalog.Status = CatalogStatus.Closed;
    await _catalogRepository.SaveCatalog(catalog);

    foreach (var auction in auctions)
    {
        auction.Status = AuctionStatus.Closed;
        await _auctionRepository.SaveAuction(auction); // VIGTIG: dette skal bruge IAuctionRepository

        var dto = new AuctionDTO
        {
            EffectId = auction.EffectId.EffectId,
            WinnerId = auction.CurrentBid?.UserId ?? Guid.Empty,
            FinalAmount = auction.CurrentBid?.Amount ?? 0,
            IsSold = auction.CurrentBid != null
        };

        await _storagePublisher.PublishAuctionAsync(dto);
    }
}
    public async Task HandleAuctionFinish(Guid catalogId)
    {
        var catalog = await _catalogRepository.GetCatalogById(catalogId)
                      ?? throw new Exception("Catalog not found");

        var auctions = await _catalogRepository.GetAuctionsByCatalogId(catalogId);

        foreach (var auction in auctions)
        {
            var dto = new AuctionDTO
            {
                EffectId = auction.EffectId.EffectId,
                WinnerId = auction.CurrentBid?.UserId ?? Guid.Empty,
                FinalAmount = auction.CurrentBid?.Amount ?? 0,
                IsSold = auction.CurrentBid != null
            };

            await _storagePublisher.PublishAuctionAsync(dto);
        }

        catalog.Status = CatalogStatus.Closed;
        await _catalogRepository.SaveCatalog(catalog);
    }
}

using Models;

public interface ICatalogRepository
{
    Task<Catalog> AddCatalog(Catalog catalog);
    Task<bool> RemoveCatalog(Guid id);
    
    Task<Catalog> GetCatalogById(Guid id);
    Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId);

    Task HandleAuctionFinish(Guid catalogId); // Når catalog har ramt sin deadline opdatere denne alle auctions til "Closed"
}
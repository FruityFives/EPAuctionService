using Models;

namespace AuctionServiceAPI.Services;

public interface ICatalogService
{
    Task<Catalog> CreateCatalog(Catalog catalog);
    Task<bool> DeleteCatalog(Guid id);
    Task<Catalog> GetCatalogById(Guid id);
    Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId);
    Task HandleAuctionFinish(Guid catalogId); // Lukker auktioner i katalog, hvis deadline er overskredet
}
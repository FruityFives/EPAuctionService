using Models;

namespace AuctionServiceAPI.Services;

public interface ICatalogService
{
    Task<Catalog> CreateCatalog(Catalog catalog);

    Task<List<Auction>> GetAllActiveAuctions();

    Task<List<Auction>> ImportEffectsFromStorageAsync();



    Task<bool> DeleteCatalog(Guid id);
    Task<Catalog> GetCatalogById(Guid id);
    Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId);
    Task HandleAuctionFinish(Guid catalogId); // Lukker auktioner i katalog, hvis deadline er overskredet
    Task<Catalog?> UpdateCatalog(Catalog catalog);

    Task EndCatalog(Guid catalogId); // Lukker kataloget og opdaterer alle auktioner til "Closed"

    Task<List<Catalog>> GetAllCatalogs();
}
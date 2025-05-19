using AuctionServiceAPI.Repositories;
using Models;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _catalogRepository;

    public CatalogService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<Catalog> CreateCatalog(Catalog catalog)
    {
        catalog.CatalogId = Guid.NewGuid();
        return await _catalogRepository.AddCatalog(catalog);
    }

    public async Task<bool> DeleteCatalog(Guid id)
    {
        return await _catalogRepository.RemoveCatalog(id);
    }

    public async Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
    {
        return await _catalogRepository.GetAuctionsByCatalogId(catalogId);
    }

    public async Task<Catalog> GetCatalogById(Guid id)
    {
        return await _catalogRepository.GetCatalogById(id);
    }

    public async Task HandleAuctionFinish(Guid catalogId)
    {
        await _catalogRepository.HandleAuctionFinish(catalogId);
    }

    public async Task<Catalog?> UpdateCatalog(Catalog catalog)
    {
        return await _catalogRepository.UpdateCatalog(catalog);
    }

    public async Task<List<Catalog>> GetAllCatalogs()
    {
        return await _catalogRepository.GetAllCatalogs();
    }


}
using Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

public interface ICatalogRepository
{
    Task<Catalog> AddCatalog(Catalog catalog);
    Task<bool> RemoveCatalog(Guid id);
    Task<Catalog> GetCatalogById(Guid id);
    Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId);
    Task<Catalog?> UpdateCatalog(Catalog catalog);
    Task<List<Catalog>> GetAllCatalogs();


    Task SaveAuction(Auction auction); // ny
    Task SaveCatalog(Catalog catalog); // ny
}

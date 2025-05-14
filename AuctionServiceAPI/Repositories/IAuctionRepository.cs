using AuctionServiceAPI.Models;

public interface IAuctionRepository
{
    Task<Catalog> CreateCatalog(Catalog catalog);
    Task<bool> DeleteCatalog(Guid id);
}
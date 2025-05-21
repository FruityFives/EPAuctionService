using Models;
public class CatalogRepository : ICatalogRepository
{
    private readonly List<Catalog> ListOfCatalogs = new();
    private readonly List<Auction> ListOfAuctions = new();

    public Task<Catalog> AddCatalog(Catalog catalog)
    {
        ListOfCatalogs.Add(catalog);
        return Task.FromResult(catalog);
    }

    public List<Catalog> SeedDataCatalog()
    {
        ListOfCatalogs.Add(new Catalog
        {
            CatalogId = Guid.Parse("d1f8c03f-8405-4d0e-b86b-6ad94ea4a3a7"),
            Name = "Catalog 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = CatalogStatus.Active
        });

        ListOfCatalogs.Add(new Catalog
        {
            CatalogId = Guid.Parse("e68e3d5f-1a12-4c0e-99e4-92793f3040d6"),
            Name = "Catalog 2",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = CatalogStatus.Closed
        });

        return ListOfCatalogs;
    }

    public Task<bool> RemoveCatalog(Guid id)
    {
        var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id);
        if (catalog == null) return Task.FromResult(false);
        ListOfCatalogs.Remove(catalog);
        return Task.FromResult(true);
    }

    public Task<Catalog> GetCatalogById(Guid id)
        => Task.FromResult(ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id));

    public Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
        => Task.FromResult(ListOfAuctions.Where(a => a.CatalogId == catalogId).ToList());

    public Task<Catalog?> UpdateCatalog(Catalog catalog)
    {
        var existing = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalog.CatalogId);
        if (existing != null)
        {
            existing.Name = catalog.Name;
            existing.StartDate = catalog.StartDate;
            existing.EndDate = catalog.EndDate;
            existing.Status = catalog.Status;
            return Task.FromResult(existing);
        }
        return Task.FromResult<Catalog?>(null);
    }

    public Task<List<Catalog>> GetAllCatalogs()
        => Task.FromResult(ListOfCatalogs);

    public Task SaveAuction(Auction auction)
        => Task.CompletedTask; // reference-type => ændringer allerede gemt

    public Task SaveCatalog(Catalog catalog)
        => Task.CompletedTask;
}

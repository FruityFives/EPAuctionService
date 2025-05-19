using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace AuctionServiceAPI.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly List<Catalog> ListOfCatalogs = new(); // liste af kataloger - 
        public List<Catalog> SeedData()
        {
            // Initialize with some sample data
            ListOfCatalogs.Add(new Catalog
            {
                CatalogId = Guid.Parse("f2b1c2e1-32dc-4ec7-9676-f1b1f469d5a7"),
                Name = "Hudson",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Auctions = new List<Auction>()
            });
            ListOfCatalogs.Add(new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "Abdu",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(6),
                Auctions = new List<Auction>()
            });
            return ListOfCatalogs;
        }
        
        public Task<Catalog> AddCatalog(Catalog catalog)
        {
            ListOfCatalogs.Add(catalog);
            return Task.FromResult(catalog);
        }

        public Task<bool> RemoveCatalog(Guid id)
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id);
            if (catalog == null)
                return Task.FromResult(false);

            ListOfCatalogs.Remove(catalog);
            return Task.FromResult(true);
        }

        public Task<Catalog> GetCatalogById(Guid id)
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == id);
            return Task.FromResult(catalog);
        }

        public Task<List<Auction>> GetAuctionsByCatalogId(Guid catalogId)
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalogId);
            return Task.FromResult(catalog?.Auctions ?? new List<Auction>());
        }

        public Task HandleAuctionFinish(Guid catalogId) // LAv en etst case for denne
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalogId);

            if (catalog != null)
            {
                // Tjek om deadline-datoen er passeret
                if (DateTime.UtcNow > catalog.EndDate)
                {
                    catalog.Status = CatalogStatus.Closed;
                    // Opdater alle auktioner i kataloget til "Closed"
                    foreach (var auction in catalog.Auctions)
                    {
                        auction.Status = AuctionStatus.Closed;
                    }

                }
            }
            return Task.CompletedTask;
        }

        public Task<Catalog?> UpdateCatalog(Catalog catalog)
        {
            var existingCatalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalog.CatalogId);
            if (existingCatalog != null)
            {
                existingCatalog.Name = catalog.Name;
                existingCatalog.StartDate = catalog.StartDate;
                existingCatalog.EndDate = catalog.EndDate;
                existingCatalog.Status = catalog.Status;
                return Task.FromResult(existingCatalog);
            }
            return Task.FromResult<Catalog?>(null);
        }

        public Task<List<Catalog>> GetAllCatalogs()
        {
            return Task.FromResult(ListOfCatalogs);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionServiceAPI.Services;
using Models;


namespace AuctionServiceAPI.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly List<Catalog> ListOfCatalogs = new(); // liste af kataloger - 
        private readonly List<Auction> ListOfAuctions = new(); // liste af auktioner 
                                                               //private readonly ILogger<CatalogRepository> _logger;



        private readonly IStoragePublisherRabbit _storagePublisherRabbit;

        public CatalogRepository(IStoragePublisherRabbit storagePublisherRabbit)
    {
        _storagePublisherRabbit = storagePublisherRabbit ?? throw new ArgumentNullException(nameof(storagePublisherRabbit));
    }

        public List<Catalog> SeedData()
        {
            // Initialize with some sample data
            ListOfCatalogs.Add(new Catalog
            {
                CatalogId = Guid.Parse("f2b1c2e1-32dc-4ec7-9676-f1b1f469d5a7"),
                Name = "Hudson",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7)
            });
            ListOfCatalogs.Add(new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "Abdu",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(6)
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
            if (catalog != null)
            {
                var auctions = ListOfAuctions.Where(a => a.CatalogId == catalogId).ToList();
                return Task.FromResult(auctions);
            }
            return Task.FromResult(new List<Auction>());
        }

        public async Task EndCatalog(Guid catalogId)
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalogId);

            var auctions = ListOfAuctions.Where(a => a.CatalogId == catalogId).ToList();

            if (catalog != null)
            {
                // Update the catalog status to closed
                catalog.Status = CatalogStatus.Closed;


                // Set auction status to closed
                foreach (var auction in auctions)
                {
                    auction.Status = AuctionStatus.Closed;
                }


                //_logger.LogInformation($"Catalog {catalogId} has been closed.");
            }
            else
            {
                throw new Exception("Catalog not found");
            }

            //Send auctions to message queue
            foreach (var auction in auctions)
            {
                var auctionDTO = new AuctionDTO
                {
                    EffectId = auction.EffectId.EffectId,
                    WinnerId = auction.CurrentBid?.UserId ?? Guid.Empty,
                    FinalAmount = auction.CurrentBid?.Amount ?? 0,
                    IsSold = auction.CurrentBid != null,
                };

                // Publish the auction to the message queue
                await _storagePublisherRabbit.PublishAuctionAsync(auctionDTO);
            }
        }

        public async Task<Task> HandleAuctionFinish(Guid catalogId) // LAv en etst case for denne
        {
            var catalog = ListOfCatalogs.FirstOrDefault(c => c.CatalogId == catalogId);
            var auction = ListOfAuctions.FirstOrDefault(a => a.CatalogId == catalogId);

            foreach (var auctionItem in ListOfAuctions)
            {
                if (auctionItem.CurrentBid == null)
                {
                    var auctionDTO = new AuctionDTO
                    {
                        EffectId = auctionItem.EffectId.EffectId,
                        WinnerId = Guid.Empty,
                        FinalAmount = 0,
                        IsSold = false,
                    };
                }
                else
                {
                    var auctionDTO = new AuctionDTO
                    {
                        EffectId = auctionItem.EffectId.EffectId,
                        WinnerId = auctionItem.CurrentBid.UserId,
                        FinalAmount = auctionItem.CurrentBid.Amount,
                        IsSold = true,
                    };

                    // Publish the auction to the message queue
                    await _storagePublisherRabbit.PublishAuctionAsync(auctionDTO);
                }

                //_logger.LogInformation($"Auction finished for catalog {catalogId}. Auction ID: {auctionItem.AuctionId}, Winner ID: {auctionItem.CurrentBid?.UserId}, Final Amount: {auctionItem.CurrentBid?.Amount}");

                return Task.CompletedTask;
            }

            if (catalog != null && auction != null)
            {
                // Update the catalog status to closed
                catalog.Status = CatalogStatus.Closed;

                // Remove the auction from the list
                ListOfAuctions.Remove(auction);


            }
            else
            {
                throw new Exception("Catalog or Auction not found");
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

        Task ICatalogRepository.HandleAuctionFinish(Guid catalogId)
        {
            return HandleAuctionFinish(catalogId);
        }
    }
}
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using AuctionServiceAPI.Repositories;
using AuctionServiceAPI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace AuctionServiceAPI.Test
{
    /// <summary>
    /// Vigtigste enhedstests for AuctionService: opret, byd og slet.
    /// </summary>
    [TestFixture]
    public class AuctionServiceTests
    {
        private Mock<IAuctionRepository> _mockAuctionRepo;
        private Mock<ICatalogRepository> _mockCatalogRepo;
        private Mock<IAuctionPublisherRabbit> _mockPublisher;
        private AuctionService _auctionService;

        [SetUp]
        public void Setup()
        {
            _mockAuctionRepo = new Mock<IAuctionRepository>();
            _mockCatalogRepo = new Mock<ICatalogRepository>();
            _mockPublisher = new Mock<IAuctionPublisherRabbit>();
            var mockLogger = new Mock<ILogger<AuctionService>>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["STORAGE_SERVICE_BASE_URL"]).Returns("http://mock-storage/api/storage");


            _auctionService = new AuctionService(
                _mockAuctionRepo.Object,
                _mockCatalogRepo.Object,
                _mockPublisher.Object,
                _mockPublisher.Object,
                mockLogger.Object,
                mockConfig.Object

            );
        }

        /// <summary>
        /// Tester at auktion bliver korrekt oprettet og knyttet til katalog.
        /// </summary>
        [Test]
        public async Task AddAuctionToCatalog_ShouldAssignAuctionToCatalog()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var catalogId = Guid.NewGuid();

            var auction = new Auction
            {
                AuctionId = auctionId,
                Name = "Testauktion",
                Status = AuctionStatus.Inactive
            };

            var catalog = new Catalog
            {
                CatalogId = catalogId,
                EndDate = DateTime.UtcNow.AddDays(7)
            };

            _mockAuctionRepo.Setup(r => r.GetAuctionById(auctionId)).ReturnsAsync(auction);
            _mockCatalogRepo.Setup(r => r.GetCatalogById(catalogId)).ReturnsAsync(catalog);
            _mockAuctionRepo.Setup(r => r.SaveAuction(It.IsAny<Auction>())).Returns(Task.CompletedTask);

            // Act
            var result = await _auctionService.AddAuctionToCatalog(auctionId, catalogId, 1500);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.CatalogId, Is.EqualTo(catalogId));
            Assert.That(result.MinPrice, Is.EqualTo(1500));
            Assert.That(result.Status, Is.EqualTo(AuctionStatus.Active));
        }

        /// <summary>
        /// Tester at et bud bliver tilføjet korrekt til aktiv auktion.
        /// </summary>
        [Test]
        public async Task CreateBidToAuctionById_ShouldAddBid_WhenAuctionIsActive()
        {
            var auctionId = Guid.NewGuid();
            var bid = new BidDTO { AuctionId = auctionId, UserId = Guid.NewGuid(), Amount = 1000 };

            var auction = new Auction
            {
                AuctionId = auctionId,
                Status = AuctionStatus.Active,
                EndDate = DateTime.UtcNow.AddMinutes(30),
                BidHistory = new List<BidDTO>()
            };

            _mockAuctionRepo.Setup(r => r.GetAuctionById(auctionId)).ReturnsAsync(auction);
            _mockAuctionRepo.Setup(r => r.SaveAuction(It.IsAny<Auction>())).Returns(Task.CompletedTask);

            var result = await _auctionService.CreateBidToAuctionById(bid);

            Assert.IsNotNull(result);
            Assert.That(result.CurrentBid.Amount, Is.EqualTo(1000));
            Assert.That(result.BidHistory.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Tester at auktion slettes korrekt.
        /// </summary>
        [Test]
        public async Task DeleteAuction_ShouldReturnTrue_WhenAuctionDeleted()
        {
            var auctionId = Guid.NewGuid();

            _mockAuctionRepo.Setup(r => r.RemoveAuction(auctionId)).ReturnsAsync(true);

            var result = await _auctionService.DeleteAuction(auctionId);

            Assert.IsTrue(result);
        }
    }
}

using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuctionServiceAPI.Services;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;
using Models;

namespace AuctionServiceAPI.Test
{
    [TestFixture]
    public class CatalogServiceTests
    {
        private Mock<ICatalogRepository> _catalogRepo;
        private Mock<IAuctionRepository> _auctionRepo;
        private Mock<IAuctionService> _auctionService;
        private Mock<IStoragePublisherRabbit> _storagePublisher;
        private Mock<ILogger<CatalogService>> _logger;

        private CatalogService _service;

        [SetUp]
        public void Setup()
        {
            _catalogRepo = new Mock<ICatalogRepository>();
            _auctionRepo = new Mock<IAuctionRepository>();
            _auctionService = new Mock<IAuctionService>();
            _storagePublisher = new Mock<IStoragePublisherRabbit>();
            _logger = new Mock<ILogger<CatalogService>>();

            _service = new CatalogService(
                _auctionService.Object,
                _catalogRepo.Object,
                _auctionRepo.Object,
                _storagePublisher.Object,
                _logger.Object
            );
        }

        /// <summary>
        /// Tester at CreateCatalog returnerer det samme katalog med sat ID.
        /// Tester også at CreateCatalog returnerer katalog med navn.
        /// </summary>
        [Test]
        public async Task CreateCatalog_ShouldReturnCatalogWithId()
        {
            // Arrange
            var input = new Catalog {CatalogId = Guid.NewGuid(), Name = "Test Catalog"};

            _catalogRepo.Setup(r => r.AddCatalog(It.IsAny<Catalog>()))
                .ReturnsAsync((Catalog c) => c);

            // Act
            var result = await _service.CreateCatalog(input);

            // Assert
            Assert.AreEqual(input.CatalogId, result.CatalogId);
            Assert.AreEqual("Test Catalog", result.Name);
        }

        /// <summary>
        /// Tester at DeleteCatalog returnerer true når katalog findes.
        /// </summary>
        [Test]
        public async Task DeleteCatalog_ShouldReturnTrue()
        {
            // Arrange
            var catalogId = Guid.NewGuid();
            _catalogRepo.Setup(r => r.RemoveCatalog(catalogId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteCatalog(catalogId);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tester at GetAllCatalogs kalder repository og returnerer listen.
        /// </summary>
        [Test]
        public async Task GetAllCatalogs_ShouldReturnCatalogList()
        {
            // Arrange
            _catalogRepo.Setup(r => r.GetAllCatalogs())
                        .ReturnsAsync(new List<Catalog>
                        {
                            new Catalog { Name = "A" },
                            new Catalog { Name = "B" }
                        });

            // Act
            var result = await _service.GetAllCatalogs();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}

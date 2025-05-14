using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Controllers;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Test
{
    [TestFixture]
    public class CatalogRepositoryTests
    {
        private Mock<IAuctionRepository> _mockRepo;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IAuctionRepository>();
        }

        [Test]
        public async Task CreateCatalog_ShouldReturnCatalog()
        {
            // Arrange
            var inputCatalog = new Catalog
            {
                Id = Guid.NewGuid(),
                Name = "TestCatalog",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Auctions = new List<Models.Auction>()
            };

            _mockRepo.Setup(repo => repo.CreateCatalog(It.IsAny<Catalog>()))
                     .ReturnsAsync((Catalog c) => c); // return input for simplicity

            // Act
            var result = await _mockRepo.Object.CreateCatalog(inputCatalog);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(inputCatalog.Id, result.Id);
            Assert.AreEqual(inputCatalog.Name, result.Name);
        }

        [Test]
        public async Task DeleteCatalog_ShouldReturnTrue_WhenFound()
        {
            // Arrange
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.DeleteCatalog(catalogId)).ReturnsAsync(true);

            // Act
            var result = await _mockRepo.Object.DeleteCatalog(catalogId);

            // Assert
            Assert.IsTrue(result);
            _mockRepo.Verify(repo => repo.DeleteCatalog(catalogId), Times.Once);
        }

        [Test]
        public async Task DeleteCatalog_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.DeleteCatalog(catalogId)).ReturnsAsync(false);

            // Act
            var result = await _mockRepo.Object.DeleteCatalog(catalogId);

            // Assert
            Assert.IsFalse(result);
            _mockRepo.Verify(repo => repo.DeleteCatalog(catalogId), Times.Once);
        }
    }
}

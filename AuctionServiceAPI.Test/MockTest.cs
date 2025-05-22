using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using AuctionServiceAPI.Repositories;

namespace AuctionServiceAPI.Test
{
    [TestFixture]
    public class CatalogRepositoryMockTests
    {
        private Mock<ICatalogRepository> _mockRepo;
        private List<Catalog> _inMemoryCatalogs;

        [SetUp]
        public void Setup()
        {
            _inMemoryCatalogs = new List<Catalog>
            {
                new Catalog
                {
                    CatalogId = Guid.NewGuid(),
                    Name = "Initial Catalog",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Status = CatalogStatus.Active
                }
            };

            _mockRepo = new Mock<ICatalogRepository>();

            _mockRepo.Setup(repo => repo.AddCatalog(It.IsAny<Catalog>()))
                     .ReturnsAsync((Catalog c) =>
                     {
                         _inMemoryCatalogs.Add(c);
                         return c;
                     });

            _mockRepo.Setup(repo => repo.RemoveCatalog(It.IsAny<Guid>()))
                     .ReturnsAsync((Guid id) =>
                     {
                         var catalog = _inMemoryCatalogs.Find(c => c.CatalogId == id);
                         if (catalog != null)
                         {
                             _inMemoryCatalogs.Remove(catalog);
                             return true;
                         }
                         return false;
                     });
        }

        [Test]
        public async Task AddCatalog_ShouldReturnCatalogAndIncreaseCount()
        {
            // Arrange
            var newCatalog = new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "New Catalog",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = CatalogStatus.Active
            };

            // Act
            var result = await _mockRepo.Object.AddCatalog(newCatalog);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, _inMemoryCatalogs.Count);
            Assert.That(_inMemoryCatalogs.Contains(result));
        }

        [Test]
        public async Task RemoveCatalog_ShouldReturnTrue_WhenCatalogExists()
        {
            // Arrange
            var existingId = _inMemoryCatalogs[0].CatalogId;

            // Act
            var result = await _mockRepo.Object.RemoveCatalog(existingId);

            // Assert
            Assert.IsTrue(result);
            Assert.That(_inMemoryCatalogs.Exists(c => c.CatalogId == existingId), Is.False);
        }

        [Test]
        public async Task RemoveCatalog_ShouldReturnFalse_WhenCatalogDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mockRepo.Object.RemoveCatalog(nonExistentId);

            // Assert
            Assert.IsFalse(result);
        }
    }
}

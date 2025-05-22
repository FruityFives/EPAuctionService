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
    public class CatalogRepositoryTests
    {
        private Mock<ICatalogRepository> _mockRepo;
        private List<Catalog> _fakeCatalogList;

        [SetUp]
        public void Setup()
        {
            _fakeCatalogList = new List<Catalog>
            {
                new Catalog
                {
                    CatalogId = Guid.NewGuid(),
                    Name = "Catalog A",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5),
                    Status = CatalogStatus.Active
                },
                new Catalog
                {
                    CatalogId = Guid.NewGuid(),
                    Name = "Catalog B",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Status = CatalogStatus.Closed
                }
            };

            _mockRepo = new Mock<ICatalogRepository>();

            _mockRepo.Setup(repo => repo.AddCatalog(It.IsAny<Catalog>()))
                     .ReturnsAsync((Catalog c) =>
                     {
                         _fakeCatalogList.Add(c);
                         return c;
                     });

            _mockRepo.Setup(repo => repo.RemoveCatalog(It.IsAny<Guid>()))
                     .ReturnsAsync((Guid id) =>
                     {
                         var catalog = _fakeCatalogList.Find(c => c.CatalogId == id);
                         if (catalog != null)
                         {
                             _fakeCatalogList.Remove(catalog);
                             return true;
                         }
                         return false;
                     });
        }

        [Test]
        public async Task AddCatalog_ShouldIncreaseCatalogList()
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
            Assert.That(_fakeCatalogList.Count, Is.EqualTo(3));
            Assert.That(result.Name, Is.EqualTo("New Catalog"));
        }

        [Test]
        public async Task RemoveCatalog_ShouldDecreaseCatalogList()
        {
            // Arrange
            var catalogIdToRemove = _fakeCatalogList[0].CatalogId;

            // Act
            var result = await _mockRepo.Object.RemoveCatalog(catalogIdToRemove);

            // Assert
            Assert.IsTrue(result);
            Assert.That(_fakeCatalogList.Count, Is.EqualTo(1));
        }
    }
}

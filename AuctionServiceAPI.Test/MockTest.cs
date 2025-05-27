using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using AuctionServiceAPI.Repositories;

namespace AuctionServiceAPI.Test
{
    /// <summary>
    /// Enhedstest af ICatalogRepository med mock og in-memory liste.
    /// </summary>
    [TestFixture]
    public class CatalogRepositoryMockTests
    {
        private Mock<ICatalogRepository> _mockRepo;
        private List<Catalog> _inMemoryCatalogs;

        /// <summary>
        /// Initialiserer in-memory data og mock-repository før hver test.
        /// </summary>
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

        /// <summary>
        /// Tester at AddCatalog returnerer det tilføjede katalog og øger listen.
        /// </summary>
        [Test]
        public async Task AddCatalog_ShouldReturnCatalogAndIncreaseCount()
        {
            var newCatalog = new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "New Catalog",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = CatalogStatus.Active
            };

            var result = await _mockRepo.Object.AddCatalog(newCatalog);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, _inMemoryCatalogs.Count);
            Assert.That(_inMemoryCatalogs.Contains(result));
        }

        /// <summary>
        /// Tester at RemoveCatalog returnerer true hvis katalog findes.
        /// </summary>
        [Test]
        public async Task RemoveCatalog_ShouldReturnTrue_WhenCatalogExists()
        {
            var existingId = _inMemoryCatalogs[0].CatalogId;

            var result = await _mockRepo.Object.RemoveCatalog(existingId);

            Assert.IsTrue(result);
            Assert.That(_inMemoryCatalogs.Exists(c => c.CatalogId == existingId), Is.False);
        }

        /// <summary>
        /// Tester at RemoveCatalog returnerer false hvis katalog ikke findes.
        /// </summary>
        [Test]
        public async Task RemoveCatalog_ShouldReturnFalse_WhenCatalogDoesNotExist()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _mockRepo.Object.RemoveCatalog(nonExistentId);

            Assert.IsFalse(result);
        }
    }
}

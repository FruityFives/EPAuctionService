using NUnit.Framework; // NUnit testing framework
using Moq; // Moq for mocking dependencies
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AuctionServiceAPI.Test
{
    // Mark this class as a test fixture for NUnit
    [TestFixture]
    [Ignore("Skipping tests for now")]
    public class CatalogRepositoryTests
    {
        // Mock object for the IAuctionRepository interface
        private Mock<ICatalogRepository> _mockRepo; // Til test cases 1-3
        private CatalogRepository _CatalogRepo;  // Til test case 4 ++

        // Setup method runs before each test
        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<ICatalogRepository>();

            // Mock collection
            var mockCatalogCollection = new Mock<IMongoCollection<Catalog>>();

            // Mock context
            var mockContext = new Mock<MongoDbContext>();
            mockContext.Setup(c => c.CatalogCollection).Returns(mockCatalogCollection.Object);

            // Brug den rigtige CatalogRepository med mocked context
            _CatalogRepo = new CatalogRepository(mockContext.Object);
        }


        // Test for getting a catalog by ID
        /*        [Test]
                public async Task T4GetCatalog_ById_From_SeedData()
                {
                    // Arrange
                    _CatalogRepo.SeedDataCatalog(); // <--- vigtig linje
                    var catalogId = Guid.Parse("f2b1c2e1-32dc-4ec7-9676-f1b1f469d5a7");

                    // Act
                    var result = await _CatalogRepo.GetCatalogById(catalogId);

                    // Assert
                    Assert.IsNotNull(result); // god sikkerhed først
                    Assert.AreEqual(catalogId, result.CatalogId);
                    Console.WriteLine(result.CatalogId);
                }
        */
        [Test]
        public async Task T5AddCatalog_SeedData()
        {
            //Arrange
            var CatalogList = _CatalogRepo.SeedDataCatalog();
            var InputCatalog = new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "Jabir",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = CatalogStatus.Active
            };
            //Act
            var result = await _CatalogRepo.AddCatalog(InputCatalog);
            //Assert
            Assert.AreEqual(3, CatalogList.Count);
            Console.WriteLine(CatalogList.Count);
        }
        [Test]
        public async Task T6RemoveCatalog_SeedData()
        {
            //arrange
            var CatalogList = _CatalogRepo.SeedDataCatalog();
            var catalogId = Guid.Parse("f2b1c2e1-32dc-4ec7-9676-f1b1f469d5a7");

            //Act
            var result = await _CatalogRepo.RemoveCatalog(catalogId);

            //Assert
            Assert.AreEqual(2, CatalogList.Count);
            Console.WriteLine(CatalogList.Count);
        }
    }
}
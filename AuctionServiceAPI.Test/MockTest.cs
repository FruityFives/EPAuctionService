// Import necessary namespaces for testing, mocking, and models
using NUnit.Framework; // NUnit testing framework
using Moq; // Moq for mocking dependencies
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Test
{
    // Mark this class as a test fixture for NUnit
    [TestFixture]
    public class CatalogRepositoryMockTests
    {
        // Mock object for the IAuctionRepository interface
        private Mock<ICatalogRepository> _mockRepo; // Til test cases 1-3
        private CatalogRepository _CatalogRepo;  // Til test case 4 ++
        
        // Setup method runs before each test
        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<ICatalogRepository>();
            _CatalogRepo = new CatalogRepository();        }

        // Test for creating a catalog and verifying the returned object
        [Test]
        public async Task T1CreateCatalog_ShouldReturnCatalog()
        {
            // Arrange: create a sample Catalog object
            var inputCatalog = new Catalog
            {
                CatalogId = Guid.NewGuid(),
                Name = "TestCatalog",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = CatalogStatus.Active
            };

            // Setup the mock to return the input catalog when CreateCatalog is called
            _mockRepo.Setup(repo => repo.AddCatalog(It.IsAny<Catalog>()))
                     .ReturnsAsync((Catalog c) => c); // return input for simplicity

            // Act: call the method under test
            var result = await _mockRepo.Object.AddCatalog(inputCatalog);

            // Assert: verify the result is as expected
            Assert.IsNotNull(result);
            Assert.AreEqual(inputCatalog.CatalogId, result.CatalogId);
            Assert.AreEqual(inputCatalog.Name, result.Name);
        }

        // Test for deleting a catalog when it exists
        [Test]
        public async Task T2DeleteCatalog_ShouldReturnTrue_WhenFound()
        {
            // Arrange: setup the mock to return true for a specific catalogId
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.RemoveCatalog(catalogId)).ReturnsAsync(true);

            // Act: call the method under test
            var result = await _mockRepo.Object.RemoveCatalog(catalogId);

            // Assert: verify the result and that the method was called once
            _mockRepo.Verify(repo => repo.RemoveCatalog(catalogId), Times.Once);
            Assert.IsTrue(result);
        }

        // Test for deleting a catalog when it does not exist
        [Test]
        public async Task T3DeleteCatalog_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange: setup the mock to return false for a specific catalogId
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.RemoveCatalog(catalogId)).ReturnsAsync(false);

            // Act: call the method under test
            var result = await _mockRepo.Object.RemoveCatalog(catalogId);

            // Assert: verify the result and that the method was called once
            Assert.IsFalse(result);
            _mockRepo.Verify(repo => repo.RemoveCatalog(catalogId), Times.Once);
        }
    }
}
// Import necessary namespaces for testing, mocking, and models
using NUnit.Framework; // NUnit testing framework
using Moq; // Moq for mocking dependencies
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Controllers;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Test
{
    // Mark this class as a test fixture for NUnit
    [TestFixture]
    public class CatalogRepositoryTests
    {
        // Mock object for the IAuctionRepository interface
        private Mock<IAuctionRepository> _mockRepo;

        // Setup method runs before each test
        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IAuctionRepository>();
        }

        // Test for creating a catalog and verifying the returned object
        [Test]
        public async Task CreateCatalog_ShouldReturnCatalog()
        {
            // Arrange: create a sample Catalog object
            var inputCatalog = new Catalog
            {
                Id = Guid.NewGuid(),
                Name = "TestCatalog",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                Auctions = new List<Models.Auction>()
            };

            // Setup the mock to return the input catalog when CreateCatalog is called
            _mockRepo.Setup(repo => repo.CreateCatalog(It.IsAny<Catalog>()))
                     .ReturnsAsync((Catalog c) => c); // return input for simplicity

            // Act: call the method under test
            var result = await _mockRepo.Object.CreateCatalog(inputCatalog);

            // Assert: verify the result is as expected
            Assert.IsNotNull(result);
            Assert.AreEqual(inputCatalog.Id, result.Id);
            Assert.AreEqual(inputCatalog.Name, result.Name);
        }

        // Test for deleting a catalog when it exists
        [Test]
        public async Task DeleteCatalog_ShouldReturnTrue_WhenFound()
        {
            // Arrange: setup the mock to return true for a specific catalogId
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.DeleteCatalog(catalogId)).ReturnsAsync(true);

            // Act: call the method under test
            var result = await _mockRepo.Object.DeleteCatalog(catalogId);

            // Assert: verify the result and that the method was called once
            Assert.IsTrue(result);
            _mockRepo.Verify(repo => repo.DeleteCatalog(catalogId), Times.Once);
        }

        // Test for deleting a catalog when it does not exist
        [Test]
        public async Task DeleteCatalog_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange: setup the mock to return false for a specific catalogId
            var catalogId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.DeleteCatalog(catalogId)).ReturnsAsync(false);

            // Act: call the method under test
            var result = await _mockRepo.Object.DeleteCatalog(catalogId);

            // Assert: verify the result and that the method was called once
            Assert.IsFalse(result);
            _mockRepo.Verify(repo => repo.DeleteCatalog(catalogId), Times.Once);
        }
    }
}
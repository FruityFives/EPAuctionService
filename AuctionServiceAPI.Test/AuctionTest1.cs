using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using AuctionServiceAPI.Services;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;
using Models;

namespace AuctionServiceAPI.Test
{
    [TestFixture]
    public class AuctionServiceTests
    {
        private Mock<IAuctionRepository> _auctionRepo;
        private Mock<ICatalogRepository> _catalogRepo;
        private Mock<ILogger<AuctionService>> _logger;
        private AuctionService _service;

        [SetUp]
        public void SetUp()
        {
            _auctionRepo = new Mock<IAuctionRepository>();
            _catalogRepo = new Mock<ICatalogRepository>();
            _logger = new Mock<ILogger<AuctionService>>();

            _service = new AuctionService(
                _auctionRepo.Object,
                _catalogRepo.Object,
                _logger.Object
            );
        }

        /// <summary>
        /// Tester at CreateAuction returnerer en auktion med et ID og status sat til Active.
        /// </summary>
        [Test]
        public async Task CreateAuction_ShouldReturnAuctionWithIdAndStatus()
        {
            // Arrange
            var inputAuction = new Auction();
            _auctionRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()))
                        .ReturnsAsync((Auction a) => a);

            // Act
            var result = await _service.CreateAuction(inputAuction);

            // Assert
            Assert.AreEqual(AuctionStatus.Active, result.Status);
        }

        /// <summary>
        /// Tester at GetAuctionById kalder repository og returnerer en auktion.
        /// </summary>
        [Test]
        public async Task GetAuctionById_ShouldReturnAuction()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            var auction = new Auction { AuctionId = auctionId };
            _auctionRepo.Setup(r => r.GetAuctionById(auctionId))
                        .ReturnsAsync(auction);

            // Act
            var result = await _service.GetAuctionById(auctionId);

            // Assert
            Assert.AreEqual(auctionId, result.AuctionId);
        }

        /// <summary>
        /// Tester at DeleteAuction returnerer true, når auktionen findes.
        /// </summary>
        [Test]
        public async Task DeleteAuction_ShouldReturnTrue_WhenAuctionIsDeleted()
        {
            // Arrange
            var auctionId = Guid.NewGuid();
            _auctionRepo.Setup(r => r.RemoveAuction(auctionId))
                        .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAuction(auctionId);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
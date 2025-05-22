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
    public class AuctionRepositoryTests
    {
        private Mock<IAuctionRepository> _mockRepo;
        private List<Auction> _fakeAuctionList;

        [SetUp]
        public void Setup()
        {
            _fakeAuctionList = new List<Auction>
            {
                new Auction
                {
                    AuctionId = Guid.NewGuid(),
                    Name = "Initial Auction",
                    Status = AuctionStatus.Active,
                    CatalogId = Guid.NewGuid(),
                    BidHistory = new List<BidDTO>(),
                    MinPrice = 1000,
                    EffectId = new EffectDTO { EffectId = Guid.NewGuid() }
                }
            };

            _mockRepo = new Mock<IAuctionRepository>();

            _mockRepo.Setup(r => r.AddAuction(It.IsAny<Auction>()))
                     .ReturnsAsync((Auction a) =>
                     {
                         _fakeAuctionList.Add(a);
                         return a;
                     });

            _mockRepo.Setup(r => r.UpdateAuctionStatus(It.IsAny<Guid>(), It.IsAny<AuctionStatus>()))
                     .ReturnsAsync((Guid id, AuctionStatus status) =>
                     {
                         var auction = _fakeAuctionList.Find(a => a.AuctionId == id);
                         if (auction != null)
                         {
                             auction.Status = status;
                         }
                         return auction;
                     });
        }

        [Test]
        public async Task AddAuction_ShouldIncreaseListCount()
        {
            // Arrange
            var newAuction = new Auction
            {
                AuctionId = Guid.NewGuid(),
                Name = "New Auction",
                Status = AuctionStatus.Active,
                CatalogId = Guid.NewGuid(),
                BidHistory = new List<BidDTO>(),
                MinPrice = 2000,
                EffectId = new EffectDTO { EffectId = Guid.NewGuid() }
            };

            // Act
            var result = await _mockRepo.Object.AddAuction(newAuction);

            // Assert
            Assert.That(_fakeAuctionList.Count, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("New Auction"));
        }

        [Test]
        public async Task UpdateAuctionStatus_ShouldChangeStatusToClosed()
        {
            // Arrange
            var auctionId = _fakeAuctionList[0].AuctionId;
            var newStatus = AuctionStatus.Closed;

            // Act
            var result = await _mockRepo.Object.UpdateAuctionStatus(auctionId, newStatus);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Status, Is.EqualTo(newStatus));
        }
    }
}

using NUnit.Framework; // NUnit testing framework
using Moq; // Moq for mocking dependencies
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Channels;
using Models;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Test
{
    // Mark this class as a test fixture for NUnit
    [TestFixture]
    public class AuctionRepositoryTests
    {
        // Mock object for the IAuctionRepository interface
        private Mock<IAuctionRepository> _mockRepo; // Til test cases 1-3
        private AuctionRepository _AuctionRepo;  // Til test case 4 ++

        // Setup method runs before each test
        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IAuctionRepository>();
            _AuctionRepo = new AuctionRepository();
        }

        // Test for getting a catalog by ID
        [Test]
        public async Task T4AddAuction_To_SeedData()
        {
            //Arrange
            var AuctionList = _AuctionRepo.SeedDataAuction();
            var InputAuction = new Auction()
            {
                AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3b3"),
                Name = "Fawad",
                Status = AuctionStatus.Active,
                CatalogId = AuctionList[0].CatalogId,
                BidHistory = new List<BidDTO>(),
                MinPrice = 5000,
                EffectId = new EffectDTO
                {
                    EffectId = Guid.NewGuid()
                }
            };
            //Act
            var result = await _AuctionRepo.AddAuction(InputAuction);
            //Assert
            Assert.AreEqual(5, AuctionList.Count);
            Console.WriteLine(AuctionList.Count);

        }

        [Test]
        public async Task T5UpdateAuction_SeedData()
        {
            //Arrange
            var AuctionList = _AuctionRepo.SeedDataAuction();
            var InputAuction = new Auction()
            {
                AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3b3"),
                Name = "Fawad",
                Status = AuctionStatus.Active,
                CatalogId = AuctionList[0].CatalogId,
                BidHistory = new List<BidDTO>(),
                MinPrice = 5000,
                EffectId = new EffectDTO
                {
                    EffectId = Guid.NewGuid()
                }
            };

            AuctionList.Add(InputAuction);
            //Act
            var result = await _AuctionRepo.UpdateAuctionStatus(InputAuction.AuctionId, AuctionStatus.Closed);

            //Assert
            Assert.That(result.Status, Is.EqualTo(AuctionStatus.Closed));
            Console.WriteLine(InputAuction.Status.ToString());


        }
/*
        [Test]
        public async Task T6AddBidToAuctionById_SeedData()
        {
            //Arrange
            var AuctionList = _AuctionRepo.SeedDataAuction();
            var InputAuction = new Auction()
            {
                AuctionId = Guid.Parse("6f8c03f1-8405-4d0e-b86b-6ad94ea4a3b3"),
                Name = "Fawad",
                Status = AuctionStatus.Active,
                CatalogId = AuctionList[0].CatalogId,
                BidHistory = new List<BidDTO>(),
                MinPrice = 5000,
                EffectId = new EffectDTO
                {
                    EffectId = Guid.NewGuid()
                }
            };
            var InputBid = new BidDTO()
            {
                BidId = Guid.NewGuid(),
                AuctionId = InputAuction.AuctionId,
                Amount = 10000,
                UserId = Guid.NewGuid(),
                Timestamp = DateTime.Now
            };

            AuctionList.Add(InputAuction);
            //Act
            var result = await _AuctionRepo.AddBidToAuctionById(InputAuction.AuctionId, InputBid);

            //Assert
            Assert.That(result.BidHistory.Count, Is.EqualTo(1));
            Console.WriteLine(InputAuction.BidHistory.Count);
        }
*/

    }
}
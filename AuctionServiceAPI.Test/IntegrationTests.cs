using NUnit.Framework;
using RabbitMQ.Client;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;

namespace AuctionServiceTests
{
    public class IntegrationTests
    {
        private readonly string auctionServiceUrl = "http://localhost:5001/api/auctions/";
        private readonly Guid testAuctionId = Guid.Parse("2f1c8a26-4c95-44e2-8f01-3f4a17d197db");

        [Test]
        public async Task AuctionService_UpdatesAuction_WhenBidMessageReceived()
        {
            // Arrange
            var bid = new BidDTO
            {
                BidId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                AuctionId = testAuctionId,
                Amount = 9999,
                Timestamp = DateTime.UtcNow
            };

            var factory = new ConnectionFactory() { HostName = "localhost" }; // evt. "rabbitmq" hvis i Docker
           using var connection = await factory.CreateConnectionAsync();
           using var channel = await connection.CreateChannelAsync();

            channel.QueueDeclareAsync(queue: "bidQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            await channel.BasicPublishAsync<ReadOnlyBasicProperties>(
    exchange: "",
    routingKey: "bidQueue",
    mandatory: false,
    basicProperties: ReadOnlyBasicProperties.Empty,
    body: body);


            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bid));
            
    

            // Act
            await Task.Delay(2000); // vent på at AuctionService behandler beskeden

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(auctionServiceUrl + testAuctionId);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.IsTrue(responseBody.Contains("9999"), "Auktionen blev ikke opdateret med buddet.");
        }
    }
}

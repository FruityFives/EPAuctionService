using Models;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace AuctionServiceAPI.Services;

public class AuctionSyncPublisherRabbit : IAuctionSyncPublisher
{
    private readonly ILogger<AuctionSyncPublisherRabbit> _logger;

    public AuctionSyncPublisherRabbit(ILogger<AuctionSyncPublisherRabbit> logger)
    {
        _logger = logger;
    }

    public async Task PublishAuctionAsync(AuctionSyncDTO auction)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };

        try
        {
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "auction-sync-queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = JsonSerializer.SerializeToUtf8Bytes(auction);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "auction-sync-queue",
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: body
            );

            _logger.LogInformation("Published sync for auction {AuctionId} to queue", auction.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish auction sync");
        }
    }
}

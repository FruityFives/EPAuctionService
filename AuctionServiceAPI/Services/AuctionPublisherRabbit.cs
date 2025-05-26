using System.Text.Json;
using System.Text;
using Models;
using RabbitMQ.Client;

namespace AuctionServiceAPI.Services;

public class AuctionPublisherRabbit : IAuctionPublisherRabbit
{
    private readonly ILogger<AuctionPublisherRabbit> _logger;

    public AuctionPublisherRabbit(ILogger<AuctionPublisherRabbit> logger)
    {
        _logger = logger;
    }

    public async Task PublishAuctionAsync(AuctionDTO auction)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "syncAuctionQueue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(auction);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "syncAuctionQueue",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body
        );

        _logger.LogInformation("Synced auction to BidService via RabbitMQ. AuctionId: {AuctionId}", auction.AuctionId);
    }
}

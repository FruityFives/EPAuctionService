using Models;
using RabbitMQ.Client;
using System.Text.Json;

namespace AuctionServiceAPI.Services;
public class StoragePublisherRabbit : IStoragePublisherRabbit
{
    private readonly ILogger<StoragePublisherRabbit> _logger;



    public StoragePublisherRabbit(ILogger<StoragePublisherRabbit> logger)
    {
        _logger = logger;
    }

    public async Task PublishAuctionAsync(AuctionDTO auction)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory() { HostName = host };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "auctionQueue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(auction);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "auctionQueue",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body
        );

        _logger.LogInformation("Published bid {id} to RabbitMQ", auction.EffectId);
    }
}
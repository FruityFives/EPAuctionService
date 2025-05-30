using Models;
using RabbitMQ.Client;
using System.Text.Json;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Service der sender afsluttede auktioner som beskeder til RabbitMQ.
/// </summary>
public class StoragePublisherRabbit : IStoragePublisherRabbit
{
    private readonly ILogger<StoragePublisherRabbit> _logger;

    /// <summary>
    /// Initialiserer StoragePublisherRabbit med logger.
    /// </summary>
    /// <param name="logger">Logger til logning af RabbitMQ-hændelser.</param>
    public StoragePublisherRabbit(ILogger<StoragePublisherRabbit> logger)
    {
        _logger = logger;
        _logger.LogInformation("StoragePublisherRabbit initialized");
    }

    /// <summary>
    /// Publicerer en afsluttet auktion til RabbitMQ køen 'auctionQueue'.
    /// </summary>
    /// <param name="auction">DTO-objekt med oplysninger om auktionen.</param>
    /// <returns>Asynkront Task.</returns>
    public async Task PublishAuctionAsync(AuctionDTO auction)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        _logger.LogInformation("Attempting to publish auction with EffectId {effectId} to RabbitMQ at host {host}", auction.EffectId, host);

        var factory = new ConnectionFactory() { HostName = host };

        try
        {
            await using var connection = await factory.CreateConnectionAsync();
            _logger.LogInformation("RabbitMQ connection established.");

            await using var channel = await connection.CreateChannelAsync();
            _logger.LogInformation("RabbitMQ channel created.");

            await channel.QueueDeclareAsync(
                queue: "auctionQueue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _logger.LogInformation("Queue 'auctionQueue' declared.");

            var body = JsonSerializer.SerializeToUtf8Bytes(auction);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "auctionQueue",
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: body
            );

            _logger.LogInformation("Published auction with EffectId {effectId} to RabbitMQ queue 'auctionQueue'.", auction.EffectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish auction with EffectId {effectId} to RabbitMQ", auction.EffectId);
            throw;
        }
    }
}

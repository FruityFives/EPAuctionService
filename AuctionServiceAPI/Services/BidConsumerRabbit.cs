using Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Baggrundsservice, der lytter på RabbitMQ-beskeder med bud og opdaterer auktioner.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initialiserer Worker med logger, konfiguration og service provider til DI.
    /// </summary>
    /// <param name="logger">Logger til logning</param>
    /// <param name="configuration">App-konfiguration</param>
    /// <param name="serviceProvider">Service provider til afhængighedsinjektion</param>
    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Udfører lytning på RabbitMQ-køen og håndterer indkomne budbeskeder.
    /// </summary>
    /// <param name="stoppingToken">Token til at afbryde eksekvering</param>
    /// <returns>En Task der repræsenterer det kørende baggrundsarbejde</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Worker started");

        var rabbitMQHost = _configuration["RABBITMQ_HOST"] ?? "localhost";
        _logger.LogInformation($"RabbitMQ host: {rabbitMQHost}");

        int maxAttempts = 10;
        int attempt = 0;

        while (attempt < maxAttempts && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMQHost,
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: "bidQueue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _logger.LogInformation(" [*] Waiting for bid messages...");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Received: {message}");

                    try
                    {
                        var bidRequest = JsonSerializer.Deserialize<BidDTO>(message);
                        if (bidRequest == null)
                        {
                            _logger.LogError("Deserialization returned null.");
                            return;
                        }

                        using var scope = _serviceProvider.CreateScope();
                        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
                        await auctionService.CreateBidToAuctionById(bidRequest);
                        _logger.LogInformation($"Bid {bidRequest.UserId} applied to auction {bidRequest.AuctionId}");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "JSON deserialization failed.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing bid.");
                    }

                    await Task.CompletedTask;
                };

                await channel.BasicConsumeAsync("bidQueue", autoAck: true, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                break; // connection succeeded, break retry loop
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Attempt {attempt} to connect to RabbitMQ failed. Retrying in 5s...", attempt);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (attempt == maxAttempts)
        {
            _logger.LogError("Failed to connect to RabbitMQ after {maxAttempts} attempts. Worker will stop.", maxAttempts);
        }
    }
}

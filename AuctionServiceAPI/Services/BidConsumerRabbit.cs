using Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AuctionServiceAPI.Services;

/// <summary>
/// Baggrundsservice der lytter til RabbitMQ for budbeskeder og behandler dem.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initialiserer Worker med logger, konfiguration og service provider.
    /// </summary>
    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Starter baggrundsservicen og opretter forbindelse til RabbitMQ.
    /// Lytter til 'bidQueue' og forsøger at anvende indkommende bud på de korrekte auktioner.
    /// </summary>
    /// <param name="stoppingToken">Token til at stoppe servicen.</param>
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

                _logger.LogInformation(" [*] Venter på budbeskeder...");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($" [x] Modtaget: {message}");

                    try
                    {
                        var bidRequest = JsonSerializer.Deserialize<BidDTO>(message);
                        if (bidRequest == null)
                        {
                            _logger.LogError("Deserialisering returnerede null.");
                            return;
                        }

                        using var scope = _serviceProvider.CreateScope();
                        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
                        await auctionService.CreateBidToAuctionById(bidRequest);
                        _logger.LogInformation($"Bud fra bruger {bidRequest.UserId} anvendt på auktion {bidRequest.AuctionId}");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "JSON-deserialisering fejlede.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fejl under behandling af bud.");
                    }

                    await Task.CompletedTask;
                };

                await channel.BasicConsumeAsync("bidQueue", autoAck: true, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                break; // succesfuld forbindelse, afslut retry-loop
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Forsøg {attempt} på at forbinde til RabbitMQ fejlede. Prøver igen om 5 sekunder...", attempt);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (attempt == maxAttempts)
        {
            _logger.LogError("Kunne ikke oprette forbindelse til RabbitMQ efter {maxAttempts} forsøg. Arbejder stoppes.", maxAttempts);
        }
    }
}

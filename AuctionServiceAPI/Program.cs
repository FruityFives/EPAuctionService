using System.IO.Compression;
using System.Text.Json.Serialization; // 👈 Tilføj dette
using AuctionServiceAPI.Repositories;
using AuctionServiceAPI.Services;
using NLog;
using NLog.Web;

var logger = NLog.Web.NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
logger.Debug("Starter auctionservice API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // 👇 Tilføj enum-konverter
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton<IAuctionRepository, AuctionRepository>();
    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
    builder.Services.AddSingleton<IAuctionService, AuctionService>();
    builder.Services.AddSingleton<ICatalogService, CatalogService>();

    builder.Services.AddSingleton<IStoragePublisherRabbit, StoragePublisherRabbit>();
    builder.Services.AddSingleton<IAuctionPublisherRabbit, AuctionPublisherRabbit>();

    builder.Services.AddHostedService<Worker>();


    var app = builder.Build();

    // 👇 Seed testdata (som du allerede har)
    using (var scope = app.Services.CreateScope())
    {
        var catalogRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();
        var auctionRepo = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();

        if (catalogRepo is CatalogRepository repo)
        {
            repo.SeedDataCatalog();
        }
        if (auctionRepo is AuctionRepository ARepo)
        {
            ARepo.SeedDataAuction();
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

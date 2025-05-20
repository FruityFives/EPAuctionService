using System.IO.Compression;
using AuctionServiceAPI.Repositories;
using AuctionServiceAPI.Services;
using NLog;
using NLog.Web;

var logger = NLog.Web.NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
logger.Debug("Starter auctionservice API");

try
{
    var builder = WebApplication.CreateBuilder(args);
// 2. Registrér NLog som logger - ryd eksisterende loggere:
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAuctionRepository, AuctionRepository>();
builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
builder.Services.AddSingleton<IAuctionService, AuctionService>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<IStoragePublisherRabbit, StoragePublisherRabbit>();
builder.Services.AddHostedService<Worker>();







var app = builder.Build();


    // Seed data
    // Kald SeedData() her
    using (var scope = app.Services.CreateScope())
    {
        var catalogRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();
        var auctionRepo = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();

        if (catalogRepo is CatalogRepository repo)
        {
            repo.SeedData();
        }
        if (auctionRepo is AuctionRepository ARepo)
        {
            ARepo.SeedDataAuction();
        }
    }

    // Configure the HTTP request pipeline.
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
    throw; // Genkast for at sikre at fejlen ikke bliver "slugt"
}
finally
{
    NLog.LogManager.Shutdown();
}
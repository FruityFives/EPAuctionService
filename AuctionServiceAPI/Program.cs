using AuctionServiceAPI.Repositories;
using AuctionServiceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAuctionRepository, AuctionRepository>();
builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
builder.Services.AddSingleton<IAuctionService, AuctionService>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
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
    if (auctionRepo is AuctionRepository auctionRepository)
    {
        auctionRepository.SeedDataAuction();
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

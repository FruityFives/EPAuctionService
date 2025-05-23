using System.IO.Compression;
using AuctionServiceAPI.Repositories;
using AuctionServiceAPI.Services;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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
    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
    builder.Services.AddSingleton<IAuctionService, AuctionService>();
    builder.Services.AddSingleton<ICatalogService, CatalogService>();
    builder.Services.AddSingleton<IStoragePublisherRabbit, StoragePublisherRabbit>();
    builder.Services.AddHostedService<Worker>();





var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


    var app = builder.Build();


    // Seed data
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
  

    app.UseHttpsRedirection();
    app.UseAuthentication(); 
    app.UseAuthorization();

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
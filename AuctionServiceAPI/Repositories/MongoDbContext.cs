using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;



namespace AuctionServiceAPI.Repositories
{
    /// <summary>
    /// Interface til MongoDbContext, som eksponerer MongoDB-kollektioner for auktioner og kataloger.
    /// </summary>
    public interface IMongoDbContext
    {
        /// <summary>
        /// MongoDB-kollektionen for auktioner.
        /// </summary>
        IMongoCollection<Auction> AuctionCollection { get; }

        /// <summary>
        /// MongoDB-kollektionen for kataloger.
        /// </summary>
        IMongoCollection<Catalog> CatalogCollection { get; }
    }

    /// <summary>
    /// MongoDbContext klasse, som håndterer forbindelsen til MongoDB og tilgår kollektioner.
    /// </summary>
    public class MongoDbContext : IMongoDbContext
    {
        /// <summary>
        /// Referencen til selve databasen.
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        /// MongoDB-kollektionen for auktioner.
        /// </summary>
        public IMongoCollection<Auction> AuctionCollection { get; }

        /// <summary>
        /// MongoDB-kollektionen for kataloger.
        /// </summary>
        public IMongoCollection<Catalog> CatalogCollection { get; }

        /// <summary>
        /// Opretter en ny MongoDbContext med konfiguration og logger.
        /// Initialiserer database og kollektioner.
        /// </summary>
        /// <param name="logger">Logger til logning af forbindelsesinformation.</param>
        /// <param name="config">Applikationskonfiguration med MongoDB URI og kollektionsnavne.</param>
        public MongoDbContext(ILogger<MongoDbContext> logger, IConfiguration config)
        {
            var connectionString = config["MONGODB_URI"] ?? "mongodb://mongo:27017";
            var dbName = config["AUCTION_DB_NAME"] ?? "AuctionServiceDB";
            var collectionAuction = config["AUCTION_COLLECTION_NAME"] ?? "AuctionCollection";
            var collectionCatalog = config["CATALOG_COLLECTION_NAME"] ?? "CatalogCollection";

            logger.LogInformation($"Connected to database {dbName}");
            logger.LogInformation($"Using collection {collectionAuction}");
            logger.LogInformation($"Using collection {collectionCatalog}");

            // 🧠 VIGTIGT: Registrer enum-serializere for at gemme enums som strings
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<CatalogStatus>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<AuctionStatus>(BsonType.String));

            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(dbName);
            AuctionCollection = Database.GetCollection<Auction>(collectionAuction);
            CatalogCollection = Database.GetCollection<Catalog>(collectionCatalog);
        }
    }
}

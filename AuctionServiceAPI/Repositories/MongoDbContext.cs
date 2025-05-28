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
    /// Interface til MongoDB-kontekst, der giver adgang til auktion- og katalog-collections.
    /// </summary>
    public interface IMongoDbContext
    {
        /// <summary>
        /// Collection for auktioner.
        /// </summary>
        IMongoCollection<Auction> AuctionCollection { get; }

        /// <summary>
        /// Collection for kataloger.
        /// </summary>
        IMongoCollection<Catalog> CatalogCollection { get; }
    }

    /// <summary>
    /// Implementation af MongoDB-kontekst til AuctionService.
    /// </summary>
    public class MongoDbContext : IMongoDbContext
    {
        /// <summary>
        /// MongoDB-database instans.
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        /// MongoDB-collection for auktioner.
        /// </summary>
        public IMongoCollection<Auction> AuctionCollection { get; }

        /// <summary>
        /// MongoDB-collection for kataloger.
        /// </summary>
        public IMongoCollection<Catalog> CatalogCollection { get; }

        /// <summary>
        /// Initialiserer MongoDB-konteksten med konfiguration og logger.
        /// </summary>
        /// <param name="logger">Logger til logning af forbindelsesinformation</param>
        /// <param name="config">Konfiguration med forbindelsesstreng og collection-navne</param>
        public MongoDbContext(ILogger<MongoDbContext> logger, IConfiguration config)
        {
            var connectionString = config["MONGODB_URI"] ?? "mongodb://mongo:27017";
            var dbName = config["AUCTION_DB_NAME"] ?? "AuctionServiceDB";
            var collectionAuction = config["AUCTION_COLLECTION_NAME"] ?? "AuctionCollection";
            var collectionCatalog = config["CATALOG_COLLECTION_NAME"] ?? "CatalogCollection";

            logger.LogInformation($"Connected to database {dbName}");
            logger.LogInformation($"Using collection {collectionAuction}");
            logger.LogInformation($"Using collection {collectionCatalog}");

            // 🧠 VIGTIGT: Registrer enum-serializere
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<CatalogStatus>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<AuctionStatus>(BsonType.String)); // hvis du bruger den andre steder

            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(dbName);
            AuctionCollection = Database.GetCollection<Auction>(collectionAuction);
            CatalogCollection = Database.GetCollection<Catalog>(collectionCatalog);
        }
    }
}

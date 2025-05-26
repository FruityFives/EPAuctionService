using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AuctionServiceAPI.Repositories
{
    public interface IMongoDbContext
    {
        IMongoCollection<Auction> AuctionCollection { get; }
        IMongoCollection<Catalog> CatalogCollection { get; }
    }

    public class MongoDbContext : IMongoDbContext
    {
        public IMongoDatabase Database { get; }
        public IMongoCollection<Auction> AuctionCollection { get; }
        public IMongoCollection<Catalog> CatalogCollection { get; }

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

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Wallet.Core.Client;

namespace Wallet.Core.Models {
    public enum TransactionType {
        Income,
        Expense
    }

    public class WalletTransaction {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? WalletTransactionId { get; set; } = null!;


        [BsonElement("UserId")]
        public string? UserId { get; set; } = null!;


        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("SubCategoryId")]
        public string? SubCategoryId { get; set; } = null!;

        [BsonElement("Amount")]
        public long? Amount { get; set; } = null!;

        [BsonElement("OnTime")]
        public DateTime? OnTime { get; set; } = null!;

        [BsonElement("Description")]
        public string? Description { get; set; } = null!;


        [BsonElement("TransactionType")]
        public TransactionType? TransactionType { get; set; } = null!;


        [BsonElement("IsActive")]
        public bool? IsActive { get; set; } = null!;

    }

    public class WalletTransactionService {
        private readonly IMongoCollection<WalletTransaction> _walletTransactionCollection;
        private readonly IMongoCollection<BsonDocument> _walletTransactionBsonCollection;
        private readonly IMongoCollection<BsonDocument> _systemCollection;
        private FilterDefinitionBuilder<WalletTransaction> _filterBuilder;
        private FilterDefinition<WalletTransaction> _isActiveFilter;

        public WalletTransactionService(
            IOptions<MongoDBSettings> walletTransactionStoreDatabaseSettings) {
            var mongoClient = new MongoClient(
                walletTransactionStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                walletTransactionStoreDatabaseSettings.Value.DatabaseName);

            _walletTransactionCollection = mongoDatabase.GetCollection<WalletTransaction>("transactions");
            _walletTransactionBsonCollection = mongoDatabase.GetCollection<BsonDocument>("transactions");
            _systemCollection = mongoDatabase.GetCollection<BsonDocument>("system");

            _filterBuilder = Builders<WalletTransaction>.Filter;
            _isActiveFilter = _filterBuilder.Eq(x => x.IsActive, true);

        }

        private class SubCategoryUsage {
            public string? _id { get; set; } = null!;
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string SubCategoryId { get; set; } = null!;
        }
        public async Task<string[]> GetRecommendationSubCategories(string userId) {
            var match = new BsonDocument {
                { "UserId", userId },
                { "IsActive", true }
            };


            var group = new BsonDocument {
                { "_id", "$SubCategoryId" },
                { "Count", new BsonDocument("$sum", 1) }
            };

            var sort = Builders<BsonDocument>.Sort.Descending("Count");

            var aggregation = _walletTransactionCollection.Aggregate()
                .Match(match)
                .Group(group)
                .Sort(sort)
                .Limit(5)
                ;

            var list = await aggregation.ToListAsync();
            return list.Select(x => x["_id"].AsObjectId.ToString()).ToArray();
        }



        private class WalletTransactionResult {
            public string _id { get; set; }
            public long NetAmount { get; set; }
        }
        public async Task<long> CalculateNetAmount(string userId) {
            var aggregationPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument{
                    { "UserId", userId },
                    { "IsActive", true }
                }),
                new BsonDocument("$group", new BsonDocument {
                    { "_id", BsonNull.Value },
                    { "TotalIncome", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray {
                        new BsonDocument("$eq", new BsonArray { "$TransactionType", TransactionType.Income }),
                        "$Amount",
                        0
                    }))},
                    { "TotalExpense", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray {
                        new BsonDocument("$eq", new BsonArray { "$TransactionType", TransactionType.Expense }),
                        "$Amount",
                        0
                    }))}
                }),
                new BsonDocument("$project", new BsonDocument {
                    { "NetAmount", new BsonDocument("$subtract", new BsonArray { "$TotalIncome", "$TotalExpense" }) }
                })
            };
            
            var result = await _walletTransactionCollection.Aggregate<WalletTransactionResult>(aggregationPipeline).FirstOrDefaultAsync();
            return result?.NetAmount ?? 0;
        }

        private FilterDefinition<WalletTransaction> getIdFilter(string walletTransactionId) {
            return _filterBuilder.Eq(x => x.WalletTransactionId, walletTransactionId);
        }


        public async Task<List<WalletTransaction>> GetListToAdminAsync() =>
            await _walletTransactionCollection.Find(filter: FilterDefinition<WalletTransaction>.Empty).ToListAsync();

        public async Task<List<WalletTransaction>> GetListAsync(string userId) { 
            var userIds4CustomFilter = _filterBuilder.Eq(x => x.UserId, userId);
            var andFilter = _filterBuilder.And(userIds4CustomFilter, _isActiveFilter);

            return await _walletTransactionCollection.Find(andFilter).ToListAsync();
        }


        public async Task<long> GetCount() =>
            await _walletTransactionCollection.CountDocumentsAsync(filter: FilterDefinition<WalletTransaction>.Empty);



        public async Task<WalletTransaction?> GetAsyncById(string walletTransactionId) =>
            await _walletTransactionCollection.Find(x => x.WalletTransactionId == walletTransactionId).FirstOrDefaultAsync();

        public async Task CreateAsync(WalletTransaction newWalletTransaction) =>
            await _walletTransactionCollection.InsertOneAsync(newWalletTransaction);

        public async Task UpdateAsync(string walletTransactionId, WalletTransaction updatedWalletTransaction) =>
            await _walletTransactionCollection.ReplaceOneAsync(x => x.WalletTransactionId == walletTransactionId, updatedWalletTransaction);

        public async Task RemoveAsync(string walletTransactionId) =>
            await _walletTransactionCollection.DeleteOneAsync(x => x.WalletTransactionId == walletTransactionId);



        public async Task InactiveAsync(string walletTransactionId) {
            //var filter = Builders<BsonDocument>.Filter.Eq("CategoryId", categoryId);
            //var update = Builders<BsonDocument>.Update.Set("IsActive", false);


            var update2 = Builders<WalletTransaction>.Update.Set(x => x.IsActive, false);

            await _walletTransactionCollection.FindOneAndUpdateAsync(getIdFilter(walletTransactionId), update2);

            //await _categoryBsonCollection.FindOneAndUpdateAsync(filter, update);
        }
    }

}

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Wallet.Core.Client;

namespace Wallet.Core.Models {
    public class Category {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }       

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("IsCustom")]
        public bool? IsCustom { get; set; } = null!;

        [BsonElement("IsActive")]
        public bool? IsActive { get; set; } = null!;
        
        [BsonElement("UserIds4Custom")]
        public string[] UserIds4Custom { get; set; } = null!;
    }

    public class CategoryService {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<BsonDocument> _categoryBsonCollection;
        private readonly IMongoCollection<BsonDocument> _systemCollection;
        private FilterDefinitionBuilder<Category> _filterBuilder;
        private FilterDefinition<Category> _isActiveFilter;
        public CategoryService(
            IOptions<MongoDBSettings> categoryStoreDatabaseSettings) {
            var mongoClient = new MongoClient(
                categoryStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                categoryStoreDatabaseSettings.Value.DatabaseName);

            _categoryCollection = mongoDatabase.GetCollection<Category>("categories");
            _categoryBsonCollection = mongoDatabase.GetCollection<BsonDocument>("categories");
            _systemCollection = mongoDatabase.GetCollection<BsonDocument>("system");

            _filterBuilder = Builders<Category>.Filter;
            _isActiveFilter = _filterBuilder.Eq(x => x.IsActive, true);

        }

        private FilterDefinition<Category> getIdFilter(string categoryId) {
            return _filterBuilder.Eq(x => x.CategoryId, categoryId);
        }


        public async Task<long> GetCount() =>
            await _categoryCollection.CountDocumentsAsync(filter: FilterDefinition<Category>.Empty);

        public async Task<List<Category>> GetListToAdminAsync() =>
            await _categoryCollection.Find(filter: FilterDefinition<Category>.Empty).ToListAsync();


        public async Task<List<Category>> GetListAsync(string userId) {
            var userIds4CustomFilter = _filterBuilder.AnyEq(x => x.UserIds4Custom, userId);
            var isCustomFilter = _filterBuilder.Eq(x => x.IsCustom, false);

            var orFilter = _filterBuilder.Or(userIds4CustomFilter, isCustomFilter);
            var andFilter = _filterBuilder.And(orFilter, _isActiveFilter);

            return await _categoryCollection.Find(andFilter).ToListAsync();
        }

        public async Task<Category?> GetAsyncById(string categoryId) =>
            await _categoryCollection.Find(x => x.CategoryId == categoryId).FirstOrDefaultAsync();

        public async Task<BsonDocument> GetBsonAsyncById(string categoryId) =>
            await _categoryBsonCollection.Find(x => x["CategoryId"] == categoryId).FirstOrDefaultAsync();
        

        public async Task<Category?> GetAsyncByName(string categoryName) =>
            await _categoryCollection.Find(x => x.Name == categoryName).FirstOrDefaultAsync();

        public async Task<BsonDocument> GetBsonAsyncByName(string categoryName) =>
                await _categoryBsonCollection.Find(x => x["Name"] == categoryName).FirstOrDefaultAsync();


        public async Task CreateAsync(Category newCategory) =>
            await _categoryCollection.InsertOneAsync(newCategory);

        public async Task CreateCustomAsync(BsonDocument bsonDocument) {
            await _categoryBsonCollection.InsertOneAsync(bsonDocument);
        }



        public async Task UpdateAsync(string categoryId, Category updatedCategory) =>
            await _categoryCollection.ReplaceOneAsync(x => x.CategoryId == categoryId, updatedCategory);


        public async Task UpdateBsonAsync(string categoryId, BsonDocument bsonDocument) =>
            await _categoryBsonCollection.ReplaceOneAsync(x => x["CategoryId"] == categoryId, bsonDocument);


        public async Task RemoveAsync(string categoryId) =>
            await _categoryCollection.DeleteOneAsync(x => x.CategoryId == categoryId);

        public async Task InactiveAsync(string categoryId) {
            //var filter = Builders<BsonDocument>.Filter.Eq("CategoryId", categoryId);
            //var update = Builders<BsonDocument>.Update.Set("IsActive", false);


            var update2 = Builders<Category>.Update.Set(x => x.IsActive, false);

            await _categoryCollection.FindOneAndUpdateAsync(getIdFilter(categoryId), update2);

            //await _categoryBsonCollection.FindOneAndUpdateAsync(filter, update);
        }


    }
}

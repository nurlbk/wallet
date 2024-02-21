using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Wallet.Core.Client;

namespace Wallet.Core.Models {
    public class SubCategory {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SubCategoryId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("CategoryId")]
        public string? CategoryId { get; set; } = null!;

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("IsCustom")]
        public bool? IsCustom { get; set; } = null!;

        [BsonElement("IsActive")]
        public bool? IsActive { get; set; } = null!;

        [BsonElement("UserIds4Custom")]
        public string[] UserIds4Custom { get; set; } = null!;
    }


    public class SubCategoryService {
        private readonly IMongoCollection<SubCategory> _subCategoryCollection;
        private readonly IMongoCollection<BsonDocument> _subCategoryBsonCollection;
        private readonly IMongoCollection<BsonDocument> _systemCollection;
        private FilterDefinitionBuilder<SubCategory> _filterBuilder;
        private FilterDefinition<SubCategory> _isActiveFilter;
        public SubCategoryService(
            IOptions<MongoDBSettings> subCategoryStoreDatabaseSettings) {
            var mongoClient = new MongoClient(
                subCategoryStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                subCategoryStoreDatabaseSettings.Value.DatabaseName);

            _subCategoryCollection = mongoDatabase.GetCollection<SubCategory>("subCategories");
            _subCategoryBsonCollection = mongoDatabase.GetCollection<BsonDocument>("subCategories");
            _systemCollection = mongoDatabase.GetCollection<BsonDocument>("system");

            _filterBuilder = Builders<SubCategory>.Filter;
            _isActiveFilter = _filterBuilder.Eq(x => x.IsActive, true);

        }

        private FilterDefinition<SubCategory> getIdFilter(string subCategoryId) {
            return _filterBuilder.Eq(x => x.SubCategoryId, subCategoryId);
        }
        public async Task<long> GetCount() =>
            await _subCategoryCollection.CountDocumentsAsync(filter: FilterDefinition<SubCategory>.Empty);

        public async Task<List<SubCategory>> GetListToAdminAsync() =>
            await _subCategoryCollection.Find(filter: FilterDefinition<SubCategory>.Empty).ToListAsync();


        public async Task<List<SubCategory>> GetListAsync(string userId, string categoryId) {
            var userIds4CustomFilter = _filterBuilder.AnyEq(x => x.UserIds4Custom, userId);
            var isCustomFilter = _filterBuilder.Eq(x => x.IsCustom, false);

            var orFilter = _filterBuilder.Or(userIds4CustomFilter, isCustomFilter);

            var categotyFilter = _filterBuilder.Eq(x => x.CategoryId, categoryId);

            var andFilter = _filterBuilder.And(orFilter, _isActiveFilter, categotyFilter);

            return await _subCategoryCollection.Find(andFilter).ToListAsync();
        }

        public async Task<SubCategory?> GetAsyncById(string subCategoryId) =>
            await _subCategoryCollection.Find(x => x.SubCategoryId == subCategoryId).FirstOrDefaultAsync();
        public async Task<SubCategory?> GetAsyncByName(string subCategoryName) =>
            await _subCategoryCollection.Find(x => x.Name == subCategoryName).FirstOrDefaultAsync();

        public async Task CreateAsync(SubCategory newSubCategory) =>
            await _subCategoryCollection.InsertOneAsync(newSubCategory);

        public async Task UpdateAsync(string subCategoryId, SubCategory updatedSubCategory) =>
            await _subCategoryCollection.ReplaceOneAsync(x => x.SubCategoryId == subCategoryId, updatedSubCategory);

        public async Task RemoveAsync(string subCategoryId) =>
            await _subCategoryCollection.DeleteOneAsync(x => x.SubCategoryId == subCategoryId);

        public async Task InactiveAsync(string subCategoryId) {
            //var filter = Builders<BsonDocument>.Filter.Eq("SubCategoryId", subCategoryId);
            //var update = Builders<BsonDocument>.Update.Set("IsActive", false);


            var update2 = Builders<SubCategory>.Update.Set(x => x.IsActive, false);

            await _subCategoryCollection.FindOneAndUpdateAsync(getIdFilter(subCategoryId), update2);

            //await _subCategoryBsonCollection.FindOneAndUpdateAsync(filter, update);
        }


    }
}

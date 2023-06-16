using MongoDB.Driver;
using System.Linq.Expressions;

namespace Play.Common.MongoDB
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> _mongoCollection;
        private readonly FilterDefinitionBuilder<T> _filterDefinitionBuilder = Builders<T>.Filter;
        public MongoRepository(
            IMongoDatabase db,
            string collectionName
            )
        {
            _mongoCollection = db.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await _mongoCollection.Find(_filterDefinitionBuilder.Empty).ToListAsync();
        }

        public async Task<T> GetAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterDefinitionBuilder.Eq(e => e.Id, id);
            return await _mongoCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task CreateAsync(T input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            await _mongoCollection.InsertOneAsync(input);
        }

        public async Task UpdateAsync(T input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            FilterDefinition<T> filter = _filterDefinitionBuilder.Eq(e => e.Id, input.Id);
            await _mongoCollection.ReplaceOneAsync(filter, input);
        }

        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterDefinitionBuilder.Eq(e => e.Id, id);
            await _mongoCollection.DeleteOneAsync(filter);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _mongoCollection.Find(predicate).ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _mongoCollection.Find(predicate).SingleOrDefaultAsync();
        }
    }
}

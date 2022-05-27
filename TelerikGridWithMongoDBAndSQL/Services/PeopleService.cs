using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Telerik.DataSource;
using TelerikGridWithMongoDBAndSQL.Data;

namespace TelerikGridWithMongoDBAndSQL.Services
{
    public class PeopleService : IPeopleService
    {
        private readonly SqlDbContext _dbContext;

        private readonly IMongoDatabase _mongoDatabase;

        public PeopleService(SqlDbContext dbContext, IMongoDbSettings settings)
        {
            _dbContext = dbContext;
            var client = new MongoClient(settings.ConnectionString);
            _mongoDatabase = client.GetDatabase(settings.DatabaseName);
        }

        public async Task<List<T>> GetFromDatabase<T>(string query) where T : class
        {
            try
            {
                DbSet<T> dbSet = _dbContext.Set<T>();
                var lst = dbSet.FromSqlRaw(query).AsQueryable();
                return await lst.Cast<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid Entity", ex);
            }
        }

        public async Task<List<T>> GetRecordsFromMongoDB<T>(string collectionName, int skip, int pageSize, FilterDefinition<T> filter, SortDefinition<T> sort) where T : class
        {
            try
            {
                IMongoCollection<T> _collection = _mongoDatabase.GetCollection<T>(collectionName);

                return await _collection.Find(filter)
                                .Skip(skip).Limit(pageSize)
                                .Sort(sort)
                                .ToListAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}

using MongoDB.Driver;
using Telerik.DataSource;

namespace TelerikGridWithMongoDBAndSQL.Services
{
    public interface IPeopleService
    {
        public Task<List<T>> GetFromDatabase<T>(string query) where T : class;
        public Task<List<T>> GetRecordsFromMongoDB<T>(string collectionName, int skip, int pageSize, FilterDefinition<T> filter, SortDefinition<T> sort) where T : class;
    }
}

using Telerik.DataSource;
using TelerikGridWithMongoDBAndSQL.Data;

namespace TelerikGridWithMongoDBAndSQL.Services
{
    public interface ICommonService
    {
        public Task<List<People>> GetPeople(int skip, int pageSize, string where, string order);
        public Task<List<People>> GetPeopleFromMongoDB(int skip, int pageSize, DataSourceRequest? request);
    }
}

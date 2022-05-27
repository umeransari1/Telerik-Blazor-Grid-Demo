using MongoDB.Driver;
using Telerik.DataSource;
using TelerikGridWithMongoDBAndSQL.Data;

namespace TelerikGridWithMongoDBAndSQL.Services
{
    public class CommonService : ICommonService
    {
        private readonly IPeopleService _peopleService;

        public CommonService(IPeopleService peopleService)
        {
            _peopleService = peopleService;
        }

        public async Task<List<People>> GetPeople(int skip, int pageSize, string where, string order)
        {
            List<People> lst = new List<People>();

            try
            {
                order = String.IsNullOrEmpty(order) ? "id" : order;
                string select = $"select * from Peoples {where} ORDER BY {order} OFFSET {skip} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                lst = await _peopleService.GetFromDatabase<People>(select);
            }
            catch (Exception ex)
            {
                throw;
            }

            return lst;
        }

        public async Task<List<People>> GetPeopleFromMongoDB(int skip, int pageSize, DataSourceRequest? request)
        {
            List<People> lst = new List<People>();

            try
            {
                var filterBuilder = Builders<PeopleMongoClass>.Filter;
                var sortBuilder = Builders<PeopleMongoClass>.Sort;
                var sort = sortBuilder.Ascending("id");
                var filter = filterBuilder.Empty;

                if(request != null && request.Sorts.Any())
                {
                    SortDescriptor obj = request.Sorts.FirstOrDefault();
                    if (obj.SortDirection == ListSortDirection.Descending)
                        sort = sortBuilder.Descending(obj.Member.ToString());
                    else
                        sort = sortBuilder.Ascending(obj.Member.ToString());
                }

                if(request != null && request.Filters.Any())
                {
                    Common common = new Common();
                    filter = common.DesccriptorToMongoDBFilter<PeopleMongoClass>(request.Filters);
                }

                var data = await _peopleService.GetRecordsFromMongoDB("Peoples", skip, pageSize, filter, sort);

                if (data.Any())
                {
                    foreach(var record in data)
                    {
                        People people = new People();
                        people.id = record.id;
                        people.FirstName = record.FirstName;
                        people.LastName = record.LastName;
                        people.DateOfBirth = record.DateOfBirth;
                        people.EmailAddress = record.EmailAddress;

                        lst.Add(people);
                    }
                }

                if(!lst.Any()) lst = new List<People>();
            }
            catch (Exception ex)
            {
                throw;
            }

            return lst;
        }
    }
}

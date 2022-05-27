using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelerikGridWithMongoDBAndSQL.Data
{
    public class PeopleMongoClass
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }
        public int id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EmailAddress { get; set; }
    }
}

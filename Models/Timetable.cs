using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TimetableServer.Models
{
    public class Timetable
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string MD5 { get; set; }

        public string SHA512 { get; set; }
        
        public string Password { get; set; }

        public string UpdatePassword { get; set; }
        
        public string Content { get; set; }

        public string IpAddress { get; set; }
    }
}

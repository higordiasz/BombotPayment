using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BombotPayment.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("customerID")]
        public string CustomerId { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("value")]
        public double Value { get; set; }

        [BsonElement("status")]
        public string PaymentStatus { get; set; }
    }
}

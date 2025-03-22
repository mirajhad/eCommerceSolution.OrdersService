using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace DataAccessLayer.Entities
{
    public class OrderItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid _id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid ProductID { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public decimal? Price { get; set; }
        [BsonRepresentation(BsonType.Int32)]
        public int Quantity { get; set; }
        [BsonRepresentation(BsonType.Double)]
        public decimal TotalPrice { get; set; }
    }
}

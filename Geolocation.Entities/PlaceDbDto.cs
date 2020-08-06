using Amazon.DynamoDBv2.DataModel;

namespace Geolocation.Entities
{
    [DynamoDBTable("Places")]
    public class PlaceDbDto: DynamoDbEntityBase
    {
        [DynamoDBHashKey]
        public string PlaceName { get; set; }

        [DynamoDBProperty]
        public string GooglePlaceId { get; set; }
    }
}

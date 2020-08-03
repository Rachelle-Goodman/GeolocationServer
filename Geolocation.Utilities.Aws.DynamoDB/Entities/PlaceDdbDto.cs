using Amazon.DynamoDBv2.DataModel;

namespace Geolocation.Utilities.Aws.DynamoDB.Entities
{
    [DynamoDBTable("Places")]
    public class PlaceDdbDto: DynamoDbEntityBase
    {
        [DynamoDBHashKey]
        public string PlaceName { get; set; }

        [DynamoDBProperty]
        public string GooglePlaceId { get; set; }
    }
}

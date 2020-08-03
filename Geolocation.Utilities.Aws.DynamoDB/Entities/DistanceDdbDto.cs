using Amazon.DynamoDBv2.DataModel;

namespace Geolocation.Utilities.Aws.DynamoDB.Entities
{
    [DynamoDBTable("Distances")]
    public class DistanceDdbDto: DynamoDbEntityBase
    {
        [DynamoDBHashKey]
        public string Source { get; set; }

        [DynamoDBRangeKey]
        public string Destination { get; set; }

        [DynamoDBProperty]
        public double Distance { get; set; }

        [DynamoDBProperty]
        public int SearchCount { get; set; }
    }
}

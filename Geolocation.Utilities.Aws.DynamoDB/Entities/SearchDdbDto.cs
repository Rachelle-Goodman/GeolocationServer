using Amazon.DynamoDBv2.DataModel;

namespace Geolocation.Utilities.Aws.DynamoDB.Entities
{
    [DynamoDBTable("SavedSearches")]
    public class SearchDdbDto: DynamoDbEntityBase
    {
        [DynamoDBHashKey]
        public string SavedSearchName { get; set; }

        [DynamoDBProperty]
        public SearchDataDdbDto SearchData { get; set; }
    }

    public class SearchDataDdbDto
    {
        [DynamoDBProperty]
        public string Source { get; set; }

        [DynamoDBProperty]
        public string Destination { get; set; }

        [DynamoDBProperty]
        public int SearchCount { get; set; }
    }
}

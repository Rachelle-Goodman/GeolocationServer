using Amazon.DynamoDBv2.DataModel;

namespace Geolocation.Entities
{
    [DynamoDBTable("SavedSearches")]
    public class SearchDbDto: DynamoDbEntityBase
    {
        [DynamoDBHashKey]
        public string SavedSearchName { get; set; }

        [DynamoDBProperty]
        public SearchDataDbDto SearchData { get; set; }
    }

    public class SearchDataDbDto
    {
        [DynamoDBProperty]
        public string Source { get; set; }

        [DynamoDBProperty]
        public string Destination { get; set; }

        [DynamoDBProperty]
        public int SearchCount { get; set; }
    }
}

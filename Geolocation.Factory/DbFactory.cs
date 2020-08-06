using Geolocation.Utilities.Aws.DynamoDB;
using Geoloocation.DB;

namespace Geolocation.Factory
{
    public static class DbFactory
    {
        public static IDB DB => DynamoDbAdapter.Instance;

        public static IDbHealthTester DbHealthTester => DynamoDbHealthTester.Instance;
    }
}

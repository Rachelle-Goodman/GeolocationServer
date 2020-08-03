using Geolocation.Utilities.Aws.DynamoDB;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    public static class HealthRepository
    {
        public static async Task HealthOfDbConnection()
        {
            await DynamoDbHealthTester.HealthCheck();
        }
    }
}

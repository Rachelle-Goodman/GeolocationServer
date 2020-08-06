using Geolocation.Factory;
using Geoloocation.DB;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    public static class HealthRepository
    {
        private static IDbHealthTester _dbHealthTester;

        static HealthRepository()
        {
            _dbHealthTester = DbFactory.DbHealthTester;
        }

        public static async Task HealthOfDbConnection()
        {
            await _dbHealthTester.HealthCheck();
        }
    }
}

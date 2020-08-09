using Geolocation.DependencyInjection;
using Geoloocation.DB;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    public interface IHealthRepository
    {
        Task HealthOfDbConnection();
    }

    [DependencyInjection(DependencyInjectionType.Singleton)]
    public class HealthRepository: IHealthRepository
    {
        private static IDbHealthTester _dbHealthTester;

        public HealthRepository(IDbHealthTester dbHealthTester)
        {
            _dbHealthTester = dbHealthTester;
        }

        public async Task HealthOfDbConnection()
        {
            await _dbHealthTester.HealthCheck();
        }
    }
}

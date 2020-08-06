using System.Threading.Tasks;

namespace Geoloocation.DB
{
    public interface IDbHealthTester
    {
        Task HealthCheck();
    }
}

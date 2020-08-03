using GelocationServer.Entities;
using Geolocation.BL;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GelocationServer.Controllers
{
    public class DistanceController: Controller
    {
        [HttpGet("distance")]
        public async Task<ActionResult<DistanceDto>> GetDistance([FromQuery] string source, [FromQuery] string destination)
        {
            return new DistanceDto
            {
                Distance = await DistanceRepository.GetDistance(source, destination),
            };
        }

        [HttpGet("popularsearch")]
        public async Task<ActionResult<SearchDto>> GetMostPopulatSearch()
        {
            (string source, string destination, int hits) = await DistanceRepository.GetMostPopulatSearch();

            return new SearchDto
            {
                Source = source,
                Destination = destination,
                Hits = hits,
            };
        }
    }
}

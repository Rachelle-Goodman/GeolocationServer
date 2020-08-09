using Geolocation.BL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GelocationServer.Controllers
{
    public class HealthController : Controller
    {
        private static IHealthRepository _healthRepository;

        public HealthController(IHealthRepository healthRepository)
        {
            _healthRepository = healthRepository;
        }


        [HttpGet("hello")]
        public ActionResult Hello()
        {
            return Ok();
        }

        [HttpGet("health")]
        public async Task<ActionResult> HealthOfDbConnection()
        {
            try
            {
                await _healthRepository.HealthOfDbConnection();
                return Ok();
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

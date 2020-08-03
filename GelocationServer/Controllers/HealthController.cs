using Geolocation.BL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GelocationServer.Controllers
{
    public class HealthController : Controller
    {
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
                await HealthRepository.HealthOfDbConnection();
                return Ok();
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

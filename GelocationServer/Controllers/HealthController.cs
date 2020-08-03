using Microsoft.AspNetCore.Mvc;

namespace GelocationServer.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("hello")]
        public ActionResult Hello()
        {
            return Ok();
        }
    }
}

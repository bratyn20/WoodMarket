using Microsoft.AspNetCore.Mvc;

namespace WoodMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Ok("Hello App!");
        }
    }
}

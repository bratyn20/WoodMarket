using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WoodMarket.Services;

namespace WoodMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly CdekDeliveryService _cdekDeliveryService;
        public HelloController(CdekDeliveryService cdekDeliveryService)
        {
            _cdekDeliveryService = cdekDeliveryService;
            
        }

        [HttpGet]
        public ActionResult Index()
        {
            return Ok("Hello App!");
        }

    }
}

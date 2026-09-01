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

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateDelivery()
        {
            var result = await _cdekDeliveryService.SearchCitiesAsync("Mos");
            return Ok(result);
        }

        [HttpPost("calculate2")]
        public async Task<ActionResult> CalculateDelivery2()
        {
            var result = await _cdekDeliveryService.CalculateDeliveryAsync(70, 10, 10, 10, 10);
            return Ok(result);
        }
    }
}

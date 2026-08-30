using Microsoft.AspNetCore.Mvc;
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
            var result = _cdekDeliveryService.SearchCitiesAsync("Томск");
            return Ok(result);
        }

        [HttpPost("calculate2")]
        public ActionResult CalculateDelivery2()
        {
            var result = _cdekDeliveryService.CalculateDeliveryAsync(70, 10, 10, 10, 10);
            return Ok(result);
        }
    }
}

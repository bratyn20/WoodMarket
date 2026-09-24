using Microsoft.AspNetCore.Mvc;
using WoodMarket.Dto;
using WoodMarket.Services;

namespace WoodMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CdekController : ControllerBase
    {
        private readonly CdekDeliveryService _cdekDeliveryService;
        public CdekController(CdekDeliveryService cdekDeliveryService)
        {
            _cdekDeliveryService = cdekDeliveryService;

        }

        /// <summary>
        /// Поиск города по полному названию
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        [HttpPost("searchCities")]
        public async Task<IActionResult> SearchCities(string city)
        {
            var result = await _cdekDeliveryService.SearchCitiesAsync(city);
            return Ok(result);
        }

        /// <summary>
        /// Расчет стоимости и сроков доставки через СДЭК
        /// </summary>
        /// <param name="request">Параметры посылки (Код города получателя (код СДЭК),вес (в Кг), длина, ширина, высота (в См))</param>
        /// <returns>Список тарифов с ценой и сроком доставки</returns>
        /// <response code="200">Успешный расчет</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost("calculateDelivery")]
        public async Task<ActionResult> CalculateDelivery(CalculateDeliveryRequest calculateDeliveryRequest)
        {
            var result = await _cdekDeliveryService.CalculateDeliveryAsync(calculateDeliveryRequest);
            return Ok(result);
        }
    }
}

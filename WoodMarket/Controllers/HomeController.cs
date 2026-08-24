
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, IMapper mapper, ILogger<HomeController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<HomeResponseDto>> GetHomeData()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
                var averageRating = await _context.Reviews.AverageAsync(r => (double?)r.Rating) ?? 0;
                var totalOrders = await _context.Orders.CountAsync();

                var settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

                var newProducts = await _context.Products
                    .Where(p => p.IsNew && p.IsActive)
                    .Include(p => p.Reviews)
                    .Include(p => p.Images)
                    .Include(p => p.Material)
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .ToListAsync();

                var advantages = new[]
                {
                    new AdvantageDto { Icon = "fa-industry", Title = "Сами производим",
                        Description = "Контролируем материал и качество" },
                    new AdvantageDto { Icon = "fa-truck", Title = "Доставка по России",
                        Description = "СДЭК, Почта России и курьерские службы" },
                    new AdvantageDto { Icon = "fa-undo", Title = "Простой возврат",
                        Description = "Помощь с обменом и решением любой вопрос" },
                    new AdvantageDto { Icon = "fa-box", Title = "Надёжная упаковка",
                        Description = "Каждое изделие защищено при перевозке" }
                };

                var response = new HomeResponseDto
                {
                    TotalProducts = totalProducts,
                    AverageRating = Math.Round(averageRating, 1),
                    TotalOrders = totalOrders,
                    RegionsCount = 18,
                    NewProducts = _mapper.Map<List<ProductDto>>(newProducts),
                    Advantages = advantages.ToList(),
                    CompanyName = settings.GetValueOrDefault("CompanyName", "КЕЛО - дерево с характером"),
                    ContactPhone = settings.GetValueOrDefault("ContactPhone", "8-913-843-0005"),
                    ContactEmail = settings.GetValueOrDefault("ContactEmail", "kelo_creates@mail.ru"),
                    GuaranteeMonths = int.Parse(settings.GetValueOrDefault("GuaranteeMonths", "6")),
                    ReadySketches = int.Parse(settings.GetValueOrDefault("ReadySketches", "120"))
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных для главной страницы");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }

}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<HomeResponse>> GetHomeData()
        {
            try
            {
                // Статистика
                var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
                var averageRating = await _context.Reviews.AverageAsync(r => (double?)r.Rating) ?? 0;
                var totalOrders = await _context.Orders.CountAsync();

                // Настройки сайта
                var settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

                // Новинки (коллекция "Резные окна")
                var newProducts = await _context.Products
                    .Where(p => p.IsNew && p.IsActive)
                    .Include(p => p.Reviews)
                    .Include(p => p.Images)
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ShortDescription = p.ShortDescription,
                        Price = p.Price,
                        OldPrice = p.OldPrice,
                        MainImageUrl = p.MainImageUrl,
                        AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                        ReviewCount = p.Reviews.Count(r => r.IsApproved),
                        StockQuantity = p.StockQuantity,
                        IsInStock = p.StockQuantity > 0,
                        IsNew = p.IsNew,
                        IsOnSale = p.OldPrice.HasValue,
                        Slug = p.Slug
                    })
                    .ToListAsync();

                // Преимущества
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

                var response = new HomeResponse
                {
                    TotalProducts = totalProducts,
                    AverageRating = Math.Round(averageRating, 1),
                    TotalOrders = totalOrders,
                    RegionsCount = 18,
                    NewProducts = newProducts,
                    Advantages = advantages,
                    CompanyName = settings.GetValueOrDefault("CompanyName", "КЕЛО - дерево с характером"),
                    ContactPhone = settings.GetValueOrDefault("ContactPhone", "8 800 555-35-35"),
                    ContactEmail = settings.GetValueOrDefault("ContactEmail", "hello@kelo-shop.ru"),
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

    // DTOs
    public class HomeResponse
    {
        public int TotalProducts { get; set; }
        public double AverageRating { get; set; }
        public int TotalOrders { get; set; }
        public int RegionsCount { get; set; }
        public List<ProductDto> NewProducts { get; set; }
        public IEnumerable<AdvantageDto> Advantages { get; set; }
        public string CompanyName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public int GuaranteeMonths { get; set; }
        public int ReadySketches { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string MainImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int StockQuantity { get; set; }
        public bool IsInStock { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public string Slug { get; set; }
    }

    public class AdvantageDto
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

}
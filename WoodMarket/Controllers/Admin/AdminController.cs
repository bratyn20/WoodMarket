using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    //[Authorize(Roles = "Admin")] // ✅ Только администраторы
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IMapper _mapper;

        public AdminController(AppDbContext context, ILogger<AdminController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // ========================================
        // 📊 СТАТИСТИКА
        // ========================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                totalProducts = await _context.Products.CountAsync(p => p.IsActive),
                totalOrders = await _context.Orders.CountAsync(),
                totalUsers = await _context.Users.CountAsync(),
                totalCategories = await _context.Categories.CountAsync(),
                revenue = await _context.Orders
                    .Where(o => o.Status == "Delivered")
                    .SumAsync(o => (decimal?)o.Total) ?? 0,
                newOrders = await _context.Orders
                    .CountAsync(o => o.Status == "New")
            };

            return Ok(stats);
        }

        // ========================================
        // 📦 УПРАВЛЕНИЕ ТОВАРАМИ
        // ========================================
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, int pageSize = 20)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Material);

            var total = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.StockQuantity,
                    p.IsActive,
                    p.IsNew,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    MaterialName = p.Material != null ? p.Material.Name : null,
                    p.Slug
                })
                .ToListAsync();

            return Ok(new { products, total, page, pageSize });
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<AdminProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Specifications)
                    .ThenInclude(ps => ps.Specification)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            var dto = _mapper.Map<AdminProductDto>(product);

            return Ok(dto);
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.CreatedAt = DateTime.UtcNow;
            product.Slug = GenerateSlug(product.Name);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар создан", productId = product.Id });
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] AdminProductUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID не совпадают" });

            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Товар не найден" });

            _mapper.Map(dto, existing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар обновлён" });
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            // Мягкое удаление (пометка как неактивный)
            product.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удалён" });
        }

        // ========================================
        // 📋 УПРАВЛЕНИЕ ЗАКАЗАМИ
        // ========================================
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string status = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.Status,
                    o.Total,
                    CustomerName = o.User != null
                        ? $"{o.User.FirstName} {o.User.LastName}"
                        : "Гость",
                    o.PaymentMethod,
                    o.ShippingMethod,
                    ItemsCount = o.OrderItems.Count
                })
                .ToListAsync();

            return Ok(new { orders, total, page, pageSize });
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Заказ не найден" });

            return Ok(order);
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Заказ не найден" });

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Статус обновлён" });
        }

        // ========================================
        // 👥 УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ
        // ========================================
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, int pageSize = 20)
        {
            var query = _context.Users
                .OrderBy(u => u.Id);

            var total = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.PhoneNumber
                })
                .ToListAsync();

            return Ok(new { users, total, page, pageSize });
        }

        // ========================================
        // 🗂️ УПРАВЛЕНИЕ КАТЕГОРИЯМИ
        // ========================================
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.Slug = GenerateSlug(category.Name);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Категория создана", categoryId = category.Id });
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id)
                return BadRequest();

            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.Entry(existing).CurrentValues.SetValues(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Категория обновлена" });
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Категория удалена" });
        }

        // ========================================
        // 🔧 HELPER МЕТОДЫ
        // ========================================
        private string GenerateSlug(string name)
        {
            return name
                .ToLower()
                .Replace(" ", "-")
                .Replace("ё", "e")
                .Replace("й", "i")
                .Replace("ц", "ts")
                .Replace("у", "u")
                .Replace("к", "k")
                .Replace("е", "e")
                .Replace("н", "n")
                .Replace("г", "g")
                .Replace("ш", "sh")
                .Replace("щ", "sch")
                .Replace("з", "z")
                .Replace("х", "h")
                .Replace("ъ", "")
                .Replace("ф", "f")
                .Replace("ы", "y")
                .Replace("в", "v")
                .Replace("а", "a")
                .Replace("п", "p")
                .Replace("р", "r")
                .Replace("о", "o")
                .Replace("л", "l")
                .Replace("д", "d")
                .Replace("ж", "zh")
                .Replace("э", "e")
                .Replace("я", "ya")
                .Replace("ч", "ch")
                .Replace("с", "s")
                .Replace("м", "m")
                .Replace("и", "i")
                .Replace("т", "t")
                .Replace("ь", "")
                .Replace("б", "b")
                .Replace("ю", "yu")
                .Replace(" ", "-")
                .Replace(".", "");
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}

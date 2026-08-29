using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WoodMarket.Dto;
using WoodMarket.Models;
using WoodMarket.Services;

namespace WoodMarket.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    //[Authorize(Roles = "Admin")] // ✅ Только администраторы
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<AdminController> _logger;
        private readonly IMapper _mapper;

        public AdminController(AppDbContext context, ILogger<AdminController> logger, IFileService fileService, IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
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
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto dto)
        {
            try
            {
                // 1. Валидация
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. Проверка уникальности SKU
                if (!string.IsNullOrEmpty(dto.Sku))
                {
                    var existing = await _context.Products
                        .FirstOrDefaultAsync(p => p.Sku == dto.Sku);
                    if (existing != null)
                        return BadRequest(new { message = "Товар с таким SKU уже существует" });
                }

                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == dto.CategoryId);
                    if (!categoryExists)
                        return BadRequest(new { message = "Указанная категория не существует" });

                // 4. Создаём товар через AutoMapper
                var product = _mapper.Map<Product>(dto);

                // 5. Устанавливаем защищённые поля
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.Slug = GenerateSlug(dto.Name);
                product.IsInStock = dto.StockQuantity > 0;
                product.IsOnSale = dto.OldPrice.HasValue;


                // 7. Обрабатываем изображение, если оно есть
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    try
                    {
                        var imageUrl = await _fileService.SaveImageAsync(dto.ImageFile, product.Id);
                        product.MainImageUrl = imageUrl;
                        await _context.SaveChangesAsync();
                    }
                    catch (ArgumentException ex)
                    {
                        // Если изображение не прошло валидацию, возвращаем ошибку
                        // Но товар уже создан — это плохо. Лучше удалить товар.
                        _context.Products.Remove(product);
                        await _context.SaveChangesAsync();
                        return BadRequest(new { message = ex.Message });
                    }
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 9. Возвращаем результат
                var response = _mapper.Map<ProductDto>(product);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании товара");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("products/{id}")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
        public async Task<IActionResult> UpdateProduct(
            [FromForm] ProductUpdateWithImageDto dto) // ✅ [FromForm] для файлов
        {
            try
            {

                // 2. Находим товар
                var existing = await _context.Products.FindAsync(dto.Id);
                if (existing == null)
                    return NotFound(new { message = "Товар не найден" });

                // 3. Обновляем основные поля
                _mapper.Map(dto, existing);

                // 4. Обработка изображения
                if (dto.RemoveImage)
                {
                    // Удаляем изображение
                    if (!string.IsNullOrEmpty(existing.MainImageUrl))
                    {
                        _fileService.DeleteImage(existing.MainImageUrl);
                        existing.MainImageUrl = null;
                    }
                }
                else if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    // Удаляем старое изображение, если есть
                    if (!string.IsNullOrEmpty(existing.MainImageUrl))
                    {
                        _fileService.DeleteImage(existing.MainImageUrl);
                    }

                    // Сохраняем новое изображение
                    var imageUrl = await _fileService.SaveImageAsync(dto.ImageFile, existing.Id);
                    existing.MainImageUrl = imageUrl;
                }

                // 5. Сохраняем изменения
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 6. Возвращаем результат
                var result = _mapper.Map<ProductDto>(existing);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении товара {ProductId}", dto.Id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }


        [HttpPost("products/{id}/image")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadProductImage(
    [FromForm] ProductImageUploadDto dto) // ✅ Используем DTO
        {
            try
            {
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                    return NotFound(new { message = "Товар не найден" });

                var image = dto.Image;
                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "Файл не выбран" });

                if (!string.IsNullOrEmpty(product.MainImageUrl))
                {
                    _fileService.DeleteImage(product.MainImageUrl);
                }

                var imageUrl = await _fileService.SaveImageAsync(image, product.Id);
                product.MainImageUrl = imageUrl;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Изображение загружено",
                    imageUrl = imageUrl
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке изображения для товара {ProductId}", dto.ProductId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("products/{id}/image")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(new { message = "Товар не найден" });

                if (string.IsNullOrEmpty(product.MainImageUrl))
                    return BadRequest(new { message = "У товара нет изображения" });

                // Удаляем файл
                _fileService.DeleteImage(product.MainImageUrl);

                // Очищаем поле в БД
                product.MainImageUrl = null;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Изображение удалено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображения товара {ProductId}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            if (!string.IsNullOrEmpty(product.MainImageUrl))
            {
                _fileService.DeleteImage(product.MainImageUrl);
                _logger.LogInformation("Удалено изображение: {ImageUrl}", product.MainImageUrl);
            }


            _context.Products.Remove(product);
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
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверяем, нет ли категории с таким именем
                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == dto.Name);
                if (existing != null)
                    return BadRequest(new { message = "Категория с таким именем уже существует" });

                // Проверяем родительскую категорию
                if (dto.ParentCategoryId.HasValue)
                {
                    var parentExists = await _context.Categories
                        .AnyAsync(c => c.Id == dto.ParentCategoryId);
                    if (!parentExists)
                        return BadRequest(new { message = "Родительская категория не найдена" });
                }

                // Создаём категорию
                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ParentCategoryId = dto.ParentCategoryId,
                    DisplayOrder = dto.DisplayOrder,
                    Slug = GenerateSlug(dto.Name)
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Возвращаем результат
                var response = _mapper.Map<CategoryDto>(category);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании категории");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = await _context.Categories.FindAsync(dto.Id);
                if (existing == null)
                    return NotFound(new { message = "Категория не найдена" });

                // Проверяем, не занято ли имя другой категорией
                var nameExists = await _context.Categories
                    .AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id);
                if (nameExists)
                    return BadRequest(new { message = "Категория с таким именем уже существует" });

                // Обновляем
                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.ParentCategoryId = dto.ParentCategoryId;
                existing.DisplayOrder = dto.DisplayOrder;
                existing.Slug = GenerateSlug(dto.Name);

                await _context.SaveChangesAsync();

                var response = _mapper.Map<CategoryDto>(existing);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении категории {CategoryId}", dto.Id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound(new { message = "Категория не найдена" });

                // Проверяем, есть ли товары в этой категории
                if (category.Products != null && category.Products.Any(p => p.IsActive))
                {
                    return BadRequest(new
                    {
                        message = "Нельзя удалить категорию, в которой есть активные товары. Сначала переместите или удалите товары."
                    });
                }

                // Проверяем, есть ли дочерние категории
                var hasChildren = await _context.Categories
                    .AnyAsync(c => c.ParentCategoryId == id);
                if (hasChildren)
                {
                    return BadRequest(new
                    {
                        message = "Нельзя удалить категорию, у которой есть дочерние категории. Сначала удалите или переместите их."
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Категория удалена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении категории {CategoryId}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var dto = _mapper.Map<CategoryDto>(category);
            dto.ProductCount = category.Products?.Count(p => p.IsActive) ?? 0;

            return Ok(dto);
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

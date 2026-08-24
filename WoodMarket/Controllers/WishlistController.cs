using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(AppDbContext context, IMapper mapper, ILogger<WishlistController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ========================================
        // GET: api/wishlist - Получить избранное
        // ========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist()
        {
            try
            {
                var userId = GetUserId();

                var items = await _context.WishlistItems
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Product)
                        .ThenInclude(p => p.Images)
                    .Include(w => w.Product)
                        .ThenInclude(p => p.Reviews)
                    .OrderByDescending(w => w.AddedAt)
                    .ToListAsync();

                return Ok(_mapper.Map<List<WishlistItemDto>>(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении избранного");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // POST: api/wishlist - Добавить в избранное
        // ========================================
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            try
            {
                var userId = GetUserId();

                // Проверяем, существует ли товар
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                    return NotFound(new { message = "Товар не найден" });

                // Проверяем, не добавлен ли уже
                var existingItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId);

                if (existingItem != null)
                    return BadRequest(new { message = "Товар уже в избранном" });

                var wishlistItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    AddedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(wishlistItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Товар добавлен в избранное" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении в избранное");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // DELETE: api/wishlist/{productId} - Удалить из избранного
        // ========================================
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();

                var item = await _context.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (item == null)
                    return NotFound(new { message = "Товар не найден в избранном" });

                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Товар удалён из избранного" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении из избранного");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // DELETE: api/wishlist - Очистить избранное
        // ========================================
        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            try
            {
                var userId = GetUserId();

                var items = await _context.WishlistItems
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

                _context.WishlistItems.RemoveRange(items);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Избранное очищено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке избранного");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // GET: api/wishlist/check/{productId} - Проверить в избранном
        // ========================================
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckInWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();

                var exists = await _context.WishlistItems
                    .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

                return Ok(new { inWishlist = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке избранного");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        private int GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            return int.Parse(userId);
        }
    }


    public class AddToWishlistRequest
    {
        [Required]
        public int ProductId { get; set; }
    }
}

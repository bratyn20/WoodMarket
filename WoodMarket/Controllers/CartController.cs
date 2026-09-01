using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CartController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить корзину
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCart")]
        [Authorize]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .ToListAsync();

            var itemsDto = _mapper.Map<List<CartItemDto>>(cartItems);
            var subtotal = itemsDto.Sum(i => i.TotalPrice);
            var shippingCost = subtotal >= 7000 ? 0 : 490;

            return Ok(new CartResponseDto
            {
                Items = itemsDto,
                Subtotal = subtotal,
                ShippingCost = shippingCost,
                DiscountAmount = 0,
                Total = subtotal + shippingCost,
                IsFreeShipping = subtotal >= 7000,
                TotalItems = itemsDto.Sum(i => i.Quantity)
            });
        }

        /// <summary>
        /// Добавление товара в корзину
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            if (product.StockQuantity < request.Quantity)
                return BadRequest(new { message = "Недостаточно товара на складе" });

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId &&
                                         c.ProductId == request.ProductId &&
                                         c.VariantName == request.VariantName);

            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    VariantName = request.VariantName,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар добавлен в корзину" });
        }

        /// <summary>
        /// Обновить количество
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity(UpdateCartRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == request.CartItemId && c.UserId == userId);

            if (cartItem == null)
                return NotFound(new { message = "Товар не найден в корзине" });

            if (request.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = request.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Корзина обновлена" });
        }

        /// <summary>
        /// Удаление товара из корзины
        /// </summary>
        /// <param name="cartItemId"></param>
        /// <returns></returns>
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
                return NotFound(new { message = "Товар не найден в корзине" });

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Товар удалён из корзины" });
        }

        /// <summary>
        /// Отчистить корзину
        /// </summary>
        /// <returns></returns>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Корзина очищена" });
        }
    }

    
    public class CartResponse
    {
        public List<CartItemDto> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public bool IsFreeShipping { get; set; }
        public int TotalItems { get; set; }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string VariantName { get; set; }
    }

    public class UpdateCartRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}

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
    [Authorize] // Требуется авторизация для всех методов
    public class OrderController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            AppDbContext context,
            IMapper mapper,
            ILogger<OrderController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ========================================
        // GET: api/order - Получить все заказы пользователя
        // ========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            try
            {
                var userId = GetUserId();

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(_mapper.Map<List<OrderResponse>>(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // GET: api/order/{id} - Получить детали заказа
        // ========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(int id)
        {
            try
            {
                var userId = GetUserId();

                var order = await _context.Orders
                    .Where(o => o.Id == id && o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync();

                if (order == null)
                    return NotFound(new { message = "Заказ не найден" });

                return Ok(_mapper.Map<OrderResponse>(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказа {OrderId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // POST: api/order - Создать новый заказ
        // ========================================
        [HttpPost]
        public async Task<ActionResult<OrderCreatedResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();

                // Получаем корзину пользователя
                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                    return BadRequest(new { message = "Корзина пуста" });

                // Проверяем наличие товаров на складе
                foreach (var item in cartItems)
                {
                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            message = $"Недостаточно товара '{item.Product.Name}' на складе. " +
                                      $"Доступно: {item.Product.StockQuantity}, запрошено: {item.Quantity}"
                        });
                    }
                }

                // Создаём заказ
                var subtotal = cartItems.Sum(c => c.Quantity * c.Product.Price);
                var shippingCost = subtotal >= 7000 ? 0 : 490;
                var discountAmount = 0m;

                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.New.ToString(),
                    Subtotal = subtotal,
                    ShippingCost = shippingCost,
                    DiscountAmount = discountAmount,
                    Total = subtotal + shippingCost - discountAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending.ToString(),
                    ShippingMethod = request.ShippingMethod,
                    ShippingAddress = request.ShippingAddress,
                    Comment = request.Comment
                };

                // Добавляем позиции заказа
                foreach (var cartItem in cartItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price,
                        TotalPrice = cartItem.Quantity * cartItem.Product.Price,
                        ProductName = cartItem.Product.Name,
                        ProductSku = cartItem.Product.Sku
                    });

                    // Уменьшаем остаток на складе
                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                }

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Ok(new OrderCreatedResponse
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Total = order.Total,
                    Message = "Заказ успешно создан"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // PUT: api/order/{id}/cancel - Отменить заказ
        // ========================================
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserId();

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                    return NotFound(new { message = "Заказ не найден" });

                if (order.Status == OrderStatus.Shipped.ToString() ||
                    order.Status == OrderStatus.Delivered.ToString())
                {
                    return BadRequest(new { message = "Заказ уже отправлен или доставлен, отмена невозможна" });
                }

                order.Status = OrderStatus.Cancelled.ToString();
                await _context.SaveChangesAsync();

                return Ok(new { message = "Заказ отменён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене заказа {OrderId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // GET: api/order/statuses - Получить статусы заказов
        // ========================================
        [HttpGet("statuses")]
        public IActionResult GetOrderStatuses()
        {
            var statuses = Enum.GetValues<OrderStatus>()
                .Select(s => new
                {
                    Value = s.ToString(),
                    DisplayName = GetStatusDisplayName(s)
                })
                .ToList();

            return Ok(statuses);
        }

        // ========================================
        // GET: api/order/payment-methods - Получить методы оплаты
        // ========================================
        [HttpGet("payment-methods")]
        public IActionResult GetPaymentMethods()
        {
            var methods = new[]
            {
                new { Value = "Card", DisplayName = "Банковской картой" },
                new { Value = "CashOnDelivery", DisplayName = "При получении" },
                new { Value = "Invoice", DisplayName = "По счёту" }
            };

            return Ok(methods);
        }

        // ========================================
        // GET: api/order/shipping-methods - Получить методы доставки
        // ========================================
        [HttpGet("shipping-methods")]
        public IActionResult GetShippingMethods()
        {
            var methods = new[]
            {
                new { Value = "SDEK", DisplayName = "СДЭК", Description = "Пункт выдачи или курьер" },
                new { Value = "RussianPost", DisplayName = "Почта России", Description = "Отделение рядом с вами" },
                new { Value = "Pickup", DisplayName = "Самовывоз", Description = "По согласованию" }
            };

            return Ok(methods);
        }

        // ========================================
        // Приватные методы
        // ========================================

        private int GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            return int.Parse(userId);
        }

        private string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"KELO-{date}-{random}";
        }

        private string GetStatusDisplayName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.New => "Новый",
                OrderStatus.Processing => "В обработке",
                OrderStatus.Shipped => "Отправлен",
                OrderStatus.Delivered => "Доставлен",
                OrderStatus.Cancelled => "Отменён",
                _ => status.ToString()
            };
        }

        public class OrderCreatedResponse
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; }
            public decimal Total { get; set; }
            public string Message { get; set; }
        }

        public class CreateOrderRequest
        {
            [Required]
            public string PaymentMethod { get; set; }

            [Required]
            public string ShippingMethod { get; set; }

            [Required]
            public string ShippingAddress { get; set; }

            public string Comment { get; set; }
        }

        // ========================================
        // Enums
        // ========================================
        public enum OrderStatus
        {
            New,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        }

        public enum PaymentStatus
        {
            Pending,
            Paid,
            Failed
        }
    }
}

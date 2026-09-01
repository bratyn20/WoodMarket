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
    public class CustomOrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomOrderController> _logger;

        public CustomOrderController(AppDbContext context, IMapper mapper, ILogger<CustomOrderController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/customorder - Получить все заявки пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CustomOrderDto>>> GetMyOrders()
        {
            try
            {
                var userId = GetUserId();

                var orders = await _context.CustomOrders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(_mapper.Map<List<CustomOrderDto>>(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заявок");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// GET: api/customorder/{id} - Получить заявку по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CustomOrderDto>> GetOrder(int id)
        {
            try
            {
                var userId = GetUserId();

                var order = await _context.CustomOrders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                    return NotFound(new { message = "Заявка не найдена" });

                return Ok(_mapper.Map<CustomOrderDto>(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заявки {Id}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// POST: api/customorder - Создать заявку
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<CustomOrderCreatedResponse>> CreateOrder([FromBody] CreateCustomOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.Identity.IsAuthenticated ? GetUserId() : 0;

                var customOrder = new CustomOrder
                {
                    UserId = userId,
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    Description = request.Description,
                    Status = CustomOrderStatus.New,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CustomOrders.Add(customOrder);
                await _context.SaveChangesAsync();

                // Здесь можно отправить уведомление менеджеру
                // ...

                return Ok(new CustomOrderCreatedResponse
                {
                    OrderId = customOrder.Id,
                    Message = "Заявка отправлена! Мы свяжемся с вами в ближайшее время."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заявки");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// GET: api/customorder/info - Информация о услуге
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public IActionResult GetServiceInfo()
        {
            return Ok(new
            {
                StartingPrice = 5900m,
                MinProductionDays = 10,
                MaxProductionDays = 20,
                MinMacetDays = 2,
                Steps = new[]
                {
                    new { Number = 1, Title = "Обсуждаем идею",
                        Description = "Получаем рисунок, фото или описание и уточняем размер изделия" },
                    new { Number = 2, Title = "Готовим макет",
                        Description = "Адаптируем детали под рельеф и согласуем визуальный эскиз" },
                    new { Number = 3, Title = "Делаем образец",
                        Description = "Проверяем глубину, чистоту линий и текстовый отпечаток" },
                    new { Number = 4, Title = "Изготавливаем заказ",
                        Description = "Финишируем, покрываем и надёжно упаковываем" }
                },
                WhatCanOrder = new[]
                {
                    "Пряничные доски по рисунку или фотографии",
                    "Формы с логотипом для кондитерских и брендов",
                    "Менажницы и сервировочные доски необычной формы",
                    "Подарочные наборы и упаковку",
                    "Повторные партии по согласованному макету"
                }
            });
        }

        private int GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            return int.Parse(userId);
        }
    }


    // ========================================
    // DTOs
    // ========================================
    

    public class CreateCustomOrderRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Description { get; set; }
    }

    public class CustomOrderCreatedResponse
    {
        public int OrderId { get; set; }
        public string Message { get; set; }
    }
}

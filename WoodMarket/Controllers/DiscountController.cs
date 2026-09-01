using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(AppDbContext context, IMapper mapper, ILogger<DiscountController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// api/discount - Получить все активные акции
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiscountDto>>> GetActiveDiscounts()
        {
            try
            {
                var now = DateTime.UtcNow;

                var discounts = await _context.Discounts
                    .Where(d => d.IsActive
                                && d.ValidFrom <= now
                                && (d.ValidTo == null || d.ValidTo >= now))
                    .OrderBy(d => d.ValidTo ?? DateTime.MaxValue)
                    .ToListAsync();

                return Ok(_mapper.Map<List<DiscountDto>>(discounts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении акций");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// POST: api/discount/validate - Проверить промокод
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("validate")]
        public async Task<ActionResult<DiscountValidationResponse>> ValidatePromoCode([FromBody] ValidatePromoCodeRequest request)
        {
            try
            {
                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code.ToUpper() == request.Code.ToUpper() && d.IsActive);

                if (discount == null)
                    return NotFound(new DiscountValidationResponse
                    {
                        IsValid = false,
                        Message = "Промокод не найден"
                    });

                var now = DateTime.UtcNow;
                if (discount.ValidFrom > now)
                {
                    return BadRequest(new DiscountValidationResponse
                    {
                        IsValid = false,
                        Message = "Промокод ещё не активен"
                    });
                }

                if (discount.ValidTo.HasValue && discount.ValidTo < now)
                {
                    return BadRequest(new DiscountValidationResponse
                    {
                        IsValid = false,
                        Message = "Промокод просрочен"
                    });
                }

                // Проверка лимита использований
                if (discount.UsageLimit.HasValue)
                {
                    var usageCount = await _context.DiscountUsages
                        .CountAsync(u => u.DiscountId == discount.Id);

                    if (usageCount >= discount.UsageLimit.Value)
                    {
                        return BadRequest(new DiscountValidationResponse
                        {
                            IsValid = false,
                            Message = "Промокод больше не действителен"
                        });
                    }
                }

                return Ok(new DiscountValidationResponse
                {
                    IsValid = true,
                    DiscountId = discount.Id,
                    Type = discount.Type.ToString(),
                    Amount = discount.Amount,
                    MinOrderAmount = discount.MinOrderAmount,
                    Description = discount.Description,
                    Message = "Промокод применён"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке промокода");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }


    public class ValidatePromoCodeRequest
    {
        [Required]
        public string Code { get; set; }

        public decimal? OrderAmount { get; set; }
    }

    public class DiscountValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public int? DiscountId { get; set; }
        public string Type { get; set; }
        public decimal? Amount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public string Description { get; set; }
    }
}

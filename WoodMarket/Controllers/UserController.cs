using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<User> userManager,
            AppDbContext context,
            IMapper mapper,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        ///  GET: api/user/profile - Получить профиль пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                    return NotFound(new { message = "Пользователь не найден" });

                return Ok(_mapper.Map<UserProfileDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// PUT: api/user/profile - Обновить профиль
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                    return NotFound(new { message = "Пользователь не найден" });

                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.PhoneNumber = request.Phone ?? user.PhoneNumber;
                user.Email = request.Email ?? user.Email;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new { message = "Профиль обновлён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении профиля");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// GET: api/user/stats - Получить статистику пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                var userId = GetUserId();

                var ordersCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId);

                var wishlistCount = await _context.WishlistItems
                    .CountAsync(w => w.UserId == userId);

                var reviewsCount = await _context.Reviews
                    .CountAsync(r => r.UserId == userId);

                var totalSpent = await _context.Orders
                    .Where(o => o.UserId == userId && o.Status == "Delivered")
                    .SumAsync(o => (decimal?)o.Total) ?? 0;

                return Ok(new UserStatsDto
                {
                    OrdersCount = ordersCount,
                    WishlistCount = wishlistCount,
                    ReviewsCount = reviewsCount,
                    TotalSpent = totalSpent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// POST: api/user/change-password - Сменить пароль
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                    return NotFound(new { message = "Пользователь не найден" });

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    request.CurrentPassword,
                    request.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new { message = "Пароль успешно изменён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене пароля");
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

    // ========================================
    // DTOs
    // ========================================
    

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class UserStatsDto
    {
        public int OrdersCount { get; set; }
        public int WishlistCount { get; set; }
        public int ReviewsCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}

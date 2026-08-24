using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _logger = logger;
        }

        // ========================================
        // 📝 РЕГИСТРАЦИЯ
        // ========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // 1. Валидация модели
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. Проверка, существует ли пользователь
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "Пользователь с таким email уже существует" });

                // 3. Создание пользователя
                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { message = "Ошибка регистрации", errors });
                }

                // 4. Добавление роли (по умолчанию "User")
                await _userManager.AddToRoleAsync(user, "User");

                // 5. Генерация токена
                var token = GenerateJwtToken(user);

                // 6. Возврат данных
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Регистрация успешна",
                    AccessToken = token,
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // 🔑 ВХОД
        // ========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // 1. Валидация модели
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. Поиск пользователя
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized(new { message = "Неверный email или пароль" });

                // 3. Проверка пароля
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return Unauthorized(new { message = "Аккаунт заблокирован. Попробуйте позже." });

                    return Unauthorized(new { message = "Неверный email или пароль" });
                }

                // 4. Обновление времени последнего входа
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // 5. Генерация токена
                var token = GenerateJwtToken(user);

                // 6. Возврат данных
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Вход выполнен",
                    AccessToken = token,
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // 🔄 ОБНОВЛЕНИЕ ТОКЕНА (Refresh Token)
        // ========================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            try
            {
                // Здесь должна быть логика с Refresh Token
                // Если у вас его нет, верните ошибку
                return BadRequest(new { message = "Refresh token не поддерживается" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении токена");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // 🚪 ВЫХОД (для клиента)
        // ========================================
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // В JWT нет серверного выхода, клиент просто удаляет токен
            // Но можно аннулировать Refresh Token, если он есть
            return Ok(new { message = "Выход выполнен" });
        }

        // ========================================
        // 🔧 ГЕНЕРАЦИЯ JWT ТОКЕНА
        // ========================================
        private string GenerateJwtToken(User user)
        {
            // Получаем роли пользователя
            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Добавляем роли в claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // ✅ Токен живёт 1 день
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ========================================
    // DTOs
    // ========================================
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RefreshTokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

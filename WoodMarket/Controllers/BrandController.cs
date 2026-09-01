using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandController> _logger;

        public BrandController(AppDbContext context, IMapper mapper, ILogger<BrandController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/brand - Получить все бренды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            try
            {
                var brands = await _context.Brands
                    .Include(b => b.Products)
                    .ToListAsync();

                return Ok(_mapper.Map<List<BrandDto>>(brands));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении брендов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// GET: api/brand/{slug} - Получить бренд по slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<BrandDto>> GetBrand(string slug)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .FirstOrDefaultAsync(b => b.Slug == slug);

                if (brand == null)
                    return NotFound(new { message = "Бренд не найден" });

                return Ok(_mapper.Map<BrandDto>(brand));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении бренда {Slug}", slug);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }

}

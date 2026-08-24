using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BlogController> _logger;

        public BlogController(AppDbContext context, IMapper mapper, ILogger<BlogController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ========================================
        // GET: api/blog - Получить все статьи
        // ========================================
        [HttpGet]
        public async Task<ActionResult<BlogListResponse>> GetPosts(
            [FromQuery] string category = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 9)
        {
            try
            {
                var query = _context.BlogPosts
                    .Where(b => b.IsPublished)
                    .Include(b => b.BlogTags)
                        .ThenInclude(bt => bt.Tag)
                    .Include(b => b.Author)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(b => b.Category == category);
                }

                var totalPosts = await query.CountAsync();

                var posts = await query
                    .OrderByDescending(b => b.PublishedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var categories = await _context.BlogPosts
                    .Where(b => b.IsPublished)
                    .Select(b => b.Category)
                    .Distinct()
                    .ToListAsync();

                return Ok(new BlogListResponse
                {
                    Posts = _mapper.Map<List<BlogPostDto>>(posts),
                    TotalPosts = totalPosts,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize),
                    Categories = categories.Where(c => !string.IsNullOrEmpty(c)).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статей блога");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // GET: api/blog/{slug} - Получить статью по slug
        // ========================================
        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPostDto>> GetPost(string slug)
        {
            try
            {
                var post = await _context.BlogPosts
                    .Include(b => b.BlogTags)
                        .ThenInclude(bt => bt.Tag)
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

                if (post == null)
                    return NotFound(new { message = "Статья не найдена" });

                // Похожие статьи
                var relatedPosts = await _context.BlogPosts
                    .Where(b => b.Category == post.Category
                                && b.Id != post.Id
                                && b.IsPublished)
                    .Take(3)
                    .ToListAsync();

                var result = _mapper.Map<BlogPostDto>(post);
                result.RelatedPosts = _mapper.Map<List<BlogPostDto>>(relatedPosts);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статьи {Slug}", slug);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        // ========================================
        // GET: api/blog/categories - Получить категории блога
        // ========================================
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.BlogPosts
                .Where(b => b.IsPublished)
                .Select(b => b.Category)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .ToListAsync();

            return Ok(categories);
        }
    }

    // ========================================
    // DTOs
    // ========================================
    

    public class BlogListResponse
    {
        public List<BlogPostDto> Posts { get; set; }
        public int TotalPosts { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<string> Categories { get; set; }
    }
}

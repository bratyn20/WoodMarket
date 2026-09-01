using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Карточка товара
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<ProductDetailsResponseDto>> GetProduct(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Material)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Specifications)
                    .ThenInclude(ps => ps.Specification)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            var reviews = product.Reviews
                .Where(r => r.IsApproved)
                .ToList();

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId
                            && p.Id != product.Id
                            && p.IsActive)
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .Include(p => p.Material)
                .Include(p => p.Brand)
                .Take(6)
                .ToListAsync();

            return Ok(new ProductDetailsResponseDto
            {
                Product = _mapper.Map<ProductFullDto>(product),
                Reviews = _mapper.Map<List<ReviewDto>>(reviews),
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = reviews.Count,
                RelatedProducts = _mapper.Map<List<ProductDto>>(relatedProducts)
            });
        }

        /// <summary>
        /// Добавить отзыв
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{productId}/reviews")]
        public async Task<IActionResult> AddReview(int productId, [FromBody] AddReviewRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Отзыв отправлен на модерацию" });
        }
    }

    public class AddReviewRequest
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}

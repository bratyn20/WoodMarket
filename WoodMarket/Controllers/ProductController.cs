using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ProductDetailsDto>> GetProduct(string slug)
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
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UserName = $"{r.User.Name} {r.User.LastName}"
                })
                .ToList();

            // Похожие товары
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId
                            && p.Id != product.Id
                            && p.IsActive)
                .Take(6)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    MainImageUrl = p.MainImageUrl,
                    Slug = p.Slug,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = p.Reviews.Count(r => r.IsApproved)
                })
                .ToListAsync();

            return Ok(new ProductDetailsDto
            {
                Product = new ProductFullDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    ShortDescription = product.ShortDescription,
                    FullDescription = product.FullDescription,
                    Price = product.Price,
                    OldPrice = product.OldPrice,
                    MainImageUrl = product.MainImageUrl,
                    Images = product.Images.Select(i => i.ImageUrl).ToList(),
                    Variants = product.Variants.Select(v => new VariantDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Size = v.Size,
                        AdditionalPrice = v.AdditionalPrice,
                        StockQuantity = v.StockQuantity,
                        Sku = v.Sku
                    }).ToList(),
                    Specifications = product.Specifications.ToDictionary(
                        ps => ps.Specification.Name,
                        ps => ps.Value),
                    Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
                    StockQuantity = product.StockQuantity,
                    IsInStock = product.StockQuantity > 0,
                    IsNew = product.IsNew,
                    IsOnSale = product.OldPrice.HasValue,
                    CategoryName = product.Category?.Name,
                    BrandName = product.Brand?.Name,
                    MaterialName = product.Material?.Name,
                    Slug = product.Slug
                },
                Reviews = reviews,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = reviews.Count,
                RelatedProducts = relatedProducts
            });
        }

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

    // DTOs
    public class ProductDetailsDto
    {
        public ProductFullDto Product { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ProductDto> RelatedProducts { get; set; }
    }

    public class ProductFullDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string MainImageUrl { get; set; }
        public List<string> Images { get; set; }
        public List<VariantDto> Variants { get; set; }
        public Dictionary<string, string> Specifications { get; set; }
        public List<string> Tags { get; set; }
        public int StockQuantity { get; set; }
        public bool IsInStock { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string MaterialName { get; set; }
        public string Slug { get; set; }
    }

    public class VariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }

    public class AddReviewRequest
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}

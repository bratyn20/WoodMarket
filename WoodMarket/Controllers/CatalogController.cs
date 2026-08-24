using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/catalog/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive),
                    //ImageUrl = c.ImageUrl ?? "/images/categories/default.jpg",
                    DisplayOrder = c.DisplayOrder
                })
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/catalog/products
        [HttpGet("products")]
        public async Task<ActionResult<CatalogResponse>> GetProducts(
            [FromQuery] CatalogFilter filter)
        {
            var query = _context.Products
                .Include(p => p.Material)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .Where(p => p.IsActive);

            // Фильтры
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.MaterialId.HasValue)
                query = query.Where(p => p.MaterialId == filter.MaterialId);

            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice);

            if (filter.OnlyInStock)
                query = query.Where(p => p.StockQuantity > 0);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(filter.SearchTerm) ||
                    p.ShortDescription.Contains(filter.SearchTerm) ||
                    p.Sku.Contains(filter.SearchTerm));
            }

            // Сортировка
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id)
            };

            var totalItems = await query.CountAsync();

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ShortDescription = p.ShortDescription,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    MainImageUrl = p.MainImageUrl,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = p.Reviews.Count(r => r.IsApproved),
                    StockQuantity = p.StockQuantity,
                    IsInStock = p.StockQuantity > 0,
                    IsNew = p.IsNew,
                    IsOnSale = p.OldPrice.HasValue,
                    Slug = p.Slug,
                    //MaterialName = p.Material.Name,
                    //BrandName = p.Brand.Name
                })
                .ToListAsync();

            // Данные для фильтров
            var materials = await _context.Materials
                .Select(m => new FilterItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Count = _context.Products.Count(p => p.MaterialId == m.Id && p.IsActive)
                })
                .ToListAsync();

            var brands = await _context.Brands
                .Select(b => new FilterItemDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Count = _context.Products.Count(p => p.BrandId == b.Id && p.IsActive)
                })
                .ToListAsync();

            return Ok(new CatalogResponse
            {
                Products = products,
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize),
                Materials = materials,
                Brands = brands
            });
        }
    }

    // DTOs
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int ProductCount { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CatalogFilter
    {
        public int? CategoryId { get; set; }
        public int? MaterialId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool OnlyInStock { get; set; }
        public string SortBy { get; set; } = "popularity";
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CatalogResponse
    {
        public List<ProductDto> Products { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<FilterItemDto> Materials { get; set; }
        public List<FilterItemDto> Brands { get; set; }
    }

    public class FilterItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}

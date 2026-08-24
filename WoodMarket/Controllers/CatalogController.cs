using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodMarket.Dto;
using WoodMarket.Models;

namespace WoodMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CatalogController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(_mapper.Map<List<CategoryDto>>(categories));
        }

        [HttpGet("products")]
        public async Task<ActionResult<CatalogResponseDto>> GetProducts(
            [FromQuery] CatalogFilter filter)
        {
            var query = _context.Products
                .Include(p => p.Material)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Images)
                .Where(p => p.IsActive);

            // Фильтры...
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
                    p.ShortDescription.Contains(filter.SearchTerm));
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
                .ToListAsync();

            var materials = await _context.Materials
                .Include(m => m.Products)
                .ToListAsync();

            var brands = await _context.Brands
                .Include(b => b.Products)
                .ToListAsync();

            return Ok(new CatalogResponseDto
            {
                Products = _mapper.Map<List<ProductDto>>(products),
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize),
                Materials = _mapper.Map<List<MaterialDto>>(materials),
                Brands = _mapper.Map<List<BrandDto>>(brands)
            });
        }
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
}

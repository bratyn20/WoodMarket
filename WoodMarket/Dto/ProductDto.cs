using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string MainImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int StockQuantity { get; set; }
        public bool IsInStock { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public string Slug { get; set; }
        public string MaterialName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
    }

    public class AdminProductDto : ProductDto
    {
        public string FullDescription { get; set; }
        public decimal? CostPrice { get; set; }
        public string Sku { get; set; }
        public int? LowStockThreshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MaterialId { get; set; }
    }

    public class AdminProductUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string Sku { get; set; }
        public int StockQuantity { get; set; }
        public int? LowStockThreshold { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public string MainImageUrl { get; set; }
        public string Slug { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MaterialId { get; set; }
    }

    public class ProductFullDto : ProductDto
    {
        public string FullDescription { get; set; }
        public List<string> Images { get; set; }
        public List<VariantDto> Variants { get; set; }
        public Dictionary<string, string> Specifications { get; set; }
        public List<string> Tags { get; set; }
    }

    public class ProductUpdateWithImageDto : AdminProductUpdateDto
    {
        public IFormFile? ImageFile { get; set; } // Одно изображение
        public bool RemoveImage { get; set; }     // Флаг для удаления
    }

    public class ProductImageUploadDto
    {
        public int ProductId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OldPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CostPrice { get; set; }

        [MaxLength(100)]
        public string Sku { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int? LowStockThreshold { get; set; }

        public bool IsNew { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        public IFormFile? ImageFile { get; set; }
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

    public class MaterialDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageUrl { get; set; }
        public int MaxQuantity { get; set; }
        public string VariantName { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }

    public class OrderItemResponse
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageUrl { get; set; }
    }


    public class DiscountDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CustomOrderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public string AdminComment { get; set; }
    }

    // Для главной страницы
    public class HomeResponseDto
    {
        public int TotalProducts { get; set; }
        public double AverageRating { get; set; }
        public int TotalOrders { get; set; }
        public int RegionsCount { get; set; }
        public List<ProductDto> NewProducts { get; set; }
        public List<AdvantageDto> Advantages { get; set; }
        public string CompanyName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public int GuaranteeMonths { get; set; }
        public int ReadySketches { get; set; }
    }

    public class AdvantageDto
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    // Для каталога
    public class CatalogResponseDto
    {
        public List<ProductDto> Products { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public List<BrandDto> Brands { get; set; }
    }

    // Для детальной страницы товара
    public class ProductDetailsResponseDto
    {
        public ProductFullDto Product { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ProductDto> RelatedProducts { get; set; }
    }

    // Для корзины
    public class CartResponseDto
    {
        public List<CartItemDto> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public bool IsFreeShipping { get; set; }
        public int TotalItems { get; set; }
    }

    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string MainImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsInStock { get; set; }
        public string Slug { get; set; }
        public DateTime AddedAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Товар/изделие
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        [MaxLength(100)]
        public string Sku { get; set; }

        public int StockQuantity { get; set; }

        public int? LowStockThreshold { get; set; }

        public bool IsInStock { get; set; }

        public bool IsOnSale { get; set; }

        public bool IsNew { get; set; }

        public bool IsActive { get; set; }

        public string MainImageUrl { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Внешние ключи
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MaterialId { get; set; }

        // Навигационные свойства
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        [ForeignKey(nameof(BrandId))]
        public Brand Brand { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material Material { get; set; }

        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductVariant> Variants { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; }
        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductSpecification> Specifications { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}

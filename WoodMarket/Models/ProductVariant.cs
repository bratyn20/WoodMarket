using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Вариант товара (размер, глубина резьбы и т.д.)
    /// </summary>
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Size { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdditionalPrice { get; set; }

        public int StockQuantity { get; set; }

        [MaxLength(100)]
        public string Sku { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}

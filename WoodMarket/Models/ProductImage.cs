using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Изображение товара
    /// </summary>
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(200)]
        public string AltText { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}

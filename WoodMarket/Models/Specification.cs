using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Характеристика товара
    /// </summary>
    public class Specification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Бренд/коллекция (КЕЛО, КЕЛО Studio, КЕЛО Home)
    /// </summary>
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string LogoUrl { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}

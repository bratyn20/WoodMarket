using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Тег товара (например, "хит", "новинка")
    /// </summary>
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public ICollection<ProductTag> ProductTags { get; set; }
    }
}

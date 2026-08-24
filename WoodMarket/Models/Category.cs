using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Категория товара
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(ParentCategoryId))]
        public Category ParentCategory { get; set; }

        public ICollection<Category> ChildCategories { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}

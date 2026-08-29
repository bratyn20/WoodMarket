using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Dto
{
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

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; } = 0;

    }

    public class UpdateCategoryDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; }
    }
}

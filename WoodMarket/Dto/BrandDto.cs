using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Dto
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string LogoUrl { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateBrandDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public IFormFile? Logo { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBrandDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public IFormFile? Logo { get; set; }
        public string? LogoUrl { get; set; }
        public bool RemoveLogo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

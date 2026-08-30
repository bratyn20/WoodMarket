using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Dto
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string FeaturedImageUrl { get; set; }
        public string Category { get; set; }
        public int ReadTimeMinutes { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } // Имя автора
        public List<string> Tags { get; set; } = new List<string>();

        public List<BlogPostDto> RelatedPosts { get; set; } = new List<BlogPostDto>();
    }

    public class CreateBlogPostDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string ShortDescription { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } // Советы, Истории, Новости, Уход за деревом

        public IFormFile? FeaturedImage { get; set; }

        public int ReadTimeMinutes { get; set; } = 5;

        public bool IsPublished { get; set; } = true;

        public List<string> Tags { get; set; } = new List<string>();
    }

    public class UpdateBlogPostDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string ShortDescription { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        public IFormFile? FeaturedImage { get; set; }

        public bool RemoveFeaturedImage { get; set; }

        public int ReadTimeMinutes { get; set; } = 5;

        public bool IsPublished { get; set; } = true;

        public List<string> Tags { get; set; } = new List<string>();
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Статья блога
    /// </summary>
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string ShortDescription { get; set; }

        public string Content { get; set; }

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; }

        [MaxLength(200)]
        public string FeaturedImageUrl { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } // Советы, Истории, Новости, Уход за деревом

        public int ReadTimeMinutes { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsPublished { get; set; }

        public int? AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public User Author { get; set; }

        public ICollection<BlogTag> BlogTags { get; set; }
    }
}

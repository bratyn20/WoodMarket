using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    public class BlogTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        [Required]
        public int TagId { get; set; }

        [ForeignKey(nameof(BlogPostId))]
        public BlogPost BlogPost { get; set; }

        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; }
    }
}

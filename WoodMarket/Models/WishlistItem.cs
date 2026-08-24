using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Избранные товары
    /// </summary>
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }

}

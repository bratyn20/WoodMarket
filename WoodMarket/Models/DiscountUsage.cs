using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Использование промокода
    /// </summary>
    public class DiscountUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DiscountId { get; set; }

        [Required]
        public int OrderId { get; set; }

        public int? UserId { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}

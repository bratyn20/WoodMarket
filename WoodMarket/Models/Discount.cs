using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Промокод/скидка
    /// </summary>
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DiscountType Type { get; set; } // Percentage, FixedAmount

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }

        public bool IsActive { get; set; }

        public ICollection<DiscountUsage> Usages { get; set; }
    }

    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }
}

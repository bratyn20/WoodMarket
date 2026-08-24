using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Заказ
    /// </summary>
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // New, Processing, Shipped, Delivered, Cancelled

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(100)]
        public string PaymentMethod { get; set; } // Card, CashOnDelivery, Invoice

        [MaxLength(50)]
        public string PaymentStatus { get; set; } // Pending, Paid, Failed

        [MaxLength(50)]
        public string ShippingMethod { get; set; } // SDEK, RussianPost, Pickup

        [MaxLength(500)]
        public string ShippingAddress { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

}

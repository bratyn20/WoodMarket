using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Индивидуальный заказ (изделие на заказ)
    /// </summary>
    public class CustomOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required]
        public string Description { get; set; } // Расскажите, что вам нужно

        public CustomOrderStatus Status { get; set; } = CustomOrderStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? QuoteSentAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedPrice { get; set; }

        [MaxLength(500)]
        public string AdminComment { get; set; }

        public int? AssignedToUserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(AssignedToUserId))]
        public User AssignedTo { get; set; }

        public ICollection<CustomOrderAttachment> Attachments { get; set; }
    }

    public enum CustomOrderStatus
    {
        New,
        InProgress,
        QuoteSent,
        AwaitingApproval,
        Approved,
        InProduction,
        Completed,
        Cancelled
    }
}

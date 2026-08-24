using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Вложение для индивидуального заказа
    /// </summary>
    public class CustomOrderAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomOrderId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; }

        [MaxLength(200)]
        public string FileName { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CustomOrderId))]
        public CustomOrder CustomOrder { get; set; }
    }
}

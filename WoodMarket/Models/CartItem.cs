using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Элемент корзины покупок
    /// </summary>
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Название варианта (если товар имеет варианты, например, размер, цвет)
        /// </summary>
        [MaxLength(100)]
        public string VariantName { get; set; }

        /// <summary>
        /// Дата добавления в корзину
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}

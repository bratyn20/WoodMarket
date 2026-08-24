using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Подписка на новости
    /// </summary>
    public class NewsletterSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? UnsubscribedAt { get; set; }
    }

}

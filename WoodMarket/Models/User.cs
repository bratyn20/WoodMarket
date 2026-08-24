using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace WoodMarket.Models
{
    /// <summary>
    /// Пользователь/покупатель
    /// </summary>
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Для Refresh Token (если будете добавлять)
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Навигационные свойства
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<CustomOrder> CustomOrders { get; set; }

        public ICollection<CustomOrder> AssignedCustomOrders { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}

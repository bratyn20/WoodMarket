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

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "testName";

        [MaxLength(100)]
        public string LastName { get; set; } = "testLastName";


        public string City { get; set; } = "testCity";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        public ICollection<Order> Orders { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}

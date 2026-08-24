using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Адрес доставки
    /// </summary>
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(200)]
        public string AddressLine { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }

}

using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    /// <summary>
    /// Системная настройка
    /// </summary>
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}

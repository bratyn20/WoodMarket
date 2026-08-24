using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Models
{
    public class ProductSpecification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SpecificationId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Value { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(SpecificationId))]
        public Specification Specification { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class Combo
    {
        [Key]
        public int ComboId { get; set; }
        [Required, StringLength(100)]
        public string ComboName { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        public string Slug { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal ComboPrice { get; set; }
        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<ComboDetail>? ComboDetails { get; set; }
    }
}

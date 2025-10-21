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

        [NotMapped]
        public decimal ComboPrice => ComboDetails?.Sum(cd => cd.FoodItem!.BasePrice * cd.Quantity) * (1 - DiscountPercentage/100) ?? 0;

        public decimal? DiscountPercentage { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<ComboDetail>? ComboDetails { get; set; }
    }
}

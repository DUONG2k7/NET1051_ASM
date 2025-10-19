using System.ComponentModel.DataAnnotations;
using static Azure.Core.HttpHeader;

namespace ASM_1.Models.Food
{
    public class ComboDetail
    {
        [Key]
        public int ComboDetailId { get; set; }

        [Required]
        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        [Required]
        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }

        public int Quantity { get; set; }
    }
}

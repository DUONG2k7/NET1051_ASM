using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class MenuItemOptionValue
    {
        [Key] public int Id { get; set; }

        [Required] public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; } = default!;

        [Required] public int OptionValueId { get; set; }
        public OptionValue OptionValue { get; set; } = default!;

        public bool IsHidden { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")] public decimal? PriceDeltaOverride { get; set; }
        public bool? IsDefaultOverride { get; set; }
        public int? SortOrderOverride { get; set; }
    }
}

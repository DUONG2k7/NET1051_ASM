using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Food
{
    public class MenuItemOptionGroup
    {
        [Key] public int Id { get; set; }

        [Required] public int FoodItemId { get; set; }   // FK tới món
        public FoodItem FoodItem { get; set; } = default!;

        [Required] public int OptionGroupId { get; set; }
        public OptionGroup OptionGroup { get; set; } = default!;

        // Cho phép override theo từng món
        public bool? Required { get; set; }
        public int? MinSelect { get; set; }
        public int? MaxSelect { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}

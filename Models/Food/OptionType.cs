using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Food
{
    public class OptionType
    {
        [Key]
        public int OptionTypeId { get; set; }

        [Required, StringLength(50)]
        public string TypeName { get; set; }

        public string? Description { get; set; }

        public ICollection<FoodOption>? FoodOptions { get; set; }
    }
}

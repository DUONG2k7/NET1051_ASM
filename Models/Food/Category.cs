using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Food
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<FoodItem>? FoodItems { get; set; }
    }
}

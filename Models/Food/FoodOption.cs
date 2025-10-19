using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_1.Models.Food
{
    public class FoodOption
    {
        [Key]
        public int FoodOptionId { get; set; }

        [Required]
        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }

        [Required, StringLength(100)]
        public string OptionName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraPrice { get; set; }

        [Required]
        public int OptionTypeId { get; set; } // FK
        public OptionType? OptionType { get; set; } // navigation property

        public bool IsAvailable { get; set; } = true; // còn phục vụ hay hết
        public int StockQuantity { get; set; } = 0;

        public ICollection<InvoiceDetailFoodOption> InvoiceDetailFoodOptions { get; set; }
    }
}

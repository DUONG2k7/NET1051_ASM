using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ASM_1.Models.Food
{
    public class FoodItem
    {
        [Key]
        public int FoodItemId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int StockQuantity { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<FoodOption>? FoodOptions { get; set; }
        public ICollection<ComboDetail>? ComboDetails { get; set; }
        public ICollection<InvoiceDetail>? InvoiceDetails { get; set; }
    }
}
